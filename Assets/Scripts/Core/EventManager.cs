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

    void Update()
    {
        // Debug shortcut F8 untuk memicu evaluasi event secara instan saat testing
        if (InputGetKeyDown(KeyCode.F8))
        {
            Debug.Log("<color=cyan>[DEBUG EVENT ENGINE]</color> Shortcut F8 ditekan -> Mengevaluasi event...");
            bool triggered = TryTriggerEligibleEvent();
            if (!triggered)
            {
                Debug.Log("<color=yellow>[DEBUG EVENT ENGINE]</color> Tidak ada event yang memenuhi syarat kondisi saat ini.");
                if (HUDController.Instance != null)
                {
                    HUDController.Instance.ShowToastNotification("Tidak ada event aktif yang memenuhi syarat saat ini");
                }
            }
        }
    }

    private bool InputGetKeyDown(KeyCode key)
    {
#if ENABLE_INPUT_SYSTEM
        if (UnityEngine.InputSystem.Keyboard.current != null)
        {
            if (key == KeyCode.F8) return UnityEngine.InputSystem.Keyboard.current.f8Key.wasPressedThisFrame;
        }
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetKeyDown(key);
#else
        return false;
#endif
    }

    // =========================================================================
    // 1. DATA-DRIVEN EVENT CONDITION ENGINE (EVALUASI & PEMILIHAN)
    // =========================================================================

    /// <summary>
    /// Mengambil kandidat event dari SQLite (tbl_events), mengevaluasi 14 parameter kondisi,
    /// memilih event dengan prioritas tertinggi, dan mengeksekusinya via DialogueManager.
    /// </summary>
    public bool TryTriggerEligibleEvent()
    {
        if (DatabaseManager.Instance == null) return false;

        try
        {
            int currentDay = GameManager.Instance != null ? GameManager.Instance.currentDay : 1;

            // 1. Ambil seluruh kandidat event dari SQLite yang belum selesai (atau repeatable) dan berada di rentang hari ini
            string query = $"SELECT * FROM tbl_events " +
                           $"WHERE (is_completed = 0 OR is_repeatable = 1) " +
                           $"  AND min_day <= {currentDay} AND max_day >= {currentDay} " +
                           $"ORDER BY priority DESC, event_id ASC;";
            DataTable dt = DatabaseManager.Instance.ExecuteQuery(query);

            if (dt == null || dt.Rows.Count == 0) return false;

            List<GameEvent> eligibleEvents = new List<GameEvent>();

            // 2. Evaluasi 14 kondisi untuk setiap kandidat
            foreach (DataRow row in dt.Rows)
            {
                GameEvent candidate = GameEvent.FromDataRow(row);
                if (candidate == null) continue;

                if (EvaluateConditions(candidate))
                {
                    eligibleEvents.Add(candidate);
                }
            }

            if (eligibleEvents.Count == 0) return false;

            // 3. Urutkan berdasarkan priority DESC (tertinggi dieksekusi pertama)
            eligibleEvents.Sort((a, b) => b.priority.CompareTo(a.priority));

            // 4. Ambil dan eksekusi event prioritas teratas
            GameEvent topEvent = eligibleEvents[0];
            ExecuteEvent(topEvent);
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[EventManager] Terjadi kesalahan saat evaluasi event: {ex.Message}");
        }

        return false;
    }

    /// <summary>
    /// Mengevaluasi 14 parameter kondisi satu GameEvent secara deklaratif dan modular.
    /// </summary>
    public bool EvaluateConditions(GameEvent e)
    {
        if (e == null) return false;

        int currentDay = GameManager.Instance != null ? GameManager.Instance.currentDay : 1;
        TimeBlock currentTimeBlock = GameManager.Instance != null ? GameManager.Instance.currentTimeBlock : TimeBlock.Pagi;

        // 1. Kondisi Rentang Hari
        if (currentDay < e.minDay || currentDay > e.maxDay) return false;

        // 2. Kondisi Time Block ('Pagi', 'Siang', 'Malam', 'Any')
        if (!string.IsNullOrEmpty(e.timeBlock) && !e.timeBlock.Equals("Any", StringComparison.OrdinalIgnoreCase))
        {
            if (!e.timeBlock.Equals(currentTimeBlock.ToString(), StringComparison.OrdinalIgnoreCase))
                return false;
        }

        // 3. Kondisi Day Type ('Workday', 'Weekend', 'Any')
        if (!string.IsNullOrEmpty(e.dayType) && !e.dayType.Equals("Any", StringComparison.OrdinalIgnoreCase))
        {
            bool isWorkday = GameManager.Instance != null && GameManager.Instance.IsWorkday();
            if (e.dayType.Equals("Workday", StringComparison.OrdinalIgnoreCase) && !isWorkday) return false;
            if (e.dayType.Equals("Weekend", StringComparison.OrdinalIgnoreCase) && isWorkday) return false;
        }

        // 4. Kondisi Player Stats (Language, Etiquette, MH [min,max], PH, Theory, Practice)
        if (PlayerStats.Instance != null)
        {
            var ps = PlayerStats.Instance;
            if (ps.languageProficiency < e.minLang) return false;
            if (ps.culturalEtiquette < e.minEtiq) return false;
            if (ps.mentalHealth < e.minMh || ps.mentalHealth > e.maxMh) return false;
            if (ps.physicalHealth < e.minPh) return false;
            if (ps.academicTheoretical < e.minTheory) return false;
            if (ps.academicPractical < e.minPractice) return false;
        }

        // 5. Kondisi Relasi Sosial & Afeksi NPC (Guanxi & Affection State)
        if (e.npcId.HasValue && e.npcId.Value > 0)
        {
            if (SocialManager.Instance == null || SocialManager.Instance.relations == null) return false;
            var rel = SocialManager.Instance.relations.Find(x => x.npcId == e.npcId.Value);
            if (rel == null) return false;
            if (rel.guanxiScore < e.minGuanxi) return false;
            if (rel.affectionState < e.minAffectionState) return false;
        }

        // 6. Kondisi Level Rumor Kampus
        int currentRumor = GameManager.Instance != null ? GameManager.Instance.globalRumorLevel : 
                           (SocialManager.Instance != null ? SocialManager.Instance.globalRumorLevel : 0);
        if (currentRumor < e.minRumorLevel) return false;

        // 7. Kondisi Prerequisite Event (Event Chaining)
        if (e.prereqEventId.HasValue && e.prereqEventId.Value > 0)
        {
            if (!IsEventCompleted(e.prereqEventId.Value)) return false;
        }

        // 8. Kondisi Story Flag Naratif
        if (!string.IsNullOrEmpty(e.reqFlagName))
        {
            if (FlagManager.Instance == null || !FlagManager.Instance.HasFlag(e.reqFlagName, e.reqFlagVal))
                return false;
        }

        return true;
    }

    // =========================================================================
    // 2. EKSEKUSI DAN PENYELESAIAN EVENT
    // =========================================================================

    /// <summary>
    /// Mengeksekusi event terpilih, memicu dialog, dan menandai status di database.
    /// </summary>
    public void ExecuteEvent(GameEvent e)
    {
        Debug.Log($"<color=yellow>[EVENT ENGINE]</color> <b>Event Dipicu:</b> '{e.eventTitle}' " +
                  $"(ID: {e.eventId}, Priority: {e.priority}, Node: {e.startNodeId})");

        // 1. Tandai event selesai di SQLite jika !isRepeatable
        if (!e.isRepeatable)
        {
            e.isCompleted = true;
            DatabaseManager.Instance.ExecuteNonQuery($"UPDATE tbl_events SET is_completed = 1 WHERE event_id = {e.eventId};");
            DatabaseManager.Instance.ExecuteNonQuery($"UPDATE tbl_game_events SET is_completed = 1 WHERE event_id = {e.eventId};");
        }

        // 2. Tampilkan notifikasi HUD
        if (HUDController.Instance != null)
        {
            HUDController.Instance.ShowToastNotification($"[EVENT] {e.eventTitle}");
        }

        // 3. Catat log telemetri
        if (TelemetryLogger.Instance != null)
        {
            TelemetryLogger.Instance.RecordCriticalEvent("TRIGGER_EVENT", $"Event {e.eventTitle} (ID: {e.eventId}) triggered.");
        }

        OnEventTriggered?.Invoke(e);

        // Kasus khusus penanganan efek rumor krisis
        if (e.startNodeId == 3001 && SocialManager.Instance != null && SocialManager.Instance.globalRumorLevel >= 3)
        {
            SocialManager.Instance.globalRumorLevel = 2;
        }

        // 4. Buka dialog melalui DialogueManager
        if (DialogueManager.Instance != null && e.startNodeId > 0)
        {
            DialogueManager.Instance.StartDialogue(e.startNodeId, () =>
            {
                CompleteEvent(e);
            });
        }
        else
        {
            CompleteEvent(e);
        }
    }

    /// <summary>
    /// Dipanggil otomatis ketika percakapan dialog event selesai.
    /// </summary>
    public void CompleteEvent(GameEvent e)
    {
        Debug.Log($"<color=green>[EVENT COMPLETED]</color> Event '{e.eventTitle}' selesai dijalankan.");

        if (TelemetryLogger.Instance != null)
        {
            TelemetryLogger.Instance.RecordCriticalEvent("EVENT_COMPLETED", $"Event '{e.eventTitle}' (ID: {e.eventId}) selesai.");
        }

        OnEventCompleted?.Invoke(e);

        // Jika event terjadi di blok Pagi, lanjutkan alur rutin pagi (kelas wajib / otonomi)
        if (GameManager.Instance != null && GameManager.Instance.currentTimeBlock == TimeBlock.Pagi)
        {
            GameManager.Instance.LanjutRutinitasPagi();
        }
        else if (HUDController.Instance != null)
        {
            HUDController.Instance.UpdateHUD();
        }
    }

    // =========================================================================
    // 3. HELPER METHOD & DUKUNGAN KOMPATIBILITAS
    // =========================================================================

    public bool TryEvaluateAndTriggerEvent(TimeBlock currentTimeBlock, int currentDay)
    {
        return TryTriggerEligibleEvent();
    }

    public bool IsEventEligible(GameEvent evt, TimeBlock currentTimeBlock, int currentDay)
    {
        return EvaluateConditions(evt);
    }

    public void TriggerEvent(GameEvent evt)
    {
        ExecuteEvent(evt);
    }

    public bool IsEventCompleted(int eventId)
    {
        if (DatabaseManager.Instance == null) return false;

        try
        {
            string query = $"SELECT is_completed FROM tbl_events WHERE event_id = {eventId};";
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
        DatabaseManager.Instance.ExecuteNonQuery($"UPDATE tbl_events SET is_completed = 0 WHERE event_id = {eventId};");
        DatabaseManager.Instance.ExecuteNonQuery($"UPDATE tbl_game_events SET is_completed = 0 WHERE event_id = {eventId};");
    }

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

    [ContextMenu("Debug: Evaluasi & Picu Event Sekarang")]
    public void DebugTriggerEventNow()
    {
        TryTriggerEligibleEvent();
    }
}
