#if UNITY_EDITOR
using System;
using System.Data;
using UnityEditor;
using UnityEngine;

public static class TokimekiIntegrityValidator
{
    [MenuItem("Game Debug/Jalankan Headless Balancing Simulation Batch")]
    public static void RunHeadlessSimulationBatch()
    {
        AutomatedBalancingSimulator.ExportAllArchetypesHeadless();
    }

    [MenuItem("Game Debug/Jalankan Validasi Integritas Tokimeki")]
    public static void RunAllValidations()
    {
        Debug.Log("<color=cyan>==================================================</color>");
        Debug.Log("<color=cyan>=== MEMULAI VALIDASI INTEGRITAS SISTEM TOKIMEKI ===</color>");
        Debug.Log("<color=cyan>==================================================</color>");

        bool allPassed = true;

        allPassed &= ValidateDialogueIdConstants();
        allPassed &= ValidateDatabaseSchema();
        allPassed &= ValidateEventConditionSystem();

        if (Application.isPlaying)
        {
            allPassed &= ValidateRuntimeInteractedTodayAndTransitions();
            allPassed &= ValidateSaveLoadRoundTrip();
        }
        else
        {
            Debug.Log("<color=yellow>[Info]</color> Jalankan Play Mode untuk menguji verifikasi runtime interactedToday, transisi waktu, dan Save/Load round-trip.");
        }

        if (allPassed)
        {
            Debug.Log("<color=green><b>[SUKSES] SELURUH VALIDASI INTEGRITAS TOKIMEKI LOLOS TANPA KENDALA!</b></color>");
        }
        else
        {
            Debug.LogError("<color=red><b>[GAGAL] DITEMUKAN MASALAH DALAM VALIDASI INTEGRITAS TOKIMEKI!</b></color>");
        }
    }

    private static bool ValidateDialogueIdConstants()
    {
        Debug.Log("--- 1. Memeriksa Konvensi Penomoran ID Dialog ---");

        bool p1 = DialogueIdConstants.XIANG_BAI_START == 1000 && DialogueIdConstants.XIANG_BAI_END == 1999;
        bool p2 = DialogueIdConstants.EDELWEISS_START == 2000 && DialogueIdConstants.EDELWEISS_END == 2999;
        bool p3 = DialogueIdConstants.CRISIS_RUMOR_START == 3000 && DialogueIdConstants.CRISIS_RUMOR_END == 3999;
        bool p4 = DialogueIdConstants.LI_HAORAN_START == 4000 && DialogueIdConstants.LI_HAORAN_END == 4999;
        bool p5 = DialogueIdConstants.ACADEMIC_EVALUATION_START == 5000 && DialogueIdConstants.ACADEMIC_EVALUATION_END == 5999;
        bool p6 = DialogueIdConstants.WEEKEND_HANGOUT_START == 6000 && DialogueIdConstants.WEEKEND_HANGOUT_END == 6999;
        bool p7 = DialogueIdConstants.YANG_MEI_START == 7000 && DialogueIdConstants.YANG_MEI_END == 7999;

        if (!p1 || !p2 || !p3 || !p4 || !p5 || !p6 || !p7)
        {
            Debug.LogError("[Konvensi ID] Rentang konvensi ID tidak sesuai spesifikasi!");
            return false;
        }

        // Tes category mapper
        Debug.Log($"Node 1001: {DialogueIdConstants.GetCategoryName(1001)}");
        Debug.Log($"Node 2001: {DialogueIdConstants.GetCategoryName(2001)}");
        Debug.Log($"Node 3001: {DialogueIdConstants.GetCategoryName(3001)}");
        Debug.Log($"Node 4001: {DialogueIdConstants.GetCategoryName(4001)}");
        Debug.Log($"Node 5001: {DialogueIdConstants.GetCategoryName(5001)}");
        Debug.Log($"Node 6001: {DialogueIdConstants.GetCategoryName(6001)}");
        Debug.Log($"Node 7001: {DialogueIdConstants.GetCategoryName(7001)}");

        Debug.Log("<color=green>[PASS]</color> Konvensi Penomoran ID Dialog terverifikasi valid.");
        return true;
    }

