using System;
using System.Data;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class SaveManager : MonoBehaviour
{
    private static SaveManager _instance;
    public static SaveManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<SaveManager>();
                if (_instance == null)
                {
                    GameObject core = GameObject.Find("GAME_CORE") ?? GameObject.Find("[GAME_CORE]");
                    if (core != null)
                    {
                        _instance = core.AddComponent<SaveManager>();
                    }
                    else
                    {
                        GameObject go = new GameObject("[SaveManager]");
                        _instance = go.AddComponent<SaveManager>();
                        DontDestroyOnLoad(go);
                    }
                }
            }
            return _instance;
        }
        private set => _instance = value;
    }

    public const int QUICK_SAVE_SLOT = 0;
    private int _lastQuickSaveFrame = -1;
    private int _lastQuickLoadFrame = -1;

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
        // Shortcut Keyboard: F5 untuk Quick Save, F6 untuk Quick Load
        if (IsKeyPressed(KeyCode.F5))
        {
            QuickSave();
        }
        else if (IsKeyPressed(KeyCode.F6))
        {
            QuickLoad();
        }
    }

    private bool IsKeyPressed(KeyCode key)
    {
#if ENABLE_INPUT_SYSTEM
        var kb = Keyboard.current;
        if (kb != null)
        {
            switch (key)
            {
                case KeyCode.F5: return kb.f5Key.wasPressedThisFrame;
                case KeyCode.F6: return kb.f6Key.wasPressedThisFrame;
            }
        }
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetKeyDown(key);
#else
        return false;
#endif
    }

    // 1. QUICK SAVE (Slot 0)
    public void QuickSave()
    {
        if (Time.frameCount == _lastQuickSaveFrame) return;
        _lastQuickSaveFrame = Time.frameCount;

        SaveGame(QUICK_SAVE_SLOT, "Quick Save");
        Debug.Log("<color=yellow>[SAVE SYSTEM]</color> Quick Save berhasil disimpan ke Slot 0 (Shortcut F5).");
        if (HUDController.Instance != null)
        {
            HUDController.Instance.ShowToastNotification("[F5] Quick Save Berhasil Disimpan (Slot 0)");
        }
    }

    // 2. QUICK LOAD (Slot 0)
    public void QuickLoad()
    {
        if (Time.frameCount == _lastQuickLoadFrame) return;
        _lastQuickLoadFrame = Time.frameCount;

        if (HasSaveData(QUICK_SAVE_SLOT))
        {
            LoadGame(QUICK_SAVE_SLOT);
            Debug.Log("<color=green>[SAVE SYSTEM]</color> Quick Save (Slot 0) berhasil dimuat (Shortcut F6).");
            if (HUDController.Instance != null)
            {
                HUDController.Instance.ShowToastNotification("[F6] Quick Save Berhasil Dimuat (Slot 0)");
            }
        }
        else
        {
            Debug.LogWarning("[SAVE SYSTEM] Data Quick Save (Slot 0) tidak ditemukan!");
            if (HUDController.Instance != null)
            {
                HUDController.Instance.ShowToastNotification("[F6] Gagal: Belum Ada Data Quick Save");
            }
        }
    }

    // 3. Simpan Permainan ke Slot Tertentu (Slot 0: Quick Save, Slot 1-5: Manual Slot)
    public void SaveGame(int slotId, string slotTitle)
    {
        if (PlayerStats.Instance == null || GameManager.Instance == null || SocialManager.Instance == null)
        {
            Debug.LogError("[SAVE ERROR] Komponen Core (PlayerStats / GameManager / SocialManager) belum siap!");
            return;
        }

        int day = GameManager.Instance.currentDay;
        string block = GameManager.Instance.currentTimeBlock.ToString();
        int ph = PlayerStats.Instance.physicalHealth;
        int mh = PlayerStats.Instance.mentalHealth;
        int lang = PlayerStats.Instance.languageProficiency;
        int etiq = PlayerStats.Instance.culturalEtiquette;
        int theo = PlayerStats.Instance.academicTheoretical;
        int prac = PlayerStats.Instance.academicPractical;
        int burnedOut = PlayerStats.Instance.isBurnedOut ? 1 : 0;

        // Buka Transaksi SQL Atomik
        try
        {
            DatabaseManager.Instance.ExecuteTransaction(cmd =>
            {
                // A. Simpan Metadata Slot
                cmd.CommandText = $"INSERT OR REPLACE INTO tbl_save_metadata " +
                                  $"(slot_id, slot_title, saved_at, current_day, time_block, ph_snapshot, mh_snapshot) " +
                                  $"VALUES ({slotId}, '{slotTitle}', datetime('now', 'localtime'), {day}, '{block}', {ph}, {mh});";
                cmd.ExecuteNonQuery();

                // B. Simpan Stats Devano
                cmd.CommandText = $"INSERT OR REPLACE INTO tbl_save_player_stats " +
                                  $"(slot_id, physical_health, mental_health, language_proficiency, cultural_etiquette, academic_theoretical, academic_practical, is_burned_out) " +
                                  $"VALUES ({slotId}, {ph}, {mh}, {lang}, {etiq}, {theo}, {prac}, {burnedOut});";
                cmd.ExecuteNonQuery();

                // C. Simpan Relasi Seluruh NPC
                cmd.CommandText = $"DELETE FROM tbl_save_npc_relations WHERE slot_id = {slotId};";
                cmd.ExecuteNonQuery();

                foreach (var rel in SocialManager.Instance.relations)
                {
                    cmd.CommandText = $"INSERT INTO tbl_save_npc_relations " +
                                      $"(slot_id, npc_id, guanxi_score, loneliness_meter, rumor_contribution, affection_state) " +
                                      $"VALUES ({slotId}, {rel.npcId}, {rel.guanxiScore}, {rel.lonelinessMeter}, {rel.rumorContribution}, {rel.affectionState});";
                    cmd.ExecuteNonQuery();
                }

                // D. Simpan Story Flags
                cmd.CommandText = $"DELETE FROM tbl_save_story_flags WHERE slot_id = {slotId};";
                cmd.ExecuteNonQuery();
                cmd.CommandText = $"INSERT INTO tbl_save_story_flags (slot_id, flag_name, flag_value, unlocked_day, description) " +
                                  $"SELECT {slotId}, flag_name, flag_value, unlocked_day, description FROM tbl_story_flags;";
                cmd.ExecuteNonQuery();

                // D2. Kompatibilitas tabel lama tbl_save_game_flags
                cmd.CommandText = $"DELETE FROM tbl_save_game_flags WHERE slot_id = {slotId};";
                cmd.ExecuteNonQuery();
                cmd.CommandText = $"INSERT INTO tbl_save_game_flags (slot_id, flag_name, flag_value) " +
                                  $"SELECT {slotId}, flag_name, flag_value FROM tbl_story_flags;";
                cmd.ExecuteNonQuery();

                // E. Simpan Status Completion Events
                cmd.CommandText = $"DELETE FROM tbl_save_game_events WHERE slot_id = {slotId};";
                cmd.ExecuteNonQuery();
                cmd.CommandText = $"INSERT INTO tbl_save_game_events (slot_id, event_id, is_completed) " +
                                  $"SELECT {slotId}, event_id, is_completed FROM tbl_events;";
                cmd.ExecuteNonQuery();
            });

            if (TelemetryLogger.Instance != null)
            {
                TelemetryLogger.Instance.RecordCriticalEvent("GAME_SAVED", $"Pemain menyimpan game ke Slot {slotId} ({slotTitle}) | Hari {day} ({block})");
            }

            Debug.Log($"<color=green>[SAVE SUCCESS]</color> Permainan berhasil disimpan ke Slot {slotId} ({slotTitle}). Hari {day} [{block}].");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[SAVE ERROR] Gagal menyimpan ke slot {slotId}: {ex.Message}");
        }
    }

    // 4. Muat Permainan dari Slot Tertentu
    public void LoadGame(int slotId)
    {
        if (!HasSaveData(slotId))
        {
            Debug.LogWarning($"[LOAD WARNING] Slot {slotId} kosong atau tidak ditemukan.");
            return;
        }

        try
        {
            // Reset riwayat backlog saat memuat save game
            if (DialogueBacklogManager.Instance != null)
            {
                DialogueBacklogManager.Instance.ClearHistory();
            }

            int day = 1;
            string block = "Pagi";

            // A. Muat Metadata & Waktu Kalender
            string qMeta = $"SELECT current_day, time_block FROM tbl_save_metadata WHERE slot_id = {slotId};";
            DataTable dtMeta = DatabaseManager.Instance.ExecuteQuery(qMeta);
            if (dtMeta != null && dtMeta.Rows.Count > 0)
            {
                day = Convert.ToInt32(dtMeta.Rows[0]["current_day"]);
                block = dtMeta.Rows[0]["time_block"].ToString();

                // Sinkronkan ke SQLite tbl_player_profile
                DatabaseManager.Instance.ExecuteNonQuery(
                    $"UPDATE tbl_player_profile SET current_day = {day}, current_time_block = '{block}' WHERE player_id = 1;");

                if (GameManager.Instance != null)
                {
                    GameManager.Instance.currentDay = day;
                    GameManager.Instance.currentTimeBlock = (TimeBlock)Enum.Parse(typeof(TimeBlock), block);
                    GameManager.Instance.SaveGameState();
                }
            }

            // B. Muat Parameter Devano
            string qStats = $"SELECT * FROM tbl_save_player_stats WHERE slot_id = {slotId};";
            DataTable dtStats = DatabaseManager.Instance.ExecuteQuery(qStats);
            if (dtStats != null && dtStats.Rows.Count > 0)
            {
                DataRow row = dtStats.Rows[0];
                int ph = Convert.ToInt32(row["physical_health"]);
                int mh = Convert.ToInt32(row["mental_health"]);
                int lang = Convert.ToInt32(row["language_proficiency"]);
                int etiq = Convert.ToInt32(row["cultural_etiquette"]);
                int theo = Convert.ToInt32(row["academic_theoretical"]);
                int prac = Convert.ToInt32(row["academic_practical"]);

                // Sinkronkan ke SQLite tbl_player_stats
                DatabaseManager.Instance.ExecuteNonQuery(
                    $"UPDATE tbl_player_stats SET physical_health = {ph}, mental_health = {mh}, " +
                    $"language_proficiency = {lang}, cultural_etiquette = {etiq}, " +
                    $"academic_theoretical = {theo}, academic_practical = {prac} WHERE player_id = 1;");

                if (PlayerStats.Instance != null)
                {
                    PlayerStats.Instance.physicalHealth = ph;
                    PlayerStats.Instance.mentalHealth = mh;
                    PlayerStats.Instance.languageProficiency = lang;
                    PlayerStats.Instance.culturalEtiquette = etiq;
                    PlayerStats.Instance.academicTheoretical = theo;
                    PlayerStats.Instance.academicPractical = prac;
                    PlayerStats.Instance.isBurnedOut = Convert.ToInt32(row["is_burned_out"]) == 1;
                    PlayerStats.Instance.SaveStatsToDatabase();
                }
            }

            // C. Muat Relasi NPC
            string qRel = $"SELECT * FROM tbl_save_npc_relations WHERE slot_id = {slotId};";
            DataTable dtRel = DatabaseManager.Instance.ExecuteQuery(qRel);
            if (dtRel != null && dtRel.Rows.Count > 0)
            {
                foreach (DataRow row in dtRel.Rows)
                {
                    int npcId = Convert.ToInt32(row["npc_id"]);
                    int gScore = Convert.ToInt32(row["guanxi_score"]);
                    int lMeter = Convert.ToInt32(row["loneliness_meter"]);
                    int rContr = Convert.ToInt32(row["rumor_contribution"]);
                    int aState = Convert.ToInt32(row["affection_state"]);

                    // Sinkronkan ke SQLite tbl_npc_relations
                    DatabaseManager.Instance.ExecuteNonQuery(
                        $"UPDATE tbl_npc_relations SET guanxi_score = {gScore}, loneliness_meter = {lMeter}, " +
                        $"rumor_contribution = {rContr}, affection_state = {aState} WHERE player_id = 1 AND npc_id = {npcId};");

                    if (SocialManager.Instance != null && SocialManager.Instance.relations != null)
                    {
                        var target = SocialManager.Instance.relations.Find(x => x.npcId == npcId);
                        if (target != null)
                        {
                            target.guanxiScore = gScore;
                            target.lonelinessMeter = lMeter;
                            target.rumorContribution = rContr;
                            target.affectionState = aState;
                            target.interactedToday = false;

                            SocialManager.Instance.EvaluasiAffectionState(target);
                        }
                    }
                }
            }

            // D. Muat Story Flags & Event Status
            DatabaseManager.Instance.ExecuteNonQuery("DELETE FROM tbl_story_flags;");
            DatabaseManager.Instance.ExecuteNonQuery(
                $"INSERT INTO tbl_story_flags (flag_name, flag_value, unlocked_day, description) " +
                $"SELECT flag_name, flag_value, unlocked_day, description FROM tbl_save_story_flags WHERE slot_id = {slotId};");

            // Kompatibilitas tabel lama
            DatabaseManager.Instance.ExecuteNonQuery("DELETE FROM tbl_game_flags;");
            DatabaseManager.Instance.ExecuteNonQuery(
                $"INSERT INTO tbl_game_flags (flag_name, flag_value) " +
                $"SELECT flag_name, flag_value FROM tbl_story_flags;");

            // Refresh cache memori FlagManager
            if (FlagManager.Instance != null)
            {
                FlagManager.Instance.LoadAllFlagsFromDatabase();
            }

            DatabaseManager.Instance.ExecuteNonQuery(
                $"UPDATE tbl_events SET is_completed = (" +
                $"SELECT IFNULL((SELECT is_completed FROM tbl_save_game_events WHERE tbl_save_game_events.event_id = tbl_events.event_id AND slot_id = {slotId}), 0));");
            DatabaseManager.Instance.ExecuteNonQuery(
                $"UPDATE tbl_game_events SET is_completed = (" +
                $"SELECT IFNULL((SELECT is_completed FROM tbl_save_game_events WHERE tbl_save_game_events.event_id = tbl_game_events.event_id AND slot_id = {slotId}), 0));");

            // E. Sinkronkan Tampilan UI jika aktif
            if (HUDController.Instance != null)
            {
                HUDController.Instance.UpdateHUD();
            }

            if (TelemetryLogger.Instance != null)
            {
                TelemetryLogger.Instance.RecordCriticalEvent("GAME_LOADED", $"Pemain memuat permainan dari Slot {slotId}");
            }

            Debug.Log($"<color=green>[LOAD SUCCESS]</color> Sesi permainan dari Slot {slotId} siap dimainkan. Hari {day} [{block}].");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[LOAD ERROR] Gagal memuat data dari slot {slotId}: {ex.Message}");
        }
    }

    public bool HasSaveData(int slotId)
    {
        string q = $"SELECT COUNT(*) as count FROM tbl_save_metadata WHERE slot_id = {slotId};";
        DataTable dt = DatabaseManager.Instance.ExecuteQuery(q);
        return dt != null && dt.Rows.Count > 0 && Convert.ToInt32(dt.Rows[0]["count"]) > 0;
    }

    public void DeleteSave(int slotId)
    {
        DatabaseManager.Instance.ExecuteNonQuery($"DELETE FROM tbl_save_metadata WHERE slot_id = {slotId};");
        DatabaseManager.Instance.ExecuteNonQuery($"DELETE FROM tbl_save_player_stats WHERE slot_id = {slotId};");
        DatabaseManager.Instance.ExecuteNonQuery($"DELETE FROM tbl_save_npc_relations WHERE slot_id = {slotId};");
        DatabaseManager.Instance.ExecuteNonQuery($"DELETE FROM tbl_save_story_flags WHERE slot_id = {slotId};");
        DatabaseManager.Instance.ExecuteNonQuery($"DELETE FROM tbl_save_game_flags WHERE slot_id = {slotId};");
        DatabaseManager.Instance.ExecuteNonQuery($"DELETE FROM tbl_save_game_events WHERE slot_id = {slotId};");
        Debug.Log($"[SAVE SYSTEM] Slot {slotId} berhasil dihapus.");
    }
}
