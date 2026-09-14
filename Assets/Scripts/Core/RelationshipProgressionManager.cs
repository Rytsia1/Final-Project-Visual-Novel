using System;
using System.Data;
using System.Collections.Generic;
using UnityEngine;

public class RelationshipProgressionManager : MonoBehaviour
{
    private static RelationshipProgressionManager _instance;
    public static RelationshipProgressionManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<RelationshipProgressionManager>();
                if (_instance == null)
                {
                    GameObject core = GameObject.Find("GAME_CORE") ?? GameObject.Find("[GAME_CORE]");
                    if (core != null)
                    {
                        _instance = core.AddComponent<RelationshipProgressionManager>();
                    }
                    else
                    {
                        GameObject go = new GameObject("[RelationshipProgressionManager]");
                        _instance = go.AddComponent<RelationshipProgressionManager>();
                        DontDestroyOnLoad(go);
                    }
                }
            }
            return _instance;
        }
        private set => _instance = value;
    }

    public event Action<int, string> OnMilestoneUnlocked;
    public event Action<int> OnMilestoneCompleted;
    public event Action<string, string> OnEndingReached;

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
    // 1. PENGECEKAN & PEMBUKAAN MILESTONE EVENT
    // =========================================================================
    /// <summary>
    /// Dipanggil setiap kali Affection State atau Guanxi berubah.
    /// Membuka event cerita khusus jika prasyarat Affection State terpenuhi.
    /// </summary>
    public void CheckMilestoneUnlocks(int npcId, int currentAffectionState)
    {
        if (DatabaseManager.Instance == null) return;

        try
        {
            string query = $"SELECT event_id, event_title, req_affection_state FROM tbl_character_events " +
                           $"WHERE npc_id = {npcId} AND req_affection_state <= {currentAffectionState} AND is_unlocked = 0;";
            DataTable dt = DatabaseManager.Instance.ExecuteQuery(query);

            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    int eventId = Convert.ToInt32(row["event_id"]);
                    string eventTitle = row["event_title"].ToString();

                    // Buka event di basis data
                    string updateQ = $"UPDATE tbl_character_events SET is_unlocked = 1 WHERE event_id = {eventId};";
                    DatabaseManager.Instance.ExecuteNonQuery(updateQ);

                    Debug.Log($"<color=gold>[MILESTONE UNLOCKED]</color> Event '{eventTitle}' (NPC ID: {npcId}) telah terbuka!");

                    if (HUDController.Instance != null)
                    {
                        HUDController.Instance.ShowToastNotification($"[EVENT BARU] {eventTitle} Terbuka!");
                    }

                    if (TelemetryLogger.Instance != null)
                    {
                        TelemetryLogger.Instance.RecordCriticalEvent("MILESTONE_UNLOCKED", $"Event '{eventTitle}' (ID {eventId}) terbuka untuk NPC {npcId}");
                    }

                    OnMilestoneUnlocked?.Invoke(npcId, eventTitle);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[RelationshipProgressionManager] Gagal memeriksa unlock milestone: {ex.Message}");
        }
    }

    /// <summary>
    /// Memeriksa apakah NPC memiliki milestone event yang sudah terbuka tapi belum diselesaikan.
    /// </summary>
    public bool HasPendingMilestoneEvent(int npcId, out int eventId, out string eventTitle, out int startNodeId)
    {
        eventId = 0;
        eventTitle = "";
        startNodeId = 0;

        if (DatabaseManager.Instance == null) return false;

        try
        {
            string query = $"SELECT event_id, event_title, start_node_id FROM tbl_character_events " +
                           $"WHERE npc_id = {npcId} AND is_unlocked = 1 AND is_completed = 0 " +
                           $"ORDER BY req_affection_state ASC LIMIT 1;";
            DataTable dt = DatabaseManager.Instance.ExecuteQuery(query);

            if (dt != null && dt.Rows.Count > 0)
            {
                eventId = Convert.ToInt32(dt.Rows[0]["event_id"]);
                eventTitle = dt.Rows[0]["event_title"].ToString();
                startNodeId = Convert.ToInt32(dt.Rows[0]["start_node_id"]);
                return true;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[RelationshipProgressionManager] Gagal cek pending milestone event: {ex.Message}");
        }

        return false;
    }

    /// <summary>
    /// Memulai milestone event jika tersedia untuk NPC tertentu.
    /// </summary>
    public bool TryStartPendingMilestoneEvent(int npcId)
    {
        if (HasPendingMilestoneEvent(npcId, out int eventId, out string title, out int startNodeId))
        {
            Debug.Log($"<color=gold>[MILESTONE STORY]</color> Memulai event '{title}' (Node: {startNodeId}) untuk NPC {npcId}");

            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.StartDialogue(startNodeId, () =>
                {
                    MarkMilestoneCompleted(eventId);

                    // Tandai bahwa Devano berinteraksi hari ini agar tidak terjadi decay
                    if (SocialManager.Instance != null && SocialManager.Instance.relations != null)
                    {
                        var rel = SocialManager.Instance.relations.Find(x => x.npcId == npcId);
                        if (rel != null) rel.interactedToday = true;
                    }
                });
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Menandai event milestone tertentu telah selesai di basis data SQLite.
    /// </summary>
    public void MarkMilestoneCompleted(int eventId)
    {
        if (DatabaseManager.Instance == null) return;

        try
        {
            string query = $"SELECT event_title, npc_id FROM tbl_character_events WHERE event_id = {eventId};";
            DataTable dt = DatabaseManager.Instance.ExecuteQuery(query);
            string title = "Milestone Event";

            if (dt != null && dt.Rows.Count > 0)
            {
                title = dt.Rows[0]["event_title"].ToString();
            }

            string updateQ = $"UPDATE tbl_character_events SET is_completed = 1 WHERE event_id = {eventId};";
            DatabaseManager.Instance.ExecuteNonQuery(updateQ);

            Debug.Log($"<color=green>[MILESTONE COMPLETED]</color> Event '{title}' (ID: {eventId}) berhasil diselesaikan.");

            if (HUDController.Instance != null)
            {
                HUDController.Instance.ShowToastNotification($"[EVENT SELESAI] {title}");
            }

            if (TelemetryLogger.Instance != null)
            {
                TelemetryLogger.Instance.RecordCriticalEvent("MILESTONE_COMPLETED", $"Event '{title}' (ID {eventId}) diselesaikan.");
            }

            OnMilestoneCompleted?.Invoke(eventId);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[RelationshipProgressionManager] Gagal menyelesaikan milestone event: {ex.Message}");
        }
    }

    // =========================================================================
    // 2. CASUAL DIALOGUE POOL (IDLE / FREE TIME CHAT)
    // =========================================================================
    /// <summary>
    /// Menarik node dialog kasual/santai yang sesuai dengan affection_state saat ini
    /// serta memprioritaskan callback konsekuensial berbasis Story Flag jika tersedia.
    /// </summary>
    public int GetCasualDialogueNode(int npcId, int currentAffectionState)
    {
        if (DatabaseManager.Instance == null) return GetFallbackNode(npcId);

        try
        {
            // 1. Prioritas Tertinggi: Kueri Consequential Callback berbasis Story Flag (Priority >= 20)
            string callbackQuery = $"SELECT node_id, priority FROM tbl_dialogue_nodes " +
                                   $"WHERE npc_id = {npcId} AND req_affection_state <= {currentAffectionState} " +
                                   $"AND req_flag_name IS NOT NULL " +
                                   $"AND req_flag_name IN (" +
                                   $"    SELECT flag_name FROM tbl_story_flags WHERE flag_value = tbl_dialogue_nodes.req_flag_val" +
                                   $") " +
                                   $"ORDER BY priority DESC, req_affection_state DESC LIMIT 1;";
            DataTable dtCallback = DatabaseManager.Instance.ExecuteQuery(callbackQuery);

            if (dtCallback != null && dtCallback.Rows.Count > 0)
            {
                int callbackNodeId = Convert.ToInt32(dtCallback.Rows[0]["node_id"]);
                Debug.Log($"<color=cyan>[CONSEQUENTIAL CALLBACK]</color> Memilih node dialog callback: {callbackNodeId} (NPC {npcId}) berbasis Story Flag.");
                return callbackNodeId;
            }

            // 2. Dialog Kasual Normal (Sesuai affection_state saat ini tanpa syarat flag)
            string query = $"SELECT node_id FROM tbl_dialogue_nodes " +
                           $"WHERE npc_id = {npcId} AND req_affection_state = {currentAffectionState} " +
                           $"AND req_flag_name IS NULL " +
                           $"AND node_id BETWEEN 1100 AND 1999;";
            DataTable dt = DatabaseManager.Instance.ExecuteQuery(query);

            if (dt != null && dt.Rows.Count > 0)
            {
                int randomIndex = UnityEngine.Random.Range(0, dt.Rows.Count);
                return Convert.ToInt32(dt.Rows[randomIndex]["node_id"]);
            }

            // 3. Fallback ke affection state yang lebih rendah jika belum ada dialog khusus
            for (int s = currentAffectionState - 1; s >= 0; s--)
            {
                string fallbackQ = $"SELECT node_id FROM tbl_dialogue_nodes " +
                                   $"WHERE npc_id = {npcId} AND req_affection_state = {s} " +
                                   $"AND req_flag_name IS NULL " +
                                   $"AND node_id BETWEEN 1100 AND 1999;";
                DataTable dtFallback = DatabaseManager.Instance.ExecuteQuery(fallbackQ);
                if (dtFallback != null && dtFallback.Rows.Count > 0)
                {
                    return Convert.ToInt32(dtFallback.Rows[0]["node_id"]);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[RelationshipProgressionManager] Gagal mengambil casual dialogue node: {ex.Message}");
        }

        return GetFallbackNode(npcId);
    }

    private int GetFallbackNode(int npcId)
    {
        switch (npcId)
        {
            case 101: return 1100;
            case 102: return 1200;
            case 103: return 1300;
            case 104: return 1400;
            default:  return 1001;
        }
    }

    /// <summary>
    /// Memulai interaksi dengan NPC di waktu luang:
    /// Jika ada milestone event yang belum selesai -> jalankan milestone event.
    /// Jika tidak ada -> jalankan dialog obrolan santai sesuai tingkat kedekatan.
    /// </summary>
    public void StartCasualInteraction(int npcId)
    {
        if (TryStartPendingMilestoneEvent(npcId))
        {
            return;
        }

        int state = 0;
        if (SocialManager.Instance != null && SocialManager.Instance.relations != null)
        {
            var rel = SocialManager.Instance.relations.Find(x => x.npcId == npcId);
            if (rel != null)
            {
                state = rel.affectionState;
                rel.interactedToday = true;
            }
        }

        int node = GetCasualDialogueNode(npcId, state);
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogue(node);
        }
    }

    // =========================================================================
    // 3. MULTI-ENDING EVALUATOR (HARI KE-60 - AKHIR SEMESTER)
    // =========================================================================
    /// <summary>
    /// Mengevaluasi seluruh relasi dan prestasi akademik di Hari ke-60 (blok Pagi)
    /// untuk menentukan epilog ending cerita yang diperoleh Devano.
    /// </summary>
    public string EvaluateEndingAtDay60()
    {
        Debug.Log("<color=gold>====================================================</color>");
        Debug.Log("<color=gold>=== EVALUASI HARI KE-60: PENENTUAN MULTI-ENDING ===</color>");
        Debug.Log("<color=gold>====================================================</color>");

        string chosenEndingCode = "END_DEFAULT_RETURN";
        string chosenEndingTitle = "Akhir Masa Pertukaran Pelajar";
        int chosenStartNodeId = 8601;

        NPCRelationData bestRelation = null;
        int maxGuanxi = -1;

        // A. Temukan relasi dengan Guanxi tertinggi
        if (SocialManager.Instance != null && SocialManager.Instance.relations != null)
        {
            foreach (var rel in SocialManager.Instance.relations)
            {
                if (rel.guanxiScore > maxGuanxi)
                {
                    maxGuanxi = rel.guanxiScore;
                    bestRelation = rel;
                }
            }
        }

        // B. Uji Kualifikasi Ending Karakter Spesifik:
        // Syarat: Guanxi >= 85, Affection State == 3, dan Prerequisite Milestone Selesai (is_completed = 1)
        bool characterEndingAchieved = false;

        if (bestRelation != null && bestRelation.affectionState >= 3 && bestRelation.guanxiScore >= 85)
        {
            bool milestonePrereqDone = CheckEndingMilestoneCompleted(bestRelation.npcId);

            if (milestonePrereqDone)
            {
                string endingDataQ = $"SELECT ending_code, ending_title, start_node_id FROM tbl_endings WHERE npc_id = {bestRelation.npcId};";
                DataTable dtEnding = DatabaseManager.Instance.ExecuteQuery(endingDataQ);

                if (dtEnding != null && dtEnding.Rows.Count > 0)
                {
                    chosenEndingCode = dtEnding.Rows[0]["ending_code"].ToString();
                    chosenEndingTitle = dtEnding.Rows[0]["ending_title"].ToString();
                    chosenStartNodeId = Convert.ToInt32(dtEnding.Rows[0]["start_node_id"]);
                    characterEndingAchieved = true;
                    Debug.Log($"<color=green>[ENDING KARAKTER]</color> Memenuhi syarat ending {bestRelation.npcName}! Guanxi: {bestRelation.guanxiScore}, State: {bestRelation.affectionState}");
                }
            }
            else
            {
                Debug.LogWarning($"[Ending Warning] {bestRelation.npcName} memiliki Guanxi {bestRelation.guanxiScore} & State 3, tapi belum menyelesaikan event milestone prasyarat.");
            }
        }

        // C. Jika Ending Karakter Tidak Tercapai, Evaluasi Rute Akademik (True Solo Academic Ending)
        if (!characterEndingAchieved)
        {
            int theoretical = PlayerStats.Instance != null ? PlayerStats.Instance.academicTheoretical : 0;
            int practical = PlayerStats.Instance != null ? PlayerStats.Instance.academicPractical : 0;

            if (theoretical >= 75 && practical >= 75)
            {
                chosenEndingCode = "END_SOLO_ACADEMIC";
                chosenEndingTitle = "Penyelesaian Studi Cum Laude";
                chosenStartNodeId = 8501;
                Debug.Log($"<color=cyan>[ENDING AKADEMIK]</color> Lulus Cum Laude! Teori: {theoretical}, Praktis: {practical}");
            }
            else
            {
                chosenEndingCode = "END_DEFAULT_RETURN";
                chosenEndingTitle = "Akhir Masa Pertukaran Pelajar";
                chosenStartNodeId = 8601;
                Debug.Log($"<color=orange>[ENDING NORMAL]</color> Kembali ke tanah air dengan pengalaman adaptasi.");
            }
        }

        // D. Tampilkan feedback visual & catat ke telemetry
        if (HUDController.Instance != null)
        {
            HUDController.Instance.ShowToastNotification($"[HARI 60 - TAMAT] {chosenEndingTitle}");
        }

        if (TelemetryLogger.Instance != null)
        {
            TelemetryLogger.Instance.RecordCriticalEvent("ENDING_REACHED", $"Pemain mencapai ending '{chosenEndingCode}' ({chosenEndingTitle})");
        }

        OnEndingReached?.Invoke(chosenEndingCode, chosenEndingTitle);

        // E. Mulai Dialog Ending
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogue(chosenStartNodeId);
        }

        return chosenEndingCode;
    }

    private bool CheckEndingMilestoneCompleted(int npcId)
    {
        if (DatabaseManager.Instance == null) return false;

        try
        {
            string query = $"SELECT is_completed FROM tbl_character_events " +
                           $"WHERE npc_id = {npcId} AND is_ending_prerequisite = 1;";
            DataTable dt = DatabaseManager.Instance.ExecuteQuery(query);

            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    if (Convert.ToInt32(row["is_completed"]) != 1)
                    {
                        return false; // Ada prasyarat yang belum tuntas
                    }
                }
                return true;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[RelationshipProgressionManager] Gagal cek status milestone ending: {ex.Message}");
        }

        return false;
    }
}