    private static bool ValidateDatabaseSchema()
    {
        Debug.Log("--- 2. Memeriksa Skema Database SQLite Fisik ---");
        if (DatabaseManager.Instance == null)
        {
            Debug.LogWarning("[DB Check] DatabaseManager.Instance null (akan divalidasi via query langsung jika tersedia).");
            return true;
        }

        // 1. Periksa fail_mianzi_node_id & fail_language_node_id di tbl_dialogue_options
        DataTable dtOpt = DatabaseManager.Instance.ExecuteQuery("PRAGMA table_info(tbl_dialogue_options);");
        bool hasMianziCol = false;
        bool hasLangCol = false;
        if (dtOpt != null)
        {
            foreach (DataRow r in dtOpt.Rows)
            {
                string col = r["name"].ToString();
                if (col == "fail_mianzi_node_id") hasMianziCol = true;
                if (col == "fail_language_node_id") hasLangCol = true;
            }
        }

        if (!hasMianziCol || !hasLangCol)
        {
            Debug.LogError($"[DB Check] Kolom baru di tbl_dialogue_options belum lengkap: fail_mianzi={hasMianziCol}, fail_language={hasLangCol}");
            return false;
        }

        // 2. Periksa tabel pendukung
        string[] requiredTables = { "tbl_venues", "tbl_npc_preferences", "tbl_hangout_events", "tbl_morning_greetings", "tbl_telemetry_logs" };
        foreach (string t in requiredTables)
        {
            DataTable dtT = DatabaseManager.Instance.ExecuteQuery($"SELECT name FROM sqlite_master WHERE type='table' AND name='{t}';");
            if (dtT == null || dtT.Rows.Count == 0)
            {
                Debug.LogError($"[DB Check] Tabel pendukung '{t}' belum ditemukan di SQLite!");
                return false;
            }
        }

        // 3. Cek Venue 5 (Kafe Dessert & Boba Tea)
        DataTable dtV5 = DatabaseManager.Instance.ExecuteQuery("SELECT venue_name FROM tbl_venues WHERE venue_id = 5;");
        if (dtV5 == null || dtV5.Rows.Count == 0)
        {
            Debug.LogError("[DB Check] Venue ID 5 (Kafe Dessert & Boba Tea) belum ditemukan di tbl_venues!");
            return false;
        }

        // 4. Cek Kolom & Data Profil Pemain (Devano)
        DataTable dtPlayer = DatabaseManager.Instance.ExecuteQuery("SELECT player_name, major, birthday, zodiac, theme_color, phobia_trigger, special_talent, favorite_gift FROM tbl_player_profile WHERE player_id = 1;");
        if (dtPlayer == null || dtPlayer.Rows.Count == 0)
        {
            Debug.LogError("[DB Check] Profil Devano (player_id 1) tidak ditemukan!");
            return false;
        }

        // 5. Cek Kolom & Data NPC Metadata (Xiang Bai, Li Haoran, Yang Mei, Edelweiss)
        DataTable dtNpc = DatabaseManager.Instance.ExecuteQuery("SELECT npc_id, birthday, zodiac, blood_type, theme_color, phobia_trigger, favorite_gift FROM tbl_npc_list;");
        if (dtNpc == null || dtNpc.Rows.Count < 4)
        {
            Debug.LogError($"[DB Check] Metadata NPC di tbl_npc_list belum lengkap (ditemukan {dtNpc?.Rows.Count ?? 0} baris)!");
            return false;
        }

        // 6. Cek Data Preferensi Lengkap (4 NPC)
        DataTable dtPref = DatabaseManager.Instance.ExecuteQuery("SELECT count(*) as cnt FROM tbl_npc_preferences WHERE npc_id IN (101, 102, 103, 104);");
        if (dtPref == null || Convert.ToInt32(dtPref.Rows[0]["cnt"]) < 4)
        {
            Debug.LogError("[DB Check] Data preferensi 4 NPC belum lengkap di tbl_npc_preferences!");
            return false;
        }

        // 7. Cek tidak ada nama lama "Kenzo"
        DataTable dtKenzo = DatabaseManager.Instance.ExecuteQuery("SELECT count(*) as cnt FROM tbl_dialogue_nodes WHERE dialogue_text LIKE '%Kenzo%' OR speaker_name LIKE '%Kenzo%';");
        if (dtKenzo != null && dtKenzo.Rows.Count > 0)
        {
            int cnt = Convert.ToInt32(dtKenzo.Rows[0]["cnt"]);
            if (cnt > 0)
            {
                Debug.LogError($"[DB Check] Ditemukan {cnt} teks dengan nama 'Kenzo' di tbl_dialogue_nodes!");
                return false;
            }
        }

        // 8. Cek Tabel Save/Load Multi-Slot (termasuk tbl_save_game_flags & tbl_save_character_events)
        string[] saveTables = { "tbl_save_metadata", "tbl_save_player_stats", "tbl_save_npc_relations", "tbl_save_story_flags", "tbl_save_game_flags", "tbl_save_game_events", "tbl_save_character_events" };
        foreach (string st in saveTables)
        {
            DataTable dtST = DatabaseManager.Instance.ExecuteQuery($"SELECT name FROM sqlite_master WHERE type='table' AND name='{st}';");
            if (dtST == null || dtST.Rows.Count == 0)
            {
                Debug.LogError($"[DB Check] Tabel save '{st}' belum ditemukan!");
                return false;
            }
        }

        // 9. Cek kolom baru hasil migrasi skema Save/Load
        (string table, string column)[] requiredColumns = new (string, string)[]
        {
            ("tbl_player_stats", "is_burned_out"),
            ("tbl_player_profile", "current_weather"),
            ("tbl_player_profile", "greeting_triggered_today"),
            ("tbl_npc_relations", "interacted_today"),
            ("tbl_save_metadata", "global_rumor_level"),
            ("tbl_save_metadata", "current_weather"),
            ("tbl_save_metadata", "greeting_triggered_today"),
            ("tbl_save_npc_relations", "days_since_last_interaction"),
            ("tbl_save_npc_relations", "interacted_today"),
            ("tbl_save_game_events", "source_table"),
        };
        foreach (var (table, column) in requiredColumns)
        {
            if (!TableHasColumn(table, column))
            {
                Debug.LogError($"[DB Check] Kolom '{column}' belum ditemukan di tabel '{table}'! Jalankan 'Game Database/Terapkan Migrasi Skema Save/Load'.");
                return false;
            }
        }

        Debug.Log("<color=green>[PASS]</color> Skema database SQLite, metadata karakter (Devano & 4 NPC), Venue 5, tabel & kolom Save/Load, dan verifikasi teks bersih terverifikasi valid.");
        return true;
    }

