#if UNITY_EDITOR
using System;
using System.IO;
using System.Data.SQLite;
using UnityEditor;
using UnityEngine;

// Alias untuk fleksibilitas kompatibilitas penamaan
using SqliteConnection = System.Data.SQLite.SQLiteConnection;
using SqliteTransaction = System.Data.SQLite.SQLiteTransaction;

/// <summary>
/// Editor script untuk melakukan seeding skenario naratif deterministik 7 Hari Pertama
/// ke dalam SQLite database (game_database.db) untuk demonstrasi Sidang PA 1.
/// Mengintegrasikan aset sprite Yang Mei, uji bahasa, uji mianzi, story flags,
/// dan evaluasi akademik mingguan Hari 7.
/// </summary>
public static class VerticalSliceDatabaseSeeder
{
    private const string YANG_MEI_SPRITE_PATH = "Assets/Character/Yang Mei/Yang Mei.png";
    private const string YANG_MEI_RESOURCES_PATH = "Assets/Resources/Character/Yang Mei/Yang Mei.png";
    private const string EDELWEISS_SPRITE_PATH = "Assets/Character/Edelweiss/Edelweiss.png";
    private const string EDELWEISS_RESOURCES_PATH = "Assets/Resources/Character/Edelweiss/Edelweiss.png";
    private const string CHARA_GIRL_SPRITE_PATH = "Assets/Textures/chara_girl_sprite.png";

    [MenuItem("Tokimeki TA/Seed Vertical Slice (Day 1-7)")]
    public static void SeedVerticalSlice()
    {
        Debug.Log("<color=cyan>[VerticalSliceDatabaseSeeder]</color> Memulai proses Seeding Vertical Slice (Hari 1-7)...");

        // 1. Konfigurasi Texture Importer untuk Aset Sprite Yang Mei
        ConfigureSpriteAssets();

        // 2. Eksekusi Seeding Database di StreamingAssets & PersistentDataPath
        string streamingDbPath = Path.Combine(Application.streamingAssetsPath, "game_database.db");
        string persistentDbPath = Path.Combine(Application.persistentDataPath, "game_database.db");

        bool successStreaming = ExecuteDatabaseSeeding(streamingDbPath);
        bool successPersistent = false;

        if (File.Exists(persistentDbPath))
        {
            successPersistent = ExecuteDatabaseSeeding(persistentDbPath);
        }

        if (successStreaming)
        {
            AssetDatabase.Refresh();
            string msg = "Data skenario naratif deterministik Hari 1-7 dan konfigurasi aset karakter Yang Mei berhasil di-seed ke database SQLite!\n\n" +
                         "• File Database: " + streamingDbPath + "\n" +
                         (File.Exists(persistentDbPath) ? "• Sinkronisasi Save/Play: " + persistentDbPath + "\n" : "") +
                         "• Aset Yang Mei: " + YANG_MEI_SPRITE_PATH + " (Sprite 2D/UI, Mipmaps Off, Clamp)\n" +
                         "• Event Terdaftar: 101 s/d 107 (Lengkap dengan alur Bahasa, Mianzi, Outing, & Evaluasi Akademik)";

            Debug.Log("<color=green>[VerticalSliceDatabaseSeeder]</color> Seeding sukses penuh!");
            EditorUtility.DisplayDialog("Vertical Slice Database Seeder", msg, "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("Vertical Slice Database Seeder - Gagal", 
                "Terjadi kesalahan saat melakukan seeding database. Periksa Console log untuk detail error.", "Tutup");
        }
    }

    [MenuItem("Tokimeki TA/Configure Character Textures")]
    public static void ConfigureCharacterTexturesMenu()
    {
        ConfigureSpriteAssets();
        EditorUtility.DisplayDialog("Konfigurasi Texture Karakter", 
            "Aset sprite Yang Mei telah berhasil dikonfigurasi dengan parameter Sprite (2D and UI), Single, Mip Maps Disabled, dan Wrap Mode Clamp.", "OK");
    }

    /// <summary>
    /// Tugas 1: Konfigurasi Importer Texture di Unity
    /// 1. TextureType diset ke Sprite (2D and UI)
    /// 2. SpriteMode diset ke Single
    /// 3. Generate Mip Maps diset ke false
    /// 4. WrapMode diset ke Clamp
    /// 5. Terapkan perubahan via AssetDatabase.ImportAsset()
    /// </summary>
    public static void ConfigureSpriteAssets()
    {
        // Pastikan file Yang Mei.png dan Edelweiss.png tersedia
        EnsureYangMeiFiles();
        EnsureEdelweissFiles();

        ConfigureSingleSpriteImporter(YANG_MEI_SPRITE_PATH);
        ConfigureSingleSpriteImporter(YANG_MEI_RESOURCES_PATH);
        ConfigureSingleSpriteImporter(EDELWEISS_SPRITE_PATH);
        ConfigureSingleSpriteImporter(EDELWEISS_RESOURCES_PATH);
        ConfigureSingleSpriteImporter(CHARA_GIRL_SPRITE_PATH);

        AssetDatabase.Refresh();
    }

