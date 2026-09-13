using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class ActivitySimulator : MonoBehaviour
{
    void Update()
    {
        // Tekan Angka 1: Pilih Aksi Belajar Mandiri (Studi Bahasa)
        if (IsKeyPressed(KeyCode.Alpha1))
        {
            Debug.Log("<color=white>[Input Aksi]</color> Devano memilih Belajar Kosakata Mandarin.");
            PlayerStats.Instance.ModifyStats(dLanguage: 15, dEtiquette: 0, dMental: -10, dPhysical: -5, dTheoretical: 0, dPractical: 0);
            GameManager.Instance.GeserWaktu();
        }

        // Tekan Angka 2: Mengajak Makan Siang Li Haoran (NPC ID: 102)
        if (IsKeyPressed(KeyCode.Alpha2))
        {
            Debug.Log("<color=white>[Input Aksi]</color> Devano mengajak Li Haoran Makan Siang.");
            PlayerStats.Instance.ModifyStats(dLanguage: 0, dEtiquette: 5, dMental: 10, dPhysical: -5, dTheoretical: 0, dPractical: 0);
            SocialManager.Instance.TambahGuanxi(npcId: 102, penambahanGuanxi: 10, reduksiLoneliness: 30);
            GameManager.Instance.GeserWaktu();
        }

        // Tekan Spasi: Langsung Geser Waktu (Skip Slot)
        if (IsKeyPressed(KeyCode.Space))
        {
            Debug.Log("<color=white>[Input Aksi]</color> Melewatkan waktu tanpa interaksi sosial.");
            GameManager.Instance.GeserWaktu();
        }

        // Tekan D: Mulai Dialog Laporan Progres ke Dosen Xiang Bai (Node 1001)
        if (IsKeyPressed(KeyCode.D))
        {
            DialogueManager.Instance.StartDialogue(1001);
        }

        // Tekan Enter: Pilih Opsi Respon Pertama (Evaluasi Stat)
        if (IsKeyPressed(KeyCode.Return))
        {
            DialogueManager.Instance.SelectOption(1);
        }

        // Shortcut Debug Status untuk Menguji 3 Rute:
        // Tekan F1: Set Status Awal (Bahasa 20, Etika 15) -> Menguji RUTE C
        if (IsKeyPressed(KeyCode.F1))
        {
            PlayerStats.Instance.physicalHealth = 100;
            PlayerStats.Instance.mentalHealth = 80;
            PlayerStats.Instance.languageProficiency = 20;
            PlayerStats.Instance.culturalEtiquette = 15;
            PlayerStats.Instance.SaveStatsToDatabase();
            Debug.Log("<color=magenta>[DEBUG STAT]</color> Diatur ke: PH 100, MH 80, Bahasa 20, Etika 15 (Target: Rute C)");
        }

        // Tekan F2: Set Status Bahasa Tinggi, Etika Rendah (Bahasa 40, Etika 20) -> Menguji RUTE B (Mianzi)
        if (IsKeyPressed(KeyCode.F2))
        {
            PlayerStats.Instance.physicalHealth = 100;
            PlayerStats.Instance.mentalHealth = 80;
            PlayerStats.Instance.languageProficiency = 40;
            PlayerStats.Instance.culturalEtiquette = 20;
            PlayerStats.Instance.SaveStatsToDatabase();
            Debug.Log("<color=magenta>[DEBUG STAT]</color> Diatur ke: PH 100, MH 80, Bahasa 40, Etika 20 (Target: Rute B / Mianzi)");
        }

        // Tekan F3: Set Status Memenuhi Syarat (Bahasa 40, Etika 60) -> Menguji RUTE A (Sukses)
        if (IsKeyPressed(KeyCode.F3))
        {
            PlayerStats.Instance.physicalHealth = 100;
            PlayerStats.Instance.mentalHealth = 80;
            PlayerStats.Instance.languageProficiency = 40;
            PlayerStats.Instance.culturalEtiquette = 60;
            PlayerStats.Instance.SaveStatsToDatabase();
            Debug.Log("<color=magenta>[DEBUG STAT]</color> Diatur ke: PH 100, MH 80, Bahasa 40, Etika 60 (Target: Rute A)");
        }

        // Tekan F4: Memicu Kondisi Burnout (PH -100, MH -100)
        if (IsKeyPressed(KeyCode.F4))
        {
            Debug.Log("<color=red>[DEBUG]</color> Memicu pengujian Burnout...");
            PlayerStats.Instance.ModifyStats(0, 0, -100, -100, 0, 0);
        }

        // Tekan F5: Simulasi Hari ke-30 dan picu Evaluasi Tengah Semester secara paksa
        if (IsKeyPressed(KeyCode.F5))
        {
            Debug.Log("<color=cyan>[DEBUG]</color> Memaksa picu Evaluasi Tengah Semester (Hari 30)...");
            // Reset flag agar evaluasi bisa diulangi tanpa harus ganti hari
            GameManager.Instance.midtermEvaluasiSudahDijalankan = false;
            GameManager.Instance.currentDay = 30;
            GameManager.Instance.EksekusiEvaluasiTengahSemester();
        }

        // Tekan F6: Jalankan Headless Simulator Arketipe PURE ACADEMIC (60 Hari)
        if (IsKeyPressed(KeyCode.F6))
        {
            if (BalancingSimulator.Instance != null)
                BalancingSimulator.Instance.JalankanSimulasi(PlayerArchetype.PureAcademic);
            else
                Debug.LogWarning("[ActivitySimulator] BalancingSimulator.Instance tidak ditemukan.");
        }

        // Tekan F7: Jalankan Headless Simulator Arketipe PURE SOCIAL (60 Hari)
        if (IsKeyPressed(KeyCode.F7))
        {
            if (BalancingSimulator.Instance != null)
                BalancingSimulator.Instance.JalankanSimulasi(PlayerArchetype.PureSocial);
            else
                Debug.LogWarning("[ActivitySimulator] BalancingSimulator.Instance tidak ditemukan.");
        }

        // Tekan F8: Jalankan Headless Simulator Arketipe BALANCED (60 Hari)
        if (IsKeyPressed(KeyCode.F8))
        {
            if (BalancingSimulator.Instance != null)
                BalancingSimulator.Instance.JalankanSimulasi(PlayerArchetype.Balanced);
            else
                Debug.LogWarning("[ActivitySimulator] BalancingSimulator.Instance tidak ditemukan.");
        }

        // Tekan F9: Ekspor database log ke file CSV
        if (IsKeyPressed(KeyCode.F9))
        {
            if (TelemetryLogger.Instance != null)
            {
                TelemetryLogger.Instance.ExportLogsToCSV();
            }
        }
    }

    // Helper pembaca input kompatibel untuk New Input System dan Legacy Input
    private bool IsKeyPressed(KeyCode key)
    {
#if ENABLE_INPUT_SYSTEM
        var kb = Keyboard.current;
        if (kb != null)
        {
            switch (key)
            {
                case KeyCode.Alpha1: return kb.digit1Key.wasPressedThisFrame || kb.numpad1Key.wasPressedThisFrame;
                case KeyCode.Alpha2: return kb.digit2Key.wasPressedThisFrame || kb.numpad2Key.wasPressedThisFrame;
                case KeyCode.Space: return kb.spaceKey.wasPressedThisFrame;
                case KeyCode.D: return kb.dKey.wasPressedThisFrame;
                case KeyCode.Return: return kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame;
                case KeyCode.F1: return kb.f1Key.wasPressedThisFrame;
                case KeyCode.F2: return kb.f2Key.wasPressedThisFrame;
                case KeyCode.F3: return kb.f3Key.wasPressedThisFrame;
                case KeyCode.F4: return kb.f4Key.wasPressedThisFrame;
                case KeyCode.F5: return kb.f5Key.wasPressedThisFrame;
                case KeyCode.F6: return kb.f6Key.wasPressedThisFrame;
                case KeyCode.F7: return kb.f7Key.wasPressedThisFrame;
                case KeyCode.F8: return kb.f8Key.wasPressedThisFrame;
                case KeyCode.F9: return kb.f9Key.wasPressedThisFrame;
            }
        }
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetKeyDown(key);
#else
        return false;
#endif
    }

#if UNITY_EDITOR
    [UnityEditor.MenuItem("Game Debug/Test Route C (Bahasa 20, Etika 15)")]
    public static void TestRouteC()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("[Test] Jalankan Play Mode terlebih dahulu sebelum menjalankan pengujian rute.");
            return;
        }

        // Pulihkan status ke kondisi aman dan target stat Rute C
        PlayerStats.Instance.physicalHealth = 100;
        PlayerStats.Instance.mentalHealth = 80;
        PlayerStats.Instance.languageProficiency = 20;
        PlayerStats.Instance.culturalEtiquette = 15;
        PlayerStats.Instance.SaveStatsToDatabase();

        Debug.Log("<color=magenta>[DEBUG STAT]</color> Diatur ke: PH 100, MH 80, Bahasa 20, Etika 15 (Target: Rute C)");
        DialogueManager.Instance.StartDialogue(1001);
        DialogueManager.Instance.SelectOption(1);
    }

    [UnityEditor.MenuItem("Game Debug/Test Route B (Bahasa 40, Etika 20)")]
    public static void TestRouteB()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("[Test] Jalankan Play Mode terlebih dahulu sebelum menjalankan pengujian rute.");
            return;
        }

        // Pulihkan status ke kondisi aman dan target stat Rute B (Mianzi)
        PlayerStats.Instance.physicalHealth = 100;
        PlayerStats.Instance.mentalHealth = 80;
        PlayerStats.Instance.languageProficiency = 40;
        PlayerStats.Instance.culturalEtiquette = 20;
        PlayerStats.Instance.SaveStatsToDatabase();

        Debug.Log("<color=magenta>[DEBUG STAT]</color> Diatur ke: PH 100, MH 80, Bahasa 40, Etika 20 (Target: Rute B / Mianzi)");
        DialogueManager.Instance.StartDialogue(1001);
        DialogueManager.Instance.SelectOption(1);
    }

    [UnityEditor.MenuItem("Game Debug/Test Route A (Bahasa 40, Etika 60)")]
    public static void TestRouteA()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("[Test] Jalankan Play Mode terlebih dahulu sebelum menjalankan pengujian rute.");
            return;
        }

        // Pulihkan status ke kondisi sehat dan etika tinggi
        PlayerStats.Instance.physicalHealth = 100;
        PlayerStats.Instance.mentalHealth = 80;
        PlayerStats.Instance.languageProficiency = 40;
        PlayerStats.Instance.culturalEtiquette = 60;
        PlayerStats.Instance.SaveStatsToDatabase();

        Debug.Log("<color=magenta>[DEBUG STAT]</color> Diatur ke: PH 100, MH 80, Bahasa 40, Etika 60 (Target: Rute A)");
        DialogueManager.Instance.StartDialogue(1001);
        DialogueManager.Instance.SelectOption(1);
    }

    [UnityEditor.MenuItem("Game Debug/Trigger Burnout Test (PH 0, MH 0)")]
    public static void TestBurnout()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("[Test] Jalankan Play Mode terlebih dahulu sebelum memicu Burnout.");
            return;
        }

        Debug.Log("<color=red>[DEBUG]</color> Mengurangi status kesehatan ke 0 untuk memicu Forced Sick Day...");
        PlayerStats.Instance.ModifyStats(0, 0, -100, -100, 0, 0);
    }

    [UnityEditor.MenuItem("Game Debug/Midterm Eval — Simulasi LULUS (Teori 60, Praktis 55)")]
    public static void TestMidtermLulus()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("[Test] Jalankan Play Mode terlebih dahulu untuk menguji Evaluasi Tengah Semester.");
            return;
        }

        // Pastikan stat melampaui ambang kelulusan
        PlayerStats.Instance.physicalHealth    = 100;
        PlayerStats.Instance.mentalHealth       = 80;
        PlayerStats.Instance.academicTheoretical = 60;
        PlayerStats.Instance.academicPractical   = 55;
        PlayerStats.Instance.SaveStatsToDatabase();

        Debug.Log("<color=cyan>[DEBUG MIDTERM]</color> Stat diatur ke Teori 60, Praktis 55 (Target: LULUS).");

        // Paksa trigger evaluasi
        GameManager.Instance.midtermEvaluasiSudahDijalankan = false;
        GameManager.Instance.currentDay = 30;
        GameManager.Instance.EksekusiEvaluasiTengahSemester();
    }

    [UnityEditor.MenuItem("Game Debug/Midterm Eval — Simulasi PROBATION (Teori 30, Praktis 25)")]
    public static void TestMidtermProbation()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("[Test] Jalankan Play Mode terlebih dahulu untuk menguji Evaluasi Tengah Semester.");
            return;
        }

        // Pastikan stat TIDAK memenuhi ambang kelulusan
        PlayerStats.Instance.physicalHealth    = 100;
        PlayerStats.Instance.mentalHealth       = 80;
        PlayerStats.Instance.academicTheoretical = 30;   // di bawah ambang 50
        PlayerStats.Instance.academicPractical   = 25;   // di bawah ambang 45
        PlayerStats.Instance.SaveStatsToDatabase();

        Debug.Log("<color=cyan>[DEBUG MIDTERM]</color> Stat diatur ke Teori 30, Praktis 25 (Target: PROBATION).");

        // Paksa trigger evaluasi
        GameManager.Instance.midtermEvaluasiSudahDijalankan = false;
        GameManager.Instance.currentDay = 30;
        GameManager.Instance.EksekusiEvaluasiTengahSemester();
    }

    [UnityEditor.MenuItem("Game Debug/Simulation: Pure Academic (F6)")]
    public static void MenuSimPureAcademic()
    {
        if (!Application.isPlaying) { Debug.LogWarning("[Simulasi] Jalankan Play Mode terlebih dahulu."); return; }
        if (BalancingSimulator.Instance != null) BalancingSimulator.Instance.JalankanSimulasi(PlayerArchetype.PureAcademic, instant: true);
    }

    [UnityEditor.MenuItem("Game Debug/Simulation: Pure Social (F7)")]
    public static void MenuSimPureSocial()
    {
        if (!Application.isPlaying) { Debug.LogWarning("[Simulasi] Jalankan Play Mode terlebih dahulu."); return; }
        if (BalancingSimulator.Instance != null) BalancingSimulator.Instance.JalankanSimulasi(PlayerArchetype.PureSocial, instant: true);
    }

    [UnityEditor.MenuItem("Game Debug/Simulation: Balanced (F8)")]
    public static void MenuSimBalanced()
    {
        if (!Application.isPlaying) { Debug.LogWarning("[Simulasi] Jalankan Play Mode terlebih dahulu."); return; }
        if (BalancingSimulator.Instance != null) BalancingSimulator.Instance.JalankanSimulasi(PlayerArchetype.Balanced, instant: true);
    }

    [UnityEditor.MenuItem("Game Debug/Sleep (Advance Day)")]
    public static void MenuSleep()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("[Test] Jalankan Play Mode terlebih dahulu untuk tidur.");
            return;
        }

        Debug.Log("<color=purple>[DEBUG]</color> Menjalankan EvaluasiAkhirHari (Tidur)...");
        GameManager.Instance.EvaluasiAkhirHari();
    }

    [UnityEditor.MenuItem("Game Debug/Export Telemetry Logs to CSV")]
    public static void MenuExportTelemetryCSV()
    {
        if (TelemetryLogger.Instance != null)
        {
            TelemetryLogger.Instance.ExportLogsToCSV();
        }
        else
        {
            // Ekspor manual langsung jika tidak di Play Mode
            string query = "SELECT * FROM tbl_telemetry_logs ORDER BY log_id ASC;";
            System.Data.DataTable dt = DatabaseManager.Instance.ExecuteQuery(query);
            if (dt == null || dt.Rows.Count == 0)
            {
                Debug.LogWarning("[Telemetry] Tidak ada data log untuk diekspor.");
                return;
            }

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                sb.Append(dt.Columns[i].ColumnName);
                if (i < dt.Columns.Count - 1) sb.Append(",");
            }
            sb.AppendLine();

            foreach (System.Data.DataRow row in dt.Rows)
            {
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    string value = row[i].ToString().Replace(",", ";");
                    sb.Append(value);
                    if (i < dt.Columns.Count - 1) sb.Append(",");
                }
                sb.AppendLine();
            }

            string exportPath = System.IO.Path.Combine(Application.persistentDataPath, $"telemetry_export_{System.DateTime.Now:yyyyMMdd_HHmmss}.csv");
            System.IO.File.WriteAllText(exportPath, sb.ToString());
            Debug.Log($"<color=green>[Telemetry Export Sukses]</color> Berkas CSV tersimpan di: <b>{exportPath}</b>");
        }
    }

    [UnityEditor.MenuItem("Game Debug/Phone/Edelweiss Intel: Li Haoran (102)")]
    public static void MenuPhoneEdelweissLi()
    {
        if (!Application.isPlaying) { Debug.LogWarning("[Phone] Jalankan Play Mode terlebih dahulu."); return; }
        if (PhoneOutingManager.Instance != null) PhoneOutingManager.Instance.CallEdelweissIntel(102);
    }

    [UnityEditor.MenuItem("Game Debug/Phone/Edelweiss Intel: Yang Mei (103)")]
    public static void MenuPhoneEdelweissYang()
    {
        if (!Application.isPlaying) { Debug.LogWarning("[Phone] Jalankan Play Mode terlebih dahulu."); return; }
        if (PhoneOutingManager.Instance != null) PhoneOutingManager.Instance.CallEdelweissIntel(103);
    }

    [UnityEditor.MenuItem("Game Debug/Phone/Edelweiss Intel: Xiang Bai (101)")]
    public static void MenuPhoneEdelweissXiang()
    {
        if (!Application.isPlaying) { Debug.LogWarning("[Phone] Jalankan Play Mode terlebih dahulu."); return; }
        if (PhoneOutingManager.Instance != null) PhoneOutingManager.Instance.CallEdelweissIntel(101);
    }

    [UnityEditor.MenuItem("Game Debug/Phone/Set Calendar to Weekend (Day 6 - Sabtu)")]
    public static void MenuSetWeekend()
    {
        if (!Application.isPlaying) { Debug.LogWarning("[Phone] Jalankan Play Mode terlebih dahulu."); return; }
        GameManager.Instance.currentDay = 6;
        GameManager.Instance.currentTimeBlock = TimeBlock.Siang;
        if (HUDController.Instance != null) HUDController.Instance.UpdateHUD();
        Debug.Log("<color=cyan>[Calendar Debug]</color> Kalender diset ke Hari 6 (Sabtu / Weekend, Siang).");
    }

    [UnityEditor.MenuItem("Game Debug/Phone/Weekend Outing: Li Haoran -> Distrik Elektronik (Favorite)")]
    public static void MenuOutingLiFav()
    {
        if (!Application.isPlaying) { Debug.LogWarning("[Phone] Jalankan Play Mode terlebih dahulu."); return; }
        if (PhoneOutingManager.Instance != null) PhoneOutingManager.Instance.AjakHangout(102, 2);
    }

    [UnityEditor.MenuItem("Game Debug/Phone/Weekend Outing: Li Haoran -> Kedai Teh (Hated)")]
    public static void MenuOutingLiHated()
    {
        if (!Application.isPlaying) { Debug.LogWarning("[Phone] Jalankan Play Mode terlebih dahulu."); return; }
        if (PhoneOutingManager.Instance != null) PhoneOutingManager.Instance.AjakHangout(102, 3);
    }

    [UnityEditor.MenuItem("Game Debug/Phone/Weekend Outing: Yang Mei -> Halal Street (Favorite)")]
    public static void MenuOutingYangFav()
    {
        if (!Application.isPlaying) { Debug.LogWarning("[Phone] Jalankan Play Mode terlebih dahulu."); return; }
        if (PhoneOutingManager.Instance != null) PhoneOutingManager.Instance.AjakHangout(103, 1);
    }
#endif
}