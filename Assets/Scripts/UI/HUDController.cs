using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    public static HUDController Instance { get; private set; }

    [Header("Top Stat Displays")]
    public TextMeshProUGUI txtPhysicalHealth;
    public TextMeshProUGUI txtMentalHealth;
    public TextMeshProUGUI txtLanguage;
    public TextMeshProUGUI txtEtiquette;
    public TextMeshProUGUI txtTheoretical;
    public TextMeshProUGUI txtPractical;

    [Header("Calendar & Time Block")]
    public TextMeshProUGUI txtDayNumber;
    public TextMeshProUGUI txtTimeBlock;

    [Header("Predictive Tooltip Display")]
    public GameObject panelTooltip;
    public TextMeshProUGUI txtActivityDesc;
    public TextMeshProUGUI txtCostGainPreview;

    [Header("Activity Buttons")]
    public Button btnStudyLanguage;
    public Button btnLunchWithNPC;
    public Button btnMeetLecturer;
    public Button btnSleep;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        UpdateHUD();
        HidePredictiveTooltip();
    }

    // Memperbarui seluruh indikator angka di layar Static HUD
    public void UpdateHUD()
    {
        if (PlayerStats.Instance == null || GameManager.Instance == null) return;

        // Render nilai parameter pemain
        if (txtPhysicalHealth != null) txtPhysicalHealth.text = $"PH: {PlayerStats.Instance.physicalHealth}/100";
        if (txtMentalHealth != null) txtMentalHealth.text = $"MH: {PlayerStats.Instance.mentalHealth}/100";
        if (txtLanguage != null) txtLanguage.text = $"Bahasa: {PlayerStats.Instance.languageProficiency}";
        if (txtEtiquette != null) txtEtiquette.text = $"Etika: {PlayerStats.Instance.culturalEtiquette}";
        if (txtTheoretical != null) txtTheoretical.text = $"Teori: {PlayerStats.Instance.academicTheoretical}";
        if (txtPractical != null) txtPractical.text = $"Praktis: {PlayerStats.Instance.academicPractical}";

        // Render tanggal dan blok waktu
        string dayFormatted = GameManager.Instance.currentDay < 10 
            ? $"0{GameManager.Instance.currentDay}" 
            : $"{GameManager.Instance.currentDay}";
        if (txtDayNumber != null) txtDayNumber.text = $"HARI {dayFormatted}";
        if (txtTimeBlock != null) txtTimeBlock.text = GameManager.Instance.currentTimeBlock.ToString().ToUpper();

        // Kunci tombol aktivitas jika sedang dalam jadwal kelas wajib pagi
        bool isFreeTime = !(GameManager.Instance.IsWorkday() && GameManager.Instance.currentTimeBlock == TimeBlock.Pagi);
        if (btnStudyLanguage != null) btnStudyLanguage.interactable = isFreeTime;
        if (btnLunchWithNPC != null) btnLunchWithNPC.interactable = isFreeTime && GameManager.Instance.currentTimeBlock == TimeBlock.Siang;
        if (btnMeetLecturer != null) btnMeetLecturer.interactable = isFreeTime;
    }

    // Menampilkan kalkulasi Opportunity Cost sebelum dieksekusi (Bab 3.3.1)
    public void ShowPredictiveTooltip(string description, string costGain)
    {
        if (panelTooltip != null) panelTooltip.SetActive(true);
        if (txtActivityDesc != null) txtActivityDesc.text = description;
        if (txtCostGainPreview != null) txtCostGainPreview.text = costGain;
    }

    public void HidePredictiveTooltip()
    {
        if (panelTooltip != null) panelTooltip.SetActive(false);
    }
}