    private static void EnsureEdelweissFiles()
    {
        if (!File.Exists(EDELWEISS_SPRITE_PATH))
        {
            EdelweissPlaceholderGenerator.GeneratePlaceholderSprite();
        }
    }

    private static void EnsureYangMeiFiles()
    {
        string dirChar = "Assets/Character/Yang Mei";
        string dirRes = "Assets/Resources/Character/Yang Mei";

        if (!Directory.Exists(dirChar)) Directory.CreateDirectory(dirChar);
        if (!Directory.Exists(dirRes)) Directory.CreateDirectory(dirRes);

        // Jika Yang Mei.png belum ada tapi ada erasebg-transformed.png, salin
        string rawTransformed = Path.Combine(dirChar, "erasebg-transformed.png");
        string targetMain = Path.Combine(dirChar, "Yang Mei.png");
        string targetRes = Path.Combine(dirRes, "Yang Mei.png");

        if (!File.Exists(targetMain) && File.Exists(rawTransformed))
        {
            File.Copy(rawTransformed, targetMain, true);
            Debug.Log("[Texture Importer] Menyalin erasebg-transformed.png -> Yang Mei.png");
        }

        if (File.Exists(targetMain) && !File.Exists(targetRes))
        {
            File.Copy(targetMain, targetRes, true);
            Debug.Log("[Texture Importer] Menyalin Yang Mei.png ke folder Resources");
        }
    }

    private static void ConfigureSingleSpriteImporter(string assetPath)
    {
        if (!File.Exists(assetPath)) return;

        TextureImporter ti = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (ti == null) return;

        bool isModified = false;

        if (ti.textureType != TextureImporterType.Sprite)
        {
            ti.textureType = TextureImporterType.Sprite;
            isModified = true;
        }

        if (ti.spriteImportMode != SpriteImportMode.Single)
        {
            ti.spriteImportMode = SpriteImportMode.Single;
            isModified = true;
        }

        if (ti.mipmapEnabled)
        {
            ti.mipmapEnabled = false;
            isModified = true;
        }

        if (ti.wrapMode != TextureWrapMode.Clamp)
        {
            ti.wrapMode = TextureWrapMode.Clamp;
            isModified = true;
        }

        if (ti.alphaIsTransparency != true)
        {
            ti.alphaIsTransparency = true;
            isModified = true;
        }

        if (isModified)
        {
            EditorUtility.SetDirty(ti);
            ti.SaveAndReimport();
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            Debug.Log($"<color=green>[Texture Importer]</color> Konfigurasi Sprite 2D berhasil diterapkan pada: {assetPath}");
        }
    }

    /// <summary>
    /// Mengeksekusi DDL & DML skenario Hari 1-7 dalam satu transaksi atomik.
    /// </summary>
    private static bool ExecuteDatabaseSeeding(string dbPath)
    {
        if (!File.Exists(dbPath))
        {
            Debug.LogError($"[VerticalSliceSeeder] File database tidak ditemukan di: {dbPath}");
            return false;
        }

        try
        {
            using (var conn = new SqliteConnection("Data Source=" + dbPath + ";Version=3;"))
            {
                conn.Open();

                // 1. Pastikan kolom-kolom tabel sesuai skema yang diminta (DDL Migrasi Idempoten)
                EnsureTableColumns(conn);

                // 2. Eksekusi seluruh data seeding dalam SATU SqliteTransaction atomik
                using (var transaction = conn.BeginTransaction())
                {
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.Transaction = transaction;

                        // A. SEED NPC MASTER & RELASI
                        SeedNpcData(cmd);

                        // B. SEED STORY FLAGS AWAL
                        SeedStoryFlags(cmd);

                        // C. SEED EVENTS (101 - 107)
                        SeedEvents(cmd);

                        // D. SEED DIALOGUE NODES (10101 - 10703)
                        SeedDialogueNodes(cmd);

                        // E. SEED DIALOGUE OPTIONS (1011 - 1071)
                        SeedDialogueOptions(cmd);
                    }

                    transaction.Commit();
                }

                conn.Close();
            }

            Debug.Log($"<color=green>[VerticalSliceSeeder]</color> Berhasil mengeksekusi transaksi seeding ke: {dbPath}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"<color=red>[VerticalSliceSeeder Error]</color> Gagal seeding {dbPath}: {ex.Message}\n{ex.StackTrace}");
            return false;
        }
    }

