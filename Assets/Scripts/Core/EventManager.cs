using System;
using System.Data;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    private static EventManager _instance;
    public static EventManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<EventManager>();
                if (_instance == null)
                {
                    GameObject core = GameObject.Find("GAME_CORE") ?? GameObject.Find("[GAME_CORE]");
                    if (core != null)
                    {
                        _instance = core.AddComponent<EventManager>();
                    }
                    else
                    {
                        GameObject go = new GameObject("[EventManager]");
                        _instance = go.AddComponent<EventManager>();
                        DontDestroyOnLoad(go);
                    }
                }
            }
            return _instance;
        }
        private set => _instance = value;
    }

    public event Action<GameEvent> OnEventTriggered;
    public event Action<GameEvent> OnEventCompleted;
    public event Action<string, int> OnFlagChanged;

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(this);
        }
    }

    // =========================================================================
    // 1. EVALUASI DAN PEMILIHAN EVENT BERBASIS KONDISI (RULE ENGINE)
    // =========================================================================

    /// <summary>
    /// Memeriksa seluruh event kondisional di tbl_game_events yang aktif.
    /// Memilih event dengan prioritas tertinggi yang seluruh prasyaratnya terpenuhi.
    /// Mengembalikan true jika ada event yang terpicu.
    /// </summary>
    public bool TryEvaluateAndTriggerEvent(TimeBlock currentTimeBlock, int currentDay)
    {
        if (DatabaseManager.Instance == null) return false;

        try
        {
            // Ambil semua event yang belum selesai atau berstatus repeatable
            string query = "SELECT * FROM tbl_game_events WHERE is_completed = 0 OR is_repeatable = 1 " +
                           "ORDER BY priority DESC, event_id ASC;";
            DataTable dt = DatabaseManager.Instance.ExecuteQuery(query);

            if (dt == null || dt.Rows.Count == 0) return false;

            foreach (DataRow row in dt.Rows)
            {
                GameEvent candidate = GameEvent.FromDataRow(row);
                if (candidate == null) continue;

                if (IsEventEligible(candidate, currentTimeBlock, currentDay))
                {
                    TriggerEvent(candidate);
                    return true;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[EventManager] Terjadi kesalahan saat evaluasi event: {ex.Message}");
        }

        return false;
    }

    /// <summary>
    /// Mengevaluasi apakah satu GameEvent memenuhi seluruh kriteria status & kalender.
    /// </summary>
    public bool IsEventEligible(GameEvent evt, TimeBlock currentTimeBlock, int currentDay)
    {
        // 1. Rentang Hari
        if (currentDay < evt.startDay || currentDay > evt.endDay) return false;

        // 2. Blok Waktu
        if (!string.IsNullOrEmpty(evt.timeBlock))
        {
            if (!evt.timeBlock.Equals(currentTimeBlock.ToString(), StringComparison.OrdinalIgnoreCase))
                return false;
        }

        // 3. Tipe Hari (0: Bebas, 1: Workday, 2: Weekend)
        if (GameManager.Instance != null)
        {
            bool isWorkday = GameManager.Instance.IsWorkday();
            if (evt.dayType == 1 && !isWorkday) return false;
            if (evt.dayType == 2 && isWorkday) return false;
        }

        // 4. Parameter Player Stats
        if (PlayerStats.Instance != null)
        {
            var ps = PlayerStats.Instance;
            if (ps.physicalHealth < evt.reqMinPh || ps.physicalHealth > evt.reqMaxPh) return false;
            if (ps.mentalHealth < evt.reqMinMh || ps.mentalHealth > evt.reqMaxMh) return false;
            if (ps.academicTheoretical < evt.reqMinAcadTheory) return false;
            if (ps.academicPractical < evt.reqMinAcadPractice) return false;
            if (ps.languageProficiency < evt.reqMinLang) return false;
            if (ps.culturalEtiquette < evt.reqMinEtiquette) return false;
        }

        // 5. Level Rumor
        if (evt.reqRumorLevel >= 0)
        {
            int currentRumor = SocialManager.Instance != null ? SocialManager.Instance.globalRumorLevel : 0;
            if (currentRumor != evt.reqRumorLevel) return false;
        }

        // 6. Relasi & Afeksi NPC (Jika event membutuhkan NPC tertentu)
        if (evt.reqNpcId > 0 && SocialManager.Instance != null)
        {
            var rel = SocialManager.Instance.relations.Find(x => x.npcId == evt.reqNpcId);
            if (rel == null) return false;
            if (rel.affectionState < evt.reqMinAffectionState) return false;
            if (rel.guanxiScore < evt.reqMinGuanxi) return false;
        }

        // 7. Prasyarat Event Sebelumnya (Event Chaining)
        if (evt.prereqEventId > 0)
        {
            if (!IsEventCompleted(evt.prereqEventId)) return false;
        }

        // 8. Prasyarat Story Flags
        if (!string.IsNullOrEmpty(evt.reqFlags))
        {
            string[] flags = evt.reqFlags.Split(new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var f in flags)
            {
                if (!HasFlag(f.Trim())) return false;
            }
        }

        return true;
    }

    // =========================================================================
    // 2. EKSEKUSI EVENT DAN HASIL
    // =========================================================================

    /// <summary>
    /// Memicu event terpilih, membuka dialog, dan mendaftarkan callback penyelesaian.
    /// </summary>
    public void TriggerEvent(GameEvent evt)
    {
        Debug.Log($"<color=yellow>[EVENT CONDITION ENGINE]</color> <b>Event Dipicu:</b> '{evt.eventName}' " +
                  $"(ID: {evt.eventId}, Code: {evt.eventCode}, Priority: {evt.priority})");

        if (HUDController.Instance != null)
        {
            HUDController.Instance.ShowToastNotification($"[EVENT] {evt.eventName}");
        }

        if (TelemetryLogger.Instance != null)
        {
            TelemetryLogger.Instance.RecordCriticalEvent("EVENT_TRIGGERED", $"Event '{evt.eventName}' ({evt.eventCode}) berhasil dipicu");
        }

        OnEventTriggered?.Invoke(evt);

        // Delegasi khusus untuk evaluasi tengah semester (Hari 30)
        if (evt.eventCode == "EVT_MIDTERM_EVAL" && GameManager.Instance != null)
        {
            GameManager.Instance.EksekusiEvaluasiTengahSemester();
            CompleteEvent(evt);
            return;
        }

        if (DialogueManager.Instance != null && evt.dialogueNodeId > 0)
        {
            DialogueManager.Instance.StartDialogue(evt.dialogueNodeId, () =>
            {
                CompleteEvent(evt);
            });
        }
        else
        {
            CompleteEvent(evt);
        }
    }

    /// <summary>
    /// Dipanggil otomatis setelah dialog event selesai.
    /// Menandai completion, menyetel flags baru, dan mengatur alur waktu game.
    /// </summary>
    public void CompleteEvent(GameEvent evt)
    {
        if (DatabaseManager.Instance == null) return;

        try
        {
            // 1. Tandai event selesai di SQLite
            string updateQ = $"UPDATE tbl_game_events SET is_completed = 1 WHERE event_id = {evt.eventId};";
            DatabaseManager.Instance.ExecuteNonQuery(updateQ);

            // 2. Penanganan kasus khusus krisis rumor (Node 3001)
            if (evt.eventCode == "EVT_RUMOR_CRISIS" || evt.dialogueNodeId == 3001)
            {
                if (SocialManager.Instance != null && SocialManager.Instance.globalRumorLevel >= 3)
                {
                    SocialManager.Instance.globalRumorLevel = 2;
                }
            }

            // 3. Set flags yang dihasilkan oleh event ini
            if (!string.IsNullOrEmpty(evt.setFlags))
            {
                string[] sFlags = evt.setFlags.Split(new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var sf in sFlags)
                {
                    SetFlag(sf.Trim(), 1, $"Dipasang oleh event {evt.eventCode}");
                }
            }

            Debug.Log($"<color=green>[EVENT COMPLETED]</color> Event '{evt.eventName}' telah diselesaikan.");

            if (TelemetryLogger.Instance != null)
            {
                TelemetryLogger.Instance.RecordCriticalEvent("EVENT_COMPLETED", $"Event '{evt.eventName}' ({evt.eventCode}) selesai");
            }

            OnEventCompleted?.Invoke(evt);

            // 4. Pengaturan Alur Waktu Game
            if (evt.costTimeBlock == 1)
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.GeserWaktu();
                }
            }
            else if (GameManager.Instance != null && GameManager.Instance.currentTimeBlock == TimeBlock.Pagi)
            {
                GameManager.Instance.LanjutRutinitasPagi();
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[EventManager] Gagal menyelesaikan event {evt.eventId}: {ex.Message}");
        }
    }

    // =========================================================================
    // 3. PENGELOLAAN STORY FLAGS (KEY-VALUE STATE STORE)
    // =========================================================================

    public void SetFlag(string flagName, int value = 1, string description = "")
    {
        if (string.IsNullOrEmpty(flagName)) return;

        if (FlagManager.Instance != null)
        {
            FlagManager.Instance.SetFlag(flagName, value, description);
        }
        OnFlagChanged?.Invoke(flagName, value);
    }

    public bool HasFlag(string flagName)
    {
        if (string.IsNullOrEmpty(flagName)) return false;

        if (FlagManager.Instance != null)
        {
            return FlagManager.Instance.HasFlag(flagName, 1);
        }
        return false;
    }

    public int GetFlag(string flagName, int defaultValue = 0)
    {
        if (string.IsNullOrEmpty(flagName)) return defaultValue;

        if (FlagManager.Instance != null)
        {
            return FlagManager.Instance.GetFlag(flagName, defaultValue);
        }
        return defaultValue;
    }

    public bool IsEventCompleted(int eventId)
    {
        if (DatabaseManager.Instance == null) return false;

        try
        {
            string query = $"SELECT is_completed FROM tbl_game_events WHERE event_id = {eventId};";
            DataTable dt = DatabaseManager.Instance.ExecuteQuery(query);
            if (dt != null && dt.Rows.Count > 0)
            {
                return Convert.ToInt32(dt.Rows[0]["is_completed"]) == 1;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[EventManager] Gagal cek is_completed event {eventId}: {ex.Message}");
        }

        return false;
    }

    public void ResetEvent(int eventId)
    {
        if (DatabaseManager.Instance == null) return;
        DatabaseManager.Instance.ExecuteNonQuery($"UPDATE tbl_game_events SET is_completed = 0 WHERE event_id = {eventId};");
    }
}
