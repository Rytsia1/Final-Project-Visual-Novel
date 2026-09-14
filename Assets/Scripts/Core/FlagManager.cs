using System;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class FlagManager : MonoBehaviour
{
    private static FlagManager _instance;
    public static FlagManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<FlagManager>();
                if (_instance == null)
                {
                    GameObject core = GameObject.Find("GAME_CORE") ?? GameObject.Find("[GAME_CORE]");
                    if (core != null)
                    {
                        _instance = core.AddComponent<FlagManager>();
                    }
                    else
                    {
                        GameObject go = new GameObject("[FlagManager]");
                        _instance = go.AddComponent<FlagManager>();
                        DontDestroyOnLoad(go);
                    }
                }
            }
            return _instance;
        }
        private set => _instance = value;
    }

    // In-memory cache agar evaluasi event tidak selalu query berulang ke disk
    private readonly Dictionary<string, int> _flagCache = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

    public event Action<string, int> OnFlagChanged;

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
            return;
        }

        LoadAllFlagsFromDatabase();
    }

    /// <summary>
    /// Memuat seluruh data dari tbl_story_flags ke dalam memory cache.
    /// Dipanggil saat inisialisasi awal dan paska load game.
    /// </summary>
    public void LoadAllFlagsFromDatabase()
    {
        _flagCache.Clear();

        if (DatabaseManager.Instance == null) return;

        try
        {
            string q = "SELECT flag_name, flag_value FROM tbl_story_flags;";
            DataTable dt = DatabaseManager.Instance.ExecuteQuery(q);

            if (dt != null)
            {
                foreach (DataRow row in dt.Rows)
                {
                    string fName = row["flag_name"].ToString();
                    int fVal = Convert.ToInt32(row["flag_value"]);
                    _flagCache[fName] = fVal;
                }
            }

            Debug.Log($"<color=cyan>[FlagManager]</color> Berhasil memuat {_flagCache.Count} story flags ke memori cache.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[FlagManager] Gagal memuat story flags dari database: {ex.Message}");
        }
    }

    /// <summary>
    /// Menetapkan nilai flag naratif secara permanen (memori cache + SQLite).
    /// </summary>
    public void SetFlag(string flagName, int value = 1, string desc = "")
    {
        if (string.IsNullOrEmpty(flagName)) return;

        _flagCache[flagName] = value;
        int currentDay = GameManager.Instance != null ? GameManager.Instance.currentDay : 1;

        if (DatabaseManager.Instance != null)
        {
            try
            {
                string q = $"INSERT INTO tbl_story_flags (flag_name, flag_value, unlocked_day, description) " +
                           $"VALUES ('{flagName}', {value}, {currentDay}, '{desc}') " +
                           $"ON CONFLICT(flag_name) DO UPDATE SET flag_value = {value}, unlocked_day = {currentDay}, description = '{desc}';";
                DatabaseManager.Instance.ExecuteNonQuery(q);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[FlagManager] Gagal menyimpan flag '{flagName}' ke SQLite: {ex.Message}");
            }
        }

        if (TelemetryLogger.Instance != null)
        {
            TelemetryLogger.Instance.RecordCriticalEvent("STORY_FLAG_SET", $"{flagName} = {value} (Hari {currentDay})");
        }

        Debug.Log($"<color=magenta>[STORY FLAG]</color> <b>{flagName}</b> disetel ke <b>{value}</b> (Hari {currentDay}). {desc}");
        OnFlagChanged?.Invoke(flagName, value);
    }

    /// <summary>
    /// Memeriksa apakah flag terdaftar di cache dan memiliki nilai yang sesuai.
    /// </summary>
    public bool HasFlag(string flagName, int requiredValue = 1)
    {
        if (string.IsNullOrEmpty(flagName)) return false;

        if (_flagCache.TryGetValue(flagName, out int val))
        {
            return val >= requiredValue;
        }

        return false;
    }

    // Akses praktis ke memory cache
    public Dictionary<string, int> flagCache => _flagCache;

    /// <summary>
    /// Mengambil integer value dari flag (return defaultVal jika tidak ditemukan).
    /// </summary>
    public int GetFlag(string flagName, int defaultVal = 0)
    {
        if (string.IsNullOrEmpty(flagName)) return defaultVal;

        if (_flagCache.TryGetValue(flagName, out int val))
        {
            return val;
        }

        return defaultVal;
    }

    /// <summary>
    /// Menghapus flag dari memory cache dan SQLite.
    /// </summary>
    public void RemoveFlag(string flagName)
    {
        if (string.IsNullOrEmpty(flagName)) return;

        if (_flagCache.ContainsKey(flagName))
        {
            _flagCache.Remove(flagName);
        }

        if (DatabaseManager.Instance != null)
        {
            string q = $"DELETE FROM tbl_story_flags WHERE flag_name = '{flagName}';";
            DatabaseManager.Instance.ExecuteNonQuery(q);
        }

        OnFlagChanged?.Invoke(flagName, 0);
    }

    /// <summary>
    /// Reset seluruh flag saat New Game (membersihkan cache dan SQLite).
    /// </summary>
    public void ClearAllFlags()
    {
        _flagCache.Clear();

        if (DatabaseManager.Instance != null)
        {
            DatabaseManager.Instance.ExecuteNonQuery("DELETE FROM tbl_story_flags;");
            DatabaseManager.Instance.ExecuteNonQuery("DELETE FROM tbl_game_flags;");
        }

        Debug.Log("<color=cyan>[FlagManager]</color> Seluruh story flags telah dibersihkan.");
    }

    /// <summary>
    /// Mengambil seluruh dictionary flags aktif (read-only).
    /// </summary>
    public IReadOnlyDictionary<string, int> GetAllFlags()
    {
        return _flagCache;
    }
}
