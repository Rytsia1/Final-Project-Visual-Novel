using System;
using System.Data;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SocialStatusWindowUI : MonoBehaviour
{
    private static SocialStatusWindowUI _instance;
    public static SocialStatusWindowUI Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<SocialStatusWindowUI>(FindObjectsInactive.Include);
            }
            return _instance;
        }
        private set { _instance = value; }
    }

    [Header("UI Panels & Container")]
    public GameObject panelRoot;
    public Transform cardsContainer;
    public GameObject npcCardPrefab;
    public Button btnClose;

    [Header("Optional Header Display")]
    public TextMeshProUGUI txtRumorLevelHeader;

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else if (_instance != this)
        {
            Destroy(this);
            return;
        }

        if (btnClose != null)
        {
            btnClose.onClick.RemoveAllListeners();
            btnClose.onClick.AddListener(CloseWindow);
        }
    }

    void Start()
    {
        // Pastikan jendela ditutup di awal
        CloseWindow();
    }

    /// <summary>
    /// Membuka jendela status hubungan dan merender kartu untuk setiap relasi NPC
    /// </summary>
    public void OpenWindow()
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(true);
        }
        else
        {
            gameObject.SetActive(true);
        }

        // 1. Bersihkan kartu lama dari container
        if (cardsContainer != null)
        {
            for (int i = cardsContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(cardsContainer.GetChild(i).gameObject);
            }
        }

        // 2. Pastikan data relasi ter-sinkronisasi dari SQLite
        if (SocialManager.Instance != null)
        {
            SocialManager.Instance.MuatRelasiDariDatabase();

            if (txtRumorLevelHeader != null)
            {
                string rumorLabel = GetRumorLabel(SocialManager.Instance.globalRumorLevel);
                txtRumorLevelHeader.text = $"Level Rumor Kampus: {rumorLabel}";
            }
        }

        // 3. Ambil kamus informasi nama & peran karakter dari tbl_npc_list
        Dictionary<int, (string name, string role)> npcDict = new Dictionary<int, (string, string)>();
        if (DatabaseManager.Instance != null)
        {
            try
            {
                DataTable dt = DatabaseManager.Instance.ExecuteQuery("SELECT npc_id, npc_name, npc_role FROM tbl_npc_list;");
                if (dt != null)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        int id = Convert.ToInt32(row["npc_id"]);
                        string name = row["npc_name"].ToString();
                        string role = row.Table.Columns.Contains("npc_role") ? row["npc_role"].ToString() : "";
                        npcDict[id] = (name, role);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[SocialStatusWindowUI] Gagal memuat data tbl_npc_list: {ex.Message}");
            }
        }

        // 4. Instansiasi kartu untuk setiap relasi NPC aktif
        if (SocialManager.Instance != null && npcCardPrefab != null && cardsContainer != null)
        {
            foreach (var rel in SocialManager.Instance.relations)
            {
                GameObject cardGO = Instantiate(npcCardPrefab, cardsContainer);
                cardGO.SetActive(true);

                NPCRelationCardUI cardUI = cardGO.GetComponent<NPCRelationCardUI>();
                if (cardUI != null)
                {
                    string charName = rel.npcName;
                    string charRole = "Mahasiswa";

                    if (npcDict.ContainsKey(rel.npcId))
                    {
                        charName = npcDict[rel.npcId].name;
                        charRole = npcDict[rel.npcId].role;
                    }

                    cardUI.BindData(rel, charName, charRole);
                }
            }
        }
    }

    /// <summary>
    /// Menutup jendela status hubungan sosial
    /// </summary>
    public void CloseWindow()
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private string GetRumorLabel(int level)
    {
        switch (level)
        {
            case 3: return "<color=red>Level 3 (LEDAKAN RUMOR)</color>";
            case 2: return "<color=orange>Level 2 (Menyebar Luas)</color>";
            case 1: return "<color=yellow>Level 1 (Bisik-Bisik)</color>";
            default: return "<color=green>Level 0 (Aman)</color>";
        }
    }
}
