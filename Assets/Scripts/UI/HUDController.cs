using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class HUDController : MonoBehaviour
{
    private static HUDController _instance;
    public static HUDController Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<HUDController>();
            }
            return _instance;
        }
        private set { _instance = value; }
    }

    [Header("Top Stat Displays")]
    public TextMeshProUGUI txtPhysicalHealth;
    public TextMeshProUGUI txtMentalHealth;
    public TextMeshProUGUI txtLanguage;
    public TextMeshProUGUI txtEtiquette;
    public TextMeshProUGUI txtTheoretical;
    public TextMeshProUGUI txtPractical;

    [Header("Stat Bars (isi lebar via anchorMax.x, 0-100)")]
    public RectTransform barPhysical;
    public RectTransform barMental;
    public RectTransform barLanguage;
    public RectTransform barEtiquette;
    public RectTransform barTheoretical;
    public RectTransform barPractical;

    [Header("Calendar & Time Block")]
    public TextMeshProUGUI txtDayNumber;
    public TextMeshProUGUI txtTimeBlock;
    public TextMeshProUGUI txtWeather;

    [Header("Predictive Tooltip Display")]
    public GameObject panelTooltip;
    public TextMeshProUGUI txtActivityDesc;
    public TextMeshProUGUI txtCostGainPreview;

    [Header("Activity Buttons")]
    public Button btnStudyLanguage;
    public Button btnLunchWithNPC;
    public Button btnMeetLecturer;
    public Button btnSleep;

    [Header("Smartphone & Social Window")]
    public Button btnOpenPhone;
    public Button btnOpenSocialWindow;

    [Header("Model Ingame Home Ver 1")]
    public Button btnHome;
    public TextMeshProUGUI txtDayBadge;
    public TextMeshProUGUI txtDateBadge;
    public TextMeshProUGUI txtPhoneClock;
    public TextMeshProUGUI txtPhoneDate;

    [Header("HUD Panels")]
    public GameObject panelTopStats;
    public GameObject panelCalendar;
    public GameObject panelActivityGrid;

    private bool _isHUDLockedHidden = false;

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            EnsureEventSystem();
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (btnHome != null)
        {
            btnHome.onClick.RemoveAllListeners();
            btnHome.onClick.AddListener(OnClick_Home);
        }
        if (btnOpenPhone != null)
        {
            btnOpenPhone.onClick.RemoveAllListeners();
            btnOpenPhone.onClick.AddListener(OnClick_OpenPhone);
        }
        if (btnOpenSocialWindow != null)
        {
            btnOpenSocialWindow.onClick.RemoveAllListeners();
            btnOpenSocialWindow.onClick.AddListener(OnClick_OpenSocialWindow);
        }

        // Cek jika sesi saat ini adalah Prolog yang membutuhkan HUD terkunci
        bool isNewGamePrologue = PlayerPrefs.GetInt("IsNewGamePrologue", 0) == 1;
        int prologueCompleted = FlagManager.Instance != null ? FlagManager.Instance.GetFlag("prologue_completed", 0) : 0;
        int currentDay = GameManager.Instance != null ? GameManager.Instance.currentDay : 1;

        if (isNewGamePrologue || (currentDay == 1 && prologueCompleted == 0))
        {
            SetHUDVisible(false);
        }
        else
        {
            UpdateHUD();
            HidePredictiveTooltip();
        }
    }

    // Memastikan EventSystem ada dan aktif di scene
    private void EnsureEventSystem()
    {
        if (FindFirstObjectByType<EventSystem>() == null)
        {
            GameObject esGO = new GameObject("EventSystem");
            esGO.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM
            esGO.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#elif ENABLE_LEGACY_INPUT_MANAGER
            esGO.AddComponent<StandaloneInputModule>();
#endif
            Debug.Log("<color=green>[EventSystem]</color> EventSystem berhasil diinisialisasi otomatis.");
        }
    }

    // Memperbarui seluruh indikator angka di layar Static HUD
    public void UpdateHUD()
    {
        if (_isHUDLockedHidden) return;
        if (PlayerStats.Instance == null || GameManager.Instance == null) return;

        // Render nilai parameter pemain + bar
        PlayerStats ps = PlayerStats.Instance;
        SetStat(txtPhysicalHealth, barPhysical, ps.physicalHealth);
        SetStat(txtMentalHealth, barMental, ps.mentalHealth);
        SetStat(txtLanguage, barLanguage, ps.languageProficiency);
        SetStat(txtEtiquette, barEtiquette, ps.culturalEtiquette);
        SetStat(txtTheoretical, barTheoretical, ps.academicTheoretical);
        SetStat(txtPractical, barPractical, ps.academicPractical);

        // Render tanggal, blok waktu, dan cuaca (Persona Style)
        if (txtDayNumber != null) txtDayNumber.text = GameManager.Instance.GetFormattedDay();
        if (txtTimeBlock != null) txtTimeBlock.text = GameManager.Instance.currentTimeBlock.ToString().ToUpper();
        if (txtWeather != null) txtWeather.text = $"CUACA: {GameManager.Instance.currentWeather.ToString().ToUpper()}";

        // Model Ingame Home Ver 1 (Calendar Badge & Phone)
        if (txtDayBadge != null) txtDayBadge.text = GameManager.Instance.GetDayName();
        System.DateTime currDate = GameManager.Instance.GetCalendarDate();
        if (txtDateBadge != null) txtDateBadge.text = $"Hari {GameManager.Instance.currentDay}  ·  {currDate:dd/MM/yyyy}";

        if (txtPhoneClock != null) txtPhoneClock.text = GameManager.Instance.GetClockText();
        if (txtPhoneDate != null) txtPhoneDate.text = $"{GameManager.Instance.GetDayName()}, {GameManager.Instance.GetDateText()}";

        // Kunci tombol aktivitas jika sedang dalam jadwal kelas wajib pagi atau terkena Burnout
        bool isBurnout = PlayerStats.Instance != null && PlayerStats.Instance.isBurnedOut;
        bool isFreeTime = !isBurnout && !(GameManager.Instance.IsWorkday() && GameManager.Instance.currentTimeBlock == TimeBlock.Pagi);

        if (btnStudyLanguage != null) btnStudyLanguage.interactable = isFreeTime;
        if (btnLunchWithNPC != null) btnLunchWithNPC.interactable = isFreeTime && GameManager.Instance.currentTimeBlock == TimeBlock.Siang;
        if (btnMeetLecturer != null) btnMeetLecturer.interactable = isFreeTime;
        if (btnSleep != null) btnSleep.interactable = true;
        if (btnOpenPhone != null) btnOpenPhone.interactable = !isBurnout;
        if (btnOpenSocialWindow != null) btnOpenSocialWindow.interactable = !isBurnout;
    }

    // ponytail: bar diskalakan ke 0-100 untuk semua stat; Bahasa/Etika/Teori/Praktik tidak punya batas atas,
    // jadi bar penuh di atas 100 — tambahkan maks per-stat bila skala berubah.
    private static void SetStat(TextMeshProUGUI txt, RectTransform bar, int value)
    {
        if (txt != null) txt.text = value.ToString();
        if (bar != null) bar.anchorMax = new Vector2(Mathf.Clamp01(value / 100f), 1f);
    }

    public void OnClick_Home()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenuScene");
    }

    public void OnClick_OpenPhone()
    {
        if (PhoneUIController.Instance != null)
        {
            PhoneUIController.Instance.BukaPhone();
        }
        else
        {
            Debug.LogWarning("<color=yellow>[HUDController]</color> PhoneUIController.Instance belum tersedia!");
        }
    }

    public void OnClick_OpenSocialWindow()
    {
        if (SocialStatusWindowUI.Instance != null)
        {
            SocialStatusWindowUI.Instance.OpenWindow();
        }
        else
        {
            Debug.LogWarning("<color=yellow>[HUDController]</color> SocialStatusWindowUI.Instance belum tersedia!");
        }
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

    /// <summary>
    /// Menampilkan atau menyembunyikan antarmuka Static HUD (Top Stats, Calendar, Activity Grid, Phone, dll.).
    /// Digunakan untuk alur cutscene/prolog agar pemain fokus pada dialog naratif.
    /// </summary>
    public void SetHUDVisible(bool visible)
    {
        _isHUDLockedHidden = !visible;

        if (panelTopStats != null) panelTopStats.SetActive(visible);
        if (panelCalendar != null) panelCalendar.SetActive(visible);
        if (panelActivityGrid != null) panelActivityGrid.SetActive(visible);
        if (btnHome != null) btnHome.gameObject.SetActive(visible);
        if (btnOpenPhone != null) btnOpenPhone.gameObject.SetActive(visible);
        if (btnOpenSocialWindow != null) btnOpenSocialWindow.gameObject.SetActive(visible);

        Canvas canvas = GetComponentInParent<Canvas>() ?? FindFirstObjectByType<Canvas>();
        Transform canvasTrans = canvas != null ? canvas.transform : transform;

        string[] panelNames = new string[] {
            "Panel_TopStats",
            "Panel_Calendar",
            "Panel_ActivityGrid",
            "Btn_OpenPhone",
            "Btn_Home",
            "Panel_PhoneScreen",
            "Panel_SocialStatusWindow"
        };

        foreach (string name in panelNames)
        {
            Transform t = canvasTrans != null ? canvasTrans.Find(name) : null;
            if (t == null)
            {
                GameObject go = GameObject.Find(name);
                if (go != null) t = go.transform;
            }
            if (t != null)
            {
                t.gameObject.SetActive(visible);
            }
        }

        if (!visible)
        {
            HidePredictiveTooltip();
        }
        else
        {
            UpdateHUD();
        }
    }

    [Header("Toast Notification")]
    public GameObject panelToast;
    public TextMeshProUGUI txtToastMessage;
    private Coroutine _toastCoroutine;

    public void ShowToastNotification(string message, float duration = 2.5f)
    {
        if (_toastCoroutine != null)
        {
            StopCoroutine(_toastCoroutine);
        }
        _toastCoroutine = StartCoroutine(RoutineShowToast(message, duration));
    }

    private System.Collections.IEnumerator RoutineShowToast(string message, float duration)
    {
        if (panelToast == null)
        {
            EnsureToastPanel();
        }

        if (panelToast != null)
        {
            if (txtToastMessage != null) txtToastMessage.text = message;
            panelToast.SetActive(true);
            yield return new WaitForSeconds(duration);
            panelToast.SetActive(false);
        }
    }

    private void EnsureToastPanel()
    {
        Transform t = transform.Find("Panel_ToastNotification");
        if (t != null)
        {
            panelToast = t.gameObject;
            txtToastMessage = panelToast.GetComponentInChildren<TextMeshProUGUI>();
            return;
        }

        GameObject toast = new GameObject("Panel_ToastNotification");
        toast.transform.SetParent(transform, false);

        RectTransform rt = toast.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.88f);
        rt.anchorMax = new Vector2(0.5f, 0.88f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(520f, 48f);

        Image img = toast.AddComponent<Image>();
        img.color = new Color(0.12f, 0.15f, 0.24f, 0.95f);

        GameObject txtObj = new GameObject("Txt_Toast");
        txtObj.transform.SetParent(toast.transform, false);
        RectTransform rtTxt = txtObj.AddComponent<RectTransform>();
        rtTxt.anchorMin = Vector2.zero;
        rtTxt.anchorMax = Vector2.one;
        rtTxt.sizeDelta = Vector2.zero;

        TextMeshProUGUI tmp = txtObj.AddComponent<TextMeshProUGUI>();
        tmp.fontSize = 15;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = new Color(1f, 0.85f, 0.35f);
        tmp.alignment = TextAlignmentOptions.Center;

        panelToast = toast;
        txtToastMessage = tmp;
        panelToast.SetActive(false);
    }
}
