using System;
using System.Data;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    private static DialogueManager _instance;
    public static DialogueManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<DialogueManager>();
            }
            return _instance;
        }
        private set { _instance = value; }
    }

    [Header("State Dialog Aktif")]
    public int currentNodeId;
    public int currentNpcId;
    public bool isDialogueActive = false;

    [Header("State Sesi Hangout")]
    public bool isHangoutDialogueActive = false;
    public int currentHangoutNpcId = 0;
    public int currentHangoutVenueId = 0;

    [Header("Callback Penutupan Dialog")]
    public System.Action onDialogueEndCallback = null;

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    // Memulai interaksi dialog dari node pembuka mana pun
    public void StartDialogue(int startingNodeId, System.Action onEndCallback = null)
    {
        isDialogueActive = true;
        currentNodeId = startingNodeId;
        onDialogueEndCallback = onEndCallback;
        RenderCurrentNode();
    }

    public void StartHangoutDialogue(int startingNodeId, int npcId, int venueId)
    {
        isHangoutDialogueActive = true;
        currentHangoutNpcId = npcId;
        currentHangoutVenueId = venueId;
        StartDialogue(startingNodeId);
    }

    // Menarik teks dan nama pembicara dari tbl_dialogue_nodes
    public void RenderCurrentNode()
    {
        string query = $"SELECT npc_id, speaker_name, dialogue_text FROM tbl_dialogue_nodes WHERE node_id = {currentNodeId};";
        DataTable dt = DatabaseManager.Instance.ExecuteQuery(query);

        if (dt != null && dt.Rows.Count > 0)
        {
            currentNpcId = Convert.ToInt32(dt.Rows[0]["npc_id"]);
            string speaker = dt.Rows[0]["speaker_name"].ToString();
            string text = dt.Rows[0]["dialogue_text"].ToString();

            Debug.Log($"<color=cyan>[DIALOG]</color> <b>{speaker}</b>: \"{text}\"");

            if (DialogueUIController.Instance != null)
            {
                DialogueUIController.Instance.DisplayDialogue(speaker, text);
            }

            LoadDialogueOptions(currentNodeId);
        }
        else
        {
            EndDialogue();
        }
    }

    // Memuat daftar opsi respon dari tbl_dialogue_options
    private void LoadDialogueOptions(int nodeId)
    {
        string query = $"SELECT option_id, option_text FROM tbl_dialogue_options WHERE node_id = {nodeId};";
        DataTable dt = DatabaseManager.Instance.ExecuteQuery(query);

        if (dt != null && dt.Rows.Count > 0)
        {
            foreach (DataRow row in dt.Rows)
            {
                int optId = Convert.ToInt32(row["option_id"]);
                string optText = row["option_text"].ToString();

                if (DialogueUIController.Instance != null)
                {
                    DialogueUIController.Instance.CreateOptionButton(optId, optText);
                }
            }
        }
        else
        {
            // Jika node tidak memiliki opsi lanjutan, tampilkan tombol tutup
            if (DialogueUIController.Instance != null)
            {
                DialogueUIController.Instance.ShowCloseButton();
            }
        }
    }

    // Titik Masuk Generik: Mengevaluasi opsi apa pun yang diklik pemain
    public void SelectOption(int optionId)
    {
        string query = $"SELECT * FROM tbl_dialogue_options WHERE option_id = {optionId};";
        DataTable dt = DatabaseManager.Instance.ExecuteQuery(query);

        if (dt == null || dt.Rows.Count == 0)
        {
            EndDialogue();
            return;
        }

        DataRow optRow = dt.Rows[0];
        int targetSuccessNodeId = optRow["next_node_id"] != DBNull.Value ? Convert.ToInt32(optRow["next_node_id"]) : 0;
        int targetMianziNodeId = optRow["fail_mianzi_node_id"] != DBNull.Value ? Convert.ToInt32(optRow["fail_mianzi_node_id"]) : 0;
        int targetLangNodeId = optRow["fail_language_node_id"] != DBNull.Value ? Convert.ToInt32(optRow["fail_language_node_id"]) : 0;

        int baseGuanxi = Convert.ToInt32(optRow["effect_guanxi"]);
        int mianziPenalty = Convert.ToInt32(optRow["effect_mianzi_penalty"]);

        // Jika tidak ada target rute lanjutan, dialog langsung selesai
        if (targetSuccessNodeId == 0)
        {
            TerapkanEfekOpsi(baseGuanxi, 0, 0);
            EndDialogue();
            return;
        }

        // Jalankan mesin evaluasi berjenjang (Hierarchical Stat-Checking)
        ExecuteHierarchicalEvaluation(optRow, optionId, targetSuccessNodeId, targetMianziNodeId, targetLangNodeId, baseGuanxi, mianziPenalty);
    }

    // Mesin Evaluasi Berjenjang Deterministik (Gambar 3.5 & Dilema 3-Tier)
    private void ExecuteHierarchicalEvaluation(DataRow optRow, int optionId, int successNodeId, int mianziNodeId, int langNodeId, int baseGuanxi, int mianziPenalty)
    {
        // 1. Ambil prasyarat dari node target sukses
        string nodeQuery = $"SELECT req_language, req_etiquette, npc_id FROM tbl_dialogue_nodes WHERE node_id = {successNodeId};";
        DataTable dtNode = DatabaseManager.Instance.ExecuteQuery(nodeQuery);

        int reqLanguage = 0;
        int reqEtiquette = 0;
        int targetNpcId = currentNpcId;

        if (dtNode != null && dtNode.Rows.Count > 0)
        {
            reqLanguage = Convert.ToInt32(dtNode.Rows[0]["req_language"]);
            reqEtiquette = Convert.ToInt32(dtNode.Rows[0]["req_etiquette"]);
            targetNpcId = Convert.ToInt32(dtNode.Rows[0]["npc_id"]);
        }

        // Ambil efek stat spesifik jika tersedia di tabel opsi
        int dEtiq = optRow.Table.Columns.Contains("effect_etiquette") && optRow["effect_etiquette"] != DBNull.Value ? Convert.ToInt32(optRow["effect_etiquette"]) : 0;
        int dLoneliness = optRow.Table.Columns.Contains("effect_loneliness") && optRow["effect_loneliness"] != DBNull.Value ? Convert.ToInt32(optRow["effect_loneliness"]) : 25;
        int dRumor = optRow.Table.Columns.Contains("effect_rumor") && optRow["effect_rumor"] != DBNull.Value ? Convert.ToInt32(optRow["effect_rumor"]) : 0;
        int dMental = optRow.Table.Columns.Contains("effect_mental") && optRow["effect_mental"] != DBNull.Value ? Convert.ToInt32(optRow["effect_mental"]) : 0;
        int dPhysical = optRow.Table.Columns.Contains("effect_physical") && optRow["effect_physical"] != DBNull.Value ? Convert.ToInt32(optRow["effect_physical"]) : 0;
        int dTheoretical = optRow.Table.Columns.Contains("effect_theoretical") && optRow["effect_theoretical"] != DBNull.Value ? Convert.ToInt32(optRow["effect_theoretical"]) : 0;
        int dPractical = optRow.Table.Columns.Contains("effect_practical") && optRow["effect_practical"] != DBNull.Value ? Convert.ToInt32(optRow["effect_practical"]) : 0;
        int dLanguage = optRow.Table.Columns.Contains("effect_language") && optRow["effect_language"] != DBNull.Value ? Convert.ToInt32(optRow["effect_language"]) : 0;

        int playerLang = PlayerStats.Instance != null ? PlayerStats.Instance.languageProficiency : 0;
        int playerEtiq = PlayerStats.Instance != null ? PlayerStats.Instance.culturalEtiquette : 0;

        int finalDestinationNode = successNodeId;
        string outcomeRoute = "RUTE_A_SUKSES";

        // 2. Evaluasi Gerbang 1: Kemampuan Bahasa (Linguistic Check)
        if (reqLanguage > 0 && playerLang < reqLanguage)
        {
            outcomeRoute = "RUTE_C_FAIL_LANGUAGE";
            finalDestinationNode = (langNodeId != 0) ? langNodeId : successNodeId;

            SocialManager.Instance.TambahGuanxi(targetNpcId, penambahanGuanxi: -5, reduksiLoneliness: 5);
            PlayerStats.Instance.ModifyStats(dLanguage: 0, dEtiquette: 0, dMental: -10, dPhysical: 0, dTheoretical: 0, dPractical: 0);

            Debug.Log($"<color=orange>[Evaluasi Dialog]</color> Gagal Gerbang Bahasa ({playerLang}/{reqLanguage}) -> Dialihkan ke Node {finalDestinationNode}");
        }
        // 3. Evaluasi Gerbang 2: Etika Budaya (Cultural Etiquette / Mianzi Check)
        else if (reqEtiquette > 0 && playerEtiq < reqEtiquette)
        {
            outcomeRoute = "RUTE_B_FAIL_MIANZI";
            finalDestinationNode = (mianziNodeId != 0) ? mianziNodeId : successNodeId;

            int penaltyGuanxi = (mianziPenalty > 0) ? -mianziPenalty : -20;
            SocialManager.Instance.TambahGuanxi(targetNpcId, penambahanGuanxi: penaltyGuanxi, reduksiLoneliness: 5);

            var rel = SocialManager.Instance.relations.Find(x => x.npcId == targetNpcId);
            if (rel != null)
            {
                rel.rumorContribution = Mathf.Clamp(rel.rumorContribution + 10, 0, 100);
            }

            PlayerStats.Instance.ModifyStats(dLanguage: 0, dEtiquette: 0, dMental: -5, dPhysical: 0, dTheoretical: 0, dPractical: 0);
            Debug.LogWarning($"<color=red>[Evaluasi Dialog]</color> Pelanggaran Mianzi! Etika ({playerEtiq}/{reqEtiquette}) -> Dialihkan ke Node {finalDestinationNode}");
        }
        // 4. Lolos Seluruh Gerbang / Skenario Dilema 3-Tier
        else
        {
            outcomeRoute = (reqLanguage == 0 && reqEtiquette == 0) ? "DILEMA_IMPRESSION" : "RUTE_A_SUKSES";
            finalDestinationNode = successNodeId;

            if (SocialManager.Instance != null)
            {
                SocialManager.Instance.TambahGuanxi(targetNpcId, penambahanGuanxi: baseGuanxi, reduksiLoneliness: dLoneliness);

                if (dRumor != 0)
                {
                    var rel = SocialManager.Instance.relations.Find(x => x.npcId == targetNpcId);
                    if (rel != null)
                    {
                        rel.rumorContribution = Mathf.Clamp(rel.rumorContribution + dRumor, 0, 100);
                    }
                }
            }

            if (PlayerStats.Instance != null)
            {
                bool hasCustomStats = (dLanguage != 0 || dEtiq != 0 || dMental != 0 || dPhysical != 0 || dTheoretical != 0 || dPractical != 0);
                if (hasCustomStats)
                {
                    PlayerStats.Instance.ModifyStats(dLanguage, dEtiq, dMental, dPhysical, dTheoretical, dPractical);
                }
                else if (reqLanguage > 0 || reqEtiquette > 0)
                {
                    // Fallback nilai akademik untuk bimbingan dosen lama
                    PlayerStats.Instance.ModifyStats(0, 0, 5, 0, 5, 5);
                }
            }

            Debug.Log($"<color=green>[Evaluasi Dialog]</color> Outcome: {outcomeRoute} -> Guanxi: {baseGuanxi}, Loneliness: -{dLoneliness}, Rumor: +{dRumor} -> Menuju Node {finalDestinationNode}");
        }

        // 5. Rekam jejak keputusan ke Telemetry Logger
        if (TelemetryLogger.Instance != null)
        {
            TelemetryLogger.Instance.RecordDialogueChoice(currentNodeId, optionId, outcomeRoute, $"P:{playerLang}/{playerEtiq} R:{reqLanguage}/{reqEtiquette} G:{baseGuanxi}");
        }

        // 6. Transisi ke Node Hasil Evaluasi
        currentNodeId = finalDestinationNode;
        RenderCurrentNode();
    }

    private void TerapkanEfekOpsi(int guanxiDelta, int phDelta, int mhDelta)
    {
        if (guanxiDelta != 0 && currentNpcId != 0 && SocialManager.Instance != null)
        {
            SocialManager.Instance.TambahGuanxi(currentNpcId, guanxiDelta, 10);
        }
        if ((phDelta != 0 || mhDelta != 0) && PlayerStats.Instance != null)
        {
            PlayerStats.Instance.ModifyStats(0, 0, mhDelta, phDelta, 0, 0);
        }
    }

    public void EndDialogue()
    {
        isDialogueActive = false;
        int finishedNodeId = currentNodeId;

        if (DialogueUIController.Instance != null)
        {
            DialogueUIController.Instance.CloseDialoguePanel();
        }

        // 1. Eksekusi Custom Callback jika didaftarkan oleh pemanggil dialog
        if (onDialogueEndCallback != null)
        {
            var callback = onDialogueEndCallback;
            onDialogueEndCallback = null;
            callback.Invoke();
            return;
        }

        // 2. Penanganan paska sesi hangout akhir pekan (Aktivitas Berbiaya Waktu)
        if (isHangoutDialogueActive)
        {
            isHangoutDialogueActive = false;
            currentHangoutNpcId = 0;
            currentHangoutVenueId = 0;
            if (GameManager.Instance != null)
            {
                GameManager.Instance.GeserWaktu();
                Debug.Log("<color=green>[Hangout Selesai]</color> Waktu bergeser 1 blok paska interaksi hangout.");
            }
            return;
        }

        // 3. Penanganan paska interupsi paksa (Ledakan Rumor 3001)
        if (finishedNodeId == 3001 && SocialManager.Instance != null && SocialManager.Instance.globalRumorLevel >= 3)
        {
            SocialManager.Instance.globalRumorLevel = 2;
        }

        // 4. Penanganan interupsi pagi tanpa biaya waktu (Peringatan Edelweiss 2001 / Sapaan / Ledakan Rumor 3001)
        // Melanjutkan jadwal normal hari itu (pagi) tanpa memotong blok waktu
        if (GameManager.Instance != null && GameManager.Instance.currentTimeBlock == TimeBlock.Pagi && (finishedNodeId == 2001 || finishedNodeId == 3001))
        {
            Debug.Log("<color=cyan>[Interupsi Selesai]</color> Melanjutkan rutinitas pagi normal tanpa memotong blok waktu.");
            GameManager.Instance.LanjutRutinitasPagi();
        }

        Debug.Log("<color=grey>[Dialog]</color> Interaksi dialog selesai.");
    }
}
