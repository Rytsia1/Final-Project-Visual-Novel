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

    [Header("Calendar & Time Block")]
    public TextMeshProUGUI txtDayNumber;
    public TextMeshProUGUI txtTimeBlock;
    public TextMeshProUGUI txtWeather;

    [Header("Day Transition Splash (Persona Style)")]
    public GameObject panelDaySplashRoot;
    public CanvasGroup splashCanvasGroup;
    public TextMeshProUGUI txtSplashDay;
    public TextMeshProUGUI txtSplashDayName;
    public TextMeshProUGUI txtSplashWeather;
    private Coroutine _splashCoroutine;

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
        UpdateHUD();
        HidePredictiveTooltip();
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
        if (PlayerStats.Instance == null || GameManager.Instance == null) return;

        // Render nilai parameter pemain
        if (txtPhysicalHealth != null) txtPhysicalHealth.text = $"PH: {PlayerStats.Instance.physicalHealth}/100";
        if (txtMentalHealth != null) txtMentalHealth.text = $"MH: {PlayerStats.Instance.mentalHealth}/100";
        if (txtLanguage != null) txtLanguage.text = $"Bahasa: {PlayerStats.Instance.languageProficiency}";
        if (txtEtiquette != null) txtEtiquette.text = $"Etika: {PlayerStats.Instance.culturalEtiquette}";
        if (txtTheoretical != null) txtTheoretical.text = $"Teori: {PlayerStats.Instance.academicTheoretical}";
        if (txtPractical != null) txtPractical.text = $"Praktis: {PlayerStats.Instance.academicPractical}";

        // Render tanggal, blok waktu, dan cuaca (Persona Style)
        if (txtDayNumber != null) txtDayNumber.text = GameManager.Instance.GetFormattedDay();
        if (txtTimeBlock != null) txtTimeBlock.text = GameManager.Instance.currentTimeBlock.ToString().ToUpper();
        if (txtWeather != null) txtWeather.text = $"CUACA: {GameManager.Instance.currentWeather.ToString().ToUpper()}";

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

    // =========================================================
    // DAY TRANSITION SPLASH (PERSONA STYLE)
    // =========================================================
    public void PlayDayTransitionSplash()
    {
        if (_splashCoroutine != null)
        {
            StopCoroutine(_splashCoroutine);
        }
        _splashCoroutine = StartCoroutine(RoutineDayTransitionSplash());
    }

    private System.Collections.IEnumerator RoutineDayTransitionSplash()
    {
        if (panelDaySplashRoot == null)
        {
            EnsureDaySplashPanel();
        }

        if (panelDaySplashRoot != null && GameManager.Instance != null)
        {
            panelDaySplashRoot.SetActive(true);

            if (splashCanvasGroup != null)
            {
                splashCanvasGroup.alpha = 1f;
            }

            string dayPadded = GameManager.Instance.currentDay < 10 
                ? $"0{GameManager.Instance.currentDay}" 
                : $"{GameManager.Instance.currentDay}";

            if (txtSplashDay != null) txtSplashDay.text = $"HARI {dayPadded}";
            if (txtSplashDayName != null) txtSplashDayName.text = $"{GameManager.Instance.GetDayName().ToUpper()} / {GameManager.Instance.currentTimeBlock.ToString().ToUpper()}";
            if (txtSplashWeather != null) txtSplashWeather.text = $"CUACA: {GameManager.Instance.currentWeather.ToString().ToUpper()}";

            yield return new WaitForSeconds(1.0f);

            float elapsed = 0f;
            float fadeDuration = 0.5f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                if (splashCanvasGroup != null)
                {
                    splashCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
                }
                yield return null;
            }

            if (splashCanvasGroup != null)
            {
                splashCanvasGroup.alpha = 0f;
            }
            panelDaySplashRoot.SetActive(false);
        }
    }

    private void EnsureDaySplashPanel()
    {
        Transform t = transform.Find("Panel_DayTransitionSplash");
        if (t != null)
        {
            panelDaySplashRoot = t.gameObject;
            splashCanvasGroup = panelDaySplashRoot.GetComponent<CanvasGroup>();
            txtSplashDay = panelDaySplashRoot.transform.Find("Txt_SplashDayNumber")?.GetComponent<TextMeshProUGUI>();
            txtSplashDayName = panelDaySplashRoot.transform.Find("Txt_SplashSubInfo")?.GetComponent<TextMeshProUGUI>();
            txtSplashWeather = panelDaySplashRoot.transform.Find("Txt_SplashWeather")?.GetComponent<TextMeshProUGUI>();
            return;
        }

        // Dynamic fallback creation
        GameObject splash = new GameObject("Panel_DayTransitionSplash");
        splash.transform.SetParent(transform, false);

        RectTransform rt = splash.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;

        Image bg = splash.AddComponent<Image>();
        bg.color = new Color(0.106f, 0.165f, 0.278f, 0.98f); // Deep charcoal #1B2A47

        splashCanvasGroup = splash.AddComponent<CanvasGroup>();
        splashCanvasGroup.alpha = 1f;

        VerticalLayoutGroup vlg = splash.AddComponent<VerticalLayoutGroup>();
        vlg.childAlignment = TextAnchor.MiddleCenter;
        vlg.spacing = 16f;

        // Txt_SplashDayNumber
        GameObject objDay = new GameObject("Txt_SplashDayNumber");
        objDay.transform.SetParent(splash.transform, false);
        txtSplashDay = objDay.AddComponent<TextMeshProUGUI>();
        txtSplashDay.fontSize = 54;
        txtSplashDay.fontStyle = FontStyles.Bold;
        txtSplashDay.alignment = TextAlignmentOptions.Center;
        txtSplashDay.color = new Color(1f, 0.85f, 0.25f);

        // Txt_SplashSubInfo
        GameObject objSub = new GameObject("Txt_SplashSubInfo");
        objSub.transform.SetParent(splash.transform, false);
        txtSplashDayName = objSub.AddComponent<TextMeshProUGUI>();
        txtSplashDayName.fontSize = 28;
        txtSplashDayName.fontStyle = FontStyles.Bold;
        txtSplashDayName.alignment = TextAlignmentOptions.Center;
        txtSplashDayName.color = Color.white;

        // Txt_SplashWeather
        GameObject objWeather = new GameObject("Txt_SplashWeather");
        objWeather.transform.SetParent(splash.transform, false);
        txtSplashWeather = objWeather.AddComponent<TextMeshProUGUI>();
        txtSplashWeather.fontSize = 20;
        txtSplashWeather.alignment = TextAlignmentOptions.Center;
        txtSplashWeather.color = new Color(0.7f, 0.85f, 1f);

        panelDaySplashRoot = splash;
        panelDaySplashRoot.SetActive(false);
    }
}
