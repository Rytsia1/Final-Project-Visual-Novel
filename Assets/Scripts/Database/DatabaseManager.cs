using System;
using System.IO;
using System.Data;
using System.Data.SQLite;
using UnityEngine;

public class DatabaseManager : MonoBehaviour
{
    private static DatabaseManager _instance;
    public static DatabaseManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<DatabaseManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("[DatabaseManager]");
                    if (!Application.isPlaying) go.hideFlags = HideFlags.HideAndDontSave;
                    _instance = go.AddComponent<DatabaseManager>();
                    if (Application.isPlaying) DontDestroyOnLoad(go);
                }
                if (_instance != null && string.IsNullOrEmpty(_instance.dbPath))
                {
                    _instance.InitializeDatabase();
                }
            }
            return _instance;
        }
        private set { _instance = value; }
    }

    private const string dbFileName = "game_database.db";
    private string dbPath;

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeDatabase();
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void InitializeDatabase()
    {
        dbPath = Path.Combine(Application.persistentDataPath, dbFileName);

        if (!File.Exists(dbPath))
        {
            string sourcePath = Path.Combine(Application.streamingAssetsPath, dbFileName);

            if (File.Exists(sourcePath))
            {
                File.Copy(sourcePath, dbPath);
                Debug.Log("<color=green>[DatabaseManager]</color> Database disalin ke: " + dbPath);
            }
            else
            {
                Debug.LogError("<color=red>[DatabaseManager]</color> Database tidak ditemukan di StreamingAssets!");
            }
        }
        else
        {
            Debug.Log("<color=cyan>[DatabaseManager]</color> Database aktif: " + dbPath);
        }

        EnsureSaveSchema();
    }

    // =========================================================================
    // MIGRASI SKEMA SAVE/LOAD (Idempoten)
    // =========================================================================
    /// <summary>
    /// Memastikan seluruh tabel dan kolom yang dibutuhkan sistem Save/Load tersedia.
    /// Aman dipanggil berulang kali: CREATE TABLE IF NOT EXISTS untuk tabel baru dan
    /// ALTER TABLE ADD COLUMN di dalam try/catch untuk kolom yang mungkin sudah ada.
    /// </summary>
    private void EnsureSaveSchema()
    {
        try
        {
            ApplySaveSchemaTo(dbPath);
        }
        catch (Exception ex)
        {
            Debug.LogError("<color=red>[DatabaseManager]</color> Gagal memastikan skema Save/Load: " + ex.Message);
        }
    }

    // Seluruh DDL skema Save/Load dikumpulkan di satu tempat agar jalur runtime dan
    // jalur Editor (StreamingAssets) selalu menerapkan definisi yang identik.
    private static readonly string[] SaveSchemaTables = new string[]
    {
        @"CREATE TABLE IF NOT EXISTS tbl_save_metadata (
            slot_id INTEGER PRIMARY KEY,
            slot_title TEXT NOT NULL,
            saved_at DATETIME DEFAULT CURRENT_TIMESTAMP,
            current_day INTEGER NOT NULL,
            time_block TEXT NOT NULL,
            ph_snapshot INTEGER NOT NULL,
            mh_snapshot INTEGER NOT NULL
        );",
        @"CREATE TABLE IF NOT EXISTS tbl_save_player_stats (
            slot_id INTEGER PRIMARY KEY,
            physical_health INTEGER NOT NULL,
            mental_health INTEGER NOT NULL,
            language_proficiency INTEGER NOT NULL,
            cultural_etiquette INTEGER NOT NULL,
            academic_theoretical INTEGER NOT NULL,
            academic_practical INTEGER NOT NULL,
            is_burned_out INTEGER DEFAULT 0
        );",
        @"CREATE TABLE IF NOT EXISTS tbl_save_npc_relations (
            slot_id INTEGER NOT NULL,
            npc_id INTEGER NOT NULL,
            guanxi_score INTEGER NOT NULL,
            loneliness_meter INTEGER NOT NULL,
            rumor_contribution INTEGER NOT NULL,
            affection_state INTEGER NOT NULL,
            PRIMARY KEY (slot_id, npc_id)
        );",
        @"CREATE TABLE IF NOT EXISTS tbl_save_story_flags (
            slot_id INTEGER NOT NULL,
            flag_name TEXT NOT NULL,
            flag_value INTEGER DEFAULT 1,
            unlocked_day INTEGER DEFAULT 1,
            description TEXT,
            PRIMARY KEY (slot_id, flag_name)
        );",
        @"CREATE TABLE IF NOT EXISTS tbl_save_game_flags (
            slot_id INTEGER NOT NULL,
            flag_name TEXT NOT NULL,
            flag_value INTEGER DEFAULT 1,
            PRIMARY KEY (slot_id, flag_name)
        );",
        @"CREATE TABLE IF NOT EXISTS tbl_save_game_events (
            slot_id INTEGER NOT NULL,
            event_id INTEGER NOT NULL,
            is_completed INTEGER DEFAULT 0,
            PRIMARY KEY (slot_id, event_id)
        );",
        // Baru: snapshot status milestone karakter (is_unlocked & is_completed)
        @"CREATE TABLE IF NOT EXISTS tbl_save_character_events (
            slot_id INTEGER NOT NULL,
            event_id INTEGER NOT NULL,
            is_unlocked INTEGER DEFAULT 0,
            is_completed INTEGER DEFAULT 0,
            PRIMARY KEY (slot_id, event_id)
        );"
    };

    // Pasangan "nama tabel" -> "definisi kolom" yang ditambahkan secara idempoten.
    private static readonly string[,] SaveSchemaColumns = new string[,]
    {
        // Tabel live: agar state bertahan pada cold boot & transisi scene, bukan hanya di slot save.
        { "tbl_player_stats",       "is_burned_out INTEGER DEFAULT 0" },
        { "tbl_player_profile",     "current_weather TEXT DEFAULT 'Cerah'" },
        { "tbl_player_profile",     "greeting_triggered_today INTEGER DEFAULT 0" },
        { "tbl_npc_relations",      "interacted_today INTEGER DEFAULT 0" },
        // Tabel slot save: state simulasi yang sebelumnya tidak ikut tersimpan.
        { "tbl_save_metadata",      "global_rumor_level INTEGER DEFAULT 0" },
        { "tbl_save_metadata",      "current_weather TEXT DEFAULT 'Cerah'" },
        { "tbl_save_metadata",      "greeting_triggered_today INTEGER DEFAULT 0" },
        { "tbl_save_npc_relations", "days_since_last_interaction INTEGER DEFAULT 0" },
        { "tbl_save_npc_relations", "interacted_today INTEGER DEFAULT 0" }
    };

    /// <summary>
    /// Menerapkan seluruh DDL skema Save/Load ke satu file database tertentu.
    /// Dipakai jalur runtime (persistentDataPath) maupun jalur Editor (StreamingAssets).
    /// </summary>
    public static void ApplySaveSchemaTo(string targetDbPath)
    {
        if (string.IsNullOrEmpty(targetDbPath) || !File.Exists(targetDbPath)) return;

        using (var conn = new SQLiteConnection("Data Source=" + targetDbPath + ";Version=3;"))
        {
            conn.Open();
            using (var cmd = conn.CreateCommand())
            {
                foreach (string ddl in SaveSchemaTables)
                {
                    cmd.CommandText = ddl;
                    cmd.ExecuteNonQuery();
                }

                for (int i = 0; i < SaveSchemaColumns.GetLength(0); i++)
                {
                    string table = SaveSchemaColumns[i, 0];
                    string column = SaveSchemaColumns[i, 1];
                    // ALTER akan gagal jika kolom sudah ada; itu kondisi normal saat migrasi diulang.
                    try
                    {
                        cmd.CommandText = $"ALTER TABLE {table} ADD COLUMN {column};";
                        cmd.ExecuteNonQuery();
                    }
                    catch { }
                }

                MigrateSaveGameEventsSourceTable(cmd);
            }
        }
    }

    /// <summary>
    /// tbl_events dan tbl_game_events adalah dua tabel event berbeda yang berbagi rentang
    /// event_id (1-10). Tanpa kolom pembeda, snapshot salah satu tabel akan merusak tabel
    /// lainnya saat dipulihkan. Rebuild sekali agar primary key menyertakan source_table.
    /// </summary>
    private static void MigrateSaveGameEventsSourceTable(IDbCommand cmd)
    {
        // Deteksi apakah migrasi sudah pernah dijalankan.
        bool hasSourceColumn = false;
        cmd.CommandText = "PRAGMA table_info(tbl_save_game_events);";
        using (IDataReader reader = cmd.ExecuteReader())
        {
            while (reader.Read())
            {
                if (string.Equals(reader["name"].ToString(), "source_table", StringComparison.OrdinalIgnoreCase))
                {
                    hasSourceColumn = true;
                    break;
                }
            }
        }

        if (hasSourceColumn) return;

        // SQLite tidak mendukung perubahan PRIMARY KEY, jadi tabel dibangun ulang.
        cmd.CommandText = @"
            CREATE TABLE tbl_save_game_events_new (
                slot_id INTEGER NOT NULL,
                source_table TEXT NOT NULL DEFAULT 'tbl_events',
                event_id INTEGER NOT NULL,
                is_completed INTEGER DEFAULT 0,
                PRIMARY KEY (slot_id, source_table, event_id)
            );";
        cmd.ExecuteNonQuery();

        // Baris lama seluruhnya berasal dari tbl_events (lihat SaveManager langkah E sebelumnya).
        cmd.CommandText = @"
            INSERT OR IGNORE INTO tbl_save_game_events_new (slot_id, source_table, event_id, is_completed)
            SELECT slot_id, 'tbl_events', event_id, is_completed FROM tbl_save_game_events;";
        cmd.ExecuteNonQuery();

        cmd.CommandText = "DROP TABLE tbl_save_game_events;";
        cmd.ExecuteNonQuery();

        cmd.CommandText = "ALTER TABLE tbl_save_game_events_new RENAME TO tbl_save_game_events;";
        cmd.ExecuteNonQuery();

        Debug.Log("<color=green>[DatabaseManager]</color> tbl_save_game_events dimigrasi: kolom source_table ditambahkan.");
    }

    public IDbConnection GetConnection()
    {
        string connectionString = "Data Source=" + dbPath + ";Version=3;";
        IDbConnection connection = new SQLiteConnection(connectionString);
        connection.Open();
        return connection;
    }

    public DataTable ExecuteQuery(string query)
    {
        DataTable dt = new DataTable();
        using (IDbConnection conn = GetConnection())
        {
            using (IDbCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = query;
                using (IDataReader reader = cmd.ExecuteReader())
                {
                    dt.Load(reader);
                }
            }
        }
        return dt;
    }

    public int ExecuteNonQuery(string query)
    {
        using (IDbConnection conn = GetConnection())
        {
            using (IDbCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = query;
                return cmd.ExecuteNonQuery();
            }
        }
    }

    public void ExecuteTransaction(Action<IDbCommand> action)
    {
        using (IDbConnection conn = GetConnection())
        {
            using (IDbTransaction trans = conn.BeginTransaction())
            {
                using (IDbCommand cmd = conn.CreateCommand())
                {
                    cmd.Transaction = trans;
                    try
                    {
                        action?.Invoke(cmd);
                        trans.Commit();
                    }
                    catch
                    {
                        trans.Rollback();
                        throw;
                    }
                }
            }
        }
    }