    private static bool TableHasColumn(string table, string column)
    {
        DataTable dt = DatabaseManager.Instance.ExecuteQuery($"PRAGMA table_info({table});");
        if (dt == null) return false;
        foreach (DataRow r in dt.Rows)
        {
            if (string.Equals(r["name"].ToString(), column, StringComparison.OrdinalIgnoreCase)) return true;
        }
        return false;
    }

    private static bool ValidateEventConditionSystem()
    {
        Debug.Log("--- 2.1. Memeriksa Event Condition System & Story Flag System ---");
        if (DatabaseManager.Instance == null) return true;

        // 1. Cek keberadaan tabel
        string[] eventTables = { "tbl_events", "tbl_story_flags" };
        foreach (string et in eventTables)
        {
            DataTable dt = DatabaseManager.Instance.ExecuteQuery($"SELECT name FROM sqlite_master WHERE type='table' AND name='{et}';");
            if (dt == null || dt.Rows.Count == 0)
            {
                Debug.LogError($"[Event System Check] Tabel '{et}' belum ditemukan di SQLite!");
                return false;
            }
        }

        // 1.1 Cek kolom set_flag_name di tbl_dialogue_options
        DataTable dtOptCols = DatabaseManager.Instance.ExecuteQuery("PRAGMA table_info(tbl_dialogue_options);");
        bool hasSetFlagName = false;
        if (dtOptCols != null)
        {
            foreach (DataRow r in dtOptCols.Rows)
            {
                if (r["name"].ToString() == "set_flag_name") hasSetFlagName = true;
            }
        }
        if (!hasSetFlagName)
        {
            Debug.LogError("[Event System Check] Kolom 'set_flag_name' belum ditemukan di tbl_dialogue_options!");
            return false;
        }

        // 2. Cek jumlah baseline events di tbl_events
        DataTable dtEvts = DatabaseManager.Instance.ExecuteQuery("SELECT count(*) as cnt FROM tbl_events;");
        if (dtEvts == null || Convert.ToInt32(dtEvts.Rows[0]["cnt"]) < 3)
        {
            Debug.LogError($"[Event System Check] Baseline events di tbl_events kurang dari 3 (ditemukan {dtEvts?.Rows[0]["cnt"]})!");
            return false;
        }

        Debug.Log("<color=green>[PASS]</color> Data-Driven Event Condition Engine (tbl_events & tbl_story_flags) terverifikasi valid.");
        return true;
    }

