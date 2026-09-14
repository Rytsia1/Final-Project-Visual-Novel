#if UNITY_EDITOR
using System;
using System.IO;
using System.Text;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public enum SimulationArchetype
{
    StudyHeavy,
    SocialHeavy,
    Balanced,
    RestHeavy
}

public enum SimulatorProfileSelection
{
    StudyHeavy = 0,
    SocialHeavy = 1,
    Balanced = 2,
    RestHeavy = 3,
    BatchAll = 4
}

public struct DailySimulationRecord
{
    public string archetype;
    public int day;
    public string dayName;
    public int physicalHealth;
    public int mentalHealth;
    public int languageProficiency;
    public int culturalEtiquette;
    public int academicTheoretical;
    public int academicPractical;
    public float averageGuanxi;
    public int maxLoneliness;
    public int globalRumorLevel;
    public bool isBurnedOut;
}

public class ArchetypeSimulationResult
{
    public SimulationArchetype archetype;
    public string archetypeName;
    public List<DailySimulationRecord> records = new List<DailySimulationRecord>();
    public int totalBurnoutDays;
    public int peakRumorLevel;
    public int peakLoneliness;
    public DailySimulationRecord utsRecord; // Day 30
    public DailySimulationRecord uasRecord; // Day 60
    public bool utsPassed;
    public string uasVerdict;

    // Final NPC details
    public int[] finalGuanxi = new int[4];
    public int[] finalLoneliness = new int[4];
}

public class AutomatedBalancingSimulator : EditorWindow
{
    private static readonly string[] NPC_NAMES = { "Dosen Xiang Bai", "Li Haoran", "Yang Mei", "Chen Feng" };
    private static readonly string[] DAY_NAMES = { "Senin", "Selasa", "Rabu", "Kamis", "Jumat", "Sabtu", "Minggu" };

    private SimulatorProfileSelection selectedProfile = SimulatorProfileSelection.BatchAll;
    private int selectedTab = 0;
    private Vector2 scrollPos;
    private Vector2 tableScrollPos;
    private string statusMessage = "";
    private MessageType statusMessageType = MessageType.Info;

    private Dictionary<SimulationArchetype, ArchetypeSimulationResult> simulationResults = new Dictionary<SimulationArchetype, ArchetypeSimulationResult>();
    private bool hasSimulated = false;

    [MenuItem("Tokimeki TA/Headless Balancing Simulator")]
    [MenuItem("Game Database/Headless Balancing Simulator")]
    public static void ShowWindow()
    {
        AutomatedBalancingSimulator window = GetWindow<AutomatedBalancingSimulator>("Headless Balancing Simulator");
        window.minSize = new Vector2(850f, 620f);
        window.Show();
    }

    private void OnGUI()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        DrawHeader();
        EditorGUILayout.Space(8);

        DrawControlBar();
        EditorGUILayout.Space(8);

        if (!string.IsNullOrEmpty(statusMessage))
        {
            EditorGUILayout.HelpBox(statusMessage, statusMessageType);
            EditorGUILayout.Space(6);
        }

        if (hasSimulated && simulationResults.Count > 0)
        {
            DrawSimulationResults();
        }
        else
        {
            DrawWelcomePlaceholder();
        }

