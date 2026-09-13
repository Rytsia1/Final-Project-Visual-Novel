using System;
using System.Data;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuController : MonoBehaviour
{
    [Header("Menu Action Buttons")]
    public Button btnNewStory;
    public Button btnLoadStory;
    public Button btnConfig;
    public Button btnExtras;
    public Button btnQuit;

    [Header("Modals Root")]
    public GameObject modalLoadStory;
    public GameObject modalConfig;
    public GameObject modalExtras;

    [Header("Load Story Modal")]
    public Transform slotsContainer;
    public GameObject slotItemPrefab;
    public Button btnCloseLoadStory;
    public TextMeshProUGUI txtEmptySaves;

    [Header("Config Modal")]
    public Button btnCloseConfig;
    public Slider sliderBgm;
    public Slider sliderSfx;
    public Slider sliderTextSpeed;

    [Header("Extras Modal")]
    public Button btnCloseExtras;

    [Header("Target Gameplay Scene")]
    public string gameplaySceneName = "SampleScene";

    void Awake()
    {
        // Pastikan DatabaseManager terinisialisasi
        if (DatabaseManager.Instance == null)
        {
            Debug.LogWarning("[MainMenuController] DatabaseManager instance null di Awake.");
        }
    }

    void Start()
    {
        // 1. Sambungkan listener tombol aksi utama
        if (btnNewStory != null) btnNewStory.onClick.AddListener(OnClick_NewStory);
        if (btnLoadStory != null) btnLoadStory.onClick.AddListener(OnClick_LoadStory);
        if (btnConfig != null) btnConfig.onClick.AddListener(OnClick_Config);
        if (btnExtras != null) btnExtras.onClick.AddListener(OnClick_Extras);
        if (btnQuit != null) btnQuit.onClick.AddListener(OnClick_Quit);

        // 2. Sambungkan listener penutup modal
        if (btnCloseLoadStory != null) btnCloseLoadStory.onClick.AddListener(CloseAllModals);
        if (btnCloseConfig != null) btnCloseConfig.onClick.AddListener(CloseAllModals);
        if (btnCloseExtras != null) btnCloseExtras.onClick.AddListener(CloseAllModals);

        // 3. Konfigurasi slider config
        InitConfigSliders();

        // 4. Tutup seluruh modal di awal
        CloseAllModals();
    }

    // ==========================================
    // 1. NEW STORY
    // ==========================================
    public void OnClick_NewStory()
    {
        Debug.Log("<color=cyan>[MAIN MENU]</color> Memulai Cerita Baru (New Story)...");

        try
        {
            DatabaseManager.Instance.ExecuteTransaction(cmd =>
            {
                // A. Reset profil pemain: Day 1, Block 'Pagi', Workday 1, Rumor 0
                cmd.CommandText = "UPDATE tbl_player_profile SET current_day = 1, current_time_block = 'Pagi', global_rumor_level = 0 WHERE player_id = 1;";
                cmd.ExecuteNonQuery();

                // B. Reset parameter stats: PH 100, MH 80, Bahasa 20, Etika 15, Teori 30, Praktis 40
                cmd.CommandText = "UPDATE tbl_player_stats SET physical_health = 100, mental_health = 80, " +
                                  "language_proficiency = 20, cultural_etiquette = 15, " +
                                  "academic_theoretical = 30, academic_practical = 40 WHERE player_id = 1;";
                cmd.ExecuteNonQuery();

                // C. Reset relasi NPC: Guanxi 20, Loneliness 0, Rumor 0, Affection 0, Days 0
                cmd.CommandText = "UPDATE tbl_npc_relations SET guanxi_score = 20, loneliness_meter = 0, " +
                                  "rumor_contribution = 0, days_since_last_interaction = 0, affection_state = 0 WHERE player_id = 1;";
                cmd.ExecuteNonQuery();
            });

            Debug.Log("<color=green>[MAIN MENU]</color> Basis data berhasil di-reset ke baseline Hari 1 Pagi.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[MAIN MENU ERROR] Gagal melakukan reset database: {ex.Message}");
        }

        // Reset riwayat backlog dialog untuk sesi baru
        if (DialogueBacklogManager.Instance != null)
        {
            DialogueBacklogManager.Instance.ClearHistory();
        }

        // Pindah scene ke gameplay
        SceneManager.LoadScene(gameplaySceneName);
    }

    // ==========================================
    // 2. LOAD STORY
    // ==========================================
    public void OnClick_LoadStory()
    {
        CloseAllModals();
        if (modalLoadStory != null)
        {
            modalLoadStory.SetActive(true);
            PopulateSaveSlots();
        }
    }

    public void PopulateSaveSlots()
    {
        if (slotsContainer == null) return;

        // Bersihkan item lama
        foreach (Transform child in slotsContainer)
        {
            Destroy(child.gameObject);
        }

        // Baca daftar snapshot dari tbl_save_metadata secara terurut (ORDER BY slot_id ASC)
        string query = "SELECT slot_id, slot_title, saved_at, current_day, time_block, ph_snapshot, mh_snapshot " +
                       "FROM tbl_save_metadata ORDER BY slot_id ASC;";
        DataTable dt = DatabaseManager.Instance.ExecuteQuery(query);

        bool hasSaves = dt != null && dt.Rows.Count > 0;

        if (txtEmptySaves != null)
        {
            txtEmptySaves.gameObject.SetActive(!hasSaves);
        }

        if (hasSaves && slotItemPrefab != null)
        {
            foreach (DataRow row in dt.Rows)
            {
                int slotId = Convert.ToInt32(row["slot_id"]);
                string title = row["slot_title"]?.ToString() ?? "";
                string savedAt = row["saved_at"]?.ToString() ?? "";
                int day = Convert.ToInt32(row["current_day"]);
                string block = row["time_block"]?.ToString() ?? "Pagi";
                int ph = Convert.ToInt32(row["ph_snapshot"]);
                int mh = Convert.ToInt32(row["mh_snapshot"]);

                GameObject itemGO = Instantiate(slotItemPrefab, slotsContainer);
                LoadSlotItemUI itemUI = itemGO.GetComponent<LoadSlotItemUI>();
                if (itemUI != null)
                {
                    itemUI.Setup(slotId, title, day, block, savedAt, ph, mh, OnSlotSelectedForLoad);
                }
            }
        }
    }

    private void OnSlotSelectedForLoad(int slotId)
    {
        Debug.Log($"<color=yellow>[MAIN MENU]</color> Memuat file simpanan Slot {slotId}...");
        
        // Muat snapshot ke database & memori
        SaveManager.Instance.LoadGame(slotId);

        // Pindah scene ke gameplay
        SceneManager.LoadScene(gameplaySceneName);
    }

    // ==========================================
    // 3. CONFIG & EXTRAS
    // ==========================================
    public void OnClick_Config()
    {
        CloseAllModals();
        if (modalConfig != null) modalConfig.SetActive(true);
    }

    public void OnClick_Extras()
    {
        CloseAllModals();
        if (modalExtras != null) modalExtras.SetActive(true);
    }

    // ==========================================
    // 4. QUIT
    // ==========================================
    public void OnClick_Quit()
    {
        Debug.Log("[MAIN MENU] Keluar dari aplikasi...");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void CloseAllModals()
    {
        if (modalLoadStory != null) modalLoadStory.SetActive(false);
        if (modalConfig != null) modalConfig.SetActive(false);
        if (modalExtras != null) modalExtras.SetActive(false);
    }

    private void InitConfigSliders()
    {
        if (sliderBgm != null)
        {
            sliderBgm.value = PlayerPrefs.GetFloat("Config_BGM", 0.8f);
            sliderBgm.onValueChanged.AddListener((v) => PlayerPrefs.SetFloat("Config_BGM", v));
        }

        if (sliderSfx != null)
        {
            sliderSfx.value = PlayerPrefs.GetFloat("Config_SFX", 1.0f);
            sliderSfx.onValueChanged.AddListener((v) => PlayerPrefs.SetFloat("Config_SFX", v));
        }

        if (sliderTextSpeed != null)
        {
            sliderTextSpeed.value = PlayerPrefs.GetFloat("Config_TextSpeed", 0.03f);
            sliderTextSpeed.onValueChanged.AddListener((v) => PlayerPrefs.SetFloat("Config_TextSpeed", v));
        }
    }
}