    /// <summary>
    /// Memastikan kolom-kolom baru tersedia pada SQLite tanpa merusak data eksisting.
    /// </summary>
    private static void EnsureTableColumns(SqliteConnection conn)
    {
        // tbl_events
        string[] eventCols = new string[]
        {
            "event_name TEXT DEFAULT ''",
            "day_trigger INTEGER DEFAULT 1",
            "day_end INTEGER DEFAULT 60",
            "time_slot TEXT DEFAULT 'Any'",
            "is_workday INTEGER DEFAULT 1",
            "required_language INTEGER DEFAULT 0",
            "required_etiquette INTEGER DEFAULT 0",
            "required_flag_key TEXT DEFAULT NULL",
            "required_flag_value INTEGER DEFAULT 1",
            "start_dialogue_id INTEGER DEFAULT 0",
            "fail_language_node_id INTEGER DEFAULT 0",
            "fail_mianzi_node_id INTEGER DEFAULT 0"
        };
        foreach (var col in eventCols)
        {
            try { ExecuteDirect(conn, $"ALTER TABLE tbl_events ADD COLUMN {col};"); } catch { }
        }

        // tbl_dialogue_nodes
        string[] nodeCols = new string[]
        {
            "dialogue_id INTEGER DEFAULT 0",
            "dialogue_code TEXT DEFAULT ''",
            "character_sprite_path TEXT DEFAULT ''",
            "next_dialogue_id INTEGER DEFAULT 0",
            "has_options INTEGER DEFAULT 0"
        };
        foreach (var col in nodeCols)
        {
            try { ExecuteDirect(conn, $"ALTER TABLE tbl_dialogue_nodes ADD COLUMN {col};"); } catch { }
        }

        // tbl_dialogue_options
        string[] optCols = new string[]
        {
            "dialogue_id INTEGER DEFAULT 0",
            "dialogue_code TEXT DEFAULT ''",
            "next_dialogue_id INTEGER DEFAULT 0",
            "next_dialogue_code TEXT DEFAULT ''",
            "stat_cost_ph INTEGER DEFAULT 0",
            "stat_cost_mh INTEGER DEFAULT 0",
            "stat_gain_theory INTEGER DEFAULT 0",
            "stat_gain_practice INTEGER DEFAULT 0",
            "stat_gain_language INTEGER DEFAULT 0",
            "stat_gain_etiquette INTEGER DEFAULT 0",
            "stat_gain_guanxi_npc_id INTEGER DEFAULT 0",
            "stat_gain_guanxi_amount INTEGER DEFAULT 0",
            "set_flag_key TEXT DEFAULT NULL",
            "set_flag_value INTEGER DEFAULT 1"
        };
        foreach (var col in optCols)
        {
            try { ExecuteDirect(conn, $"ALTER TABLE tbl_dialogue_options ADD COLUMN {col};"); } catch { }
        }
    }

