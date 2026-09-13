using System;
using System.Data;
using System.Collections.Generic;
using UnityEngine;

public class MorningGreetingManager : MonoBehaviour
{
    private static MorningGreetingManager _instance;
    public static MorningGreetingManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<MorningGreetingManager>();
                if (_instance == null)
                {
                    GameObject core = GameObject.Find("GAME_CORE");
                    if (core != null)
                    {
                        _instance = core.AddComponent<MorningGreetingManager>();
                    }
                    else
                    {
                        GameObject go = new GameObject("[MorningGreetingManager]");
                        _instance = go.AddComponent<MorningGreetingManager>();
                        DontDestroyOnLoad(go);
                    }
                }
            }
            return _instance;
        }
        private set => _instance = value;
    }

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

    /// <summary>
    /// Mengevaluasi apakah ada NPC dengan relasi tinggi (Guanxi >= 60 atau Affection State >= 2)
    /// yang akan menyapa Devano di pagi hari sebelum jadwal kelas/otonomi dimulai.
    /// </summary>
    /// <returns>True jika sapaan pagi berhasil dipicu, False jika tidak ada kandidat.</returns>
    public bool TryTriggerMorningGreeting()
    {
        if (SocialManager.Instance == null || SocialManager.Instance.relations == null || SocialManager.Instance.relations.Count == 0)
        {
            return false;
        }

        // 1. Filter NPC yang memenuhi kriteria kedekatan (Guanxi >= 60 atau Affection State >= 2)
        List<NPCRelationData> candidates = new List<NPCRelationData>();
        foreach (var rel in SocialManager.Instance.relations)
        {
            if (rel.guanxiScore >= 60 || rel.affectionState >= 2)
            {
                candidates.Add(rel);
            }
        }

        if (candidates.Count == 0)
        {
            return false;
        }

        // 2. Pilih NPC dengan Guanxi tertinggi (acak jika ada nilai seimbang di tingkat teratas)
        candidates.Sort((a, b) => b.guanxiScore.CompareTo(a.guanxiScore));
        int highestGuanxi = candidates[0].guanxiScore;
        List<NPCRelationData> topCandidates = candidates.FindAll(c => c.guanxiScore == highestGuanxi);
        NPCRelationData selectedNpc = topCandidates[UnityEngine.Random.Range(0, topCandidates.Count)];

        // Pastikan affectionState selalu mutakhir
        SocialManager.Instance.EvaluasiAffectionState(selectedNpc);

        // 3. Kueri dialog sapaan dari tbl_morning_greetings berdasarkan npc_id dan batas minimum affection state
        string query = $"SELECT greeting_id, min_affection_state, greeting_text, bonus_mh, bonus_guanxi " +
                       $"FROM tbl_morning_greetings " +
                       $"WHERE npc_id = {selectedNpc.npcId} AND min_affection_state <= {selectedNpc.affectionState} " +
                       $"ORDER BY min_affection_state DESC;";

        DataTable dt = DatabaseManager.Instance.ExecuteQuery(query);
        if (dt == null || dt.Rows.Count == 0)
        {
            Debug.LogWarning($"<color=orange>[MorningGreetingManager]</color> Tidak ada baris sapaan di SQLite untuk {selectedNpc.npcName} (State {selectedNpc.affectionState}).");
            return false;
        }

        // Ambil baris dengan min_affection_state tertinggi yang dapat diakses
        int maxAffFound = Convert.ToInt32(dt.Rows[0]["min_affection_state"]);
        List<DataRow> matchingRows = new List<DataRow>();
        foreach (DataRow row in dt.Rows)
        {
            if (Convert.ToInt32(row["min_affection_state"]) == maxAffFound)
            {
                matchingRows.Add(row);
            }
        }
        DataRow chosenRow = matchingRows[UnityEngine.Random.Range(0, matchingRows.Count)];

        string greetingText = chosenRow["greeting_text"].ToString();
        int bonusMh = Convert.ToInt32(chosenRow["bonus_mh"]);
        int bonusGuanxi = Convert.ToInt32(chosenRow["bonus_guanxi"]);

        Debug.Log($"<color=green>[Dynamic Greeting]</color> {selectedNpc.npcName} (Guanxi: {selectedNpc.guanxiScore}, State: {selectedNpc.affectionState}) menyapa Devano di pagi hari!");

        // 4. Tampilkan dialog interaktif melalui DialogueUIController
        if (DialogueUIController.Instance != null)
        {
            DialogueUIController.Instance.DisplayDialogue(selectedNpc.npcName, greetingText);
            DialogueUIController.Instance.CreateCustomActionButton("Sapa balik dengan senyuman", () =>
            {
                OnGreetingAcknowledged(selectedNpc.npcId, selectedNpc.npcName, bonusMh, bonusGuanxi);
            });
        }

        return true;
    }

    /// <summary>
    /// Callback saat pemain menanggapi sapaan ramah di pagi hari.
    /// Memberikan bonus Mental Health, Guanxi, reduksi Loneliness, mencatat telemetri, dan melanjutkan hari.
    /// </summary>
    public void OnGreetingAcknowledged(int npcId, string npcName, int bonusMh, int bonusGuanxi)
    {
        // 1. Berikan efek pemulihan mental health
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.ModifyStats(dLanguage: 0, dEtiquette: 0, dMental: bonusMh, dPhysical: 0, dTheoretical: 0, dPractical: 0);
        }

        // 2. Berikan penambahan Guanxi, reduksi Loneliness (-15), dan tandai interaksi hari ini
        if (SocialManager.Instance != null && SocialManager.Instance.relations != null)
        {
            var rel = SocialManager.Instance.relations.Find(x => x.npcId == npcId);
            if (rel != null) rel.interactedToday = true;

            SocialManager.Instance.TambahGuanxi(npcId, penambahanGuanxi: bonusGuanxi, reduksiLoneliness: 15);
        }

        // 3. Catat telemetri
        if (TelemetryLogger.Instance != null)
        {
            TelemetryLogger.Instance.RecordCriticalEvent("DYNAMIC_GREETING", $"Greeting dari {npcName} (Bonus MH: +{bonusMh}, Guanxi: +{bonusGuanxi}, Loneliness: -15)");
        }

        Debug.Log($"<color=cyan>[Dynamic Greeting]</color> Devano menyapa balik {npcName}. Mendapat buff MH +{bonusMh}, Guanxi +{bonusGuanxi}, Loneliness -15.");

        // 4. Tutup panel dialog
        if (DialogueUIController.Instance != null)
        {
            DialogueUIController.Instance.CloseDialoguePanel();
        }

        // 5. Lanjutkan jadwal pagi (Kuliah Wajib pada hari kerja / Otonomi pemain pada akhir pekan)
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LanjutRutinitasPagi();
        }
    }
}
