using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;

public class TelemetryLogger : MonoBehaviour
{
    private static TelemetryLogger _instance;
    public static TelemetryLogger Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<TelemetryLogger>();
            }
            return _instance;
        }
        private set { _instance = value; }
    }

    [Header("Identitas Sesi")]
    public int playerId = 1;

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

    // 1. Merekam Snapshot Harian Komprehensif (Tren Grafik Balancing)
    public void RecordDailySnapshot(string note = "")
    {
        if (PlayerStats.Instance == null || GameManager.Instance == null || SocialManager.Instance == null) return;

        int day = GameManager.Instance.currentDay;
        string block = GameManager.Instance.currentTimeBlock.ToString();
        int ph = PlayerStats.Instance.physicalHealth;
        int mh = PlayerStats.Instance.mentalHealth;
        int lang = PlayerStats.Instance.languageProficiency;
        int etiq = PlayerStats.Instance.culturalEtiquette;
        int theo = PlayerStats.Instance.academicTheoretical;
        int prac = PlayerStats.Instance.academicPractical;
        int rumor = SocialManager.Instance.globalRumorLevel;

        // Hitung rata-rata Guanxi seluruh NPC
        float avgGuanxi = 0f;
        if (SocialManager.Instance.relations.Count > 0)
        {
            int totalGuanxi = 0;
            foreach (var r in SocialManager.Instance.relations) totalGuanxi += r.guanxiScore;
            avgGuanxi = (float)totalGuanxi / SocialManager.Instance.relations.Count;
        }

        string avgGuanxiStr = avgGuanxi.ToString("F2", CultureInfo.InvariantCulture);

        string query = $"INSERT INTO tbl_telemetry_logs " +
                       $"(player_id, in_game_day, time_block, event_type, ph, mh, language_proficiency, cultural_etiquette, academic_theoretical, academic_practical, global_rumor_level, avg_guanxi, details) " +
                       $"VALUES ({playerId}, {day}, '{block}', 'DAILY_SNAPSHOT', {ph}, {mh}, {lang}, {etiq}, {theo}, {prac}, {rumor}, {avgGuanxiStr}, '{EscapeSQL(note)}');";

        DatabaseManager.Instance.ExecuteNonQuery(query);
        Debug.Log($"<color=cyan>[Telemetry]</color> Snapshot Hari {day} berhasil direkam ke SQLite.");
    }

    // 2. Merekam Pilihan Respon Dialog & Evaluasi Stat (Pendeteksi Choke Points)
    public void RecordDialogueChoice(int nodeId, int optionId, string outcomeRoute, string evaluationResult)
    {
        if (GameManager.Instance == null || PlayerStats.Instance == null) return;

        int day = GameManager.Instance.currentDay;
        string block = GameManager.Instance.currentTimeBlock.ToString();
        string detailJson = $"{{\"node_id\":{nodeId}, \"option_id\":{optionId}, \"route\":\"{outcomeRoute}\", \"result\":\"{evaluationResult}\"}}";

        int rumor = SocialManager.Instance != null ? SocialManager.Instance.globalRumorLevel : 0;

        string query = $"INSERT INTO tbl_telemetry_logs " +
                       $"(player_id, in_game_day, time_block, event_type, ph, mh, language_proficiency, cultural_etiquette, academic_theoretical, academic_practical, global_rumor_level, avg_guanxi, details) " +
                       $"VALUES ({playerId}, {day}, '{block}', 'DIALOGUE_CHOICE', " +
                       $"{PlayerStats.Instance.physicalHealth}, {PlayerStats.Instance.mentalHealth}, " +
                       $"{PlayerStats.Instance.languageProficiency}, {PlayerStats.Instance.culturalEtiquette}, " +
                       $"{PlayerStats.Instance.academicTheoretical}, {PlayerStats.Instance.academicPractical}, " +
                       $"{rumor}, 0, '{EscapeSQL(detailJson)}');";

        DatabaseManager.Instance.ExecuteNonQuery(query);
        Debug.Log($"<color=cyan>[Telemetry]</color> Pilihan dialog dicatat: Node {nodeId} -> Opsi {optionId} ({outcomeRoute}).");
    }

    // 3. Merekam Anomali Kritis (Burnout, Ledakan Rumor, Ujian Gagal)
    public void RecordCriticalEvent(string eventType, string description)
    {
        if (GameManager.Instance == null || PlayerStats.Instance == null) return;

        int day = GameManager.Instance.currentDay;
        string block = GameManager.Instance.currentTimeBlock.ToString();

        int rumor = SocialManager.Instance != null ? SocialManager.Instance.globalRumorLevel : 0;

        string query = $"INSERT INTO tbl_telemetry_logs " +
                       $"(player_id, in_game_day, time_block, event_type, ph, mh, language_proficiency, cultural_etiquette, academic_theoretical, academic_practical, global_rumor_level, avg_guanxi, details) " +
                       $"VALUES ({playerId}, {day}, '{block}', '{EscapeSQL(eventType)}', " +
                       $"{PlayerStats.Instance.physicalHealth}, {PlayerStats.Instance.mentalHealth}, " +
                       $"{PlayerStats.Instance.languageProficiency}, {PlayerStats.Instance.culturalEtiquette}, " +
                       $"{PlayerStats.Instance.academicTheoretical}, {PlayerStats.Instance.academicPractical}, " +
                       $"{rumor}, 0, '{EscapeSQL(description)}');";

        DatabaseManager.Instance.ExecuteNonQuery(query);
        Debug.LogWarning($"<color=red>[Telemetry Critical]</color> Log anomali {eventType} tercatat: {description}");
    }

    // Alias metode generik untuk perekaman event khusus
    public void RecordEvent(string eventType, string description)
    {
        RecordCriticalEvent(eventType, description);
    }

    // 4. Ekspor Data Log SQLite ke File CSV untuk Pemodelan Grafik Bab 4
    public void ExportLogsToCSV()
    {
        if (DatabaseManager.Instance == null)
        {
            Debug.LogWarning("[Telemetry] DatabaseManager.Instance tidak ditemukan.");
            return;
        }

        string query = "SELECT * FROM tbl_telemetry_logs ORDER BY log_id ASC;";
        DataTable dt = DatabaseManager.Instance.ExecuteQuery(query);

        if (dt == null || dt.Rows.Count == 0)
        {
            Debug.LogWarning("[Telemetry] Tidak ada data log untuk diekspor.");
            return;
        }

        StringBuilder sb = new StringBuilder();

        // Tulis Header Kolom
        for (int i = 0; i < dt.Columns.Count; i++)
        {
            sb.Append(dt.Columns[i].ColumnName);
            if (i < dt.Columns.Count - 1) sb.Append(",");
        }
        sb.AppendLine();

        // Tulis Baris Data
        foreach (DataRow row in dt.Rows)
        {
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                string value = row[i].ToString().Replace(",", ";"); // Escape koma dalam field text
                sb.Append(value);
                if (i < dt.Columns.Count - 1) sb.Append(",");
            }
            sb.AppendLine();
        }

        string exportPath = Path.Combine(Application.persistentDataPath, $"telemetry_export_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        File.WriteAllText(exportPath, sb.ToString());

        Debug.Log($"<color=green>[Telemetry Export Sukses]</color> Berkas CSV tersimpan di: <b>{exportPath}</b>");
    }

    private string EscapeSQL(string text)
    {
        if (string.IsNullOrEmpty(text)) return "";
        return text.Replace("'", "''");
    }
}