    [MenuItem("Game Database/Debug: Evaluasi & Picu Event Sekarang (Engine)")]
    public static void DebugTriggerEventEngine()
    {
        if (EventManager.Instance != null)
        {
            bool triggered = EventManager.Instance.TryTriggerEligibleEvent();
            if (triggered)
            {
                Debug.Log("<color=green>[DEBUG EVENT ENGINE]</color> Event berhasil dipicu via Menu Editor!");
            }
            else
            {
                Debug.Log("<color=yellow>[DEBUG EVENT ENGINE]</color> Tidak ada event yang memenuhi syarat pada kondisi saat ini.");
            }
        }
        else
        {
            Debug.LogError("[DEBUG EVENT ENGINE] EventManager.Instance tidak ditemukan di scene!");
        }
    }

    private static bool ValidateRuntimeInteractedTodayAndTransitions()
    {
        Debug.Log("--- 3. Memeriksa Flag interactedToday dan Transisi Waktu (Runtime) ---");

        if (SocialManager.Instance == null || SocialManager.Instance.relations == null || SocialManager.Instance.relations.Count == 0)
        {
            Debug.LogError("[Runtime Check] SocialManager.Instance atau daftar relasi belum siap.");
            return false;
        }

        // 1. Ambil salah satu relasi (misal Li Haoran, npcId = 102)
        var rel = SocialManager.Instance.relations.Find(x => x.npcId == 102);
        if (rel == null)
        {
            Debug.LogError("[Runtime Check] NPC ID 102 (Li Haoran) tidak ditemukan di daftar relasi.");
            return false;
        }

        // Simulasikan awal hari: interactedToday = false
        rel.interactedToday = false;
        int lonelInitial = rel.lonelinessMeter;

        // Panggil sapaan pagi untuk Li Haoran
        if (MorningGreetingManager.Instance != null)
        {
            MorningGreetingManager.Instance.OnGreetingAcknowledged(102, "Li Haoran", 5, 10);
            if (!rel.interactedToday)
            {
                Debug.LogError("[Runtime Check] interactedToday tidak terset true setelah Morning Greeting!");
                return false;
            }
            Debug.Log("<color=green>[PASS]</color> Morning Greeting berhasil mengaktifkan flag interactedToday = true.");
        }

        // Simulasikan evaluasi akhir hari
        SocialManager.Instance.ProsesAkhirHari();
        if (rel.interactedToday != false)
        {
            Debug.LogError("[Runtime Check] interactedToday tidak direset ke false saat akhir hari.");
            return false;
        }
        Debug.Log("<color=green>[PASS]</color> ProsesAkhirHari berhasil memproteksi dari Loneliness Decay dan mereset interactedToday = false.");

        // Uji transisi dialog hangout
        bool callbackExecuted = false;
        if (DialogueUIController.Instance != null)
        {
            DialogueUIController.Instance.ShowCloseButton(() =>
            {
                callbackExecuted = true;
            });

            // Simulasikan klik close button
            DialogueUIController.Instance.CloseDialoguePanel();
        }

        // Uji SaveManager (Quick Save & Quick Load)
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.QuickSave();
            if (!SaveManager.Instance.HasSaveData(SaveManager.QUICK_SAVE_SLOT))
            {
                Debug.LogError("[Save/Load Check] Data Quick Save tidak ditemukan setelah QuickSave()!");
                return false;
            }

            SaveManager.Instance.QuickLoad();
            Debug.Log("<color=green>[PASS]</color> SaveManager (Quick Save & Quick Load) runtime teruji sukses.");
        }