    private static void ExecuteDirect(SqliteConnection conn, string sql)
    {
        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();
        }
    }

    // =========================================================================
    // DATA SEEDING IMPLEMENTATIONS
    // =========================================================================

    private static void SeedNpcData(SQLiteCommand cmd)
    {
        // 1. Pastikan entri Yang Mei (ID 1 & 103), Edelweiss (ID 2 & 102), dan Xiang Bai (ID 3 & 101) terdaftar di tbl_npc_list
        cmd.CommandText = @"
            INSERT OR REPLACE INTO tbl_npc_list (npc_id, npc_name, npc_role, role, base_decay_rate) VALUES
            (1, 'Yang Mei', 'Mahasiswi Berprestasi', 'Mahasiswi Berprestasi', 5),
            (2, 'Edelweiss', 'Mahasiswi Senior & Mentor Orientasi', 'Mahasiswi Senior & Mentor Orientasi', 4),
            (3, 'Dosen Xiang Bai', 'Dosen IT & Pembimbing Akademik', 'Dosen IT & Pembimbing Akademik', 2),
            (101, 'Xiang Bai (向白)', 'Dosen IT / Pembimbing Akademik', 'Dosen IT / Pembimbing Akademik', 2),
            (102, 'Edelweiss (艾德薇丝)', 'Mahasiswi Senior & Mentor Orientasi', 'Mahasiswi Senior & Mentor Orientasi', 4),
            (103, 'Yang Mei (杨梅)', 'Mahasiswi Berprestasi', 'Mahasiswi Berprestasi', 5);
        ";
        cmd.ExecuteNonQuery();

        // 2. Seeding tbl_npc_relations sesuai spesifikasi:
        // - ID 1: Yang Mei (Guanxi awal 20, Loneliness 0)
        // - ID 2: Edelweiss (Guanxi awal 20, Loneliness 0)
        // - ID 3: Dosen Xiang Bai (Guanxi awal 15, Loneliness 0)
        // Juga sinkronisasi dengan ID 101, 102, 103 untuk kompatibilitas sistem modul lain
        cmd.CommandText = @"
            INSERT OR REPLACE INTO tbl_npc_relations 
            (relation_id, player_id, npc_id, guanxi_score, loneliness_meter, rumor_contribution, days_since_last_interaction, affection_state, interacted_today)
            VALUES
            (1, 1, 1, 20, 0, 0, 0, 1, 0),
            (2, 1, 2, 20, 0, 0, 0, 1, 0),
            (3, 1, 3, 15, 0, 0, 0, 1, 0),
            (101, 1, 101, 15, 0, 0, 0, 1, 0),
            (102, 1, 102, 20, 0, 0, 0, 1, 0),
            (103, 1, 103, 20, 0, 0, 0, 1, 0);
        ";
        cmd.ExecuteNonQuery();
    }

    private static void SeedStoryFlags(SQLiteCommand cmd)
    {
        cmd.CommandText = @"
            INSERT OR REPLACE INTO tbl_story_flags (flag_name, flag_value, unlocked_day, description) VALUES
            ('prologue_completed', 0, 0, 'Status kelulusan dialog prolog kedatangan'),
            ('prologue_friendly', 0, 0, 'Menyambut orientasi Edelweiss dengan ramah'),
            ('prologue_independent', 0, 0, 'Memilih bersikap mandiri saat orientasi Edelweiss'),
            ('independence_flag', 0, 1, 'Memilih bersikap mandiri dari bantuan teman lokal'),
            ('dosen_teguran', 0, 3, 'Teguran keras akibat pelanggaran kehormatan mianzi dosen di forum umum'),
            ('weekend_outing_accepted', 0, 5, 'Ajakan jalan-jalan akhir pekan Yang Mei diterima'),
            ('academic_warning', 0, 7, 'Peringatan akademik minggu pertama akibat evaluasi belum memenuhi standar');
        ";
        cmd.ExecuteNonQuery();
    }

    private static void SeedEvents(SQLiteCommand cmd)
    {
        // 101: Hari 1 (Senin Pagi) - Temu Yang Mei
        // 102: Hari 2 (Selasa Siang) - Kantin Uji Bahasa (req_lang: 25, fail: 10203)
        // 103: Hari 3 (Rabu Pagi) - Dosen Mianzi (req_etiq: 20, fail: 10303)
        // 104: Hari 4 (Kamis Malam) - Chatting WeTalk Asrama
        // 105: Hari 5 (Jumat Siang) - Ajakan Akhir Pekan
        // 106: Hari 6 (Sabtu Siang) - Eksekusi Pilihan Akhir Pekan
        // 107: Hari 7 (Minggu Siang) - Evaluasi Kuis/Ujian Mingguan Dosen
        cmd.CommandText = @"
            INSERT OR REPLACE INTO tbl_events 
            (event_id, event_name, event_title, npc_id, day_trigger, day_end, min_day, max_day, 
             time_slot, time_block, is_workday, day_type, required_language, min_lang, required_etiquette, min_etiq, 
             required_flag_key, req_flag_name, required_flag_value, req_flag_val, 
             start_dialogue_id, start_node_id, fail_language_node_id, fail_mianzi_node_id, priority, is_repeatable, is_completed)
            VALUES
            (101, 'EVT_DAY1_MEET_YANGMEI', 'Pertemuan Pertama Yang Mei', 103, 1, 1, 1, 1, 'Morning', 'Pagi', 1, 'Workday', 0, 0, 0, 0, NULL, NULL, 1, 1, 10101, 10101, 0, 0, 85, 0, 0),
            (102, 'EVT_DAY2_CANTEEN_ORDER', 'Memesan Makanan di Kantin Kampus', NULL, 2, 2, 2, 2, 'Afternoon', 'Siang', 1, 'Workday', 25, 25, 0, 0, NULL, NULL, 1, 1, 10201, 10201, 10203, 0, 80, 0, 0),
            (103, 'EVT_DAY3_DOSEN_MIANZI', 'Konsultasi Lab & Uji Etika Mianzi', 101, 3, 3, 3, 3, 'Morning', 'Pagi', 1, 'Workday', 0, 0, 20, 20, NULL, NULL, 1, 1, 10301, 10301, 0, 10303, 80, 0, 0),
            (104, 'EVT_DAY4_ROOMMATE_CHAT', 'Obrolan Malam WeTalk Yang Mei', 103, 4, 4, 4, 4, 'Night', 'Malam', 1, 'Workday', 0, 0, 0, 0, NULL, NULL, 1, 1, 10401, 10401, 0, 0, 75, 0, 0),
            (105, 'EVT_DAY5_WEEKEND_INVITE', 'Ajakan Akhir Pekan Yang Mei', 103, 5, 5, 5, 5, 'Afternoon', 'Siang', 1, 'Workday', 0, 0, 0, 0, NULL, NULL, 1, 1, 10501, 10501, 0, 0, 80, 0, 0),
            (106, 'EVT_DAY6_OUTING_EXECUTE', 'Eksekusi Pilihan Akhir Pekan', 103, 6, 6, 6, 6, 'Afternoon', 'Siang', 0, 'Weekend', 0, 0, 0, 0, NULL, NULL, 1, 1, 10601, 10601, 0, 0, 80, 0, 0),
            (107, 'EVT_DAY7_WEEKLY_EXAM', 'Review & Evaluasi Akademik Mingguan', 101, 7, 7, 7, 7, 'Afternoon', 'Siang', 0, 'Weekend', 0, 0, 0, 0, NULL, NULL, 1, 1, 10701, 10701, 0, 0, 90, 0, 0);
        ";
        cmd.ExecuteNonQuery();
    }

    private static void SeedDialogueNodes(SQLiteCommand cmd)
    {
        string spriteYangMei = YANG_MEI_SPRITE_PATH;
        string spriteEdelweiss = EDELWEISS_SPRITE_PATH;

        cmd.CommandText = $@"
            INSERT OR REPLACE INTO tbl_dialogue_nodes 
            (node_id, dialogue_id, dialogue_code, speaker_name, dialogue_text, character_sprite_path, next_dialogue_id, has_options, npc_id, req_language, req_etiquette, priority)
            VALUES
            -- HARI 0: PROLOG KEDATANGAN (EDELWEISS)
            (10001, 10001, 'D_PROLOGUE_01', 'Devano', 'Gerbang kampus lebih besar dari yang kubayangkan. Koperku terhenti di celah ubin asrama. Di balik rasa lelah setelah penerbangan panjang dari Indonesia, ada kecemasan mendalam tentang adaptasi budaya dan bahasa di kampus internasional ini.', '', 10002, 0, 0, 0, 0, 10),
            (10002, 10002, 'D_PROLOGUE_02', 'Edelweiss', 'DEVANO?! Kamu Devano kan, mahasiswa pertukaran dari Indonesia? Halo! Aku Edelweiss, ditugaskan dosen untuk menyambutmu dan membantu orientasi awal di asrama ini. Selamat datang di Tiongkok!', '{spriteEdelweiss}', 10003, 0, 2, 0, 0, 10),
            (10003, 10003, 'D_PROLOGUE_03', 'Edelweiss', 'Satu tips penting untuk adaptasimu: jaga etika komunikasi dengan dosen dan staf asrama (jangan sampai membuat mereka kehilangan muka / diu mianzi), serta perhatikan kehadiran di kelas kuliah wajib. Besok pagi harimu resmi dimulai. Siap?', '{spriteEdelweiss}', 0, 1, 2, 0, 0, 10),
            (10004, 10004, 'D_PROLOGUE_END_A', 'Edelweiss', 'Sama-sama! Senang bisa menyambutmu. Istirahatlah yang cukup malam ini di kamar 503, besok pagi jam 08.00 perkuliahan wajibmu sudah dimulai. Semangat ya!', '{spriteEdelweiss}', 0, 0, 2, 0, 0, 10),
            (10005, 10005, 'D_PROLOGUE_END_B', 'Edelweiss', 'Bagus kalau kamu sudah siap mandiri! Jangan sungkan bertanya kalau ada kendala adaptasi ya. Selamat beristirahat untuk persiapan kelas besok pagi.', '{spriteEdelweiss}', 0, 0, 2, 0, 0, 10),

            -- HARI 1: PERTEMUAN YANG MEI
            (10101, 10101, 'D_DAY1_01', 'Yang Mei', 'Halo! Kamu mahasiswa pertukaran dari Indonesia ya? Namaku Yang Mei. Senang bertemu denganmu! Kalau kamu butuh bantuan orientasi kampus atau mencari ruang kuliah, jangan sungkan tanya aku ya!', '{spriteYangMei}', 0, 1, 103, 0, 0, 10),
            (10102, 10102, 'D_DAY1_02', 'Yang Mei', 'Sama-sama! Kita sekelas di mata kuliah Rekayasa Perangkat Lunak. Nanti istirahat siang, kita bisa ngobrol lagi di kantin. Semangat kuliahnya!', '{spriteYangMei}', 0, 0, 103, 0, 0, 10),
            (10103, 10103, 'D_DAY1_03', 'Yang Mei', 'Oh... begitu ya. Baiklah kalau kamu lebih nyaman mandiri. Tapi pintuku selalu terbuka kalau ada yang membingungkan. Sampai jumpa di kelas.', '{spriteYangMei}', 0, 0, 103, 0, 0, 10),

            -- HARI 2: KANTIN KAMPUS (UJI BAHASA)
            (10201, 10201, 'D_CANTEEN_ORDER', 'Devano', 'Kantin kampus (shitang) sangat ramai saat jam makan siang. Antrean di depan gerai mie tarik bergerak cepat. Tiba giliranku untuk memesan makanan langsung ke bibi kantin.', '', 0, 1, 0, 0, 0, 10),
            (10202, 10202, 'D_CANTEEN_SUCCESS', 'Bibi Kantin', 'Hao de! Semangkuk mie daging sapi panas siap disajikan! Pelafalan Mandarinmu bagus sekali untuk mahasiswa asing. Selamat menikmati!', '', 0, 0, 0, 0, 0, 10),
            (10203, 10203, 'D_CANTEEN_FAIL', 'Bibi Kantin', 'Hah? Shenme? Kamu mau pesan apa? Nada bicaramu keliru nak, antrean di belakangmu sudah panjang! Bibi kasih menu standar saja ya.', '', 10204, 0, 0, 0, 0, 10),
            (10204, 10204, 'D_CANTEEN_PANIC', 'Devano', 'Aku salah menyebut nada (tone) sehingga bibi kantin kebingungan dan mahasiswa lain di antrean mulai menghela napas. Rasa panik dan canggung membuat kepalaku pusing. (Mental Health -10, Physical Health +5).', '', 0, 0, 0, 0, 0, 10),

            -- HARI 3: DOSEN XIANG BAI (UJI MIANZI)
            (10301, 10301, 'D_DOSEN_START', 'Dosen Xiang Bai', 'Mahasiswa Devano, mari kita tinjau draf diagram arsitektur sistem untuk tugas lab pertamamu. Beberapa struktur modul di sini kurang efisien menurut standar departemen.', '', 0, 1, 101, 0, 0, 10),
            (10302, 10302, 'D_DOSEN_SUCCESS', 'Dosen Xiang Bai', 'Bagus. Sikap rendah hati dan kesediaanmu menerima telaah kritis adalah cerminan akademisi sejati. Terapkan pola ini pada tugas mingguanmu. (Teori +5, Guanxi Dosen +5).', '', 0, 0, 101, 0, 0, 10),
            (10303, 10303, 'D_DOSEN_FAIL_MIANZI', 'Dosen Xiang Bai', 'Lancang sekali kamu! Membantah dosen secara agresif di hadapan rekan-rekanmu bukan hanya merusak etika tata krama, tapi juga mencoreng kehormatan (diu lian) forum ilmiah ini! Jaga integritas dan kesopananmu!', '', 10304, 0, 101, 0, 0, 10),
            (10304, 10304, 'D_DOSEN_TEGURAN', 'Devano', 'Ruang lab mendadak hening membeku. Teguran keras Dosen Xiang Bai membuatku sangat terpukul dan rumor mulai menyebar di antara asisten lab. (Guanxi Dosen -15, Mental Health -15, Rumor +10, Flag dosen_teguran = 1).', '', 0, 0, 101, 0, 0, 10),

            -- HARI 4: WETALK CHAT ASRAMA
            (10401, 10401, 'D_CHAT_01', 'Yang Mei (WeTalk)', '[Pesan WeTalk] Ding-dong! Devano, masih sibuk ngoding di kamar asrama? Tadi siang aku dengar kamu menyelesaikan modul sulit di lab. Jangan lupa minum air hangat dan istirahat ya!', '{spriteYangMei}', 0, 1, 103, 0, 0, 10),
            (10402, 10402, 'D_CHAT_02', 'Yang Mei (WeTalk)', 'Hehe, senang mendengarnya! Obrolan singkat malam ini membuat suasana asrama jadi tidak sepi. Selamat tidur dan mimpi indah, Devano!', '{spriteYangMei}', 0, 0, 103, 0, 0, 10),

            -- HARI 5: AJAKAN AKHIR PEKAN
            (10501, 10501, 'D_INVITE_01', 'Yang Mei', 'Devano! Besok sudah akhir pekan lho. Di dekat stasiun ada pasar malam dan toko buku seni rupa tradisional. Mau pergi jalan-jalan bareng aku Sabtu siang besok?', '{spriteYangMei}', 0, 1, 103, 0, 0, 10),
            (10502, 10502, 'D_INVITE_YES', 'Yang Mei', 'Yey! Sudah kuputuskan jadwalnya ya! Sampai ketemu besok Sabtu siang di gerbang kampus. Jangan sampai terlambat!', '{spriteYangMei}', 0, 0, 103, 0, 0, 10),
            (10503, 10503, 'D_INVITE_NO', 'Yang Mei', 'Oh... begitu ya. Sayang sekali, padahal aku ingin menunjukkan tempat-tempat seru. Jangan terlalu memaksakan diri ya Devano...', '{spriteYangMei}', 0, 0, 103, 0, 0, 10),

            -- HARI 6: EKSEKUSI AKHIR PEKAN (OUTING vs BELAJAR)
            (10601, 10601, 'D_OUTING_DECIDE', 'Devano', 'Sabtu siang telah tiba. Hari ini adalah waktu untuk menentukan bagaimana akhir pekan pertama di kampus kuhabiskan.', '', 0, 1, 103, 0, 0, 10),
            (10602, 10602, 'D_OUTING_EXECUTE', 'Yang Mei', 'Wah, seru sekali tadi! Terima kasih sudah menemaniku melihat kaligrafi dan mencicipi camilan tradisional. Kamu cepat paham etika lokal saat tawar-menawar! (Guanxi +10, Cultural Etiquette +5, Physical Health -15).', '{spriteYangMei}', 0, 0, 103, 0, 0, 10),
            (10603, 10603, 'D_STUDY_EXECUTE', 'Devano', 'Suasana lab yang hening membantuku memahami abstraksi algoritma dan konfigurasi data. Meski tubuh lelah dan terasa sepi, kemampuan akademisku meningkat. (Teori +10, Praktik +5, Physical Health -10, Loneliness +10).', '', 0, 0, 0, 0, 0, 10),

            -- HARI 7: EVALUASI AKADEMIK MINGGUAN (ACADEMIC EVALUATION SYSTEM)
            (10701, 10701, 'D_EXAM_START', 'Dosen Xiang Bai', 'Perhatian seluruh mahasiswa. Satu minggu masa adaptasi dan perkuliahan intensif telah berlalu. Saya telah merekapitulasi skor pemahaman teori dan capaian praktikum kalian untuk evaluasi mingguan.', '', 0, 1, 101, 0, 0, 10),
            (10702, 10702, 'D_EXAM_PASS', 'Dosen Xiang Bai', '[HASIL REVIEW: LULUS] Mahasiswa Pratama, akumulasi pemahaman teori dan praktikummu melampaui standar minimal minggu pertama. Pertahankan konsistensi ini! (Mental Health +15, Status Akademik: Aman).', '', 0, 0, 101, 0, 0, 10),
            (10703, 10703, 'D_EXAM_FAIL', 'Dosen Xiang Bai', '[HASIL REVIEW: ACADEMIC WARNING] Mahasiswa Pratama, skor akumulasimu belum mencapai ambang batas minimum. Jika dalam minggu berikutnya tidak ada peningkatan, kamu terancam Academic Probation! Segera evaluasi metode belajarmu! (Mental Health -15, Flag: academic_warning = 1).', '', 0, 0, 101, 0, 0, 10);
        ";
        cmd.ExecuteNonQuery();
    }

    private static void SeedDialogueOptions(SQLiteCommand cmd)
    {
        cmd.CommandText = @"
            INSERT OR REPLACE INTO tbl_dialogue_options
            (option_id, dialogue_id, dialogue_code, node_id, option_text, next_dialogue_id, next_dialogue_code, next_node_id, 
             stat_cost_ph, delta_ph, stat_cost_mh, delta_mh, 
             stat_gain_theory, delta_theory, stat_gain_practice, delta_practice, 
             stat_gain_language, delta_lang, stat_gain_etiquette, delta_etiq, 
             stat_gain_guanxi_npc_id, target_npc_id, stat_gain_guanxi_amount, delta_guanxi, delta_loneliness, delta_rumor, 
             set_flag_key, set_flag_name, set_flag_value, set_flag_val, 
             min_lang, min_etiq, fail_language_node_id, fail_mianzi_node_id)
            VALUES
            -- HARI 0: PILIHAN PROLOG EDELWEISS
            (10001, 'D_PROLOGUE_03', 'D_PROLOGUE_03', 10003, 'Terima kasih atas tipsnya, salam kenal!', 'D_PROLOGUE_END_A', 'D_PROLOGUE_END_A', 10004, 0, 0, 0, 5, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 5, 5, 0, 0, 'prologue_friendly', 'prologue_friendly', 1, 1, 0, 0, 0, 0),
            (10002, 'D_PROLOGUE_03', 'D_PROLOGUE_03', 10003, 'Iya, saya sudah paham sistemnya.', 'D_PROLOGUE_END_B', 'D_PROLOGUE_END_B', 10005, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 0, 0, 0, 0, 'prologue_independent', 'prologue_independent', 1, 1, 0, 0, 0, 0),

            -- HARI 1: PILIHAN INTERAKSI YANG MEI
            (1011, 10101, 'D_DAY1_01', 10101, 'Terima kasih banyak, mohon bantuannya!', 10102, 'D_DAY1_02', 10102, 0, 0, 0, 5, 0, 0, 0, 0, 0, 0, 0, 0, 103, 103, 5, 5, 0, 0, NULL, NULL, 1, 1, 0, 0, 0, 0),
            (1012, 10101, 'D_DAY1_01', 10101, 'Saya bisa sendiri, terima kasih.', 10103, 'D_DAY1_03', 10103, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 103, 103, -2, -2, 0, 0, 'independence_flag', 'independence_flag', 1, 1, 0, 0, 0, 0),

            -- HARI 2: PILIHAN ORDER KANTIN (UJI BAHASA: min_lang 25)
            (1021, 10201, 'D_CANTEEN_ORDER', 10201, 'Pesan semangkuk mie daging sapi (Niurou Mian) dengan pelafalan Mandarin yang tepat.', 10202, 'D_CANTEEN_SUCCESS', 10202, 0, 10, 0, 0, 0, 0, 0, 0, 2, 2, 0, 0, NULL, NULL, 0, 0, 0, 0, NULL, NULL, 1, 1, 25, 0, 10203, 0),
            (1022, 10201, 'D_CANTEEN_ORDER', 10201, 'Tunjuk menu secara ragu-ragu menggunakan gestur tangan.', 10203, 'D_CANTEEN_FAIL', 10203, 0, 5, 10, -10, 0, 0, 0, 0, 0, 0, 0, 0, NULL, NULL, 0, 0, 0, 0, NULL, NULL, 1, 1, 0, 0, 0, 0),

            -- HARI 3: PILIHAN BIMBINGAN LAB (UJI MIANZI: min_etiq 20)
            (1031, 10301, 'D_DOSEN_START', 10301, 'Sapa formal (Nin) dan dengarkan bimbingan revisi dosen dengan penuh hormat.', 10302, 'D_DOSEN_SUCCESS', 10302, 0, 0, 0, 0, 5, 5, 0, 0, 0, 0, 0, 0, 101, 101, 5, 5, 0, 0, NULL, NULL, 1, 1, 0, 20, 0, 10303),
            (1032, 10301, 'D_DOSEN_START', 10301, 'Bantah koreksi dosen secara lantang di depan mahasiswa lain: ""Metode saya lebih modern, Pak!""', 10303, 'D_DOSEN_FAIL_MIANZI', 10303, 0, 0, 15, -15, 0, 0, 0, 0, 0, 0, 0, 0, 101, 101, -15, -15, 0, 10, 'dosen_teguran', 'dosen_teguran', 1, 1, 0, 0, 0, 0),

            -- HARI 4: PILIHAN WETALK CHAT
            (1041, 10401, 'D_CHAT_01', 10401, 'Balas pesan dengan ceria dan bertukar cerita tentang adaptasi di kampus.', 10402, 'D_CHAT_02', 10402, 0, 0, 0, 5, 0, 0, 0, 0, 0, 0, 0, 0, 103, 103, 4, 4, -15, 0, NULL, NULL, 1, 1, 0, 0, 0, 0),

            -- HARI 5: PILIHAN AJAKAN AKHIR PEKAN (OPPORTUNITY COST)
            (1051, 10501, 'D_INVITE_01', 10501, 'Terima ajakan: ""Boleh sekali! Aku sangat ingin menjelajahi pasar malam bersamamu.""', 10502, 'D_INVITE_YES', 10502, 0, 0, 0, 5, 0, 0, 0, 0, 0, 0, 0, 0, 103, 103, 5, 5, 0, 0, 'weekend_outing_accepted', 'weekend_outing_accepted', 1, 1, 0, 0, 0, 0),
            (1052, 10501, 'D_INVITE_01', 10501, 'Tolak demi belajar di lab: ""Maaf Yang Mei, aku harus memprioritaskan belajar di lab.""', 10503, 'D_INVITE_NO', 10503, 0, 0, 5, -5, 0, 0, 0, 0, 0, 0, 0, 0, 103, 103, -5, -5, 0, 0, 'weekend_outing_accepted', 'weekend_outing_accepted', 0, 0, 0, 0, 0, 0),

            -- HARI 6: PILIHAN SABTU SIANG (OUTING vs BELAJAR MANDIRI)
            (1061, 10601, 'D_OUTING_DECIDE', 10601, '[Pilihan Outing] Pergi ke Pasar Malam & Toko Buku bersama Yang Mei.', 10602, 'D_OUTING_EXECUTE', 10602, 15, -15, 0, 0, 0, 0, 0, 0, 0, 0, 5, 5, 103, 103, 10, 10, 0, 0, NULL, NULL, 1, 1, 0, 0, 0, 0),
            (1062, 10601, 'D_OUTING_DECIDE', 10601, '[Pilihan Belajar] Menetap di Lab Komputer untuk membedah teori & latihan praktikum.', 10603, 'D_STUDY_EXECUTE', 10603, 10, -10, 0, 0, 10, 10, 5, 5, 0, 0, 0, 0, NULL, NULL, 0, 0, 10, 0, NULL, NULL, 1, 1, 0, 0, 0, 0),

            -- HARI 7: EVALUASI MINGGUAN (DIARAHKAN OTOMATIS OLEH AcademicEvaluationSystem)
            (1071, 10701, 'D_EXAM_START', 10701, 'Dengarkan pengumuman hasil evaluasi mingguan dari Dosen Xiang Bai.', 10702, 'D_EXAM_PASS', 10702, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 101, 101, 0, 0, 0, 0, NULL, NULL, 1, 1, 0, 0, 0, 0);
        ";
        cmd.ExecuteNonQuery();
    }
}
#endif