        EditorGUILayout.EndScrollView();
    }

    // =========================================================================
    // GUI COMPONENTS
    // =========================================================================

    private void DrawHeader()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 17,
            normal = { textColor = new Color(0.2f, 0.7f, 1f) }
        };
        EditorGUILayout.LabelField("Headless Time Management Simulator & Dataset Exporter", titleStyle);
        EditorGUILayout.LabelField(
            "Simulasi komputasional 60 hari tanpa rendering UI untuk evaluasi balancing 4 arketipe perilaku Devano (Bab 4 Skripsi).",
            EditorStyles.wordWrappedMiniLabel
        );
        EditorGUILayout.EndVertical();
    }

    private void DrawControlBar()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Pilih Profil Simulasi:", EditorStyles.boldLabel, GUILayout.Width(150));

        string[] profileOptions = {
            "Simulasi A: Study-Heavy (Fokus Akademik & Bahasa)",
            "Simulasi B: Social-Heavy (Fokus Nongkrong & Guanxi)",
            "Simulasi C: Balanced (Adaptasi Seimbang)",
            "Simulasi D: Rest-Heavy (Fokus Pemulihan & Tidur)",
            "Jalankan Semua Arketipe (Batch All 4 Archetypes)"
        };

        selectedProfile = (SimulatorProfileSelection)EditorGUILayout.Popup((int)selectedProfile, profileOptions);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(6);

        EditorGUILayout.BeginHorizontal();

        GUI.backgroundColor = new Color(0.2f, 0.75f, 0.4f);
        if (GUILayout.Button("▶ Execute Simulation (Fast-Forward 60 Days)", GUILayout.Height(34)))
        {
            ExecuteSimulation();
        }

        GUI.backgroundColor = new Color(0.18f, 0.55f, 0.85f);
        GUI.enabled = hasSimulated && simulationResults.Count > 0;
        if (GUILayout.Button("📊 Export Dataset to CSV", GUILayout.Height(34), GUILayout.Width(220)))
        {
            ExportToCSV();
        }
        GUI.enabled = true;
        GUI.backgroundColor = Color.white;

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
    }

    private void DrawWelcomePlaceholder()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.Space(15);
        EditorGUILayout.LabelField("Belum Ada Data Simulasi", EditorStyles.boldLabel);
        EditorGUILayout.LabelField(
            "Pilih profil simulasi di atas dan klik tombol 'Execute Simulation' untuk menjalankan siklus 60 hari secara headless. " +
            "Sistem akan mengevaluasi seluruh decision tree per blok waktu (Pagi, Siang, Malam, Night Cycle) dan menghasilkan matriks akademik serta relasi sosial.",
            EditorStyles.wordWrappedLabel
        );
        EditorGUILayout.Space(15);
        EditorGUILayout.EndVertical();
    }

    private void DrawSimulationResults()
    {
        // Tab Navigation
        List<SimulationArchetype> activeKeys = simulationResults.Keys.ToList();
        List<string> tabTitles = new List<string>();

        foreach (var key in activeKeys)
        {
            tabTitles.Add(GetArchetypeName(key));
        }

        if (activeKeys.Count > 1)
        {
            tabTitles.Add("Perbandingan Metrik (All)");
        }

        selectedTab = Mathf.Clamp(selectedTab, 0, tabTitles.Count - 1);
        selectedTab = GUILayout.Toolbar(selectedTab, tabTitles.ToArray(), GUILayout.Height(28));
        EditorGUILayout.Space(8);

        if (activeKeys.Count > 1 && selectedTab == tabTitles.Count - 1)
        {
            DrawComparisonOverview();
        }
        else
        {
            SimulationArchetype currentArchetype = activeKeys[selectedTab];
            DrawArchetypeDetail(simulationResults[currentArchetype]);
        }
    }

    private void DrawComparisonOverview()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Tabel Perbandingan 4 Arketipe Perilaku Devano (Hari 60)", EditorStyles.boldLabel);
        EditorGUILayout.Space(4);

        DrawTableRow(true, "Arketipe", "UTS (H30)", "UAS (H60)", "Teori/Prac", "Bahasa", "Avg Guanxi", "Max Lonely", "Rumor", "Burnout");

        foreach (var kvp in simulationResults)
        {
            var res = kvp.Value;
            string utsStr = res.utsPassed ? "LULUS" : "PROBATION";
            string acadStr = $"{res.uasRecord.academicTheoretical} / {res.uasRecord.academicPractical}";
            string burnStr = $"{res.totalBurnoutDays} hari";

            DrawTableRow(false,
                res.archetypeName,
                utsStr,
                res.uasVerdict,
                acadStr,
                res.uasRecord.languageProficiency.ToString(),
                res.uasRecord.averageGuanxi.ToString("F1", CultureInfo.InvariantCulture),
                res.peakLoneliness.ToString(),
                res.peakRumorLevel.ToString(),
                burnStr
            );
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawArchetypeDetail(ArchetypeSimulationResult result)
    {
        // 1. Executive Summary Cards
        EditorGUILayout.BeginHorizontal();

        // Card 1: Hasil Akademik & Ujian
        EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(Screen.width * 0.48f));
        EditorGUILayout.LabelField("🎓 Hasil Ujian & Akademik", EditorStyles.boldLabel);
        EditorGUILayout.Space(2);

        EditorGUILayout.LabelField($"UTS (Hari 30): Teori {result.utsRecord.academicTheoretical}/50, Praktis {result.utsRecord.academicPractical}/45", EditorStyles.miniLabel);
        EditorGUILayout.LabelField($"Status UTS: {(result.utsPassed ? "LULUS" : "PROBATION (Penalti)")}", EditorStyles.boldLabel);
        EditorGUILayout.Space(2);

        EditorGUILayout.LabelField($"UAS (Hari 60): Teori {result.uasRecord.academicTheoretical}, Praktis {result.uasRecord.academicPractical}, Bahasa {result.uasRecord.languageProficiency}", EditorStyles.miniLabel);
        EditorGUILayout.LabelField($"Predikat Akhir: {result.uasVerdict}", EditorStyles.boldLabel);
        EditorGUILayout.EndVertical();

        // Card 2: Vitalitas, Burnout & Sosial
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("❤️ Vitalitas & Matriks Sosial", EditorStyles.boldLabel);
        EditorGUILayout.Space(2);

        EditorGUILayout.LabelField($"Vitalitas Akhir: PH {result.uasRecord.physicalHealth}/100, MH {result.uasRecord.mentalHealth}/100", EditorStyles.miniLabel);
        EditorGUILayout.LabelField($"Total Hari Burnout: {result.totalBurnoutDays} Hari ({(result.totalBurnoutDays > 0 ? "Kritis" : "Stabil")})", EditorStyles.boldLabel);
        EditorGUILayout.Space(2);

        EditorGUILayout.LabelField($"Rata-rata Guanxi Akhir: {result.uasRecord.averageGuanxi:F1} / 100", EditorStyles.miniLabel);
        EditorGUILayout.LabelField($"Puncak Loneliness: {result.peakLoneliness} | Puncak Rumor: Level {result.peakRumorLevel}", EditorStyles.boldLabel);
        EditorGUILayout.EndVertical();

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(6);

        // 2. Status Relasi 4 NPC
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("👥 Status Akhir Relasi 4 NPC (Hari 60)", EditorStyles.boldLabel);
        EditorGUILayout.Space(2);

        EditorGUILayout.BeginHorizontal();
        for (int i = 0; i < 4; i++)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField(NPC_NAMES[i], EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Guanxi: {result.finalGuanxi[i]}/100", EditorStyles.miniLabel);
            EditorGUILayout.LabelField($"Loneliness: {result.finalLoneliness[i]}/100", EditorStyles.miniLabel);
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(6);

        // 3. Scrollable Table of 60-Day Snapshots
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField($"📋 Telemetry Snapshot Harian ({result.records.Count} Hari)", EditorStyles.boldLabel);
        EditorGUILayout.Space(4);

        tableScrollPos = EditorGUILayout.BeginScrollView(tableScrollPos, GUILayout.Height(260));

        DrawDataHeader();

        for (int i = 0; i < result.records.Count; i++)
        {
            var rec = result.records[i];
            DrawDataRow(rec);
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    private void DrawDataHeader()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        GUILayout.Label("Hari", EditorStyles.boldLabel, GUILayout.Width(50));
        GUILayout.Label("Nama Hari", EditorStyles.boldLabel, GUILayout.Width(65));
        GUILayout.Label("PH", EditorStyles.boldLabel, GUILayout.Width(45));
        GUILayout.Label("MH", EditorStyles.boldLabel, GUILayout.Width(45));
        GUILayout.Label("Teori", EditorStyles.boldLabel, GUILayout.Width(50));
        GUILayout.Label("Praktis", EditorStyles.boldLabel, GUILayout.Width(50));
        GUILayout.Label("Bahasa", EditorStyles.boldLabel, GUILayout.Width(50));
        GUILayout.Label("Etika", EditorStyles.boldLabel, GUILayout.Width(45));
        GUILayout.Label("Avg Guanxi", EditorStyles.boldLabel, GUILayout.Width(75));
        GUILayout.Label("Max Lonely", EditorStyles.boldLabel, GUILayout.Width(75));
        GUILayout.Label("Rumor", EditorStyles.boldLabel, GUILayout.Width(50));
        GUILayout.Label("Burnout?", EditorStyles.boldLabel, GUILayout.Width(65));
        EditorGUILayout.EndHorizontal();
    }

    private void DrawDataRow(DailySimulationRecord rec)
    {
        GUIStyle rowStyle = new GUIStyle(EditorStyles.miniLabel);
        if (rec.isBurnedOut)
        {
            rowStyle.normal.textColor = new Color(1f, 0.35f, 0.35f);
        }
        else if (rec.globalRumorLevel >= 2)
        {
            rowStyle.normal.textColor = new Color(1f, 0.75f, 0.25f);
        }

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label(rec.day.ToString(), rowStyle, GUILayout.Width(50));
        GUILayout.Label(rec.dayName, rowStyle, GUILayout.Width(65));
        GUILayout.Label(rec.physicalHealth.ToString(), rowStyle, GUILayout.Width(45));
        GUILayout.Label(rec.mentalHealth.ToString(), rowStyle, GUILayout.Width(45));
        GUILayout.Label(rec.academicTheoretical.ToString(), rowStyle, GUILayout.Width(50));
        GUILayout.Label(rec.academicPractical.ToString(), rowStyle, GUILayout.Width(50));
        GUILayout.Label(rec.languageProficiency.ToString(), rowStyle, GUILayout.Width(50));
        GUILayout.Label(rec.culturalEtiquette.ToString(), rowStyle, GUILayout.Width(45));
        GUILayout.Label(rec.averageGuanxi.ToString("F1", CultureInfo.InvariantCulture), rowStyle, GUILayout.Width(75));
        GUILayout.Label(rec.maxLoneliness.ToString(), rowStyle, GUILayout.Width(75));
        GUILayout.Label(rec.globalRumorLevel.ToString(), rowStyle, GUILayout.Width(50));
        GUILayout.Label(rec.isBurnedOut ? "YA" : "TIDAK", rowStyle, GUILayout.Width(65));
        EditorGUILayout.EndHorizontal();
    }

    private void DrawTableRow(bool isHeader, params string[] cols)
    {
        GUIStyle style = isHeader ? EditorStyles.boldLabel : EditorStyles.miniLabel;
        EditorGUILayout.BeginHorizontal();
        for (int i = 0; i < cols.Length; i++)
        {
            float w = (i == 0) ? 140 : 65;
            if (i == 1 || i == 2) w = 90;
            if (i == 4) w = 75;
            GUILayout.Label(cols[i], style, GUILayout.Width(w));
        }
        EditorGUILayout.EndHorizontal();
    }

    // =========================================================================
    // SIMULATION ENGINE LOGIC
    // =========================================================================

    private void ExecuteSimulation()
    {
        simulationResults.Clear();

        if (selectedProfile == SimulatorProfileSelection.BatchAll)
        {
            simulationResults[SimulationArchetype.StudyHeavy] = RunSimulation(SimulationArchetype.StudyHeavy);
            simulationResults[SimulationArchetype.SocialHeavy] = RunSimulation(SimulationArchetype.SocialHeavy);
            simulationResults[SimulationArchetype.Balanced] = RunSimulation(SimulationArchetype.Balanced);
            simulationResults[SimulationArchetype.RestHeavy] = RunSimulation(SimulationArchetype.RestHeavy);
            statusMessage = "Simulasi Batch 4 Arketipe (Total 240 Hari Gameplay) selesai dieksekusi secara headless.";
        }
        else
        {
            SimulationArchetype target = (SimulationArchetype)(int)selectedProfile;
            simulationResults[target] = RunSimulation(target);
            statusMessage = $"Simulasi {GetArchetypeName(target)} (60 Hari) selesai dieksekusi secara headless.";
        }

        hasSimulated = true;
        statusMessageType = MessageType.Info;
    }

    public static ArchetypeSimulationResult RunSimulation(SimulationArchetype archetype)
    {
        ArchetypeSimulationResult result = new ArchetypeSimulationResult
        {
            archetype = archetype,
            archetypeName = GetArchetypeName(archetype)
        };

        // 1. Initial State Initialization
        int simPH = 100;
        int simMH = 80;
        int simLanguage = 20;
        int simEtiquette = 15;
        int simTheory = 30;
        int simPractice = 40;

        int[] guanxi = { 20, 20, 20, 20 };
        int[] loneliness = { 0, 0, 0, 0 };
        bool[] interactedToday = { false, false, false, false };

        int simRumor = 0;
        bool isBurnedOut = false;
        int burnoutCounter = 0;

        void ApplyStat(ref int stat, int delta)
        {
            if (delta > 0 && isBurnedOut)
            {
                delta = Mathf.RoundToInt(delta * 0.5f);
            }
            stat = Mathf.Clamp(stat + delta, 0, 100);
        }

        int totalBurnoutCount = 0;
        int maxLonelinessPeak = 0;
        int maxRumorPeak = 0;

        // 2. 60-Day Computational Loop
        for (int day = 1; day <= 60; day++)
        {
            int dayOfWeek = (day - 1) % 7; // 0 = Senin, 4 = Jumat, 5 = Sabtu, 6 = Minggu
            bool isWorkday = dayOfWeek < 5;
            string dayName = DAY_NAMES[dayOfWeek];

            // Reset interaction tracking at the start of each day
            for (int i = 0; i < 4; i++) interactedToday[i] = false;

            // -------------------------------------------------------------
            // BLOK 1: PAGI
            // -------------------------------------------------------------
            if (isWorkday)
            {
                // Kelas Wajib: Teori +3, Praktik +3, PH -10, MH -5
                ApplyStat(ref simTheory, 3);
                ApplyStat(ref simPractice, 3);
                ApplyStat(ref simPH, -10);
                ApplyStat(ref simMH, -5);
            }
            else
            {
                // Akhir Pekan
                switch (archetype)
                {
                    case SimulationArchetype.StudyHeavy:
                        // Belajar Mandiri: Teori +8, PH -10, MH -10
                        ApplyStat(ref simTheory, 8);
                        ApplyStat(ref simPH, -10);
                        ApplyStat(ref simMH, -10);
                        break;

                    case SimulationArchetype.SocialHeavy:
                        // Outing dengan NPC prioritas (loneliness tertinggi): Guanxi +12, PH -15, MH +10
                        int targetNpc = GetHighestLonelinessNPC(loneliness);
                        ApplyStat(ref guanxi[targetNpc], 12);
                        loneliness[targetNpc] = Mathf.Max(0, loneliness[targetNpc] - 30);
                        ApplyStat(ref simPH, -15);
                        ApplyStat(ref simMH, 10);
                        interactedToday[targetNpc] = true;
                        break;

                    case SimulationArchetype.Balanced:
                        // Outing jika ada NPC dengan Loneliness >= 50, jika tidak maka Belajar
                        int maxL = loneliness.Max();
                        if (maxL >= 50)
                        {
                            int target = GetHighestLonelinessNPC(loneliness);
                            ApplyStat(ref guanxi[target], 12);
                            loneliness[target] = Mathf.Max(0, loneliness[target] - 30);
                            ApplyStat(ref simPH, -15);
                            ApplyStat(ref simMH, 10);
                            interactedToday[target] = true;
                        }
                        else
                        {
                            ApplyStat(ref simTheory, 8);
                            ApplyStat(ref simPH, -10);
                            ApplyStat(ref simMH, -10);
                        }
                        break;

                    case SimulationArchetype.RestHeavy:
                        // Istirahat Asrama: PH +20, MH +15
                        ApplyStat(ref simPH, 20);
                        ApplyStat(ref simMH, 15);
                        break;
                }
            }

            // Cek ambang batas vitalitas siang
            if (simPH <= 0 || simMH <= 0)
            {
                isBurnedOut = true;
                burnoutCounter = Mathf.Max(burnoutCounter, 2);
            }

            // -------------------------------------------------------------
            // BLOK 2: SIANG
            // -------------------------------------------------------------
            switch (archetype)
            {
                case SimulationArchetype.StudyHeavy:
                    // Belajar Praktik Lab: Praktik +8, PH -10, MH -10
                    ApplyStat(ref simPractice, 8);
                    ApplyStat(ref simPH, -10);
                    ApplyStat(ref simMH, -10);
                    break;

                case SimulationArchetype.SocialHeavy:
                    // Makan Siang Bersama NPC: Guanxi +6, Loneliness -15, PH -5
                    int target = GetHighestLonelinessNPC(loneliness);
                    ApplyStat(ref guanxi[target], 6);
                    loneliness[target] = Mathf.Max(0, loneliness[target] - 15);
                    ApplyStat(ref simPH, -5);
                    interactedToday[target] = true;
                    break;

                case SimulationArchetype.Balanced:
                    // Belajar jika PH >= 40 && MH >= 40; jika tidak, Istirahat Asrama
                    if (simPH >= 40 && simMH >= 40)
                    {
                        ApplyStat(ref simPractice, 8);
                        ApplyStat(ref simPH, -10);
                        ApplyStat(ref simMH, -10);
                    }
                    else
                    {
                        ApplyStat(ref simPH, 20);
                        ApplyStat(ref simMH, 15);
                    }
                    break;

                case SimulationArchetype.RestHeavy:
                    // Istirahat Asrama: PH +20, MH +15
                    ApplyStat(ref simPH, 20);
                    ApplyStat(ref simMH, 15);
                    break;
            }

            // Cek ambang batas vitalitas sore
            if (simPH <= 0 || simMH <= 0)
            {
                isBurnedOut = true;
                burnoutCounter = Mathf.Max(burnoutCounter, 2);
            }

            // -------------------------------------------------------------
            // BLOK 3: MALAM
            // -------------------------------------------------------------
            switch (archetype)
            {
                case SimulationArchetype.StudyHeavy:
                    // Belajar Bahasa Larut Malam: Bahasa +8, PH -15, MH -15
                    ApplyStat(ref simLanguage, 8);
                    ApplyStat(ref simPH, -15);
                    ApplyStat(ref simMH, -15);
                    break;

                case SimulationArchetype.SocialHeavy:
                    // Chatting WeTalk ke NPC dengan Loneliness tertinggi: Loneliness -20
                    int chatTarget = GetHighestLonelinessNPC(loneliness);
                    loneliness[chatTarget] = Mathf.Max(0, loneliness[chatTarget] - 20);
                    interactedToday[chatTarget] = true;
                    break;

                case SimulationArchetype.Balanced:
                    // Menyapa NPC bergiliran agar tidak terkena decay, lalu tidur
                    int rotateTarget = (day - 1) % 4;
                    interactedToday[rotateTarget] = true;
                    ApplyStat(ref guanxi[rotateTarget], 2);
                    loneliness[rotateTarget] = Mathf.Max(0, loneliness[rotateTarget] - 20);
                    ApplyStat(ref simPH, 10);
                    ApplyStat(ref simMH, 5);
                    break;

                case SimulationArchetype.RestHeavy:
                    // Tidur Cepat: PH +25, MH +20
                    ApplyStat(ref simPH, 25);
                    ApplyStat(ref simMH, 20);
                    break;
            }

            // -------------------------------------------------------------
            // BLOK 4: AKHIR HARI (NIGHT CYCLE)
            // -------------------------------------------------------------
            // 1. Terapkan pemulihan tidur dasar: PH +15, MH +10
            ApplyStat(ref simPH, 15);
            ApplyStat(ref simMH, 10);

            // 2. Terapkan Guanxi Decay: -2 untuk NPC yang tidak disapa hari itu, Loneliness +10
            for (int i = 0; i < 4; i++)
            {
                if (!interactedToday[i])
                {
                    ApplyStat(ref guanxi[i], -2);
                    loneliness[i] = Mathf.Clamp(loneliness[i] + 10, 0, 100);
                }
            }

            // 3. Cek ledakan rumor: jika ada NPC dengan Loneliness >= 80, Rumor Level naik +1
            if (loneliness.Any(l => l >= 80))
            {
                simRumor = Mathf.Clamp(simRumor + 1, 0, 5);
            }

            // 4. Evaluasi ambang batas Burnout
            if (simPH <= 0 || simMH <= 0)
            {
                isBurnedOut = true;
                burnoutCounter = 2; // Set status Burnout selama 2 hari
            }
            else if (burnoutCounter > 0)
            {
                burnoutCounter--;
                if (burnoutCounter <= 0)
                {
                    isBurnedOut = false;
                }
            }

            if (isBurnedOut) totalBurnoutCount++;

            // Hitung metrik harian
            float avgG = (guanxi[0] + guanxi[1] + guanxi[2] + guanxi[3]) / 4f;
            int maxLCurrent = loneliness.Max();

            if (maxLCurrent > maxLonelinessPeak) maxLonelinessPeak = maxLCurrent;
            if (simRumor > maxRumorPeak) maxRumorPeak = simRumor;

            DailySimulationRecord record = new DailySimulationRecord
            {
                archetype = archetype.ToString(),
                day = day,
                dayName = dayName,
                physicalHealth = simPH,
                mentalHealth = simMH,
                languageProficiency = simLanguage,
                culturalEtiquette = simEtiquette,
                academicTheoretical = simTheory,
                academicPractical = simPractice,
                averageGuanxi = avgG,
                maxLoneliness = maxLCurrent,
                globalRumorLevel = simRumor,
                isBurnedOut = isBurnedOut
            };

            result.records.Add(record);

            if (day == 30)
            {
                result.utsRecord = record;
                result.utsPassed = (simTheory >= 50 && simPractice >= 45);
            }

            if (day == 60)
            {
                result.uasRecord = record;
                if (simTheory >= 80 && simPractice >= 75 && simLanguage >= 60)
                    result.uasVerdict = "Sangat Memuaskan (Cum Laude)";
                else if (simTheory >= 65 && simPractice >= 60)
                    result.uasVerdict = "Lulus Memuaskan";
                else if (simTheory >= 50 && simPractice >= 45)
                    result.uasVerdict = "Lulus Bersyarat";
                else
                    result.uasVerdict = "Academic Probation / Tidak Lulus";
            }
        }

        result.totalBurnoutDays = totalBurnoutCount;
        result.peakLoneliness = maxLonelinessPeak;
        result.peakRumorLevel = maxRumorPeak;

        for (int i = 0; i < 4; i++)
        {
            result.finalGuanxi[i] = guanxi[i];
            result.finalLoneliness[i] = loneliness[i];
        }

        return result;
    }

    private static int GetHighestLonelinessNPC(int[] loneliness)
    {
        int maxVal = -1;
        int bestIdx = 0;
        for (int i = 0; i < loneliness.Length; i++)
        {
            if (loneliness[i] > maxVal)
            {
                maxVal = loneliness[i];
                bestIdx = i;
            }
        }
        return bestIdx;
    }

    private static string GetArchetypeName(SimulationArchetype archetype)
    {
        switch (archetype)
        {
            case SimulationArchetype.StudyHeavy: return "Study-Heavy";
            case SimulationArchetype.SocialHeavy: return "Social-Heavy";
            case SimulationArchetype.Balanced: return "Balanced";
            case SimulationArchetype.RestHeavy: return "Rest-Heavy";
            default: return archetype.ToString();
        }
    }

    // =========================================================================
    // CSV DATASET EXPORTER
    // =========================================================================

    private void ExportToCSV()
    {
        try
        {
            string projectRoot = Directory.GetCurrentDirectory();
            string analyticsDir = Path.Combine(projectRoot, "Analytics");
            if (!Directory.Exists(analyticsDir)) Directory.CreateDirectory(analyticsDir);

            string userDocsDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Tokimeki_Analytics");
            if (!Directory.Exists(userDocsDir)) Directory.CreateDirectory(userDocsDir);

            List<string> exportedFiles = new List<string>();

            // 1. Ekspor file per-arketipe
            foreach (var kvp in simulationResults)
            {
                SimulationArchetype arc = kvp.Key;
                var res = kvp.Value;
                string fileName = $"telemetry_dataset_{arc.ToString().ToLower()}.csv";
                string fullPath = Path.Combine(analyticsDir, fileName);

                string csvContent = GenerateCsvContent(res.records);
                File.WriteAllText(fullPath, csvContent, Encoding.UTF8);
                File.WriteAllText(Path.Combine(userDocsDir, fileName), csvContent, Encoding.UTF8);

                exportedFiles.Add(fileName);
            }

            // 2. Jika batch all, buat juga master combined CSV
            if (simulationResults.Count > 1)
            {
                List<DailySimulationRecord> allRecords = new List<DailySimulationRecord>();
                foreach (var res in simulationResults.Values)
                {
                    allRecords.AddRange(res.records);
                }

                string masterFile = "telemetry_dataset_all_archetypes.csv";
                string masterFullPath = Path.Combine(analyticsDir, masterFile);
                string masterCsv = GenerateCsvContent(allRecords);

                File.WriteAllText(masterFullPath, masterCsv, Encoding.UTF8);
                File.WriteAllText(Path.Combine(userDocsDir, masterFile), masterCsv, Encoding.UTF8);
                exportedFiles.Add(masterFile);
            }

            statusMessage = $"Berhasil mengekspor {exportedFiles.Count} file CSV ke:\n1. {analyticsDir}\n2. {userDocsDir}";
            statusMessageType = MessageType.Info;

            EditorUtility.DisplayDialog(
                "Export Dataset Selesai",
                $"Dataset telemetri 60 hari berhasil diekspor ke:\n{analyticsDir}\n\nFile siap dianalisis di Excel, SPSS, atau Google Colab/Pandas.",
                "OK"
            );
        }
        catch (Exception ex)
        {
            statusMessage = $"Gagal mengekspor CSV: {ex.Message}";
            statusMessageType = MessageType.Error;
            Debug.LogError($"[AutomatedBalancingSimulator] Error exporting CSV: {ex}");
        }
    }

    [MenuItem("Tokimeki TA/Export All Archetype Datasets (Batch Headless)")]
    [MenuItem("Game Debug/Export All Archetype Datasets (Batch Headless)")]
    public static void ExportAllArchetypesHeadless()
    {
        Debug.Log("<color=cyan>=== MENJALANKAN BATCH HEADLESS BALANCING SIMULATOR (4 ARTIKETIPE x 60 HARI) ===</color>");

        var results = new Dictionary<SimulationArchetype, ArchetypeSimulationResult>
        {
            [SimulationArchetype.StudyHeavy] = RunSimulation(SimulationArchetype.StudyHeavy),
            [SimulationArchetype.SocialHeavy] = RunSimulation(SimulationArchetype.SocialHeavy),
            [SimulationArchetype.Balanced] = RunSimulation(SimulationArchetype.Balanced),
            [SimulationArchetype.RestHeavy] = RunSimulation(SimulationArchetype.RestHeavy)
        };

        string projectRoot = Directory.GetCurrentDirectory();
        string analyticsDir = Path.Combine(projectRoot, "Analytics");
        if (!Directory.Exists(analyticsDir)) Directory.CreateDirectory(analyticsDir);

        string userDocsDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Tokimeki_Analytics");
        if (!Directory.Exists(userDocsDir)) Directory.CreateDirectory(userDocsDir);

        List<DailySimulationRecord> allRecords = new List<DailySimulationRecord>();

        foreach (var kvp in results)
        {
            var res = kvp.Value;
            string fileName = $"telemetry_dataset_{kvp.Key.ToString().ToLower()}.csv";
            string fullPath = Path.Combine(analyticsDir, fileName);
            string csv = GenerateCsvContent(res.records);
            File.WriteAllText(fullPath, csv, Encoding.UTF8);
            File.WriteAllText(Path.Combine(userDocsDir, fileName), csv, Encoding.UTF8);

            allRecords.AddRange(res.records);

            Debug.Log($"<color=green>[Simulasi Selesai]</color> {res.archetypeName} -> UTS: {(res.utsPassed ? "LULUS" : "PROBATION")} | UAS: {res.uasVerdict} | Burnout: {res.totalBurnoutDays} hari | Puncak Rumor: Lv {res.peakRumorLevel}");
        }

        string masterFile = "telemetry_dataset_all_archetypes.csv";
        string masterFullPath = Path.Combine(analyticsDir, masterFile);
        string masterCsv = GenerateCsvContent(allRecords);
        File.WriteAllText(masterFullPath, masterCsv, Encoding.UTF8);
        File.WriteAllText(Path.Combine(userDocsDir, masterFile), masterCsv, Encoding.UTF8);

        Debug.Log($"<color=green>=== BATCH EXPORT SELESAI ===</color> Seluruh dataset tersimpan di: {analyticsDir}");
    }

    private static string GenerateCsvContent(List<DailySimulationRecord> records)
    {
        StringBuilder sb = new StringBuilder();
        // Header CSV persis seperti permintaan spesifikasi
        sb.AppendLine("Archetype,Day,DayName,PH,MH,Language,Etiquette,Theory,Practice,AvgGuanxi,MaxLoneliness,RumorLevel,IsBurnedOut");

        foreach (var r in records)
        {
            sb.AppendLine(string.Format(
                CultureInfo.InvariantCulture,
                "{0},{1},{2},{3},{4},{5},{6},{7},{8},{9:F2},{10},{11},{12}",
                r.archetype,
                r.day,
                r.dayName,
                r.physicalHealth,
                r.mentalHealth,
                r.languageProficiency,
                r.culturalEtiquette,
                r.academicTheoretical,
                r.academicPractical,
                r.averageGuanxi,
                r.maxLoneliness,
                r.globalRumorLevel,
                r.isBurnedOut ? 1 : 0
            ));
        }

        return sb.ToString();
    }
}
#endif
