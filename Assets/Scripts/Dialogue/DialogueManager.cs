using System;
using System.Data;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("State Dialog Aktif")]
    public int currentNodeId;
    public bool isDialogueActive = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Memulai interaksi dialog berdasarkan Node ID
    public void StartDialogue(int startingNodeId)
    {
        isDialogueActive = true;
        currentNodeId = startingNodeId;
        RenderCurrentNode();
    }

    private void RenderCurrentNode()
    {
        string query = $"SELECT speaker_name, dialogue_text FROM tbl_dialogue_nodes WHERE node_id = {currentNodeId};";
        DataTable dt = DatabaseManager.Instance.ExecuteQuery(query);

        if (dt != null && dt.Rows.Count > 0)
        {
            string speaker = dt.Rows[0]["speaker_name"].ToString();
            string text = dt.Rows[0]["dialogue_text"].ToString();

            Debug.Log($"<color=cyan>[DIALOG]</color> <b>{speaker}</b>: \"{text}\"");

            // Tampilkan ke antarmuka visual
            if (DialogueUIController.Instance != null)
            {
                DialogueUIController.Instance.DisplayDialogue(speaker, text);
            }

            LoadDialogueOptions(currentNodeId);
        }
    }

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

                Debug.Log($"<color=white>[PILIHAN RESPON]</color> Opsi {optId}: {optText}");

                if (DialogueUIController.Instance != null)
                {
                    DialogueUIController.Instance.CreateOptionButton(optId, optText);
                }
            }
        }
        else
        {
            Debug.Log("<color=grey>[Dialog Selesai]</color> Tekan aksi berikutnya.");
            isDialogueActive = false;

            // Jika tidak ada opsi lanjutan (akhir percabangan), tampilkan tombol selesai
            if (DialogueUIController.Instance != null)
            {
                DialogueUIController.Instance.ShowCloseButton();
            }
        }
    }

    // Eksekusi Pilihan Respon dengan Hierarchical Stat-Checking (Gambar 3.5)
    public void SelectOption(int optionId)
    {
        // 1. Respon Laporan Progres Dosen (Stat Check)
        if (currentNodeId == 1001 && optionId == 1)
        {
            EvaluasiPercabanganLaporanDosen();
            return;
        }

        // 2. Respon Peringatan Edelweiss (Node 2001 -> Opsi 201)
        if (currentNodeId == 2001 && optionId == 201)
        {
            SocialManager.Instance.TambahGuanxi(npcId: 104, penambahanGuanxi: 5, reduksiLoneliness: 15);
            if (DialogueUIController.Instance != null)
            {
                DialogueUIController.Instance.CloseDialoguePanel();
            }

            // Jika ini terjadi di pagi hari kerja, lanjutkan ke kelas wajib setelah interupsi
            if (GameManager.Instance.IsWorkday() && GameManager.Instance.currentTimeBlock == TimeBlock.Pagi)
            {
                GameManager.Instance.MulaiHari();
            }
            return;
        }

        // 3. Respon Teguran Ledakan Rumor Dosen Xiang Bai (Node 3001 -> Opsi 301)
        if (currentNodeId == 3001 && optionId == 301)
        {
            if (DialogueUIController.Instance != null)
            {
                DialogueUIController.Instance.CloseDialoguePanel();
            }
            
            // Reduksi sedikit tensi setelah teguran, turunkan rumor level ke 2
            SocialManager.Instance.globalRumorLevel = 2;
            
            // Lanjutkan siklus hari
            if (GameManager.Instance.IsWorkday() && GameManager.Instance.currentTimeBlock == TimeBlock.Pagi)
            {
                GameManager.Instance.MulaiHari();
            }
            return;
        }
    }

    private void EvaluasiPercabanganLaporanDosen()
    {
        int bahasa = PlayerStats.Instance.languageProficiency;
        int etika = PlayerStats.Instance.culturalEtiquette;

        Debug.Log($"<color=yellow>[Evaluasi Stat Gerbang]</color> Bahasa: {bahasa}/30, Etika: {etika}/50");

        // Evaluasi Gerbang 1: Kemampuan Bahasa (Language Proficiency >= 30)
        if (bahasa < 30)
        {
            // Rute C: Kegagalan Linguistik
            currentNodeId = 1004;
            SocialManager.Instance.TambahGuanxi(npcId: 101, penambahanGuanxi: -5, reduksiLoneliness: 10);
            PlayerStats.Instance.ModifyStats(dLanguage: 0, dEtiquette: 0, dMental: -10, dPhysical: 0, dTheoretical: 0, dPractical: 0);
        }
        else
        {
            // Evaluasi Gerbang 2: Etika Budaya (Cultural Etiquette >= 50)
            if (etika < 50)
            {
                // Rute B: Pelanggaran Mianzi
                currentNodeId = 1003;
                SocialManager.Instance.TambahGuanxi(npcId: 101, penambahanGuanxi: -20, reduksiLoneliness: 10);
                
                // Tambah Rumor Contribution +10 karena mempermalukan dosen di depan umum
                var rel = SocialManager.Instance.relations.Find(x => x.npcId == 101);
                if (rel != null) rel.rumorContribution = Mathf.Clamp(rel.rumorContribution + 10, 0, 100);

                Debug.LogWarning("<color=red>[PENALTI MIANZI]</color> Kamu mempermalukan Dosen Xiang Bai!");
            }
            else
            {
                // Rute A: Sukses Sosio-Kultural
                currentNodeId = 1002;
                SocialManager.Instance.TambahGuanxi(npcId: 101, penambahanGuanxi: 15, reduksiLoneliness: 30);
                PlayerStats.Instance.ModifyStats(dLanguage: 0, dEtiquette: 0, dMental: 5, dPhysical: 0, dTheoretical: 5, dPractical: 5);
            }
        }

        RenderCurrentNode();
    }
}
