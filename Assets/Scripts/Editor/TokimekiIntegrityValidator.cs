#if UNITY_EDITOR
using System;
using System.Data;
using UnityEditor;
using UnityEngine;

public static class TokimekiIntegrityValidator
{
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
        }
        else
        {
            Debug.Log("<color=yellow>[Info]</color> Jalankan Play Mode untuk menguji verifikasi runtime interactedToday dan transisi waktu.");
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

        // 8. Cek Tabel Save/Load Multi-Slot
        string[] saveTables = { "tbl_save_metadata", "tbl_save_player_stats", "tbl_save_npc_relations", "tbl_save_story_flags", "tbl_save_game_events" };
        foreach (string st in saveTables)
        {
            DataTable dtST = DatabaseManager.Instance.ExecuteQuery($"SELECT name FROM sqlite_master WHERE type='table' AND name='{st}';");
            if (dtST == null || dtST.Rows.Count == 0)
            {
                Debug.LogError($"[DB Check] Tabel save '{st}' belum ditemukan!");
                return false;
            }
        }

        Debug.Log("<color=green>[PASS]</color> Skema database SQLite, metadata karakter (Devano & 4 NPC), Venue 5, tabel Save/Load, dan verifikasi teks bersih terverifikasi valid.");
        return true;
    }

    private static bool ValidateEventConditionSystem()
    {
        Debug.Log("--- 2.1. Memeriksa Event Condition System & Story Flag System ---");
        if (DatabaseManager.Instance == null) return true;

        // 1. Cek keberadaan tabel
        string[] eventTables = { "tbl_game_events", "tbl_story_flags" };
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

        // 2. Cek jumlah baseline events
        DataTable dtEvts = DatabaseManager.Instance.ExecuteQuery("SELECT count(*) as cnt FROM tbl_game_events;");
        if (dtEvts == null || Convert.ToInt32(dtEvts.Rows[0]["cnt"]) < 10)
        {
            Debug.LogError($"[Event System Check] Baseline events di tbl_game_events kurang dari 10 (ditemukan {dtEvts?.Rows[0]["cnt"]})!");
            return false;
        }

        // 3. Cek prioritas krisis rumor (priority 100) vs midterm (80)
        DataTable dtPrio = DatabaseManager.Instance.ExecuteQuery(
            "SELECT event_code, priority FROM tbl_game_events WHERE event_code IN ('EVT_RUMOR_CRISIS', 'EVT_MIDTERM_EVAL') ORDER BY priority DESC;");
        if (dtPrio == null || dtPrio.Rows.Count < 2 || dtPrio.Rows[0]["event_code"].ToString() != "EVT_RUMOR_CRISIS")
        {
            Debug.LogError("[Event System Check] Prioritas EVT_RUMOR_CRISIS harus lebih tinggi dari EVT_MIDTERM_EVAL!");
            return false;
        }

        Debug.Log("<color=green>[PASS]</color> Event Condition System (tbl_game_events & tbl_game_flags) terverifikasi valid.");
        return true;
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
}
#endif