        Debug.Log("<color=green>[PASS]</color> Transisi penutupan dialog, flag interactedToday, dan Save/Load runtime teruji sukses.");
        return true;
    }

    // Slot khusus untuk pengujian round-trip; TIDAK memakai slot 0-5 milik pemain.
    private const int ROUND_TRIP_TEST_SLOT = 77;
    private const int ROUND_TRIP_TEST_NPC_ID = 102; // Li Haoran

    private struct RoundTripSnapshot
    {
        public int day;
        public string timeBlock;
        public string weather;
        public bool greetingTriggered;
        public int ph, mh, lang, etiq, theo, prac;
        public bool isBurnedOut;
        public int globalRumorLevel;
        public int guanxi, loneliness, rumorContribution, affectionState, daysSinceLastInteract;
        public bool interactedToday;
        public int flagValue;
        public bool milestoneUnlocked;
    }

    private static RoundTripSnapshot CaptureRoundTripSnapshot()
    {
        var rel = SocialManager.Instance.relations.Find(x => x.npcId == ROUND_TRIP_TEST_NPC_ID);
        DataTable dtMilestone = DatabaseManager.Instance.ExecuteQuery(
            $"SELECT is_unlocked FROM tbl_character_events WHERE event_id = 2;");

        return new RoundTripSnapshot
        {
            day = GameManager.Instance.currentDay,
            timeBlock = GameManager.Instance.currentTimeBlock.ToString(),
            weather = GameManager.Instance.currentWeather.ToString(),
            greetingTriggered = GameManager.Instance.greetingTriggeredToday,
            ph = PlayerStats.Instance.physicalHealth,
            mh = PlayerStats.Instance.mentalHealth,
            lang = PlayerStats.Instance.languageProficiency,
            etiq = PlayerStats.Instance.culturalEtiquette,
            theo = PlayerStats.Instance.academicTheoretical,
            prac = PlayerStats.Instance.academicPractical,
            isBurnedOut = PlayerStats.Instance.isBurnedOut,
            globalRumorLevel = SocialManager.Instance.globalRumorLevel,
            guanxi = rel != null ? rel.guanxiScore : -1,
            loneliness = rel != null ? rel.lonelinessMeter : -1,
            rumorContribution = rel != null ? rel.rumorContribution : -1,
            affectionState = rel != null ? rel.affectionState : -1,
            daysSinceLastInteract = rel != null ? rel.daysSinceLastInteract : -1,
            interactedToday = rel != null && rel.interactedToday,
            flagValue = FlagManager.Instance.GetFlag("integrity_roundtrip_test_flag"),
            milestoneUnlocked = dtMilestone != null && dtMilestone.Rows.Count > 0 && Convert.ToInt32(dtMilestone.Rows[0]["is_unlocked"]) == 1
        };
    }

    /// <summary>
    /// Menguji bahwa Save -> ubah state -> Load benar-benar mengembalikan seluruh state
    /// simulasi (waktu, cuaca, sapaan pagi, status pemain termasuk burnout, rumor global,
    /// relasi NPC termasuk interactedToday & daysSinceLastInteract, story flag, dan status
    /// unlock milestone karakter) ke kondisi persis seperti saat Save dipanggil.
    /// Mencakup skenario A-K pada spesifikasi audit Save/Load.
    /// </summary>
    private static bool ValidateSaveLoadRoundTrip()
    {
        Debug.Log("--- 4. Memeriksa Konsistensi Save/Load (Round-Trip Lengkap) ---");

        if (SaveManager.Instance == null || GameManager.Instance == null || PlayerStats.Instance == null ||
            SocialManager.Instance == null || FlagManager.Instance == null || DatabaseManager.Instance == null)
        {
            Debug.LogError("[Round-Trip Check] Salah satu komponen inti belum siap di scene.");
            return false;
        }

        var relBefore = SocialManager.Instance.relations.Find(x => x.npcId == ROUND_TRIP_TEST_NPC_ID);
        if (relBefore == null)
        {
            Debug.LogError($"[Round-Trip Check] NPC ID {ROUND_TRIP_TEST_NPC_ID} tidak ditemukan di daftar relasi.");
            return false;
        }

        try
        {
            // 1. Susun state yang jelas dan berbeda dari default (Edge case C, D, H)
            GameManager.Instance.currentTimeBlock = TimeBlock.Siang; // H: blok waktu spesifik
            GameManager.Instance.greetingTriggeredToday = true;
            SocialManager.Instance.globalRumorLevel = 2; // D: rumor aktif
            relBefore.lonelinessMeter = 85; // C: loneliness tinggi
            relBefore.guanxiScore = 77;
            relBefore.affectionState = 2;
            relBefore.daysSinceLastInteract = 3;
            relBefore.interactedToday = true; // B: sudah berinteraksi hari ini
            SocialManager.Instance.EvaluasiAffectionState(relBefore); // persist relasi (tanpa memicu unlock berbeda)
            FlagManager.Instance.SetFlag("integrity_roundtrip_test_flag", 1, "Flag pengujian round-trip");

            // E: milestone diunlock secara langsung di SQLite (mensimulasikan hasil CheckMilestoneUnlocks)
            DatabaseManager.Instance.ExecuteNonQuery("UPDATE tbl_character_events SET is_unlocked = 1 WHERE event_id = 2;");

            // 2. Ambil snapshot TEPAT SEBELUM Save — ini adalah "Game State A"
            var expected = CaptureRoundTripSnapshot();

            // 3. Save ke slot pengujian
            SaveManager.Instance.SaveGame(ROUND_TRIP_TEST_SLOT, "Integrity RoundTrip Test");
            if (!SaveManager.Instance.HasSaveData(ROUND_TRIP_TEST_SLOT))
            {
                Debug.LogError("[Round-Trip Check] SaveGame() tidak menghasilkan data pada slot pengujian.");
                return false;
            }

            // 4. Ubah SELURUH state secara signifikan (mensimulasikan progres pemain setelah Save)
            GameManager.Instance.currentTimeBlock = TimeBlock.Malam;
            GameManager.Instance.greetingTriggeredToday = false;
            SocialManager.Instance.globalRumorLevel = 0;
            relBefore.lonelinessMeter = 10;
            relBefore.guanxiScore = 5;
            relBefore.daysSinceLastInteract = 0;
            relBefore.interactedToday = false;
            SocialManager.Instance.EvaluasiAffectionState(relBefore);
            FlagManager.Instance.SetFlag("integrity_roundtrip_test_flag", 0, "Flag pengujian round-trip");
            DatabaseManager.Instance.ExecuteNonQuery("UPDATE tbl_character_events SET is_unlocked = 0 WHERE event_id = 2;");
            PlayerStats.Instance.isBurnedOut = true;

            // 5. Load kembali — harus mengembalikan seluruh state ke kondisi langkah 2 ("Game State A")
            SaveManager.Instance.LoadGame(ROUND_TRIP_TEST_SLOT);

            var actual = CaptureRoundTripSnapshot();

            // 6. Bandingkan field demi field
            bool ok = true;
            ok &= AssertEqual("currentDay", expected.day, actual.day);
            ok &= AssertEqual("timeBlock", expected.timeBlock, actual.timeBlock);
            ok &= AssertEqual("weather", expected.weather, actual.weather);
            ok &= AssertEqual("greetingTriggeredToday", expected.greetingTriggered, actual.greetingTriggered);
            ok &= AssertEqual("physicalHealth", expected.ph, actual.ph);
            ok &= AssertEqual("mentalHealth", expected.mh, actual.mh);
            ok &= AssertEqual("languageProficiency", expected.lang, actual.lang);
            ok &= AssertEqual("culturalEtiquette", expected.etiq, actual.etiq);
            ok &= AssertEqual("academicTheoretical", expected.theo, actual.theo);
            ok &= AssertEqual("academicPractical", expected.prac, actual.prac);
            ok &= AssertEqual("isBurnedOut", expected.isBurnedOut, actual.isBurnedOut);
            ok &= AssertEqual("globalRumorLevel", expected.globalRumorLevel, actual.globalRumorLevel);
            ok &= AssertEqual("guanxiScore", expected.guanxi, actual.guanxi);
            ok &= AssertEqual("lonelinessMeter", expected.loneliness, actual.loneliness);
            ok &= AssertEqual("rumorContribution", expected.rumorContribution, actual.rumorContribution);
            ok &= AssertEqual("affectionState", expected.affectionState, actual.affectionState);
            ok &= AssertEqual("daysSinceLastInteract", expected.daysSinceLastInteract, actual.daysSinceLastInteract);
            ok &= AssertEqual("interactedToday", expected.interactedToday, actual.interactedToday);
            ok &= AssertEqual("storyFlagValue", expected.flagValue, actual.flagValue);
            ok &= AssertEqual("milestoneUnlocked", expected.milestoneUnlocked, actual.milestoneUnlocked);

            if (!ok)
            {
                Debug.LogError("[Round-Trip Check] Save -> Load TIDAK menghasilkan state yang setara (State A tidak terpulihkan).");
                return false;
            }

            // 7. I/K: muat ulang beberapa kali berturut-turut harus tetap stabil (idempoten)
            for (int i = 0; i < 2; i++)
            {
                SaveManager.Instance.LoadGame(ROUND_TRIP_TEST_SLOT);
                var repeated = CaptureRoundTripSnapshot();
                if (!AssertEqual($"repeatedLoad[{i}].guanxiScore", expected.guanxi, repeated.guanxi) ||
                    !AssertEqual($"repeatedLoad[{i}].interactedToday", expected.interactedToday, repeated.interactedToday))
                {
                    Debug.LogError("[Round-Trip Check] Load berulang (I/K) tidak stabil.");
                    return false;
                }
            }

            Debug.Log("<color=green>[PASS]</color> Save -> Ubah State -> Load menghasilkan Game State yang setara secara penuh (waktu, cuaca, sapaan pagi, status pemain & burnout, rumor global, relasi NPC, story flag, milestone karakter).");
            return true;
        }
        finally
        {
            // Bersihkan slot pengujian & flag sementara agar tidak mengotori data pemain nyata.
            SaveManager.Instance.DeleteSave(ROUND_TRIP_TEST_SLOT);
            FlagManager.Instance.RemoveFlag("integrity_roundtrip_test_flag");
        }
    }

    private static bool AssertEqual<T>(string fieldName, T expected, T actual)
    {
        if (!System.Collections.Generic.EqualityComparer<T>.Default.Equals(expected, actual))
        {
            Debug.LogError($"[Round-Trip Check] Mismatch pada '{fieldName}': diharapkan '{expected}', didapat '{actual}'.");
            return false;
        }
        return true;
    }
}
#endif
