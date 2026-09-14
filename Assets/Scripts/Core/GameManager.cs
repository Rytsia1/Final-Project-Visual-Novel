using System;
using System.Data;
using UnityEngine;

public enum TimeBlock
{
    Pagi,
    Siang,
    Malam
}

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<GameManager>();
            }
            return _instance;
        }
        private set { _instance = value; }
    }

    [Header("Identitas Sesi")]
    public int playerId = 1;

    [Header("State Manajemen Waktu")]
    public int currentDay = 1;
    public TimeBlock currentTimeBlock = TimeBlock.Pagi;
    public bool warningTriggeredToday = false;
    public bool greetingTriggeredToday = false;

    [Header("Milestone Akademik")]
    // Ambang batas kelulusan Evaluasi Tengah Semester (Hari ke-30)
    public const int PASS_THEORETICAL = 50;
    public const int PASS_PRACTICAL   = 45;
    // Flag pencegah double-trigger: evaluasi hanya berjalan sekali per playthrough
    [HideInInspector] public bool midtermEvaluasiSudahDijalankan = false;

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

    void Start()
    {
        // 0. Cek jika ada permintaan load slot tertunda dari Main Menu
        if (PlayerPrefs.HasKey("PENDING_LOAD_SLOT"))
        {
            int pendingSlot = PlayerPrefs.GetInt("PENDING_LOAD_SLOT");
            PlayerPrefs.DeleteKey("PENDING_LOAD_SLOT");
            PlayerPrefs.Save();

            if (SaveManager.Instance != null && SaveManager.Instance.HasSaveData(pendingSlot))
            {
                SaveManager.Instance.LoadGame(pendingSlot);
                if (HUDController.Instance != null)
                {
                    HUDController.Instance.UpdateHUD();
                }
                return;
            }
        }

        // 1. Muat state kalender & waktu dari profil pemain
        LoadGameState();

        // 2. Cegah race condition: Sinkronkan data PlayerStats ke memori terlebih dahulu
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.LoadStatsFromDatabase();
        }
        else
        {
            Debug.LogError("<color=red>[GameManager Error]</color> Instance PlayerStats tidak ditemukan di scene!");
            return;
        }

        // 3. Jalankan siklus hari setelah seluruh data di memori valid
        MulaiHari();
    }

    // Membaca progres kalender dari tabel tbl_player_profile
    public void LoadGameState()
    {
        string query = $"SELECT current_day, current_time_block FROM tbl_player_profile WHERE player_id = {playerId};";
        DataTable dt = DatabaseManager.Instance.ExecuteQuery(query);

        if (dt != null && dt.Rows.Count > 0)
        {
            currentDay = Convert.ToInt32(dt.Rows[0]["current_day"]);
            string blockStr = dt.Rows[0]["current_time_block"].ToString();
            currentTimeBlock = (TimeBlock)Enum.Parse(typeof(TimeBlock), blockStr);
        }
        else
        {
            Debug.LogError($"<color=red>[GameManager Error]</color> Profil player_id {playerId} tidak ditemukan di tbl_player_profile!");
        }
    }

    // Evaluasi kalender: Hari 1-5 adalah Workday (Senin-Jumat), Hari 6-7 adalah Weekend (Sabtu-Minggu)
    public bool IsWorkday()
    {
        int dayOfWeek = (currentDay - 1) % 7; // 0 = Senin ... 4 = Jumat, 5 = Sabtu, 6 = Minggu
        return dayOfWeek < 5;
    }

    // Titik awal setiap hari baru
    public void MulaiHari()
    {
        Debug.Log($"<color=green>=== MEMULAI HARI {currentDay} ({currentTimeBlock}) ===</color>");

        // 0. Prioritas Tertinggi: Evaluasi Akhir Semester & Multi-Ending (Hari ke-60)
        if (currentDay == 60 && currentTimeBlock == TimeBlock.Pagi)
        {
            if (RelationshipProgressionManager.Instance != null)
            {
                RelationshipProgressionManager.Instance.EvaluateEndingAtDay60();
            }
            return;
        }

        // 1. Evaluasi Event Condition System (Data-Driven Event Manager)
        // Mengevaluasi seluruh kondisi deklaratif di tbl_game_events:
        // Ledakan Rumor, Peringatan Dini Edelweiss, Evaluasi Tengah Semester (Day 30),
        // Krisis Fisik/Mental, Festival Budaya, dsb.
        if (EventManager.Instance != null && EventManager.Instance.TryEvaluateAndTriggerEvent(currentTimeBlock, currentDay))
        {
            return; // Event berhasil dipicu dan mengambil kendali alur
        }

        // 2. Pengecekan Dynamic Morning Greeting (Tokimeki Memorial Style) jika tidak ada interupsi event
        if (currentTimeBlock == TimeBlock.Pagi && !greetingTriggeredToday)
        {
            if (MorningGreetingManager.Instance != null && MorningGreetingManager.Instance.TryTriggerMorningGreeting())
            {
                greetingTriggeredToday = true;
                return; // Biarkan pemain membaca dialog sapaan sebelum masuk ke rutinitas harian
            }
        }

        // 3. Rutinitas Normal: Kelas Wajib di Hari Kerja / Otonomi Pemain di Akhir Pekan
        LanjutRutinitasPagi();
    }

    // Melanjutkan jadwal pagi setelah sapaan atau interupsi selesai
    public void LanjutRutinitasPagi()
    {
        if (IsWorkday() && currentTimeBlock == TimeBlock.Pagi)
        {
            EksekusiKelasWajib();
        }
        else
        {
            Debug.Log($"<color=cyan>[Otonomi Pemain]</color> Blok waktu aktif: {currentTimeBlock}. Silakan pilih aktivitas.");
        }

        if (HUDController.Instance != null)
        {
            HUDController.Instance.UpdateHUD();
        }
    }

    // Aktivitas Akademik Wajib (Sesuai Matriks Tabel 3.4)
    private void EksekusiKelasWajib()
    {
        Debug.Log("<color=orange>[Jadwal Wajib]</color> Menghadiri Kuliah Pemrograman & Data bersama Dosen Xiang Bai.");
        
        // Pengurangan beban fisik & mental, penambahan teori dan praktis
        PlayerStats.Instance.ModifyStats(
            dLanguage: 0, 
            dEtiquette: 0, 
            dMental: -20, 
            dPhysical: -15, 
            dTheoretical: 10, 
            dPractical: 5
        );

        // Kelas wajib pagi selesai -> Waktu bergeser otomatis ke blok Siang
        GeserWaktu();
    }

    // Perpindahan blok waktu sekuensial: Pagi -> Siang -> Malam -> Evaluasi Akhir Hari
    public void GeserWaktu()
    {
        if (currentTimeBlock == TimeBlock.Pagi)
        {
            currentTimeBlock = TimeBlock.Siang;
            Debug.Log($"<color=yellow>[Waktu Bergeser]</color> Memasuki blok: {currentTimeBlock}");
        }
        else if (currentTimeBlock == TimeBlock.Siang)
        {
            currentTimeBlock = TimeBlock.Malam;
            Debug.Log($"<color=yellow>[Waktu Bergeser]</color> Memasuki blok: {currentTimeBlock}");
        }
        else if (currentTimeBlock == TimeBlock.Malam)
        {
            EvaluasiAkhirHari();
            return; // EvaluasiAkhirHari akan menangani reset blok waktu ke Pagi
        }

        SaveGameState();

        if (HUDController.Instance != null)
        {
            HUDController.Instance.UpdateHUD();
        }
    }

    // =========================================================
    // MILESTONE: EVALUASI TENGAH SEMESTER (HARI KE-30)
    // =========================================================
    /// <summary>
    /// Dipanggil otomatis pada pagi Hari ke-30. Membandingkan stat akademik
    /// Devano terhadap ambang batas kelulusan, lalu memulai dialog node 5001.
    /// Reward/penalti diproses dari tbl_dialogue_options melalui DialogueManager
    /// setelah pemain memilih opsi di node 5002 (lulus) atau 5003 (gagal).
    /// </summary>
    public void EksekusiEvaluasiTengahSemester()
    {
        if (PlayerStats.Instance == null)
        {
            Debug.LogError("[Midterm] PlayerStats.Instance null — evaluasi dibatalkan.");
            return;
        }

        int teori   = PlayerStats.Instance.academicTheoretical;
        int praktis = PlayerStats.Instance.academicPractical;
        bool lulus  = (teori >= PASS_THEORETICAL) && (praktis >= PASS_PRACTICAL);

        Debug.Log($"<color=cyan>[MIDTERM EVAL]</color> Hari 30 — Teori: {teori}/{PASS_THEORETICAL}, Praktis: {praktis}/{PASS_PRACTICAL} → {(lulus ? "<color=green>LULUS</color>" : "<color=red>PROBATION</color>")}");

        // Catat peristiwa ke sistem telemetry
        if (TelemetryLogger.Instance != null)
        {
            string detail = lulus
                ? $"MIDTERM LULUS — Teori:{teori}, Praktis:{praktis}"
                : $"MIDTERM PROBATION — Teori:{teori} (min {PASS_THEORETICAL}), Praktis:{praktis} (min {PASS_PRACTICAL})";
            TelemetryLogger.Instance.RecordEvent("MIDTERM_EVALUATION", detail);
        }

        // Terapkan reward / penalti langsung sebelum dialog
        if (lulus)
        {
            // Reward: Guanxi naik (diwakili MH+15 karena rasa percaya diri) + kepercayaan dosen
            PlayerStats.Instance.ModifyStats(
                dLanguage: 0, dEtiquette: 0,
                dMental: 15, dPhysical: 0,
                dTheoretical: 0, dPractical: 0
            );
            if (SocialManager.Instance != null)
                SocialManager.Instance.TambahGuanxi(npcId: 101, penambahanGuanxi: 20, reduksiLoneliness: 10);
        }
        else
        {
            // Penalti: Academic Probation — MH turun, rumor bertambah, guanxi dosen turun
            PlayerStats.Instance.ModifyStats(
                dLanguage: 0, dEtiquette: 0,
                dMental: -25, dPhysical: 0,
                dTheoretical: 0, dPractical: 0
            );
            if (SocialManager.Instance != null)
            {
                SocialManager.Instance.TambahGuanxi(npcId: 101, penambahanGuanxi: -20, reduksiLoneliness: 0);
                SocialManager.Instance.TambahRumor(20);
            }
        }

        // Tampilkan dialog Xiang Bai (node 5001 → 5002 atau 5003 tergantung branch DB)
        // Set tujuan next_node_id untuk opsi 5011 secara dinamis sesuai hasil evaluasi
        int targetNode = lulus ? 5002 : 5003;
        DatabaseManager.Instance.ExecuteNonQuery($"UPDATE tbl_dialogue_options SET next_node_id = {targetNode} WHERE option_id = 5011;");

        if (DialogueManager.Instance != null)
            DialogueManager.Instance.StartDialogue(5001);
    }

    // Eksekusi penalti Burnout: Melewatkan seluruh blok waktu aktif hari ini
    public void EksekusiBurnoutLock()
    {
        Debug.LogWarning("<color=red>[BURNOUT LOCK]</color> Devano dipaksa istirahat di kamar asrama seharian penuh. Semua aktivitas terkunci.");

        // Notifikasi popup dialog ke pemain via DialogueUI
        if (DialogueUIController.Instance != null)
        {
            DialogueUIController.Instance.DisplayDialogue(
                "Sistem", 
                "Devano ambruk di tempat tidur karena kelelahan ekstrem. Kamu terpaksa absen kuliah dan istirahat seharian penuh untuk memulihkan kondisi."
            );
            DialogueUIController.Instance.ShowCloseButton();
        }

        // Kunci tombol aktivitas di HUD
        if (HUDController.Instance != null)
        {
            if (HUDController.Instance.btnStudyLanguage != null) HUDController.Instance.btnStudyLanguage.interactable = false;
            if (HUDController.Instance.btnLunchWithNPC != null) HUDController.Instance.btnLunchWithNPC.interactable = false;
            if (HUDController.Instance.btnMeetLecturer != null) HUDController.Instance.btnMeetLecturer.interactable = false;
            if (HUDController.Instance.btnSleep != null) HUDController.Instance.btnSleep.interactable = true;
        }
    }

    // Evaluasi Akhir Hari (Malam -> Tidur -> Pagi Hari Berikutnya)
    public void EvaluasiAkhirHari()
    {
        Debug.Log("<color=purple>=== EVALUASI AKHIR HARI ===</color>");

        // Rekam status harian sebelum status di-reset untuk pemulihan malam
        if (TelemetryLogger.Instance != null)
        {
            TelemetryLogger.Instance.RecordDailySnapshot($"Evaluasi penutupan Hari {currentDay}");
        }

        // 1. Eksekusi kalkulasi sosial (Guanxi Decay tetap berjalan meski sakit)
        if (SocialManager.Instance != null)
        {
            SocialManager.Instance.ProsesAkhirHari();
        }

        // 2. Jika dalam kondisi Burnout, Devano mendapat pemulihan darurat tetapi terkena penalti akademik/etika
        if (PlayerStats.Instance != null && PlayerStats.Instance.isBurnedOut)
        {
            PlayerStats.Instance.isBurnedOut = false;
            
            // Pemulihan stamina darurat (PH +50, MH +50) dengan penalti etika/akademik karena bolos
            PlayerStats.Instance.ModifyStats(
                dLanguage: 0, 
                dEtiquette: -5, 
                dMental: 50, 
                dPhysical: 50, 
                dTheoretical: -5, 
                dPractical: 0
            );
            
            Debug.Log("<color=green>[Recovery Burnout]</color> Devano pulih dari kondisi sakit. Hari baru dimulai.");
        }
        else
        {
            // Pemulihan tidur normal harian (Persamaan 3.1: PH +40, MH +40)
            if (PlayerStats.Instance != null)
            {
                PlayerStats.Instance.ModifyStats(
                    dLanguage: 0, 
                    dEtiquette: 0, 
                    dMental: 40, 
                    dPhysical: 40, 
                    dTheoretical: 0, 
                    dPractical: 0
                );
            }
        }

        // 3. Inkrementasi hari kalender dan kembalikan siklus ke Pagi
        currentDay++;
        currentTimeBlock = TimeBlock.Pagi;
        warningTriggeredToday = false;
        greetingTriggeredToday = false;
        SaveGameState();

        Debug.Log($"<color=green>Devano telah tidur lelap. Memasuki Hari ke-{currentDay}.</color>");
        MulaiHari();
    }

    // Menyimpan state waktu ke SQLite
    public void SaveGameState()
    {
        string query = $"UPDATE tbl_player_profile SET " +
                       $"current_day = {currentDay}, " +
                       $"current_time_block = '{currentTimeBlock}' " +
                       $"WHERE player_id = {playerId};";

        DatabaseManager.Instance.ExecuteNonQuery(query);
    }
}