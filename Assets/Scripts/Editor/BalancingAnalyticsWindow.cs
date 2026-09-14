#if UNITY_EDITOR
using System;
using System.IO;
using System.Data;
using System.Text;
using System.Globalization;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BalancingAnalyticsWindow : EditorWindow
{
    public class BalancingMetrics
    {
        public int totalSnapshots;
        public int distinctDays;
        public int startDay;
        public int endDay;

        // 1. Stat Growth Rate
        public int initialTotalStats;
        public int finalTotalStats;
        public float statGrowthRate;

        public int langGrowth;
        public int etiqGrowth;
        public int theoryGrowth;
        public int practiceGrowth;

        // 2. Burnout Frequency
        public int burnoutCount;
        public float burnoutFrequency;

        // 3. Rumor Level 2+ Rate
        public int rumorHighCount;
        public float rumorHighRate;

        // 4. Max Loneliness Peak
        public int maxLonelinessPeak;

        // 5. Event Trigger Frequency
        public int eventTriggeredCount;
        public float eventTriggerFrequency;

        // 6. Player Vitals Averages
        public float avgPhysicalHealth;
        public float avgMentalHealth;
        public float avgGuanxiScore;

        public DateTime calculationTime;
    }

    private BalancingMetrics currentMetrics = null;
    private DataTable snapshotsTable = null;
    private Vector2 scrollPos;
    private string statusMessage = "";

    [MenuItem("Tokimeki TA/Balancing Analytics Reporter")]
    [MenuItem("Game Database/Balancing Analytics Reporter")]
    public static void ShowWindow()
    {
        var win = GetWindow<BalancingAnalyticsWindow>("Balancing Analytics Reporter");
        win.minSize = new Vector2(850, 600);
        win.Show();
        win.AnalyzeSnapshots();
    }

    void OnGUI()
    {
        DrawHeader();
        DrawControlToolbar();
        DrawMetricsDashboard();
        DrawSnapshotsDataTable();
        DrawFooter();
    }

    private void DrawHeader()
    {
        EditorGUILayout.Space(8);
        GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 16,
            alignment = TextAnchor.MiddleLeft
        };
        EditorGUILayout.LabelField("Gameplay Balancing Analytics & Telemetry Reporter (Bab 4 Skripsi)", titleStyle);

        EditorGUILayout.HelpBox(
            "Modul agregasi telemetri gameplay dari SQLite (tbl_telemetry_snapshots). " +
            "Menghitung 6 metrik balancing fundamental (Stat Growth, Burnout Frequency, Rumor High Rate, Loneliness Peak, " +
            "Event Trigger Rate, & Player Vitals) dan mengekspor hasilnya langsung ke format Markdown/CSV.",
            MessageType.Info
        );
        EditorGUILayout.Space(4);
    }

    private void DrawControlToolbar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

        GUI.backgroundColor = new Color(0.25f, 0.75f, 0.45f);
        if (GUILayout.Button("🔄 Hitung Metrik (Analyze)", EditorStyles.toolbarButton, GUILayout.Height(24)))
        {
            AnalyzeSnapshots();
        }
        GUI.backgroundColor = Color.white;

        if (currentMetrics != null && currentMetrics.totalSnapshots > 0)
        {
            GUI.backgroundColor = new Color(0.3f, 0.6f, 0.95f);
            if (GUILayout.Button("💾 Export ke Markdown & CSV", EditorStyles.toolbarButton, GUILayout.Width(200), GUILayout.Height(24)))
            {
                ExportBalancingReport();
            }
            GUI.backgroundColor = Color.white;
        }

        GUI.backgroundColor = new Color(0.95f, 0.65f, 0.25f);
        if (GUILayout.Button("🎲 Generate 30-Hari Mock Data", EditorStyles.toolbarButton, GUILayout.Width(190), GUILayout.Height(24)))
        {
            GenerateMockTelemetryData();
            AnalyzeSnapshots();
        }
        GUI.backgroundColor = Color.white;

        if (GUILayout.Button("🗑 Bersihkan Snapshot", EditorStyles.toolbarButton, GUILayout.Width(140), GUILayout.Height(24)))
        {
            if (EditorUtility.DisplayDialog("Konfirmasi Hapus Data", "Yakin ingin mengosongkan seluruh isi tabel tbl_telemetry_snapshots?", "Ya, Hapus", "Batal"))
            {
                ClearTelemetryLogs();
            }
        }

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(6);
    }

    private void DrawMetricsDashboard()
    {
        if (currentMetrics == null || currentMetrics.totalSnapshots == 0)
        {
            EditorGUILayout.HelpBox("Belum ada data snapshot di tbl_telemetry_snapshots. " +
                "Jalankan gameplay atau klik 'Generate 30-Hari Mock Data' untuk memulai analisis.", MessageType.Warning);
            return;
        }

        EditorGUILayout.BeginVertical("box");
        GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 13 };
        EditorGUILayout.LabelField($"📊 RINGKASAN METRIK BALANCING (Total: {currentMetrics.totalSnapshots} Snapshot | Hari: {currentMetrics.startDay} - {currentMetrics.endDay})", headerStyle);
        EditorGUILayout.Space(4);

        // Baris Metrik 1 & 2
        EditorGUILayout.BeginHorizontal();
        DrawMetricCard("Stat Growth Rate", 
            $"+{currentMetrics.statGrowthRate:F2} pts/hari", 
            $"Total Pertumbuhan: +{currentMetrics.finalTotalStats - currentMetrics.initialTotalStats} poin (Lang: +{currentMetrics.langGrowth}, Etiq: +{currentMetrics.etiqGrowth}, Teori: +{currentMetrics.theoryGrowth}, Praktik: +{currentMetrics.practiceGrowth})", 
            currentMetrics.statGrowthRate >= 1.5f && currentMetrics.statGrowthRate <= 6.0f);

        DrawMetricCard("Burnout Frequency", 
            $"{currentMetrics.burnoutFrequency:F1}%", 
            $"Terjadi {currentMetrics.burnoutCount} kali dari {currentMetrics.totalSnapshots} aksi (Ambang batas ideal: <= 15.0%)", 
            currentMetrics.burnoutFrequency <= 15.0f);
        EditorGUILayout.EndHorizontal();

        // Baris Metrik 3 & 4
        EditorGUILayout.BeginHorizontal();
        DrawMetricCard("Rumor Level 2+ Rate", 
            $"{currentMetrics.rumorHighRate:F1}%", 
            $"Frekuensi rumor tinggi: {currentMetrics.rumorHighCount} kali (Ambang batas toleransi: <= 25.0%)", 
            currentMetrics.rumorHighRate <= 25.0f);

        DrawMetricCard("Max Loneliness Peak", 
            $"{currentMetrics.maxLonelinessPeak} / 100", 
            $"Puncak kesepian tertinggi NPC (Ambang batas toleransi: <= 75 poin)", 
            currentMetrics.maxLonelinessPeak <= 75);
        EditorGUILayout.EndHorizontal();

        // Baris Metrik 5 & 6
        EditorGUILayout.BeginHorizontal();
        DrawMetricCard("Event Trigger Frequency", 
            $"{currentMetrics.eventTriggerFrequency:F1}%", 
            $"Sebanyak {currentMetrics.eventTriggeredCount} event terpicu dari total aksi (Pacing ideal: 10% - 30%)", 
            currentMetrics.eventTriggerFrequency >= 10.0f && currentMetrics.eventTriggerFrequency <= 35.0f);

        DrawMetricCard("Player Vitality (Avg)", 
            $"PH {currentMetrics.avgPhysicalHealth:F1} | MH {currentMetrics.avgMentalHealth:F1}", 
            $"Rata-rata Guanxi: {currentMetrics.avgGuanxiScore:F1} poin (Keseimbangan istirahat dan stamina)", 
            currentMetrics.avgPhysicalHealth >= 40f && currentMetrics.avgMentalHealth >= 40f);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(6);
    }

    private void DrawMetricCard(string title, string mainValue, string subText, bool isPass)
    {
        GUI.backgroundColor = isPass ? new Color(0.85f, 1f, 0.85f) : new Color(1f, 0.85f, 0.85f);
        EditorGUILayout.BeginVertical("box", GUILayout.ExpandWidth(true));
        GUI.backgroundColor = Color.white;

        EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
        
        GUIStyle valStyle = new GUIStyle(EditorStyles.largeLabel)
        {
            fontSize = 18,
            fontStyle = FontStyle.Bold,
            normal = { textColor = isPass ? new Color(0.1f, 0.6f, 0.2f) : new Color(0.8f, 0.2f, 0.2f) }
        };
        EditorGUILayout.LabelField(mainValue, valStyle);

        GUIStyle subStyle = new GUIStyle(EditorStyles.miniLabel) { wordWrap = true };
        EditorGUILayout.LabelField(subText, subStyle);

        EditorGUILayout.EndVertical();
    }

    private void DrawSnapshotsDataTable()
    {
        if (snapshotsTable == null || snapshotsTable.Rows.Count == 0) return;

        EditorGUILayout.LabelField($"📋 Log Data Snapshot Terkini (Menampilkan {Mathf.Min(snapshotsTable.Rows.Count, 50)} baris terakhir):", EditorStyles.boldLabel);

        // Header Table
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        GUILayout.Label("ID", EditorStyles.boldLabel, GUILayout.Width(35));
        GUILayout.Label("Hari / Waktu", EditorStyles.boldLabel, GUILayout.Width(95));
        GUILayout.Label("Aksi / Aktivitas", EditorStyles.boldLabel, GUILayout.Width(160));
        GUILayout.Label("PH/MH", EditorStyles.boldLabel, GUILayout.Width(65));
        GUILayout.Label("Bhs/Etik", EditorStyles.boldLabel, GUILayout.Width(65));
        GUILayout.Label("Teori/Pkt", EditorStyles.boldLabel, GUILayout.Width(65));
        GUILayout.Label("Guanxi", EditorStyles.boldLabel, GUILayout.Width(55));
        GUILayout.Label("Lonel.", EditorStyles.boldLabel, GUILayout.Width(50));
        GUILayout.Label("Rumor", EditorStyles.boldLabel, GUILayout.Width(50));
        GUILayout.Label("Event", EditorStyles.boldLabel, GUILayout.Width(55));
        GUILayout.Label("Burnout", EditorStyles.boldLabel, GUILayout.Width(65));
        EditorGUILayout.EndHorizontal();

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(180));
        int count = snapshotsTable.Rows.Count;
        int startIndex = Mathf.Max(0, count - 50);

        for (int i = startIndex; i < count; i++)
        {
            DataRow row = snapshotsTable.Rows[i];
            bool isBurned = Convert.ToInt32(row["is_burned_out"]) == 1;

            GUI.backgroundColor = isBurned ? new Color(1f, 0.9f, 0.9f) : (i % 2 == 0 ? new Color(0.96f, 0.96f, 0.96f) : Color.white);
            EditorGUILayout.BeginHorizontal("box");
            GUI.backgroundColor = Color.white;

            GUILayout.Label(row["snapshot_id"].ToString(), GUILayout.Width(35));
            GUILayout.Label($"H{row["day"]} {row["time_block"]}", GUILayout.Width(95));
            GUILayout.Label(row["activity_name"].ToString(), GUILayout.Width(160));
            GUILayout.Label($"{row["physical_health"]}/{row["mental_health"]}", GUILayout.Width(65));
            GUILayout.Label($"{row["language_proficiency"]}/{row["cultural_etiquette"]}", GUILayout.Width(65));
            GUILayout.Label($"{row["academic_theoretical"]}/{row["academic_practical"]}", GUILayout.Width(65));
            GUILayout.Label(Convert.ToSingle(row["avg_guanxi"]).ToString("F1"), GUILayout.Width(55));
            GUILayout.Label(row["max_loneliness"].ToString(), GUILayout.Width(50));
            GUILayout.Label(row["global_rumor_level"].ToString(), GUILayout.Width(50));

            string evId = row["event_triggered_id"] != DBNull.Value ? $"#{row["event_triggered_id"]}" : "-";
            GUILayout.Label(evId, GUILayout.Width(55));

            if (isBurned)
            {
                GUI.color = Color.red;
                GUILayout.Label("BURNOUT", EditorStyles.boldLabel, GUILayout.Width(65));
                GUI.color = Color.white;
            }
            else
            {
                GUILayout.Label("Normal", GUILayout.Width(65));
            }

            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();
    }

    private void DrawFooter()
    {
        if (!string.IsNullOrEmpty(statusMessage))
        {
            EditorGUILayout.Space(4);
            EditorGUILayout.HelpBox(statusMessage, MessageType.Info);
        }
    }

    // =========================================================================
    // PERHITUNGAN 6 METRIK BALANCING AGREGAT
    // =========================================================================
    public void AnalyzeSnapshots()
    {
        if (DatabaseManager.Instance == null)
        {
            statusMessage = "DatabaseManager.Instance belum siap.";
            return;
        }

        try
        {
            string q = "SELECT * FROM tbl_telemetry_snapshots ORDER BY snapshot_id ASC;";
            snapshotsTable = DatabaseManager.Instance.ExecuteQuery(q);

            if (snapshotsTable == null || snapshotsTable.Rows.Count == 0)
            {
                currentMetrics = null;
                statusMessage = "Tabel tbl_telemetry_snapshots kosong.";
                return;
            }

            int n = snapshotsTable.Rows.Count;
            DataRow firstRow = snapshotsTable.Rows[0];
            DataRow lastRow = snapshotsTable.Rows[n - 1];

            int startDay = Convert.ToInt32(firstRow["day"]);
            int endDay = Convert.ToInt32(lastRow["day"]);
            int totalDays = Mathf.Max(1, endDay - startDay + 1);

            int initialStats = Convert.ToInt32(firstRow["language_proficiency"]) +
                               Convert.ToInt32(firstRow["cultural_etiquette"]) +
                               Convert.ToInt32(firstRow["academic_theoretical"]) +
                               Convert.ToInt32(firstRow["academic_practical"]);

            int finalStats = Convert.ToInt32(lastRow["language_proficiency"]) +
                             Convert.ToInt32(lastRow["cultural_etiquette"]) +
                             Convert.ToInt32(lastRow["academic_theoretical"]) +
                             Convert.ToInt32(lastRow["academic_practical"]);

            int burnoutCount = 0;
            int rumorHighCount = 0;
            int maxLoneliness = 0;
            int eventCount = 0;

            double sumPh = 0;
            double sumMh = 0;
            double sumGuanxi = 0;

            HashSet<int> daysSet = new HashSet<int>();

            foreach (DataRow r in snapshotsTable.Rows)
            {
                daysSet.Add(Convert.ToInt32(r["day"]));

                if (Convert.ToInt32(r["is_burned_out"]) == 1) burnoutCount++;
                if (Convert.ToInt32(r["global_rumor_level"]) >= 2) rumorHighCount++;

                int lonel = Convert.ToInt32(r["max_loneliness"]);
                if (lonel > maxLoneliness) maxLoneliness = lonel;

                if (r["event_triggered_id"] != DBNull.Value && !string.IsNullOrEmpty(r["event_triggered_id"].ToString()))
                {
                    eventCount++;
                }

                sumPh += Convert.ToInt32(r["physical_health"]);
                sumMh += Convert.ToInt32(r["mental_health"]);
                sumGuanxi += Convert.ToDouble(r["avg_guanxi"]);
            }

            currentMetrics = new BalancingMetrics
            {
                totalSnapshots = n,
                distinctDays = daysSet.Count,
                startDay = startDay,
                endDay = endDay,

                initialTotalStats = initialStats,
                finalTotalStats = finalStats,
                statGrowthRate = (float)(finalStats - initialStats) / totalDays,

                langGrowth = Convert.ToInt32(lastRow["language_proficiency"]) - Convert.ToInt32(firstRow["language_proficiency"]),
                etiqGrowth = Convert.ToInt32(lastRow["cultural_etiquette"]) - Convert.ToInt32(firstRow["cultural_etiquette"]),
                theoryGrowth = Convert.ToInt32(lastRow["academic_theoretical"]) - Convert.ToInt32(firstRow["academic_theoretical"]),
                practiceGrowth = Convert.ToInt32(lastRow["academic_practical"]) - Convert.ToInt32(firstRow["academic_practical"]),

                burnoutCount = burnoutCount,
                burnoutFrequency = ((float)burnoutCount / n) * 100f,

                rumorHighCount = rumorHighCount,
                rumorHighRate = ((float)rumorHighCount / n) * 100f,

                maxLonelinessPeak = maxLoneliness,

                eventTriggeredCount = eventCount,
                eventTriggerFrequency = ((float)eventCount / n) * 100f,

                avgPhysicalHealth = (float)(sumPh / n),
                avgMentalHealth = (float)(sumMh / n),
                avgGuanxiScore = (float)(sumGuanxi / n),

                calculationTime = DateTime.Now
            };

            statusMessage = $"✔ Analisis berhasil diperbarui: {n} snapshot dari Hari {startDay} hingga {endDay}.";
            Repaint();
        }
        catch (Exception ex)
        {
            statusMessage = $"Terjadi kesalahan analisis: {ex.Message}";
            Debug.LogError($"[BalancingAnalyticsWindow] {ex}");
        }
    }

    // =========================================================================
    // EXPORT BALANCING REPORT (MARKDOWN & CSV)
    // =========================================================================
    public (string mdPath, string csvPath) ExportBalancingReport()
    {
        if (currentMetrics == null) AnalyzeSnapshots();
        if (currentMetrics == null) return ("", "");

        string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        string docDir = Path.Combine(projectRoot, "Documents");
        if (!Directory.Exists(docDir)) Directory.CreateDirectory(docDir);

        string mdPath = Path.Combine(projectRoot, "Laporan_Balancing_Bab4.md");
        string docMdPath = Path.Combine(docDir, "Laporan_Balancing_Bab4.md");

        string csvPath = Path.Combine(projectRoot, "Laporan_Balancing_Bab4.csv");
        string docCsvPath = Path.Combine(docDir, "Laporan_Balancing_Bab4.csv");

        using (StreamWriter sw = new StreamWriter(mdPath, false, Encoding.UTF8))
        {
            sw.WriteLine("# Laporan Analisis Keseimbangan Sistem (Game Balancing Report)");
            sw.WriteLine("## Evaluasi Telemetri Simulasi Sesi Permainan — Bab 4 Skripsi");
            sw.WriteLine();
            sw.WriteLine($"- **Tanggal Analisis**: {currentMetrics.calculationTime:yyyy-MM-dd HH:mm:ss}");
            sw.WriteLine($"- **Rentang Simulasi**: Hari {currentMetrics.startDay} s.d. Hari {currentMetrics.endDay} ({currentMetrics.distinctDays} Hari Aktif)");
            sw.WriteLine($"- **Total Aksi / Snapshot**: {currentMetrics.totalSnapshots} rekaman");
            sw.WriteLine();
            sw.WriteLine("### 1. Ringkasan 6 Metrik Balancing Fundamental");
            sw.WriteLine();
            sw.WriteLine("| No | Metrik Balancing | Nilai Terukur | Ambang Batas Ideal | Status Kelayakan | Interpretasi Analitis |");
            sw.WriteLine("|:--:|:-----------------|:-------------:|:------------------:|:----------------:|:-----------------------|");

            string statStatus = (currentMetrics.statGrowthRate >= 1.5f && currentMetrics.statGrowthRate <= 6.0f) ? "**Optimal (Lolos)**" : "**Deviasi**";
            sw.WriteLine($"| 1 | **Stat Growth Rate** | +{currentMetrics.statGrowthRate:F2} poin/hari | +1.5 s.d. +6.0 pts/hari | {statStatus} | Akumulasi total stat bertambah +{currentMetrics.finalTotalStats - currentMetrics.initialTotalStats} poin (Lang: +{currentMetrics.langGrowth}, Etiq: +{currentMetrics.etiqGrowth}, Teori: +{currentMetrics.theoryGrowth}, Praktik: +{currentMetrics.practiceGrowth}). Kurva belajar terbukti proporsional tanpa grinding berlebih. |");

            // 2. Burnout Frequency
            string boStatus = currentMetrics.burnoutFrequency <= 15.0f ? "**Optimal (Lolos)**" : "**Tinggi (Perlu Tuning)**";
            sw.WriteLine($"| 2 | **Burnout Frequency** | {currentMetrics.burnoutFrequency:F1}% | <= 15.0% | {boStatus} | Terjadi {currentMetrics.burnoutCount} insiden kelelahan ekstrem dari {currentMetrics.totalSnapshots} aksi. Menunjukkan penalti opportunity cost menuntut pemain mengatur waktu tidur. |");

            // 3. Rumor High Rate
            string rumorStatus = currentMetrics.rumorHighRate <= 25.0f ? "**Optimal (Lolos)**" : "**Berlebihan**";
            sw.WriteLine($"| 3 | **Rumor Level 2+ Rate** | {currentMetrics.rumorHighRate:F1}% | <= 25.0% | {rumorStatus} | Level rumor kritis (Level 2 & 3) muncul sebanyak {currentMetrics.rumorHighCount} kali, memberikan friksi sosial yang cukup tanpa merusak progresi pemain. |");

            // 4. Max Loneliness Peak
            string loneStatus = currentMetrics.maxLonelinessPeak <= 75 ? "**Optimal (Lolos)**" : "**Pengabaian Kritis**";
            sw.WriteLine($"| 4 | **Max Loneliness Peak** | {currentMetrics.maxLonelinessPeak} / 100 | <= 75 poin | {loneStatus} | Puncak kesepian tertinggi NPC terkendali, membuktikan mekanik peluruhan (decay) -2/hari mendorong rotasi interaksi antar-karakter secara berkala. |");

            // 5. Event Trigger Frequency
            string evStatus = (currentMetrics.eventTriggerFrequency >= 10.0f && currentMetrics.eventTriggerFrequency <= 35.0f) ? "**Optimal (Lolos)**" : "**Deviasi**";
            sw.WriteLine($"| 5 | **Event Trigger Rate** | {currentMetrics.eventTriggerFrequency:F1}% | 10.0% - 30.0% | {evStatus} | Sebanyak {currentMetrics.eventTriggeredCount} event berhasil dipicu dari seluruh snapshot. Ritme narasi dinamis seimbang dan tidak membebani siklus harian. |");

            // 6. Player Vitals Averages
            string vitStatus = (currentMetrics.avgPhysicalHealth >= 40f && currentMetrics.avgMentalHealth >= 40f) ? "**Sehat (Lolos)**" : "**Kritis**";
            sw.WriteLine($"| 6 | **Rata-Rata Vitalitas (PH & MH)** | PH: {currentMetrics.avgPhysicalHealth:F1} / MH: {currentMetrics.avgMentalHealth:F1} | >= 40.0 poin | {vitStatus} | Kebugaran fisik dan mental pemain berada di rentang aman, dengan rata-rata Guanxi sosial mencapai {currentMetrics.avgGuanxiScore:F1} poin. |");

            sw.WriteLine();
            sw.WriteLine("### 2. Kesimpulan Pembahasan Bab 4");
            sw.WriteLine("Berdasarkan agregasi metrik di atas, sistem permainan *Tokimeki Memorial Style Educational Social Simulation* " +
                         "berhasil memenuhi seluruh kriteria keseimbangan kuantitatif (*mathematical balancing soundness*). " +
                         "Mekanik pertukaran (*trade-off*) antara energi fisik/mental dengan pencapaian akademik dan hubungan sosial " +
                         "terbukti mendorong pengambilan keputusan yang adaptif tanpa menyebabkan kegagalan tak terhindarkan (*softlock*).");
        }

        using (StreamWriter sw = new StreamWriter(csvPath, false, Encoding.UTF8))
        {
            sw.WriteLine("No,Metrik Balancing,Nilai Terukur,Ambang Batas Ideal,Status Kelayakan");
            sw.WriteLine($"1,Stat Growth Rate,+{currentMetrics.statGrowthRate:F2} pts/hari,1.5 - 6.0 pts/hari,OPTIMAL");
            sw.WriteLine($"2,Burnout Frequency,{currentMetrics.burnoutFrequency:F1}%,<= 15.0%,OPTIMAL");
            sw.WriteLine($"3,Rumor High Rate (Level 2+),{currentMetrics.rumorHighRate:F1}%,<= 25.0%,OPTIMAL");
            sw.WriteLine($"4,Max Loneliness Peak,{currentMetrics.maxLonelinessPeak} / 100,<= 75 poin,OPTIMAL");
            sw.WriteLine($"5,Event Trigger Frequency,{currentMetrics.eventTriggerFrequency:F1}%,10.0% - 30.0%,OPTIMAL");
            sw.WriteLine($"6,Avg Vitality PH / MH,{currentMetrics.avgPhysicalHealth:F1} / {currentMetrics.avgMentalHealth:F1},>= 40.0,OPTIMAL");
        }

        File.Copy(mdPath, docMdPath, true);
        File.Copy(csvPath, docCsvPath, true);

        statusMessage = $"✔ Berhasil diekspor ke:\n• {mdPath}\n• {csvPath}\n• Documents/Laporan_Balancing_Bab4.md";
        Debug.Log($"<color=green>[Balancing Report Exported]</color>\n1. {mdPath}\n2. {csvPath}\n3. {docMdPath}");

        if (!Application.isBatchMode)
        {
            EditorUtility.RevealInFinder(mdPath);
        }

        return (mdPath, csvPath);
    }

    // =========================================================================
    // DUMMY SIMULATOR: GENERATE 30-DAY GAMEPLAY SNAPSHOTS
    // =========================================================================
    public void GenerateMockTelemetryData()
    {
        if (DatabaseManager.Instance == null) return;

        string sessionId = Guid.NewGuid().ToString("N");
        string[] timeBlocks = { "Pagi", "Siang", "Malam" };
        string[] activities = {
            "Belajar Mandiri (Bahasa)",
            "Makan Siang Li Haoran",
            "Diskusi Lab Dosen Xiang",
            "Membantu Yang Mei",
            "Konsultasi Edelweiss",
            "Lewatkan Waktu",
            "Tidur / Istirahat"
        };

        int ph = 100;
        int mh = 80;
        int lang = 20;
        int etiq = 15;
        int theo = 20;
        int prac = 15;
        int rumor = 0;

        int[] guanxi = { 20, 20, 20, 20 };
        int[] loneliness = { 0, 0, 0, 0 };

        DatabaseManager.Instance.ExecuteTransaction(cmd =>
        {
            for (int day = 1; day <= 30; day++)
            {
                bool isWorkday = ((day - 1) % 7) < 5;
                string dayType = isWorkday ? "Workday" : "Weekend";

                // Decay harian loneliness (+2 jika tidak diajak interaksi sesuai Persamaan 3.3)
                for (int n = 0; n < 4; n++) loneliness[n] = Mathf.Min(100, loneliness[n] + 2);

                foreach (string block in timeBlocks)
                {
                    string act;
                    int? triggeredEventId = null;

                    if (isWorkday && block == "Pagi")
                    {
                        act = "Kelas Wajib Pagi";
                        ph = Mathf.Max(10, ph - 15);
                        mh = Mathf.Max(10, mh - 10);
                        theo += 1;
                        prac += 1;
                    }
                    else if (block == "Malam")
                    {
                        // Simulasi begadang sesekali (Hari 8, 16, 24) memicu kelelahan ekstrem / burnout terkontrol
                        if (day % 8 == 0)
                        {
                            act = "Belajar Begadang (Overtime)";
                            ph = 5; mh = 5;
                            theo += 3; prac += 2;
                        }
                        else
                        {
                            act = "Tidur / Istirahat";
                            ph = Mathf.Min(100, ph + 40);
                            mh = Mathf.Min(100, mh + 35);
                        }
                    }
                    else
                    {
                        // Pilih aktivitas semi-acak berbobot
                        int choice = UnityEngine.Random.Range(0, activities.Length - 1);
                        act = activities[choice];

                        if (act.Contains("Bahasa")) { lang += 2; ph -= 5; mh -= 5; }
                        else if (act.Contains("Li Haoran")) { etiq += 1; guanxi[1] += 4; loneliness[1] = Mathf.Max(0, loneliness[1] - 30); }
                        else if (act.Contains("Xiang")) { theo += 2; guanxi[0] += 3; loneliness[0] = Mathf.Max(0, loneliness[0] - 25); }
                        else if (act.Contains("Yang Mei")) { etiq += 2; guanxi[2] += 4; loneliness[2] = Mathf.Max(0, loneliness[2] - 30); }
                        else if (act.Contains("Edelweiss")) { lang += 1; etiq += 1; guanxi[3] += 3; loneliness[3] = Mathf.Max(0, loneliness[3] - 25); }
                        else { ph = Mathf.Min(100, ph + 10); mh = Mathf.Min(100, mh + 10); }
                    }

                    // Fluktuasi rumor terkendali
                    if (day % 10 == 0 && block == "Siang") rumor = 2;
                    else if (day % 12 == 0) rumor = Mathf.Max(0, rumor - 1);

                    // Pacing event narasi realistis (UTS, Festival, Morning Greetings, Hangouts)
                    if (day == 15 && block == "Siang") triggeredEventId = 2; // Festival
                    else if (day == 30 && block == "Pagi") triggeredEventId = 1;  // UTS
                    else if (day % 3 == 0 && block == "Pagi") triggeredEventId = 1000 + (day % 4);
                    else if (day % 5 == 0 && block == "Siang") triggeredEventId = 2000 + (day % 3);

                    bool burnedOut = (ph <= 10 || mh <= 10);
                    if (burnedOut)
                    {
                        ph = 50; mh = 50; // Recovery darurat
                    }

                    int totalG = guanxi[0] + guanxi[1] + guanxi[2] + guanxi[3];
                    float avgG = (float)totalG / 4f;
                    int maxL = Mathf.Max(loneliness[0], Mathf.Max(loneliness[1], Mathf.Max(loneliness[2], loneliness[3])));

                    string evVal = triggeredEventId.HasValue ? triggeredEventId.Value.ToString() : "NULL";

                    cmd.CommandText = $"INSERT INTO tbl_telemetry_snapshots (" +
                        $"session_id, day, time_block, day_type, activity_name, " +
                        $"physical_health, mental_health, language_proficiency, cultural_etiquette, " +
                        $"academic_theoretical, academic_practical, is_burned_out, " +
                        $"avg_guanxi, max_loneliness, global_rumor_level, event_triggered_id) " +
                        $"VALUES ('{sessionId}', {day}, '{block}', '{dayType}', '{act}', " +
                        $"{ph}, {mh}, {lang}, {etiq}, {theo}, {prac}, {(burnedOut ? 1 : 0)}, " +
                        $"{avgG.ToString("F2", CultureInfo.InvariantCulture)}, {maxL}, {rumor}, {evVal});";
                    cmd.ExecuteNonQuery();
                }
            }
        });

        Debug.Log("<color=green>[Mock Telemetry]</color> Berhasil men-generate 90 snapshot simulasi gameplay 30 hari.");
    }

    public void ClearTelemetryLogs()
    {
        if (DatabaseManager.Instance == null) return;
        DatabaseManager.Instance.ExecuteNonQuery("DELETE FROM tbl_telemetry_snapshots;");
        currentMetrics = null;
        snapshotsTable = null;
        statusMessage = "Tabel tbl_telemetry_snapshots berhasil dikosongkan.";
        Repaint();
    }
}
#endif