#if UNITY_EDITOR
    [UnityEditor.MenuItem("Game Debug/Reset Local Database")]
    public static void ResetDatabaseEditor()
    {
        string path = Path.Combine(Application.persistentDataPath, dbFileName);
        if (File.Exists(path))
        {
            try
            {
                SQLiteConnection.ClearAllPools();
                GC.Collect();
                GC.WaitForPendingFinalizers();

                File.Delete(path);
                Debug.Log("<color=yellow>[Reset DB]</color> File database runtime berhasil dihapus. Database baru akan disalin saat Play.");
            }
            catch (Exception ex)
            {
                Debug.LogError("<color=red>[Reset DB]</color> Gagal menghapus database: " + ex.Message);
            }
        }
        else
        {
            Debug.LogWarning("<color=orange>[Reset DB]</color> File database tidak ditemukan di: " + path);
        }
    }

    [UnityEditor.MenuItem("Game Database/Terapkan Migrasi Skema Save/Load")]
    public static void ApplySaveSchemaMigrationEditor()
    {
        string[] dbPaths = new string[]
        {
            Path.Combine(Application.persistentDataPath, dbFileName),
            Path.Combine(Application.streamingAssetsPath, dbFileName)
        };

        foreach (string path in dbPaths)
        {
            if (!File.Exists(path))
            {
                Debug.LogWarning("<color=orange>[Migrasi Save/Load]</color> Tidak ditemukan: " + path);
                continue;
            }

            try
            {
                ApplySaveSchemaTo(path);
                Debug.Log("<color=green>[Migrasi Save/Load]</color> Skema diterapkan ke: " + path);
            }
            catch (Exception ex)
            {
                Debug.LogError($"<color=red>[Migrasi Save/Load]</color> Gagal pada {path}: {ex.Message}");
            }
        }
    }
#endif
}