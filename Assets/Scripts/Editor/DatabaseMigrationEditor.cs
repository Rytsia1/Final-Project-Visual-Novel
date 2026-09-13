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

        string ddlHangoutEvents = @"
            CREATE TABLE IF NOT EXISTS tbl_hangout_events (
                event_id INTEGER PRIMARY KEY AUTOINCREMENT,
                venue_id INTEGER NOT NULL,
                npc_id INTEGER NOT NULL,
                start_node_id INTEGER NOT NULL,
                FOREIGN KEY(venue_id) REFERENCES tbl_venues(venue_id),
                FOREIGN KEY(npc_id) REFERENCES tbl_npc_list(npc_id),
                FOREIGN KEY(start_node_id) REFERENCES tbl_dialogue_nodes(node_id)
            );";

        string seedHangoutEvents = @"
            INSERT OR REPLACE INTO tbl_hangout_events (event_id, venue_id, npc_id, start_node_id) VALUES
            (1, 3, 101, 6001),
            (2, 2, 102, 6101),
            (3, 1, 103, 6201);";

        string seedDilemmaNodes = @"
            INSERT OR REPLACE INTO tbl_dialogue_nodes (node_id, npc_id, speaker_name, dialogue_text, req_language, req_etiquette) VALUES
            (6001, 101, 'Xiang Bai', 'Pelayan kedai menyajikan teko teh panas di atas meja kayu. Teh baru saja diseduh dan uapnya mengepul wangi. Dosen Xiang Bai tersenyum dan mengamati gerak-gerikmu.', 0, 0),
            (6002, 101, 'Xiang Bai', 'Bagus sekali tata kramamu, Devano. Menghargai tradisi teh menunjukkan ketenangan dan rasa hormat yang mendalam. Obrolan riset kita sore ini sangat menyenangkan.', 0, 0),
            (6003, 101, 'Xiang Bai', 'Tehnya cukup harum. Mari kita nikmati sore ini sambil membahas sedikit ringkasan bahan kuliah minggu depan.', 0, 0),
            (6004, 101, 'Xiang Bai', 'Devano! Menyeruput langsung dari teko adalah pelanggaran etika yang memalukan di depan umum. Di mana kesopanan dan kendali dirimu sebagai mahasiswa akademisi?', 0, 0),

            (6101, 102, 'Li Haoran', 'Haoran sedang menawar periferal GPU bekas di salah satu kios elektronik, tetapi pemilik kios bersikeras mempertahankan harga tinggi dengan nada ketus.', 0, 0),
            (6102, 102, 'Li Haoran', 'Wah gila, analisismu tajam banget! Penjualnya langsung luluh dan kasih diskon mahasiswa. Kamu rekan berburu hardware terbaik, Devano!', 0, 0),
            (6103, 102, 'Li Haoran', 'Benar juga katamu, ngapain buang waktu di sini. Ayo kita coba cek toko komponen di blok seberang.', 0, 0),
            (6104, 102, 'Li Haoran', 'Waduh Devano, jangan teriak-teriak begitu dong... Penjualnya jadi marah dan kita diusir dari lorong ini. Malu-maluin banget didengar pengunjung lain.', 0, 0),

            (6201, 103, 'Yang Mei', 'Semangkuk mie tarik daging sapi hangat mengepul di meja. Yang Mei tampak memandangi botol sambal khas Indonesia yang kamu bawa dengan mata berbinar penasaran.', 0, 0),
            (6202, 103, 'Yang Mei', 'Enak banget! Pedas tapi ada aroma rempah yang gurih dan unik. Ceritamu tentang kuliner Nusantara seru sekali, Devano!', 0, 0),
            (6203, 103, 'Yang Mei', 'Mienya kenyal dan kuahnya gurih segar. Pembahasan agenda kampus tadi juga produktif. Kapan-kapan kita makan bareng lagi ya.', 0, 0),
            (6204, 103, 'Yang Mei', 'Uhuk! Uhuk... Pedas sekali! Air... tolong air! Kenapa kamu malah tertawa dan memaksaku makan sebanyak ini?!', 0, 0);";

        string seedDilemmaOptions = @"
            INSERT OR REPLACE INTO tbl_dialogue_options (option_id, node_id, option_text, next_node_id, effect_guanxi, effect_etiquette, effect_loneliness, effect_rumor, effect_mental, effect_physical, effect_theoretical, effect_practical, effect_language) VALUES
            (601, 6001, 'Tuangkan cangkir Dosen Xiang Bai terlebih dahulu dengan kedua tangan, lalu ketuk jari meja sebagai tanda hormat.', 6002, 25, 10, 50, 0, 5, 0, 0, 0, 0),
            (602, 6001, 'Tunggu dosen menuang sendiri, lalu isi cangkirmu sendiri dengan sopan.', 6003, 10, 0, 25, 0, 0, 0, 0, 0, 0),
            (603, 6001, 'Langsung seruput teh dari teko karena terburu-buru dan haus.', 6004, -20, -10, 0, 15, -10, 0, 0, 0, 0),

            (611, 6101, 'Bantu Haoran memeriksa spesifikasi chip dan dukung argumen teknisnya tanpa mempermalukan penjual.', 6102, 25, 0, 60, 0, 5, 0, 5, 0, 0),
            (612, 6101, 'Beri tahu Haoran untuk mencari perbandingan di kios sebelah saja.', 6103, 10, 0, 30, 0, 0, 0, 0, 0, 0),
            (613, 6101, 'Tertawa keras dan bilang barang itu rongsokan di depan muka penjual kios.', 6104, -15, -10, 0, 10, -5, 0, 0, 0, 0),

            (621, 6201, 'Tawarkan mencoba sambal Indonesia dengan sendok bersih sambil menceritakan latar belakang rempahnya.', 6202, 25, 0, 60, 0, 5, 0, 0, 0, 5),
            (622, 6201, 'Makan makanan masing-masing sambil membicarakan agenda kegiatan kampus minggu depan.', 6203, 10, 0, 30, 0, 0, 0, 0, 0, 0),
            (623, 6201, 'Memaksa Yang Mei mencicipi sambal dalam jumlah banyak hingga ia tersedak kepedasan.', 6204, -15, -10, 0, 0, -5, 0, 0, 0, 0);";

        if (DatabaseManager.Instance != null)
        {
            DatabaseManager.Instance.ExecuteNonQuery(ddlVenues);
            DatabaseManager.Instance.ExecuteNonQuery(seedVenues);
            DatabaseManager.Instance.ExecuteNonQuery(ddlPreferences);
            DatabaseManager.Instance.ExecuteNonQuery(seedPreferences);
            DatabaseManager.Instance.ExecuteNonQuery(ddlHangoutEvents);
            DatabaseManager.Instance.ExecuteNonQuery(seedHangoutEvents);
            DatabaseManager.Instance.ExecuteNonQuery(seedDilemmaNodes);

            string[] optCols = new string[] { "effect_etiquette", "effect_loneliness", "effect_rumor", "effect_mental", "effect_physical", "effect_theoretical", "effect_practical", "effect_language" };
            foreach (var col in optCols)
            {
                try { DatabaseManager.Instance.ExecuteNonQuery($"ALTER TABLE tbl_dialogue_options ADD COLUMN {col} INTEGER DEFAULT 0;"); } catch { }
            }
            DatabaseManager.Instance.ExecuteNonQuery(seedDilemmaOptions);

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
                            cmd.CommandText = ddlHangoutEvents; cmd.ExecuteNonQuery();
                            cmd.CommandText = seedHangoutEvents; cmd.ExecuteNonQuery();
                            cmd.CommandText = seedDilemmaNodes; cmd.ExecuteNonQuery();

                            string[] optCols = new string[] { "effect_etiquette", "effect_loneliness", "effect_rumor", "effect_mental", "effect_physical", "effect_theoretical", "effect_practical", "effect_language" };
                            foreach (var col in optCols)
                            {
                                try { cmd.CommandText = $"ALTER TABLE tbl_dialogue_options ADD COLUMN {col} INTEGER DEFAULT 0;"; cmd.ExecuteNonQuery(); } catch { }
                            }
                            cmd.CommandText = seedDilemmaOptions; cmd.ExecuteNonQuery();

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

        Debug.Log("<color=green>[Migrasi Sukses]</color> Tabel tbl_venues, tbl_npc_preferences, tbl_hangout_events, dan skenario dilema berhasil disuntikkan ke SQLite!");
    }
}
#endif
