using System;
using System.IO;
using System.Data;
using System.Data.SQLite;
using UnityEngine;

public class DatabaseManager : MonoBehaviour
{
    public static DatabaseManager Instance { get; private set; }

    private const string dbFileName = "game_database.db";
    private string dbPath;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeDatabase();
        }
        else
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
#endif
}