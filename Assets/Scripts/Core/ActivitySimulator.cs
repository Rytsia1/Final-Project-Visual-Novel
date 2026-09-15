using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// ALAT DEBUG / BALANCING / SIMULATION — bukan bagian dari alur gameplay normal.
/// Gameplay normal pemain SELALU melalui tombol UI di <see cref="ActivityButtonHandler"/>;
/// kelas ini hanya menyediakan pintasan keyboard untuk mempercepat playtest manual,
/// preset status untuk pengujian rute dialog, pemicu simulasi balancing headless (BalancingSimulator),
/// dan ekspor telemetry — semuanya untuk kebutuhan eksperimen balancing skripsi.
/// Seluruh input di Update() HANYA aktif di dalam Unity Editor (#if UNITY_EDITOR) agar
/// tidak pernah bocor ke build yang dimainkan pemain sungguhan.
/// </summary>
public class ActivitySimulator : MonoBehaviour
{
    void Update()
    {
#if UNITY_EDITOR
        // =====================================================================
        // AKSI CEPAT MANUAL (mirror aksi UI ActivityButtonHandler, untuk playtest
        // tanpa perlu klik tombol). CATATAN: Alpha2 & D adalah versi singkat yang
        // TIDAK menjalankan seluruh cabang narasi milik tombol UI aslinya — lihat
        // ActivityButtonHandler.OnClick_LunchWithLiHaoran / OnClick_ReportToLecturer.
        // =====================================================================
        // Tekan Angka 1: Pilih Aksi Belajar Mandiri (Studi Bahasa)
        if (IsKeyPressed(KeyCode.Alpha1))
        {
            Debug.Log("<color=white>[Input Aksi]</color> Devano memilih Belajar Kosakata Mandarin.");
            PlayerStats.Instance.ModifyStats(dLanguage: 15, dEtiquette: 0, dMental: -10, dPhysical: -5, dTheoretical: 0, dPractical: 0);
            if (TelemetryLogger.Instance != null) TelemetryLogger.Instance.LogActionSnapshot("Belajar Kosakata Mandarin");
            GameManager.Instance.GeserWaktu();
        }

        // Tekan Angka 2: Mengajak Makan Siang Li Haoran (NPC ID: 102)
        if (IsKeyPressed(KeyCode.Alpha2))
        {
            Debug.Log("<color=white>[Input Aksi]</color> Devano mengajak Li Haoran Makan Siang.");
            PlayerStats.Instance.ModifyStats(dLanguage: 0, dEtiquette: 5, dMental: 10, dPhysical: -5, dTheoretical: 0, dPractical: 0);
            SocialManager.Instance.TambahGuanxi(npcId: 102, penambahanGuanxi: 10, reduksiLoneliness: 30);
            if (TelemetryLogger.Instance != null) TelemetryLogger.Instance.LogActionSnapshot("Makan Siang Li Haoran");
            GameManager.Instance.GeserWaktu();
        }

        // Tekan Spasi: Langsung Geser Waktu (Skip Slot)
        if (IsKeyPressed(KeyCode.Space))
        {
            Debug.Log("<color=white>[Input Aksi]</color> Melewatkan waktu tanpa interaksi sosial.");
            if (TelemetryLogger.Instance != null) TelemetryLogger.Instance.LogActionSnapshot("Lewatkan Waktu");
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

        // =====================================================================
        // DEBUG STAT PRESET — Shortcut Debug Status untuk Menguji 3 Rute Dialog
        // =====================================================================
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

        // CATATAN: F5 (Quick Save) & F6 (Quick Load) SENGAJA TIDAK ditangani di sini.
        // Keduanya adalah fitur gameplay normal yang sudah dimiliki SaveManager.Update()
        // (lihat SaveManager.cs) — menduplikasinya di sini hanya berisiko membuat kedua
        // listener berebut frame yang sama tanpa manfaat tambahan.

        // =====================================================================
        // BALANCING SIMULATION (Headless, 60 Hari) — satu arketipe per tombol.
        // Simulasi mengunci diri sendiri (BalancingSimulator.isSimulating) sehingga
        // menekan tombol lain saat simulasi berjalan tidak akan memicu simulasi kedua.
        // =====================================================================
        // Tekan F9: Jalankan Headless Simulator Arketipe PURE ACADEMIC (60 Hari)
        if (IsKeyPressed(KeyCode.F9))
        {
            if (BalancingSimulator.Instance != null)
                BalancingSimulator.Instance.JalankanSimulasi(PlayerArchetype.PureAcademic);
            else
                Debug.LogWarning("[ActivitySimulator] BalancingSimulator.Instance tidak ditemukan.");
        }

        // Tekan F10: Jalankan Headless Simulator Arketipe PURE SOCIAL (60 Hari)
        if (IsKeyPressed(KeyCode.F10))
        {
            if (BalancingSimulator.Instance != null)
                BalancingSimulator.Instance.JalankanSimulasi(PlayerArchetype.PureSocial);
            else
                Debug.LogWarning("[ActivitySimulator] BalancingSimulator.Instance tidak ditemukan.");
        }

        // Tekan F11: Jalankan Headless Simulator Arketipe BALANCED (60 Hari)
        if (IsKeyPressed(KeyCode.F11))
        {
            if (BalancingSimulator.Instance != null)
                BalancingSimulator.Instance.JalankanSimulasi(PlayerArchetype.Balanced);
            else
                Debug.LogWarning("[ActivitySimulator] BalancingSimulator.Instance tidak ditemukan.");
        }

        // =====================================================================
        // TELEMETRY EXPORT — tombol terpisah dari simulasi (dulu salah dibagi F10
        // bersama Pure Social, sehingga satu kali tekan F10 menjalankan simulasi
        // SEKALIGUS ekspor CSV secara tidak sengaja). Catatan: setiap simulasi
        // balancing (F9/F10/F11) SUDAH otomatis mengekspor CSV di akhir run
        // (lihat BalancingSimulator.RoutineSimulasi) — F12 di sini untuk ekspor
        // manual/ad-hoc di luar konteks simulasi (mis. setelah playtest manual).
        // =====================================================================
        // Tekan F12: Ekspor seluruh log telemetry ke file CSV
        if (IsKeyPressed(KeyCode.F12))
        {
            if (TelemetryLogger.Instance != null)
            {
                TelemetryLogger.Instance.ExportLogsToCSV();
            }
            else
            {
                Debug.LogWarning("[ActivitySimulator] TelemetryLogger.Instance tidak ditemukan.");
            }
        }
#endif
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
                case KeyCode.F9: return kb.f9Key.wasPressedThisFrame;
                case KeyCode.F10: return kb.f10Key.wasPressedThisFrame;
                // F11 sebelumnya tidak ada case di sini: di bawah profil input yang HANYA
                // memakai New Input System (tanpa Legacy Input Manager aktif), tombol F11
                // akan selalu gagal terbaca walau komentar di Update() mengklaim ia memicu
                // simulasi Balanced. Ditambahkan agar F11 konsisten di semua konfigurasi input.
                case KeyCode.F11: return kb.f11Key.wasPressedThisFrame;
                case KeyCode.F12: return kb.f12Key.wasPressedThisFrame;
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

    // Label sebelumnya mengklaim hotkey (F6)/(F7)/(F8) — keliru: F6 adalah Quick Load
    // (SaveManager) dan hotkey Update() sesungguhnya untuk arketipe ini adalah F9/F10/F11.
    // Menu item ini sendiri TIDAK terikat ke tombol apa pun (hanya dapat diklik lewat
    // menu Editor); label diperbaiki agar tidak menyesatkan.
    [UnityEditor.MenuItem("Game Debug/Simulation: Pure Academic (Instant, sama seperti F9)")]
    public static void MenuSimPureAcademic()
    {
        if (!Application.isPlaying) { Debug.LogWarning("[Simulasi] Jalankan Play Mode terlebih dahulu."); return; }
        if (BalancingSimulator.Instance != null) BalancingSimulator.Instance.JalankanSimulasi(PlayerArchetype.PureAcademic, instant: true);
    }

    [UnityEditor.MenuItem("Game Debug/Simulation: Pure Social (Instant, sama seperti F10)")]
    public static void MenuSimPureSocial()
    {
        if (!Application.isPlaying) { Debug.LogWarning("[Simulasi] Jalankan Play Mode terlebih dahulu."); return; }
        if (BalancingSimulator.Instance != null) BalancingSimulator.Instance.JalankanSimulasi(PlayerArchetype.PureSocial, instant: true);
    }

    [UnityEditor.MenuItem("Game Debug/Simulation: Balanced (Instant, sama seperti F11)")]
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

    [UnityEditor.MenuItem("Game Debug/Greeting/Set Li Haoran Guanxi 85 (Tokimeki) & Trigger Pagi")]
    public static void MenuGreetingLiTokimeki()
    {
        if (!Application.isPlaying) { Debug.LogWarning("[Greeting Debug] Jalankan Play Mode terlebih dahulu."); return; }
        if (SocialManager.Instance != null)
        {
            var rel = SocialManager.Instance.relations.Find(r => r.npcId == 102);
            if (rel != null)
            {
                rel.guanxiScore = 85;
                SocialManager.Instance.EvaluasiAffectionState(rel);
            }
        }
        if (GameManager.Instance != null)
        {
            GameManager.Instance.currentDay = 2; // Selasa (Workday)
            GameManager.Instance.currentTimeBlock = TimeBlock.Pagi;
            GameManager.Instance.greetingTriggeredToday = false;
            GameManager.Instance.MulaiHari();
        }
    }

    [UnityEditor.MenuItem("Game Debug/Greeting/Set Yang Mei Guanxi 70 (Sahabat) & Trigger Pagi")]
    public static void MenuGreetingYangSahabat()
    {
        if (!Application.isPlaying) { Debug.LogWarning("[Greeting Debug] Jalankan Play Mode terlebih dahulu."); return; }
        if (SocialManager.Instance != null)
        {
            var rel = SocialManager.Instance.relations.Find(r => r.npcId == 103);
            if (rel != null)
            {
                rel.guanxiScore = 70;
                SocialManager.Instance.EvaluasiAffectionState(rel);
            }
        }
        if (GameManager.Instance != null)
        {
            GameManager.Instance.currentDay = 2;
            GameManager.Instance.currentTimeBlock = TimeBlock.Pagi;
            GameManager.Instance.greetingTriggeredToday = false;
            GameManager.Instance.MulaiHari();
        }
    }

    [UnityEditor.MenuItem("Game Debug/Greeting/Reset All Guanxi 40 & Trigger Pagi (No Greeting)")]
    public static void MenuGreetingResetNone()
    {
        if (!Application.isPlaying) { Debug.LogWarning("[Greeting Debug] Jalankan Play Mode terlebih dahulu."); return; }
        if (SocialManager.Instance != null)
        {
            foreach (var rel in SocialManager.Instance.relations)
            {
                rel.guanxiScore = 40;
                SocialManager.Instance.EvaluasiAffectionState(rel);
            }
        }
        if (GameManager.Instance != null)
        {
            GameManager.Instance.currentDay = 2;
            GameManager.Instance.currentTimeBlock = TimeBlock.Pagi;
            GameManager.Instance.greetingTriggeredToday = false;
            GameManager.Instance.MulaiHari();
        }
    }
#endif
}