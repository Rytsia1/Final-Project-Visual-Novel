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
    public static GameManager Instance { get; private set; }

    [Header("Identitas Sesi")]
    public int playerId = 1;

    [Header("State Manajemen Waktu")]
    public int currentDay = 1;
    public TimeBlock currentTimeBlock = TimeBlock.Pagi;
    public bool warningTriggeredToday = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
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

        // 1. Prioritas Tertinggi: Cek Ledakan Rumor Level 3
        if (SocialManager.Instance != null && SocialManager.Instance.globalRumorLevel >= 3)
        {
            Debug.LogWarning("<color=red>[FORCED EVENT]</color> Terjadi Ledakan Rumor! Mengunci kendali dan memanggil cutscene Dosen Xiang Bai.");
            DialogueManager.Instance.StartDialogue(3001);
            return;
        }

        // 2. Prioritas Menengah: Peringatan Edelweiss (Rumor Level 1 / Level 2 saat baru muncul)
        if (SocialManager.Instance != null && SocialManager.Instance.globalRumorLevel == 1 && currentTimeBlock == TimeBlock.Pagi && !warningTriggeredToday)
        {
            warningTriggeredToday = true;
            Debug.Log("<color=yellow>[WARNING EVENT]</color> Edelweiss mencegat Kenzo di depan asrama.");
            DialogueManager.Instance.StartDialogue(2001);
            return;
        }

        // 3. Rutinitas Normal: Kelas Wajib di Hari Kerja
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

    // Eksekusi penalti Burnout: Melewatkan seluruh blok waktu aktif hari ini
    public void EksekusiBurnoutLock()
    {
        Debug.LogWarning("<color=red>[BURNOUT LOCK]</color> Kenzo dipaksa istirahat di kamar asrama seharian penuh. Semua aktivitas terkunci.");

        // Notifikasi popup dialog ke pemain via DialogueUI
        if (DialogueUIController.Instance != null)
        {
            DialogueUIController.Instance.DisplayDialogue(
                "Sistem", 
                "Kenzo ambruk di tempat tidur karena kelelahan ekstrem. Kamu terpaksa absen kuliah dan istirahat seharian penuh untuk memulihkan kondisi."
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

        // 2. Jika dalam kondisi Burnout, Kenzo mendapat pemulihan darurat tetapi terkena penalti akademik/etika
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
            
            Debug.Log("<color=green>[Recovery Burnout]</color> Kenzo pulih dari kondisi sakit. Hari baru dimulai.");
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
        SaveGameState();

        Debug.Log($"<color=green>Kenzo telah tidur lelap. Memasuki Hari ke-{currentDay}.</color>");
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