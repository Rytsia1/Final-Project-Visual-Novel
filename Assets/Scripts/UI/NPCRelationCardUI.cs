using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NPCRelationCardUI : MonoBehaviour
{
    [Header("Profile Display")]
    public Image imgPortrait;
    public TextMeshProUGUI txtNpcName;
    public TextMeshProUGUI txtNpcRole;

    [Header("Guanxi & Affection Display")]
    public Slider sliderGuanxi;
    public TextMeshProUGUI txtGuanxiValue;
    public Image imgAffectionIcon;
    public TextMeshProUGUI txtAffectionLabel;

    [Header("Bakudan (Emotional Bomb) Display")]
    public GameObject panelBakudanWarning;
    public Image imgBakudanIcon;
    public TextMeshProUGUI txtBakudanStatus;

    [Header("State Animasi")]
    private bool isPulsing = false;
    private float pulseSpeed = 4.5f;
    private Vector3 initialBakudanScale = Vector3.one;

    void Awake()
    {
        if (panelBakudanWarning != null)
        {
            initialBakudanScale = panelBakudanWarning.transform.localScale;
        }
    }

    void Update()
    {
        // Efek denyut berdetak (pulsating animation) jika bom emosional dalam status kritis (Loneliness >= 80)
        if (isPulsing && panelBakudanWarning != null && panelBakudanWarning.activeSelf)
        {
            float scaleOffset = Mathf.PingPong(Time.time * pulseSpeed, 0.18f);
            panelBakudanWarning.transform.localScale = initialBakudanScale * (1f + scaleOffset);
        }
    }

    /// <summary>
    /// Mengisi data visual kartu berdasarkan relasi NPC aktual
    /// </summary>
    public void BindData(NPCRelationData data, string npcName, string role)
    {
        if (data == null) return;

        // 1. Profil Karakter
        if (txtNpcName != null)
        {
            txtNpcName.text = string.IsNullOrEmpty(npcName) ? data.npcName : npcName;
        }

        if (txtNpcRole != null)
        {
            txtNpcRole.text = role ?? "";
        }

        if (imgPortrait != null)
        {
            // Berikan warna aksen sesuai identitas karakter
            Color accentColor = GetNpcAccentColor(data.npcId);
            imgPortrait.color = accentColor;
        }

        // 2. Bar Guanxi (0–100)
        if (sliderGuanxi != null)
        {
            sliderGuanxi.minValue = 0;
            sliderGuanxi.maxValue = 100;
            sliderGuanxi.value = data.guanxiScore;
        }

        if (txtGuanxiValue != null)
        {
            txtGuanxiValue.text = $"{data.guanxiScore} / 100";
        }

        // 3. Status Afeksi Emosional (Heart / Flower State)
        UpdateAffectionDisplay(data.affectionState);

        // 4. Status Bom Emosional (Bakudan Indicator)
        UpdateBakudanDisplay(data.lonelinessMeter);
    }

    private void UpdateAffectionDisplay(int state)
    {
        Color stateColor;
        string stateLabel;

        switch (state)
        {
            case 3: // Tokimeki (Lingkaran Inti)
                stateColor = new Color(1.0f, 0.35f, 0.65f, 1f); // Vibrant Pink
                stateLabel = "Tokimeki (Inti)";
                break;
            case 2: // Sahabat Akrab (Friendly)
                stateColor = new Color(0.25f, 0.75f, 1.0f, 1f);  // Cyan / Sky Blue
                stateLabel = "Sahabat Akrab";
                break;
            case 1: // Netral / Kenal (Neutral)
                stateColor = new Color(0.40f, 0.85f, 0.50f, 1f); // Soft Green
                stateLabel = "Netral / Kenal";
                break;
            default: // State 0: Dingin / Formal (Cold)
                stateColor = new Color(0.60f, 0.62f, 0.70f, 1f); // Muted Grey
                stateLabel = "Dingin / Formal";
                break;
        }

        if (imgAffectionIcon != null)
        {
            imgAffectionIcon.color = stateColor;
        }

        if (txtAffectionLabel != null)
        {
            txtAffectionLabel.text = stateLabel;
            txtAffectionLabel.color = stateColor;
        }
    }

    private void UpdateBakudanDisplay(int loneliness)
    {
        if (panelBakudanWarning == null) return;

        if (loneliness < 50)
        {
            // Nilai < 50: Nonaktif / Tersembunyi (Kondisi Aman)
            isPulsing = false;
            panelBakudanWarning.SetActive(false);
            panelBakudanWarning.transform.localScale = initialBakudanScale;
        }
        else if (loneliness < 80)
        {
            // Nilai 50 - 79: Status Waspada (Warna kuning/oranye)
            isPulsing = false;
            panelBakudanWarning.SetActive(true);
            panelBakudanWarning.transform.localScale = initialBakudanScale;

            if (imgBakudanIcon != null)
            {
                imgBakudanIcon.color = new Color(1.0f, 0.75f, 0.15f, 1f); // Orange/Yellow
            }

            if (txtBakudanStatus != null)
            {
                txtBakudanStatus.text = $"⚠️ WASPADA ({loneliness}/100)";
                txtBakudanStatus.color = new Color(1.0f, 0.82f, 0.25f, 1f);
            }
        }
        else
        {
            // Nilai >= 80: Status Kritis (Warna merah menyala, berdenyut)
            isPulsing = true;
            pulseSpeed = 5.0f;
            panelBakudanWarning.SetActive(true);

            if (imgBakudanIcon != null)
            {
                imgBakudanIcon.color = new Color(1.0f, 0.20f, 0.25f, 1f); // Critical Red
            }

            if (txtBakudanStatus != null)
            {
                txtBakudanStatus.text = $"💣 KRITIS! ({loneliness}/100)";
                txtBakudanStatus.color = new Color(1.0f, 0.30f, 0.35f, 1f);
            }
        }
    }

    private Color GetNpcAccentColor(int npcId)
    {
        switch (npcId)
        {
            case 101: return new Color(0.20f, 0.35f, 0.58f, 1f); // Xiang Bai: Akademik Navy
            case 102: return new Color(0.18f, 0.48f, 0.40f, 1f); // Li Haoran: Tech Teal
            case 103: return new Color(0.58f, 0.28f, 0.38f, 1f); // Yang Mei: Rose Wine
            case 104: return new Color(0.48f, 0.30f, 0.65f, 1f); // Edelweiss: Elegant Purple
            default:  return new Color(0.25f, 0.30f, 0.42f, 1f);
        }
    }
}
