using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;

public enum PlayerArchetype
{
    PureAcademic,   // Fokus belajar, mengabaikan sosial (Menguji Ledakan Rumor & Burnout)
    PureSocial,     // Nongkrong terus, abai belajar (Menguji Probation Akademik)
    Balanced        // Menjaga ritme belajar, sosialisasi, dan istirahat
}

public class BalancingSimulator : MonoBehaviour
{
    private static BalancingSimulator _instance;
    public static BalancingSimulator Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<BalancingSimulator>();
                if (_instance == null)
                {
                    GameObject core = GameObject.Find("GAME_CORE");
                    if (core != null)
                    {
                        _instance = core.AddComponent<BalancingSimulator>();
                    }
                    else
                    {
                        GameObject go = new GameObject("[BalancingSimulator]");
                        _instance = go.AddComponent<BalancingSimulator>();
                        DontDestroyOnLoad(go);
                    }
                }
            }
            return _instance;
        }
        private set => _instance = value;
    }

    [Header("Parameter Simulasi")]
    public int totalSimulationDays = 60;
    public bool isSimulating = false;

    void Awake()
    {
        Application.runInBackground = true;
        if (_instance == null) _instance = this;
        else if (_instance != this) Destroy(this);
    }

    // Menjalankan simulasi kilat berdasarkan arketipe perilaku pemain
    public void JalankanSimulasi(PlayerArchetype archetype, bool instant = false)
    {
        if (isSimulating)
        {
            Debug.LogWarning("[BalancingSimulator] Simulasi sedang berjalan. Tunggu hingga selesai.");
            return;
        }
        StartCoroutine(RoutineSimulasi(archetype, instant));
    }

    private IEnumerator RoutineSimulasi(PlayerArchetype archetype, bool instant = false)
    {
        isSimulating = true;
        Debug.Log($"<color=magenta>=== MEMULAI HEADLESS SIMULATION: {archetype} (1 - {totalSimulationDays} HARI) ===</color>");

        // 1. Reset status ke kondisi awal Devano
        PlayerStats.Instance.physicalHealth = 100;
        PlayerStats.Instance.mentalHealth = 80;
        PlayerStats.Instance.languageProficiency = 20;
        PlayerStats.Instance.culturalEtiquette = 15;
        PlayerStats.Instance.academicTheoretical = 30;
        PlayerStats.Instance.academicPractical = 40;
        PlayerStats.Instance.isBurnedOut = false;
        PlayerStats.Instance.SaveStatsToDatabase();

        GameManager.Instance.currentDay = 1;
        GameManager.Instance.currentTimeBlock = TimeBlock.Pagi;
        SocialManager.Instance.globalRumorLevel = 0;

        // Reset relasi NPC ke default
        foreach (var rel in SocialManager.Instance.relations)
        {
            rel.guanxiScore = 20;
            rel.lonelinessMeter = 0;
            rel.rumorContribution = 0;
            rel.daysSinceLastInteract = 0;
            rel.interactedToday = false;
        }

        // 2. Loop simulasi per hari
        for (int day = 1; day <= totalSimulationDays; day++)
        {
            GameManager.Instance.currentDay = day;

            // Blok Pagi: Jika hari kerja, kelas wajib dieksekusi (kecuali PureSocial yang bolos demi nongkrong)
            if (GameManager.Instance.IsWorkday())
            {
                if (archetype != PlayerArchetype.PureSocial)
                {
                    PlayerStats.Instance.ModifyStats(0, 0, -20, -15, 10, 5);
                }
                else
                {
                    // PureSocial membolos kelas pagi (abai belajar) demi istirahat santai
                    PlayerStats.Instance.ModifyStats(0, 0, 5, 0, 0, 0);
                }
            }

            // Blok Siang & Malam: Eksekusi keputusan berdasarkan pola arketipe
            EksekusiPerilakuArketipe(archetype, TimeBlock.Siang);
            EksekusiPerilakuArketipe(archetype, TimeBlock.Malam);

            // Blok Evaluasi Tengah Semester (Hari 30)
            if (day == 30)
            {
                bool pass = (PlayerStats.Instance.academicTheoretical >= GameManager.PASS_THEORETICAL) &&
                            (PlayerStats.Instance.academicPractical >= GameManager.PASS_PRACTICAL);

                if (pass)
                {
                    SocialManager.Instance.TambahGuanxi(101, 20, 20);
                    PlayerStats.Instance.ModifyStats(0, 0, 15, 0, 0, 0);
                    TelemetryLogger.Instance.RecordCriticalEvent("SIM_UTS_PASS", $"Hari 30: Lulus Evaluasi");
                }
                else
                {
                    SocialManager.Instance.TambahGuanxi(101, -20, 0);
                    PlayerStats.Instance.ModifyStats(0, 0, -25, 0, 0, 0);
                    TelemetryLogger.Instance.RecordCriticalEvent("SIM_UTS_FAIL", $"Hari 30: Kena Probation");
                }
            }

            // Akhir Hari: Snapshot telemetri & kalkulasi sosial
            TelemetryLogger.Instance.RecordDailySnapshot($"Simulasi Archetype: {archetype}");
            SocialManager.Instance.ProsesAkhirHari();

            // Pemulihan Tidur Malam
            if (PlayerStats.Instance.isBurnedOut)
            {
                PlayerStats.Instance.isBurnedOut = false;
                PlayerStats.Instance.ModifyStats(0, -5, 50, 50, -5, 0);
            }
            else
            {
                PlayerStats.Instance.ModifyStats(0, 0, 40, 40, 0, 0);
            }

            // Yield frame minimal jika bukan instant mode
            if (!instant)
            {
                yield return null;
            }
        }

        Debug.Log($"<color=green>=== SIMULASI SELESAI ===</color> Seluruh metrik tersimpan di database.");
        TelemetryLogger.Instance.ExportLogsToCSV();
        isSimulating = false;
    }

    private void EksekusiPerilakuArketipe(PlayerArchetype archetype, TimeBlock block)
    {
        if (PlayerStats.Instance.isBurnedOut) return;

        switch (archetype)
        {
            case PlayerArchetype.PureAcademic:
                // Hanya belajar bahasa & materi teori, tidak pernah bersosialisasi
                PlayerStats.Instance.ModifyStats(dLanguage: 10, dEtiquette: 0, dMental: -10, dPhysical: -5, dTheoretical: 5, dPractical: 0);
                break;

            case PlayerArchetype.PureSocial:
                // Selalu nongkrong & mengobrol dengan NPC acak
                if (SocialManager.Instance.relations.Count > 0)
                {
                    int targetNpc = SocialManager.Instance.relations[Random.Range(0, SocialManager.Instance.relations.Count)].npcId;
                    SocialManager.Instance.TambahGuanxi(targetNpc, 10, 25);
                }
                PlayerStats.Instance.ModifyStats(dLanguage: 0, dEtiquette: 5, dMental: 5, dPhysical: -5, dTheoretical: 0, dPractical: 0);
                break;

            case PlayerArchetype.Balanced:
                // Siang interaksi sosial terdistribusi (merawat NPC dengan loneliness tertinggi agar tidak memicu rumor), malam belajar mandiri
                if (block == TimeBlock.Siang)
                {
                    int targetNpc = 102; // Default Li Haoran
                    int maxLoneliness = -1;
                    if (SocialManager.Instance.relations.Count > 0)
                    {
                        foreach (var r in SocialManager.Instance.relations)
                        {
                            if (r.lonelinessMeter > maxLoneliness)
                            {
                                maxLoneliness = r.lonelinessMeter;
                                targetNpc = r.npcId;
                            }
                        }
                    }
                    SocialManager.Instance.TambahGuanxi(targetNpc, 10, 20);
                    PlayerStats.Instance.ModifyStats(0, 5, 5, -5, 0, 0);
                }
                else
                {
                    PlayerStats.Instance.ModifyStats(5, 0, -5, -5, 5, 5); // Belajar malam
                }
                break;
        }
    }
}
