using System;
using System.Data;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance { get; private set; }

    [Header("Identitas Pemain")]
    public int playerId = 1;
    public string playerName = "Kenzo Pratama";

    [Header("Parameter Status (Runtime Memory)")]
    public int languageProficiency;
    public int culturalEtiquette;
    public int mentalHealth;
    public int physicalHealth;
    public int academicTheoretical;
    public int academicPractical;

    // Batas konstan parameter kesehatan fisik dan mental
    public const int MAX_HEALTH = 100;
    public const int MIN_HEALTH = 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Dikosongkan agar pemuatan data dipanggil secara eksplisit oleh GameManager.Start()
    }

    // Memuat parameter status dari tabel SQLite ke memori RAM
    public void LoadStatsFromDatabase()
    {
        string query = $"SELECT * FROM tbl_player_stats WHERE player_id = {playerId};";
        DataTable dt = DatabaseManager.Instance.ExecuteQuery(query);

        if (dt != null && dt.Rows.Count > 0)
        {
            DataRow row = dt.Rows[0];
            languageProficiency = Convert.ToInt32(row["language_proficiency"]);
            culturalEtiquette = Convert.ToInt32(row["cultural_etiquette"]);
            mentalHealth = Convert.ToInt32(row["mental_health"]);
            physicalHealth = Convert.ToInt32(row["physical_health"]);
            academicTheoretical = Convert.ToInt32(row["academic_theoretical"]);
            academicPractical = Convert.ToInt32(row["academic_practical"]);

            Debug.Log($"<color=cyan>[PlayerStats]</color> Data status {playerName} berhasil disinkronkan dari SQLite.");
        }
        else
        {
            Debug.LogError($"<color=red>[PlayerStats Error]</color> Baris data tidak ditemukan di tbl_player_stats untuk player_id: {playerId}");
        }
    }

    // Fungsi mutasi status berdasarkan Opportunity Cost aktivitas
    public void ModifyStats(int dLanguage, int dEtiquette, int dMental, int dPhysical, int dTheoretical, int dPractical)
    {
        languageProficiency = Mathf.Max(0, languageProficiency + dLanguage);
        culturalEtiquette = Mathf.Max(0, culturalEtiquette + dEtiquette);
        academicTheoretical = Mathf.Max(0, academicTheoretical + dTheoretical);
        academicPractical = Mathf.Max(0, academicPractical + dPractical);

        // Eksekusi Persamaan 3.1: Clamp batas nilai PH dan MH (0 - 100)
        physicalHealth = Mathf.Clamp(physicalHealth + dPhysical, MIN_HEALTH, MAX_HEALTH);
        mentalHealth = Mathf.Clamp(mentalHealth + dMental, MIN_HEALTH, MAX_HEALTH);

        Debug.Log($"<color=yellow>[Stat Update]</color> PH: {physicalHealth} | MH: {mentalHealth} | Bahasa: {languageProficiency} | Etika: {culturalEtiquette} | Teori: {academicTheoretical} | Praktis: {academicPractical}");

        // Evaluasi kondisi kritis kelelahan ekstrem (Burnout)
        if (physicalHealth <= MIN_HEALTH || mentalHealth <= MIN_HEALTH)
        {
            TriggerBurnoutWarning();
        }

        SaveStatsToDatabase();

        if (HUDController.Instance != null)
        {
            HUDController.Instance.UpdateHUD();
        }
    }

    private void TriggerBurnoutWarning()
    {
        Debug.LogWarning("<color=red>[CRITICAL ALERT]</color> Kenzo mengalami status BURNOUT! Nilai Physical Health atau Mental Health mencapai ambang batas 0.");

        if (TelemetryLogger.Instance != null)
        {
            TelemetryLogger.Instance.RecordCriticalEvent("BURNOUT", $"Kenzo kelelahan ekstrem pada PH: {physicalHealth}, MH: {mentalHealth}");
        }
    }

    // Menyimpan kembali perubahan data status runtime ke database SQLite
    public void SaveStatsToDatabase()
    {
        string updateQuery = $"UPDATE tbl_player_stats SET " +
                             $"language_proficiency = {languageProficiency}, " +
                             $"cultural_etiquette = {culturalEtiquette}, " +
                             $"mental_health = {mentalHealth}, " +
                             $"physical_health = {physicalHealth}, " +
                             $"academic_theoretical = {academicTheoretical}, " +
                             $"academic_practical = {academicPractical} " +
                             $"WHERE player_id = {playerId};";

        DatabaseManager.Instance.ExecuteNonQuery(updateQuery);
    }
}