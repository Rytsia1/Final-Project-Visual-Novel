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

    // Meng-escape tanda kutip tunggal agar string yang disisipkan ke SQL literal
    // (mis. judul slot) tidak memutus statement dan membatalkan transaksi.
    private static string Esc(string s) => s?.Replace("'", "''") ?? "";

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
        string weather = GameManager.Instance.currentWeather.ToString();
        int greetingFlag = GameManager.Instance.greetingTriggeredToday ? 1 : 0;
        int ph = PlayerStats.Instance.physicalHealth;
        int mh = PlayerStats.Instance.mentalHealth;
        int lang = PlayerStats.Instance.languageProficiency;
        int etiq = PlayerStats.Instance.culturalEtiquette;
        int theo = PlayerStats.Instance.academicTheoretical;
        int prac = PlayerStats.Instance.academicPractical;
        int burnedOut = PlayerStats.Instance.isBurnedOut ? 1 : 0;
        int rumorLevel = SocialManager.Instance.globalRumorLevel;
        string safeTitle = Esc(slotTitle);

        // Buka Transaksi SQL Atomik
        try
        {
            DatabaseManager.Instance.ExecuteTransaction(cmd =>
            {
                // A. Simpan Metadata Slot (termasuk rumor global, cuaca & status sapaan pagi)
                cmd.CommandText = $"INSERT OR REPLACE INTO tbl_save_metadata " +
                                  $"(slot_id, slot_title, saved_at, current_day, time_block, ph_snapshot, mh_snapshot, " +
                                  $"global_rumor_level, current_weather, greeting_triggered_today) " +
                                  $"VALUES ({slotId}, '{safeTitle}', datetime('now', 'localtime'), {day}, '{block}', {ph}, {mh}, " +
                                  $"{rumorLevel}, '{weather}', {greetingFlag});";
                cmd.ExecuteNonQuery();

                // B. Simpan Stats Devano
                cmd.CommandText = $"INSERT OR REPLACE INTO tbl_save_player_stats " +
                                  $"(slot_id, physical_health, mental_health, language_proficiency, cultural_etiquette, academic_theoretical, academic_practical, is_burned_out) " +
                                  $"VALUES ({slotId}, {ph}, {mh}, {lang}, {etiq}, {theo}, {prac}, {burnedOut});";
                cmd.ExecuteNonQuery();

                // C. Simpan Relasi Seluruh NPC (termasuk daysSinceLastInteract & interactedToday)
                cmd.CommandText = $"DELETE FROM tbl_save_npc_relations WHERE slot_id = {slotId};";
                cmd.ExecuteNonQuery();

                foreach (var rel in SocialManager.Instance.relations)
                {
                    int interactedFlag = rel.interactedToday ? 1 : 0;
                    cmd.CommandText = $"INSERT INTO tbl_save_npc_relations " +
                                      $"(slot_id, npc_id, guanxi_score, loneliness_meter, rumor_contribution, affection_state, " +
                                      $"days_since_last_interaction, interacted_today) " +
                                      $"VALUES ({slotId}, {rel.npcId}, {rel.guanxiScore}, {rel.lonelinessMeter}, {rel.rumorContribution}, {rel.affectionState}, " +
                                      $"{rel.daysSinceLastInteract}, {interactedFlag});";
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

                // E. Simpan Status Completion Events dari KEDUA tabel event, dibedakan via source_table
                //    (tbl_events dan tbl_game_events berbagi rentang event_id yang sama tapi berbeda makna)
                cmd.CommandText = $"DELETE FROM tbl_save_game_events WHERE slot_id = {slotId};";
                cmd.ExecuteNonQuery();
                cmd.CommandText = $"INSERT INTO tbl_save_game_events (slot_id, source_table, event_id, is_completed) " +
                                  $"SELECT {slotId}, 'tbl_events', event_id, is_completed FROM tbl_events;";
                cmd.ExecuteNonQuery();
                cmd.CommandText = $"INSERT INTO tbl_save_game_events (slot_id, source_table, event_id, is_completed) " +
                                  $"SELECT {slotId}, 'tbl_game_events', event_id, is_completed FROM tbl_game_events;";
                cmd.ExecuteNonQuery();

                // F. Simpan Status Milestone Karakter (unlock & completion)
                cmd.CommandText = $"DELETE FROM tbl_save_character_events WHERE slot_id = {slotId};";
                cmd.ExecuteNonQuery();
                cmd.CommandText = $"INSERT INTO tbl_save_character_events (slot_id, event_id, is_unlocked, is_completed) " +
                                  $"SELECT {slotId}, event_id, is_unlocked, is_completed FROM tbl_character_events;";
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

    // Struct sementara untuk membawa satu baris relasi NPC dari fase baca ke fase tulis/sinkron.
    private struct SavedRelation
    {
        public int npcId, guanxiScore, lonelinessMeter, rumorContribution, affectionState, daysSinceLastInteract;
        public bool interactedToday;
    }

    // 4. Muat Permainan dari Slot Tertentu
    // Direstrukturisasi menjadi 3 fase: (1) BACA seluruh data slot ke memori lokal,
    // (2) TULIS seluruh state SQLite live dalam SATU transaksi atomik,
    // (3) SINKRON runtime singleton HANYA setelah transaksi commit sukses.
    // Ini menutup celah lama: kegagalan di tengah proses load (mis. saat DELETE tbl_story_flags)
    // tidak lagi meninggalkan database dalam keadaan setengah-pulih tanpa rollback.
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

            // =====================================================================
            // FASE 1: BACA — seluruh query, tidak ada tulis di fase ini.
            // =====================================================================
            int day = 1;
            string block = "Pagi";
            string weather = "Cerah";
            int greetingFlag = 0;
            int rumorLevel = 0;

            string qMeta = $"SELECT current_day, time_block, global_rumor_level, current_weather, greeting_triggered_today " +
                           $"FROM tbl_save_metadata WHERE slot_id = {slotId};";
            DataTable dtMeta = DatabaseManager.Instance.ExecuteQuery(qMeta);
            if (dtMeta == null || dtMeta.Rows.Count == 0)
            {
                Debug.LogError($"[LOAD ERROR] Metadata slot {slotId} tidak ditemukan.");
                return;
            }

            DataRow metaRow = dtMeta.Rows[0];
            day = Convert.ToInt32(metaRow["current_day"]);
            block = metaRow["time_block"].ToString();
            if (dtMeta.Columns.Contains("global_rumor_level") && metaRow["global_rumor_level"] != DBNull.Value)
                rumorLevel = Convert.ToInt32(metaRow["global_rumor_level"]);
            if (dtMeta.Columns.Contains("current_weather") && metaRow["current_weather"] != DBNull.Value)
                weather = metaRow["current_weather"].ToString();
            if (dtMeta.Columns.Contains("greeting_triggered_today") && metaRow["greeting_triggered_today"] != DBNull.Value)
                greetingFlag = Convert.ToInt32(metaRow["greeting_triggered_today"]);

            string qStats = $"SELECT * FROM tbl_save_player_stats WHERE slot_id = {slotId};";
            DataTable dtStats = DatabaseManager.Instance.ExecuteQuery(qStats);
            bool hasStats = dtStats != null && dtStats.Rows.Count > 0;
            int ph = 0, mh = 0, lang = 0, etiq = 0, theo = 0, prac = 0, burnedOut = 0;
            if (hasStats)
            {
                DataRow row = dtStats.Rows[0];
                ph = Convert.ToInt32(row["physical_health"]);
                mh = Convert.ToInt32(row["mental_health"]);
                lang = Convert.ToInt32(row["language_proficiency"]);
                etiq = Convert.ToInt32(row["cultural_etiquette"]);
                theo = Convert.ToInt32(row["academic_theoretical"]);
                prac = Convert.ToInt32(row["academic_practical"]);
                burnedOut = Convert.ToInt32(row["is_burned_out"]);
            }

            string qRel = $"SELECT * FROM tbl_save_npc_relations WHERE slot_id = {slotId};";
            DataTable dtRel = DatabaseManager.Instance.ExecuteQuery(qRel);
            var relations = new System.Collections.Generic.List<SavedRelation>();
            if (dtRel != null)
            {
                foreach (DataRow row in dtRel.Rows)
                {
                    var sr = new SavedRelation
                    {
                        npcId = Convert.ToInt32(row["npc_id"]),
                        guanxiScore = Convert.ToInt32(row["guanxi_score"]),
                        lonelinessMeter = Convert.ToInt32(row["loneliness_meter"]),
                        rumorContribution = Convert.ToInt32(row["rumor_contribution"]),
                        affectionState = Convert.ToInt32(row["affection_state"]),
                        daysSinceLastInteract = 0,
                        interactedToday = false
                    };
                    if (dtRel.Columns.Contains("days_since_last_interaction") && row["days_since_last_interaction"] != DBNull.Value)
                        sr.daysSinceLastInteract = Convert.ToInt32(row["days_since_last_interaction"]);
                    if (dtRel.Columns.Contains("interacted_today") && row["interacted_today"] != DBNull.Value)
                        sr.interactedToday = Convert.ToInt32(row["interacted_today"]) == 1;
                    relations.Add(sr);
                }
            }

            // =====================================================================
            // FASE 2: TULIS — satu transaksi atomik mencakup SELURUH tabel live.
            // =====================================================================
            DatabaseManager.Instance.ExecuteTransaction(cmd =>
            {
                // A. Kalender, cuaca & status sapaan pagi
                cmd.CommandText = $"UPDATE tbl_player_profile SET current_day = {day}, current_time_block = '{block}', " +
                                  $"current_weather = '{weather}', greeting_triggered_today = {greetingFlag}, " +
                                  $"global_rumor_level = {rumorLevel} WHERE player_id = 1;";
                cmd.ExecuteNonQuery();

                // B. Parameter Devano
                if (hasStats)
                {
                    cmd.CommandText = $"UPDATE tbl_player_stats SET physical_health = {ph}, mental_health = {mh}, " +
                        $"language_proficiency = {lang}, cultural_etiquette = {etiq}, " +
                        $"academic_theoretical = {theo}, academic_practical = {prac}, is_burned_out = {burnedOut} WHERE player_id = 1;";
                    cmd.ExecuteNonQuery();
                }

                // C. Relasi NPC
                foreach (var rel in relations)
                {
                    int interactedFlag = rel.interactedToday ? 1 : 0;
                    cmd.CommandText = $"UPDATE tbl_npc_relations SET guanxi_score = {rel.guanxiScore}, loneliness_meter = {rel.lonelinessMeter}, " +
                        $"rumor_contribution = {rel.rumorContribution}, affection_state = {rel.affectionState}, " +
                        $"days_since_last_interaction = {rel.daysSinceLastInteract}, interacted_today = {interactedFlag} " +
                        $"WHERE player_id = 1 AND npc_id = {rel.npcId};";
                    cmd.ExecuteNonQuery();
                }

                // D. Story Flags & kompatibilitas tabel lama
                cmd.CommandText = "DELETE FROM tbl_story_flags;";
                cmd.ExecuteNonQuery();
                cmd.CommandText = $"INSERT INTO tbl_story_flags (flag_name, flag_value, unlocked_day, description) " +
                                  $"SELECT flag_name, flag_value, unlocked_day, description FROM tbl_save_story_flags WHERE slot_id = {slotId};";
                cmd.ExecuteNonQuery();

                cmd.CommandText = "DELETE FROM tbl_game_flags;";
                cmd.ExecuteNonQuery();
                cmd.CommandText = "INSERT INTO tbl_game_flags (flag_name, flag_value) SELECT flag_name, flag_value FROM tbl_story_flags;";
                cmd.ExecuteNonQuery();

                // E. Status Completion Events, dipulihkan terpisah per tabel sumber
                cmd.CommandText = $"UPDATE tbl_events SET is_completed = (" +
                    $"SELECT IFNULL((SELECT is_completed FROM tbl_save_game_events " +
                    $"WHERE tbl_save_game_events.event_id = tbl_events.event_id AND tbl_save_game_events.source_table = 'tbl_events' AND slot_id = {slotId}), 0));";
                cmd.ExecuteNonQuery();
                cmd.CommandText = $"UPDATE tbl_game_events SET is_completed = (" +
                    $"SELECT IFNULL((SELECT is_completed FROM tbl_save_game_events " +
                    $"WHERE tbl_save_game_events.event_id = tbl_game_events.event_id AND tbl_save_game_events.source_table = 'tbl_game_events' AND slot_id = {slotId}), 0));";
                cmd.ExecuteNonQuery();

                // F. Status Milestone Karakter (is_unlocked & is_completed)
                cmd.CommandText = $"UPDATE tbl_character_events SET " +
                    $"is_unlocked = (SELECT IFNULL((SELECT is_unlocked FROM tbl_save_character_events " +
                    $"WHERE tbl_save_character_events.event_id = tbl_character_events.event_id AND slot_id = {slotId}), 0)), " +
                    $"is_completed = (SELECT IFNULL((SELECT is_completed FROM tbl_save_character_events " +
                    $"WHERE tbl_save_character_events.event_id = tbl_character_events.event_id AND slot_id = {slotId}), 0));";
                cmd.ExecuteNonQuery();
            });

            // =====================================================================
            // FASE 3: SINKRON RUNTIME — hanya dijalankan setelah transaksi commit sukses.
            // =====================================================================
            if (GameManager.Instance != null)
            {
                GameManager.Instance.currentDay = day;
                GameManager.Instance.currentTimeBlock = (TimeBlock)Enum.Parse(typeof(TimeBlock), block);
                if (Enum.TryParse(weather, out WeatherState parsedWeather))
                {
                    GameManager.Instance.currentWeather = parsedWeather;
                }
                GameManager.Instance.greetingTriggeredToday = greetingFlag == 1;
            }

            if (hasStats && PlayerStats.Instance != null)
            {
                PlayerStats.Instance.physicalHealth = ph;
                PlayerStats.Instance.mentalHealth = mh;
                PlayerStats.Instance.languageProficiency = lang;
                PlayerStats.Instance.culturalEtiquette = etiq;
                PlayerStats.Instance.academicTheoretical = theo;
                PlayerStats.Instance.academicPractical = prac;
                PlayerStats.Instance.isBurnedOut = burnedOut == 1;
            }

            if (SocialManager.Instance != null)
            {
                SocialManager.Instance.globalRumorLevel = rumorLevel;

                if (SocialManager.Instance.relations != null)
                {
                    foreach (var rel in relations)
                    {
                        var target = SocialManager.Instance.relations.Find(x => x.npcId == rel.npcId);
                        if (target != null)
                        {
                            // Ditugaskan langsung dari data slot (BUKAN via EvaluasiAffectionState),
                            // karena metode itu memicu CheckMilestoneUnlocks yang akan menimpa ulang
                            // is_unlocked milestone yang baru saja dipulihkan pada Fase 2.
                            target.guanxiScore = rel.guanxiScore;
                            target.lonelinessMeter = rel.lonelinessMeter;
                            target.rumorContribution = rel.rumorContribution;
                            target.affectionState = rel.affectionState;
                            target.daysSinceLastInteract = rel.daysSinceLastInteract;
                            target.interactedToday = rel.interactedToday;
                        }
                    }
                }
            }

            // Refresh cache memori FlagManager
            if (FlagManager.Instance != null)
            {
                FlagManager.Instance.LoadAllFlagsFromDatabase();
            }

            // Sinkronkan Tampilan UI jika aktif
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
        try
        {
            DatabaseManager.Instance.ExecuteTransaction(cmd =>
            {
                cmd.CommandText = $"DELETE FROM tbl_save_metadata WHERE slot_id = {slotId};";
                cmd.ExecuteNonQuery();
                cmd.CommandText = $"DELETE FROM tbl_save_player_stats WHERE slot_id = {slotId};";
                cmd.ExecuteNonQuery();
                cmd.CommandText = $"DELETE FROM tbl_save_npc_relations WHERE slot_id = {slotId};";
                cmd.ExecuteNonQuery();
                cmd.CommandText = $"DELETE FROM tbl_save_story_flags WHERE slot_id = {slotId};";
                cmd.ExecuteNonQuery();
                cmd.CommandText = $"DELETE FROM tbl_save_game_flags WHERE slot_id = {slotId};";
                cmd.ExecuteNonQuery();
                cmd.CommandText = $"DELETE FROM tbl_save_game_events WHERE slot_id = {slotId};";
                cmd.ExecuteNonQuery();
                cmd.CommandText = $"DELETE FROM tbl_save_character_events WHERE slot_id = {slotId};";
                cmd.ExecuteNonQuery();
            });
            Debug.Log($"[SAVE SYSTEM] Slot {slotId} berhasil dihapus.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[DELETE SAVE ERROR] Gagal menghapus slot {slotId}: {ex.Message}");
        }
    }
}
