using System;
using System.Data;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NPCRelationData
{
    public int npcId;
    public string npcName;
    public int baseDecayRate;
    public int guanxiScore;
    public int lonelinessMeter;
    public int rumorContribution;
    public int daysSinceLastInteract;
    public bool interactedToday;
}

public class SocialManager : MonoBehaviour
{
    private static SocialManager _instance;
    public static SocialManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<SocialManager>();
            }
            return _instance;
        }
        private set { _instance = value; }
    }

    [Header("Identitas Sesi")]
    public int playerId = 1;

    [Header("Status Rumor Global")]
    public int globalRumorLevel = 0; // 0: Aman, 1: Bisik-bisik, 2: Menyebar, 3: Ledakan

    [Header("Cache Relasi NPC (Runtime)")]
    public List<NPCRelationData> relations = new List<NPCRelationData>();

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else if (_instance != this)
        {
            Destroy(this);
        }
    }

    void Start()
    {
        MuatRelasiDariDatabase();
    }

    // Memuat seluruh data relasi NPC dan nilai dasar decay dari SQLite
    public void MuatRelasiDariDatabase()
    {
        relations.Clear();

        string query = "SELECT r.npc_id, n.npc_name, n.base_decay_rate, r.guanxi_score, " +
                       "r.loneliness_meter, r.rumor_contribution, r.days_since_last_interaction " +
                       "FROM tbl_npc_relations r " +
                       "JOIN tbl_npc_list n ON r.npc_id = n.npc_id " +
                       $"WHERE r.player_id = {playerId};";

        DataTable dt = DatabaseManager.Instance.ExecuteQuery(query);

        if (dt != null && dt.Rows.Count > 0)
        {
            foreach (DataRow row in dt.Rows)
            {
                NPCRelationData data = new NPCRelationData
                {
                    npcId = Convert.ToInt32(row["npc_id"]),
                    npcName = row["npc_name"].ToString(),
                    baseDecayRate = Convert.ToInt32(row["base_decay_rate"]),
                    guanxiScore = Convert.ToInt32(row["guanxi_score"]),
                    lonelinessMeter = Convert.ToInt32(row["loneliness_meter"]),
                    rumorContribution = Convert.ToInt32(row["rumor_contribution"]),
                    daysSinceLastInteract = Convert.ToInt32(row["days_since_last_interaction"]),
                    interactedToday = false
                };
                relations.Add(data);
            }
            Debug.Log($"<color=cyan>[SocialManager]</color> Berhasil memuat {relations.Count} data relasi NPC.");
        }
    }

    // Dipanggil oleh GameManager saat Devano tidur di akhir hari
    public void ProsesAkhirHari()
    {
        Debug.Log("<color=purple>[SocialManager] Mengeksekusi kalkulasi sosial akhir hari...</color>");

        bool triggerLevel3Explosion = false;
        int highestRumorFound = 0;

        foreach (var rel in relations)
        {
            if (!rel.interactedToday)
            {
                // Persamaan 3.2: Loneliness bertambah sesuai laju pembusukan dasar (R)
                rel.daysSinceLastInteract++;
                rel.lonelinessMeter = Mathf.Clamp(rel.lonelinessMeter + rel.baseDecayRate, 0, 100);

                // Tabel 3.5: Jika Loneliness >= 80, Rumor Contribution bertambah +5 per hari
                if (rel.lonelinessMeter >= 80)
                {
                    rel.rumorContribution = Mathf.Clamp(rel.rumorContribution + 5, 0, 100);
                }
            }
            else
            {
                // Reset flag harian jika ada interaksi hari ini
                rel.daysSinceLastInteract = 0;
                rel.interactedToday = false;
            }

            // Evaluasi Tingkat Rumor Individu
            if (rel.rumorContribution >= 100 || rel.lonelinessMeter >= 100)
            {
                triggerLevel3Explosion = true;
            }
            else if (rel.lonelinessMeter >= 80)
            {
                highestRumorFound = Mathf.Max(highestRumorFound, 2);
            }
            else if (rel.lonelinessMeter >= 50)
            {
                highestRumorFound = Mathf.Max(highestRumorFound, 1);
            }

            // Simpan perubahan individu ke database
            SimpanRelasiKeDatabase(rel);
        }

        // Tentukan Global Rumor Level
        if (triggerLevel3Explosion)
        {
            globalRumorLevel = 3;
            EksekusiLedakanRumor();
        }
        else
        {
            globalRumorLevel = highestRumorFound;
            EvaluasiEfekRumorHarian();
        }

        // Simpan status rumor global ke tbl_player_profile
        string qUpdateProfile = $"UPDATE tbl_player_profile SET global_rumor_level = {globalRumorLevel} WHERE player_id = {playerId};";
        DatabaseManager.Instance.ExecuteNonQuery(qUpdateProfile);

        Debug.Log($"<color=yellow>[SocialManager Update]</color> Global Rumor Level saat ini: {globalRumorLevel}");
    }

    // Efek pasif harian Rumor Level 1 & Level 2 (Tabel 3.6)
    private void EvaluasiEfekRumorHarian()
    {
        if (globalRumorLevel == 1)
        {
            Debug.Log("<color=yellow>[Rumor Level 1]</color> Kamu merasa ada yang membicarakanmu di belakang... (Peringatan Edelweiss aktif)");
        }
        else if (globalRumorLevel == 2)
        {
            Debug.Log("<color=orange>[Rumor Level 2]</color> Rumor menyebar! Beberapa mahasiswa mulai menghindarimu saat makan siang.");
            // Penalti pasif: Guanxi semua NPC turun -1, PH & MH turun ekstra -1
            foreach (var rel in relations)
            {
                rel.guanxiScore = Mathf.Max(0, rel.guanxiScore - 1);
                SimpanRelasiKeDatabase(rel);
            }
            PlayerStats.Instance.ModifyStats(0, 0, -1, -1, 0, 0);
        }
    }

    // Efek fatal Rumor Level 3: Ledakan Rumor (Tabel 3.6)
    private void EksekusiLedakanRumor()
    {
        Debug.LogWarning("<color=red>[CRITICAL EVENT - RUMOR LEVEL 3]</color> LEDAKAN RUMOR TERJADI!");
        Debug.LogWarning("Dosen Xiang Bai memanggil Devano ke ruangannya. Semua Guanxi turun -15!");

        foreach (var rel in relations)
        {
            rel.guanxiScore = Mathf.Max(0, rel.guanxiScore - 15);
            SimpanRelasiKeDatabase(rel);
        }

        // Penalti panggilan dosen (MH -10)
        PlayerStats.Instance.ModifyStats(0, 0, -10, 0, 0, 0);
    }

    // Aksi Reduksi Rumor (Tabel 3.7)
    public void DefuseRumor(int npcId, int nilaiDefuse)
    {
        NPCRelationData rel = relations.Find(x => x.npcId == npcId);
        if (rel != null)
        {
            rel.lonelinessMeter = Mathf.Max(0, rel.lonelinessMeter - nilaiDefuse);
            rel.rumorContribution = Mathf.Max(0, rel.rumorContribution - (nilaiDefuse / 2));
            rel.interactedToday = true;

            SimpanRelasiKeDatabase(rel);
            Debug.Log($"<color=green>[Defuse Sukses]</color> Interaksi dengan {rel.npcName} berhasil. Loneliness berkurang -{nilaiDefuse}.");
        }
    }

    // Aksi Interaksi Positif (Makan bareng / Bantu tugas)
    public void TambahGuanxi(int npcId, int penambahanGuanxi, int reduksiLoneliness)
    {
        NPCRelationData rel = relations.Find(x => x.npcId == npcId);
        if (rel != null)
        {
            rel.guanxiScore = Mathf.Clamp(rel.guanxiScore + penambahanGuanxi, 0, 100);
            rel.lonelinessMeter = Mathf.Max(0, rel.lonelinessMeter - reduksiLoneliness);
            rel.interactedToday = true;

            SimpanRelasiKeDatabase(rel);
            Debug.Log($"<color=green>[Guanxi Naik]</color> {rel.npcName}: Guanxi +{penambahanGuanxi} (Total: {rel.guanxiScore})");
        }
    }

    // Menambah rumor contribution ke seluruh relasi (misal saat peristiwa kritis/gagal ujian)
    public void TambahRumor(int penambahanRumor)
    {
        foreach (var rel in relations)
        {
            rel.rumorContribution = Mathf.Clamp(rel.rumorContribution + penambahanRumor, 0, 100);
            SimpanRelasiKeDatabase(rel);
        }
        Debug.LogWarning($"<color=yellow>[Rumor Meningkat]</color> Kontribusi rumor semua relasi bertambah +{penambahanRumor}.");
    }

    private void SimpanRelasiKeDatabase(NPCRelationData rel)
    {
        string query = $"UPDATE tbl_npc_relations SET " +
                       $"guanxi_score = {rel.guanxiScore}, " +
                       $"loneliness_meter = {rel.lonelinessMeter}, " +
                       $"rumor_contribution = {rel.rumorContribution}, " +
                       $"days_since_last_interaction = {rel.daysSinceLastInteract} " +
                       $"WHERE player_id = {playerId} AND npc_id = {rel.npcId};";

        DatabaseManager.Instance.ExecuteNonQuery(query);
    }
}