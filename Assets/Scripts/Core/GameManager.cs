using System;
using System.Data;
using UnityEngine;

public enum TimeBlock
{
    Pagi,
    Siang,
    Malam
}

public enum WeatherState
{
    Cerah,
    Berawan,
    HujanBadai
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
    public WeatherState currentWeather = WeatherState.Cerah;
    public bool warningTriggeredToday = false;
    public bool greetingTriggeredToday = false;

    [Header("Milestone Akademik")]
    // Ambang batas kelulusan Evaluasi Tengah Semester (Hari ke-30)
    public const int PASS_THEORETICAL = 50;
    public const int PASS_PRACTICAL   = 45;
    // Flag pencegah double-trigger: evaluasi hanya berjalan sekali per playthrough
    [HideInInspector] public bool midtermEvaluasiSudahDijalankan = false;

    // Akses praktis status rumor global untuk evaluasi event
    public int globalRumorLevel => SocialManager.Instance != null ? SocialManager.Instance.globalRumorLevel : 0;

    public void ModifyGlobalRumor(int delta)
    {
        if (SocialManager.Instance != null)
        {
            SocialManager.Instance.globalRumorLevel = Mathf.Clamp(SocialManager.Instance.globalRumorLevel + delta, 0, 3);
            DatabaseManager.Instance.ExecuteNonQuery(
                $"UPDATE tbl_player_profile SET global_rumor_level = {SocialManager.Instance.globalRumorLevel} WHERE player_id = {playerId};");
            Debug.Log($"<color=orange>[Rumor Modified]</color> Global Rumor Level: {SocialManager.Instance.globalRumorLevel}");
        }
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
        string query = $"SELECT current_day, current_time_block, current_weather, greeting_triggered_today " +
                       $"FROM tbl_player_profile WHERE player_id = {playerId};";
        DataTable dt = DatabaseManager.Instance.ExecuteQuery(query);

        if (dt != null && dt.Rows.Count > 0)
        {
            DataRow row = dt.Rows[0];
            currentDay = Convert.ToInt32(row["current_day"]);
            string blockStr = row["current_time_block"].ToString();
            currentTimeBlock = (TimeBlock)Enum.Parse(typeof(TimeBlock), blockStr);

            if (dt.Columns.Contains("current_weather") && row["current_weather"] != DBNull.Value)
            {
                string weatherStr = row["current_weather"].ToString();
                if (Enum.TryParse(weatherStr, out WeatherState parsedWeather))
                {
                    currentWeather = parsedWeather;
                }
            }

            if (dt.Columns.Contains("greeting_triggered_today") && row["greeting_triggered_today"] != DBNull.Value)
            {
                greetingTriggeredToday = Convert.ToInt32(row["greeting_triggered_today"]) == 1;
            }
        }
        else
        {
            Debug.LogError($"<color=red>[GameManager Error]</color> Profil player_id {playerId} tidak ditemukan di tbl_player_profile!");
        }
    }

    // Konversi angka hari (1-60) ke nama hari (Hari 1 = Senin, Hari 7 = Minggu, berulang)
    public string GetDayName()
    {
        string[] namaHari = { "Senin", "Selasa", "Rabu", "Kamis", "Jumat", "Sabtu", "Minggu" };
        int index = (currentDay - 1) % 7;
        if (index < 0) index = 0;
        return namaHari[index];
    }

    // Format gabungan untuk HUD: "HARI 01 - SENIN"
    public string GetFormattedDay()
    {
        string dayPadded = currentDay < 10 ? $"0{currentDay}" : $"{currentDay}";
        return $"HARI {dayPadded} - {GetDayName().ToUpper()}";
    }

    // Evaluasi kalender: Hari 1-5 adalah Workday (Senin-Jumat), Hari 6-7 adalah Weekend (Sabtu-Minggu)
    public bool IsWorkday()
    {
        int dayOfWeek = (currentDay - 1) % 7; // 0 = Senin ... 4 = Jumat, 5 = Sabtu, 6 = Minggu
        return dayOfWeek < 5;
    }

