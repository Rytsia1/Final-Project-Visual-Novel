#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Data.SQLite;

public class DatabaseMigrationEditor : Editor
{
    [MenuItem("Game Database/Terapkan Migrasi Fitur Smartphone & Hangout")]
    public static void ApplyTokiMemoMigration()
    {
        string ddlVenues = @"
            CREATE TABLE IF NOT EXISTS tbl_venues (
                venue_id INTEGER PRIMARY KEY AUTOINCREMENT,
                venue_name TEXT NOT NULL,
                description TEXT,
                cost_ph INTEGER DEFAULT 10,
                cost_mh INTEGER DEFAULT 5,
                min_etiquette_req INTEGER DEFAULT 0
            );";

        string seedVenues = @"
            INSERT OR IGNORE INTO tbl_venues (venue_id, venue_name, description, cost_ph, cost_mh, min_etiquette_req) VALUES
            (1, 'Kantin Muslim / Halal Street', 'Kawasan kuliner halal dekat kampus.', 10, 0, 15),
            (2, 'Distrik Elektronik', 'Pusat komponen hardware dan gadget favorit mahasiswa IT.', 15, 5, 20),
            (3, 'Kedai Teh Tradisional', 'Tempat tenang sarat etika minum teh dan diskusi serius.', 10, -5, 45),
            (4, 'Perpustakaan Kota', 'Ruang baca modern dengan arsip riset lokal.', 10, -10, 30);";

        string ddlPreferences = @"
            CREATE TABLE IF NOT EXISTS tbl_npc_preferences (
                pref_id INTEGER PRIMARY KEY AUTOINCREMENT,
                npc_id INTEGER NOT NULL UNIQUE,
                favorite_venue_id INTEGER,
                hated_venue_id INTEGER,
                favorite_topic TEXT,
                sensitive_topic TEXT,
                intel_hint TEXT,
                FOREIGN KEY(npc_id) REFERENCES tbl_npc_list(npc_id),
                FOREIGN KEY(favorite_venue_id) REFERENCES tbl_venues(venue_id)
            );";

        string seedPreferences = @"
            INSERT OR IGNORE INTO tbl_npc_preferences (npc_id, favorite_venue_id, hated_venue_id, favorite_topic, sensitive_topic, intel_hint) VALUES
            (101, 3, 2, 'Metodologi Penelitian', 'Debat Nilai Terbuka', 'Dosen Xiang Bai sangat menghargai etika tradisional. Jika ke Kedai Teh, tuangkan cangkir teh untuknya terlebih dahulu.'),
            (102, 2, 3, 'Optimasi Algoritma & Modding PC', 'Privasi Finansial', 'Li Haoran lebih suka diajak santai keliling Distrik Elektronik dibanding tempat formal yang kaku.'),
            (103, 1, 2, 'Kuliner Asing & Event Kampus', 'Komparasi Nilai Ujian', 'Yang Mei suka eksplorasi kuliner. Membawanya ke Muslim Street akan mencairkan suasana dengan cepat.');";

        if (DatabaseManager.Instance != null)
        {
            DatabaseManager.Instance.ExecuteNonQuery(ddlVenues);
            DatabaseManager.Instance.ExecuteNonQuery(seedVenues);
            DatabaseManager.Instance.ExecuteNonQuery(ddlPreferences);
            DatabaseManager.Instance.ExecuteNonQuery(seedPreferences);

            try
            {
                DatabaseManager.Instance.ExecuteNonQuery("ALTER TABLE tbl_npc_relations ADD COLUMN affection_state INTEGER DEFAULT 0;");
            }
            catch
            {
                // Kolom sudah ada sebelumnya
            }
        }
        
        // Selalu pastikan file database baik di PersistentDataPath maupun StreamingAssetsPath termigrasi
        string[] dbPaths = new string[]
        {
            Path.Combine(Application.persistentDataPath, "game_database.db"),
            Path.Combine(Application.streamingAssetsPath, "game_database.db")
        };

        foreach (string dbPath in dbPaths)
        {
            if (File.Exists(dbPath))
            {
                try
                {
                    using (var conn = new SQLiteConnection("Data Source=" + dbPath + ";Version=3;"))
                    {
                        conn.Open();
                        using (var cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = ddlVenues; cmd.ExecuteNonQuery();
                            cmd.CommandText = seedVenues; cmd.ExecuteNonQuery();
                            cmd.CommandText = ddlPreferences; cmd.ExecuteNonQuery();
                            cmd.CommandText = seedPreferences; cmd.ExecuteNonQuery();
                            try
                            {
                                cmd.CommandText = "ALTER TABLE tbl_npc_relations ADD COLUMN affection_state INTEGER DEFAULT 0;";
                                cmd.ExecuteNonQuery();
                            }
                            catch { }
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"[Migrasi Warning] Path {dbPath}: {ex.Message}");
                }
            }
        }

        Debug.Log("<color=green>[Migrasi Sukses]</color> Tabel tbl_venues, tbl_npc_preferences, dan affection_state berhasil disuntikkan ke SQLite!");
    }
}
#endif