    // Sistem cuaca harian acak (Persona Style). Perhitungan roll didelegasikan ke
    // WeatherSystem; GameManager tetap memutuskan kapan cuaca berubah dan efek sampingnya.
    public void RollDailyWeather()
    {
        currentWeather = WeatherSystem.RollDailyWeather();

        if (FlagManager.Instance != null)
        {
            FlagManager.Instance.SetFlag("weather_thunderstorm", currentWeather == WeatherState.HujanBadai ? 1 : 0, "Kondisi badai petir");
        }

        Debug.Log($"<color=cyan>[CUACA HARIAN]</color> Hari {currentDay} ({GetDayName()}): <b>{currentWeather}</b>");
    }

    // Titik awal setiap hari baru
    public void MulaiHari()
    {
        Debug.Log($"<color=green>=== MEMULAI {GetFormattedDay()} ({currentTimeBlock}) ===</color>");

        // Roll cuaca harian jika di blok Pagi
        if (currentTimeBlock == TimeBlock.Pagi)
        {
            RollDailyWeather();
        }

        // 0. Prioritas Tertinggi: Evaluasi Akhir Semester & Multi-Ending (Hari ke-60)
        if (currentDay == 60 && currentTimeBlock == TimeBlock.Pagi)
        {
            if (RelationshipProgressionManager.Instance != null)
            {
                RelationshipProgressionManager.Instance.EvaluateEndingAtDay60();
            }
            return;
        }

        // 1. Evaluasi Data-Driven Event Condition Engine (tbl_events)
        if (EventManager.Instance != null && EventManager.Instance.TryTriggerEligibleEvent())
        {
            return; // Tahan aktivitas otonom hingga dialog event selesai
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
        if (TelemetryLogger.Instance != null)
        {
            TelemetryLogger.Instance.LogActionSnapshot("Kelas Wajib Pagi");
        }
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

        if (TelemetryLogger.Instance != null)
        {
            TelemetryLogger.Instance.LogActionSnapshot($"Transisi Waktu: {currentTimeBlock}");
        }

        if (HUDController.Instance != null)
        {
            HUDController.Instance.UpdateHUD();
        }

        // Evaluasi Data-Driven Event Condition Engine saat memasuki blok waktu baru
        if (EventManager.Instance != null && EventManager.Instance.TryTriggerEligibleEvent())
        {
            return;
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
        // Perhitungan lulus/tidak lulus & penerapan reward/penalti didelegasikan ke
        // AcademicEvaluationSystem; GameManager tetap memutuskan node dialog berikutnya.
        bool? lulus = AcademicEvaluationSystem.EvaluateMidterm();
        if (lulus == null)
        {
            return; // Evaluasi dibatalkan (sudah di-log oleh AcademicEvaluationSystem)
        }

        // Tampilkan dialog Xiang Bai (node 5001 → 5002 atau 5003 tergantung branch DB)
        // Set tujuan next_node_id untuk opsi 5011 secara dinamis sesuai hasil evaluasi
        int targetNode = lulus.Value ? 5002 : 5003;
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
            TelemetryLogger.Instance.LogActionSnapshot($"Evaluasi Akhir Hari {currentDay}");
        }

        // 1. Eksekusi kalkulasi sosial (Guanxi Decay tetap berjalan meski sakit)
        if (SocialManager.Instance != null)
        {
            SocialManager.Instance.ProsesAkhirHari();
        }

        // 2. Pemulihan malam hari: darurat jika Burnout (dengan penalti), normal jika tidak.
        // Formula pemulihan didelegasikan ke BurnoutSystem; deteksi trigger burnout itu
        // sendiri tetap di PlayerStats.TriggerBurnoutState().
        BurnoutSystem.ApplyOvernightRecovery();

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
        int greetingFlag = greetingTriggeredToday ? 1 : 0;
        string query = $"UPDATE tbl_player_profile SET " +
                       $"current_day = {currentDay}, " +
                       $"current_time_block = '{currentTimeBlock}', " +
                       $"current_weather = '{currentWeather}', " +
                       $"greeting_triggered_today = {greetingFlag} " +
                       $"WHERE player_id = {playerId};";

        DatabaseManager.Instance.ExecuteNonQuery(query);
    }
}