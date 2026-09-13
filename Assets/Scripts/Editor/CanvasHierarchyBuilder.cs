using System.IO;
using System.Data.SQLite;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEditor.SceneManagement;

public static class CanvasHierarchyBuilder
{
    [MenuItem("Game Debug/Build HUD Canvas Hierarchy")]
    public static void BuildHierarchy()
    {
        // 1. Dapatkan atau buat Canvas
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        GameObject canvasGO;

        if (canvas == null)
        {
            canvasGO = new GameObject("Canvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<GraphicRaycaster>();
        }
        else
        {
            canvasGO = canvas.gameObject;
        }

        // Konfigurasi Canvas Scaler sesuai spesifikasi
        CanvasScaler scaler = canvasGO.GetComponent<CanvasScaler>();
        if (scaler == null) scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        // Pastikan EventSystem ada di Scene
        EventSystem es = null;
        var activeScene = EditorSceneManager.GetActiveScene();
        foreach (var root in activeScene.GetRootGameObjects())
        {
            var found = root.GetComponentInChildren<EventSystem>(true);
            if (found != null) { es = found; break; }
        }

        if (es == null)
        {
            GameObject esGO = new GameObject("EventSystem");
            es = esGO.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM
            esGO.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
            esGO.AddComponent<StandaloneInputModule>();
#endif
            EditorUtility.SetDirty(esGO);
            Undo.RegisterCreatedObjectUndo(esGO, "Create EventSystem");
        }

        // Hapus struktur panel lama jika ingin membuat baru secara bersih
        Transform existingTop = canvasGO.transform.Find("Panel_TopStats");
        if (existingTop != null) Object.DestroyImmediate(existingTop.gameObject);

        Transform existingCal = canvasGO.transform.Find("Panel_Calendar");
        if (existingCal != null) Object.DestroyImmediate(existingCal.gameObject);

        Transform existingAct = canvasGO.transform.Find("Panel_ActivityGrid");
        if (existingAct != null) Object.DestroyImmediate(existingAct.gameObject);

        Transform existingTooltip = canvasGO.transform.Find("Panel_PredictiveTooltip");
        if (existingTooltip != null) Object.DestroyImmediate(existingTooltip.gameObject);

        Transform existingDialogue = canvasGO.transform.Find("Panel_DialogueBox");
        if (existingDialogue != null) Object.DestroyImmediate(existingDialogue.gameObject);

        // 2. Buat Panel_TopStats (Static HUD Bar)
        GameObject panelTopStats = CreateUIObject("Panel_TopStats", canvasGO.transform);
        RectTransform rtTop = panelTopStats.GetComponent<RectTransform>();
        rtTop.anchorMin = new Vector2(0f, 1f);
        rtTop.anchorMax = new Vector2(1f, 1f);
        rtTop.pivot = new Vector2(0.5f, 1f);
        rtTop.anchoredPosition = Vector2.zero;
        rtTop.sizeDelta = new Vector2(0f, 60f);

        Image imgTop = panelTopStats.AddComponent<Image>();
        imgTop.color = new Color(0.08f, 0.09f, 0.14f, 0.92f);

        HorizontalLayoutGroup hlgTop = panelTopStats.AddComponent<HorizontalLayoutGroup>();
        hlgTop.padding = new RectOffset(40, 40, 10, 10);
        hlgTop.spacing = 30;
        hlgTop.childAlignment = TextAnchor.MiddleCenter;
        hlgTop.childControlWidth = true;
        hlgTop.childControlHeight = true;
        hlgTop.childForceExpandWidth = true;
        hlgTop.childForceExpandHeight = false;

        TextMeshProUGUI txtPH = CreateText("Txt_PhysicalHealth", panelTopStats.transform, "PH: 100/100", 20, new Color(1f, 0.4f, 0.4f));
        TextMeshProUGUI txtMH = CreateText("Txt_MentalHealth", panelTopStats.transform, "MH: 80/100", 20, new Color(0.4f, 0.8f, 1f));
        TextMeshProUGUI txtLang = CreateText("Txt_Language", panelTopStats.transform, "Bahasa: 20", 20, new Color(0.4f, 1f, 0.5f));
        TextMeshProUGUI txtEtiq = CreateText("Txt_Etiquette", panelTopStats.transform, "Etika: 15", 20, new Color(1f, 0.85f, 0.3f));
        TextMeshProUGUI txtTheo = CreateText("Txt_Theoretical", panelTopStats.transform, "Teori: 30", 20, new Color(0.6f, 0.7f, 1f));
        TextMeshProUGUI txtPrac = CreateText("Txt_Practical", panelTopStats.transform, "Praktis: 40", 20, new Color(1f, 0.6f, 0.2f));

        // Tombol Buka Status Hubungan Sosial (Guanxi & Bakudan)
        Button btnOpenSocial = CreateButton("Btn_OpenSocial", panelTopStats.transform, "🌸 Relasi", new Color(0.38f, 0.22f, 0.55f));
        RectTransform rtBtnSocial = btnOpenSocial.GetComponent<RectTransform>();
        rtBtnSocial.sizeDelta = new Vector2(140f, 40f);
        LayoutElement leBtnSocial = btnOpenSocial.gameObject.AddComponent<LayoutElement>();
        leBtnSocial.preferredWidth = 140f;
        leBtnSocial.preferredHeight = 40f;
        leBtnSocial.flexibleWidth = 0f;

        // 3. Buat Panel_Calendar (Pojok Kanan Atas)
        GameObject panelCalendar = CreateUIObject("Panel_Calendar", canvasGO.transform);
        RectTransform rtCal = panelCalendar.GetComponent<RectTransform>();
        rtCal.anchorMin = new Vector2(1f, 1f);
        rtCal.anchorMax = new Vector2(1f, 1f);
        rtCal.pivot = new Vector2(1f, 1f);
        rtCal.anchoredPosition = new Vector2(-40f, -80f);
        rtCal.sizeDelta = new Vector2(240f, 100f);

        Image imgCal = panelCalendar.AddComponent<Image>();
        imgCal.color = new Color(0.12f, 0.14f, 0.22f, 0.9f);

        VerticalLayoutGroup vlgCal = panelCalendar.AddComponent<VerticalLayoutGroup>();
        vlgCal.padding = new RectOffset(15, 15, 12, 12);
        vlgCal.spacing = 6;
        vlgCal.childAlignment = TextAnchor.MiddleCenter;
        vlgCal.childControlWidth = true;
        vlgCal.childControlHeight = true;
        vlgCal.childForceExpandWidth = true;
        vlgCal.childForceExpandHeight = true;

        TextMeshProUGUI txtDay = CreateText("Txt_DayNumber", panelCalendar.transform, "HARI 01", 24, Color.white, true);
        txtDay.alignment = TextAlignmentOptions.Center;
        TextMeshProUGUI txtTime = CreateText("Txt_TimeBlock", panelCalendar.transform, "PAGI", 20, new Color(1f, 0.85f, 0.2f), true);
        txtTime.alignment = TextAlignmentOptions.Center;

        // 4. Buat Panel_ActivityGrid (Tengah / Kiri)
        GameObject panelActivity = CreateUIObject("Panel_ActivityGrid", canvasGO.transform);
        RectTransform rtAct = panelActivity.GetComponent<RectTransform>();
        rtAct.anchorMin = new Vector2(0f, 0.5f);
        rtAct.anchorMax = new Vector2(0f, 0.5f);
        rtAct.pivot = new Vector2(0f, 0.5f);
        rtAct.anchoredPosition = new Vector2(50f, 20f);
        rtAct.sizeDelta = new Vector2(340f, 380f);

        Image imgAct = panelActivity.AddComponent<Image>();
        imgAct.color = new Color(0.09f, 0.1f, 0.16f, 0.85f);

        VerticalLayoutGroup vlgAct = panelActivity.AddComponent<VerticalLayoutGroup>();
        vlgAct.padding = new RectOffset(20, 20, 25, 25);
        vlgAct.spacing = 16;
        vlgAct.childAlignment = TextAnchor.UpperCenter;
        vlgAct.childControlWidth = true;
        vlgAct.childControlHeight = false;
        vlgAct.childForceExpandWidth = true;
        vlgAct.childForceExpandHeight = false;

        // Pasang ActivityButtonHandler pada Canvas atau Panel
        ActivityButtonHandler btnHandler = canvasGO.GetComponent<ActivityButtonHandler>();
        if (btnHandler == null) btnHandler = canvasGO.AddComponent<ActivityButtonHandler>();

        Button btnStudy = CreateButton("Btn_StudyLanguage", panelActivity.transform, "Belajar Kosakata", new Color(0.2f, 0.45f, 0.75f));
        Button btnLunch = CreateButton("Btn_LunchWithNPC", panelActivity.transform, "Makan Siang Li Haoran", new Color(0.25f, 0.6f, 0.4f));
        Button btnLecturer = CreateButton("Btn_MeetLecturer", panelActivity.transform, "Laporan Progres Dosen", new Color(0.75f, 0.45f, 0.2f));
        Button btnSleep = CreateButton("Btn_SleepEarly", panelActivity.transform, "Istirahat / Tidur", new Color(0.5f, 0.35f, 0.65f));

        // Hubungkan Event Klik tombol ke ActivityButtonHandler secara persistent (tersimpan di Scene)
        UnityEditor.Events.UnityEventTools.AddPersistentListener(btnStudy.onClick, btnHandler.OnClick_StudyLanguage);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(btnLunch.onClick, btnHandler.OnClick_LunchWithLiHaoran);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(btnLecturer.onClick, btnHandler.OnClick_ReportToLecturer);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(btnSleep.onClick, btnHandler.OnClick_Sleep);

        // Tambahkan ActivityTooltipTrigger untuk fitur Predictive Visual Feedback (Bab 3.3.1 A)
        ActivityTooltipTrigger ttStudy = btnStudy.gameObject.AddComponent<ActivityTooltipTrigger>();
        ttStudy.activityDescription = "Menghafal kosakata Mandarin intensif di perpustakaan.";
        ttStudy.costGainPreview = "Biaya: PH -5, MH -10 | Efek: Bahasa +15";

        ActivityTooltipTrigger ttLunch = btnLunch.gameObject.AddComponent<ActivityTooltipTrigger>();
        ttLunch.activityDescription = "Makan siang bersama Li Haoran sambil membawa bekal masakan Indonesia.";
        ttLunch.costGainPreview = "Biaya: PH -5, MH +10 | Efek: Guanxi +10, Etika +5";

        ActivityTooltipTrigger ttLecturer = btnLecturer.gameObject.AddComponent<ActivityTooltipTrigger>();
        ttLecturer.activityDescription = "Menemui Dosen Xiang Bai untuk asistensi progres analisis data.";
        ttLecturer.costGainPreview = "Syarat: Bahasa >= 30, Etika >= 50 | Risiko: Penalti Mianzi";

        ActivityTooltipTrigger ttSleep = btnSleep.gameObject.AddComponent<ActivityTooltipTrigger>();
        ttSleep.activityDescription = "Mengakhiri hari lebih awal untuk istirahat penuh.";
        ttSleep.costGainPreview = "Efek: PH +40, MH +40, Hari Berlanjut";

        // Tambahkan hover listener alternatif jika EventTrigger dibutuhkan
        AddHoverEvents(btnStudy.gameObject, btnHandler.OnHover_StudyLanguage, btnHandler.OnPointerExit);
        AddHoverEvents(btnLunch.gameObject, btnHandler.OnHover_LunchWithLiHaoran, btnHandler.OnPointerExit);
        AddHoverEvents(btnLecturer.gameObject, btnHandler.OnHover_ReportToLecturer, btnHandler.OnPointerExit);
        AddHoverEvents(btnSleep.gameObject, btnHandler.OnHover_Sleep, btnHandler.OnPointerExit);

        // 5. Buat Panel_PredictiveTooltip (Bawah)
        GameObject panelTooltip = CreateUIObject("Panel_PredictiveTooltip", canvasGO.transform);
        RectTransform rtTooltip = panelTooltip.GetComponent<RectTransform>();
        rtTooltip.anchorMin = new Vector2(0.5f, 0f);
        rtTooltip.anchorMax = new Vector2(0.5f, 0f);
        rtTooltip.pivot = new Vector2(0.5f, 0f);
        rtTooltip.anchoredPosition = new Vector2(0f, 30f);
        rtTooltip.sizeDelta = new Vector2(850f, 95f);

        Image imgTooltip = panelTooltip.AddComponent<Image>();
        imgTooltip.color = new Color(0.08f, 0.09f, 0.15f, 0.95f);

        VerticalLayoutGroup vlgTooltip = panelTooltip.AddComponent<VerticalLayoutGroup>();
        vlgTooltip.padding = new RectOffset(25, 25, 12, 12);
        vlgTooltip.spacing = 4;
        vlgTooltip.childAlignment = TextAnchor.MiddleCenter;
        vlgTooltip.childControlWidth = true;
        vlgTooltip.childControlHeight = true;
        vlgTooltip.childForceExpandWidth = true;
        vlgTooltip.childForceExpandHeight = true;

        TextMeshProUGUI txtDesc = CreateText("Txt_ActivityDescription", panelTooltip.transform, "Pilih aktivitas harian Devano Baskara Pratama.", 18, Color.white);
        txtDesc.alignment = TextAlignmentOptions.Center;
        TextMeshProUGUI txtCost = CreateText("Txt_CostGainPreview", panelTooltip.transform, "Biaya & Efek", 16, new Color(1f, 0.85f, 0.3f), true);
        txtCost.alignment = TextAlignmentOptions.Center;

        // 6. Buat Panel_DialogueBox (Overlay Visual Novel - Set Inactive di Awal)
        GameObject panelDialogue = CreateUIObject("Panel_DialogueBox", canvasGO.transform);
        RectTransform rtDia = panelDialogue.GetComponent<RectTransform>();
        rtDia.anchorMin = new Vector2(0.12f, 0f);
        rtDia.anchorMax = new Vector2(0.88f, 0f);
        rtDia.pivot = new Vector2(0.5f, 0f);
        rtDia.anchoredPosition = new Vector2(0f, 30f);
        rtDia.sizeDelta = new Vector2(0f, 260f);

        Image imgDia = panelDialogue.AddComponent<Image>();
        imgDia.color = new Color(0.06f, 0.07f, 0.12f, 0.97f);

        // Pembicara
        GameObject goSpeaker = CreateUIObject("Txt_SpeakerName", panelDialogue.transform);
        RectTransform rtSpeaker = goSpeaker.GetComponent<RectTransform>();
        rtSpeaker.anchorMin = new Vector2(0f, 1f);
        rtSpeaker.anchorMax = new Vector2(0.5f, 1f);
        rtSpeaker.pivot = new Vector2(0f, 1f);
        rtSpeaker.anchoredPosition = new Vector2(30f, -15f);
        rtSpeaker.sizeDelta = new Vector2(400f, 40f);
        TextMeshProUGUI txtSpeaker = goSpeaker.AddComponent<TextMeshProUGUI>();
        txtSpeaker.text = "Xiang Bai";
        txtSpeaker.fontSize = 24;
        txtSpeaker.fontStyle = FontStyles.Bold;
        txtSpeaker.color = new Color(0.3f, 0.85f, 1f);

        // Konten Dialog
        GameObject goContent = CreateUIObject("Txt_DialogueContent", panelDialogue.transform);
        RectTransform rtContent = goContent.GetComponent<RectTransform>();
        rtContent.anchorMin = new Vector2(0f, 0f);
        rtContent.anchorMax = new Vector2(1f, 1f);
        rtContent.pivot = new Vector2(0.5f, 0.5f);
        rtContent.offsetMin = new Vector2(30f, 90f);
        rtContent.offsetMax = new Vector2(-30f, -60f);
        TextMeshProUGUI txtContent = goContent.AddComponent<TextMeshProUGUI>();
        txtContent.text = "Devano, bagaimana progres analisis data untuk tugas mingguanmu?";
        txtContent.fontSize = 20;
        txtContent.textWrappingMode = TextWrappingModes.Normal;

        // Container Pilihan Opsi
        GameObject goOptions = CreateUIObject("OptionsContainer", panelDialogue.transform);
        RectTransform rtOptions = goOptions.GetComponent<RectTransform>();
        rtOptions.anchorMin = new Vector2(0f, 0f);
        rtOptions.anchorMax = new Vector2(1f, 0f);
        rtOptions.pivot = new Vector2(0.5f, 0f);
        rtOptions.anchoredPosition = new Vector2(0f, 15f);
        rtOptions.sizeDelta = new Vector2(-60f, 70f);

        VerticalLayoutGroup vlgOptions = goOptions.AddComponent<VerticalLayoutGroup>();
        vlgOptions.spacing = 8;
        vlgOptions.childControlWidth = true;
        vlgOptions.childControlHeight = true;
        vlgOptions.childForceExpandWidth = true;
        vlgOptions.childForceExpandHeight = true;

        // 7. Siapkan Prefab Tombol Respon dan Konfigurasi DialogueUIController
        GameObject optionPrefab = CreateOrLoadOptionButtonPrefab();

        DialogueUIController dialogueUI = panelDialogue.GetComponent<DialogueUIController>();
        if (dialogueUI == null) dialogueUI = panelDialogue.AddComponent<DialogueUIController>();

        dialogueUI.dialoguePanel = panelDialogue;
        dialogueUI.txtSpeakerName = txtSpeaker;
        dialogueUI.txtDialogueContent = txtContent;
        dialogueUI.optionsContainer = goOptions.transform;
        dialogueUI.optionButtonPrefab = optionPrefab;

        // Set Inactive di awal sesuai spesifikasi
        panelDialogue.SetActive(false);

        // 8. Konfigurasi HUDController dan sambungkan seluruh referensi
        HUDController hud = canvasGO.GetComponent<HUDController>();
        if (hud == null) hud = canvasGO.AddComponent<HUDController>();

        hud.txtPhysicalHealth = txtPH;
        hud.txtMentalHealth = txtMH;
        hud.txtLanguage = txtLang;
        hud.txtEtiquette = txtEtiq;
        hud.txtTheoretical = txtTheo;
        hud.txtPractical = txtPrac;

        hud.txtDayNumber = txtDay;
        hud.txtTimeBlock = txtTime;

        hud.panelTooltip = panelTooltip;
        hud.txtActivityDesc = txtDesc;
        hud.txtCostGainPreview = txtCost;

        hud.btnStudyLanguage = btnStudy;
        hud.btnLunchWithNPC = btnLunch;
        hud.btnMeetLecturer = btnLecturer;
        hud.btnSleep = btnSleep;

        // 9. Bangun Antarmuka Smartphone (PhoneUIController)
        BuildPhoneUI(canvasGO, hud);

        // 10. Bangun Jendela Status Hubungan & Sistem Bakudan (SocialStatusWindowUI)
        BuildSocialStatusWindowUI(canvasGO, hud, btnOpenSocial);

        // Pastikan PhoneOutingManager dan TelemetryLogger terpasang di GameObject GAME_CORE
        GameObject gameCore = GameObject.Find("GAME_CORE") ?? GameObject.Find("[GAME_CORE]");
        if (gameCore == null)
        {
            GameManager gm = Object.FindFirstObjectByType<GameManager>();
            if (gm != null) gameCore = gm.gameObject;
        }

        if (gameCore != null)
        {
            TelemetryLogger tel = gameCore.GetComponent<TelemetryLogger>();
            if (tel == null)
            {
                tel = gameCore.AddComponent<TelemetryLogger>();
                EditorUtility.SetDirty(gameCore);
            }

            PhoneOutingManager pom = gameCore.GetComponent<PhoneOutingManager>();
            if (pom == null)
            {
                pom = gameCore.AddComponent<PhoneOutingManager>();
                EditorUtility.SetDirty(gameCore);
            }
        }

        // Simpan perubahan ke Scene
        EditorUtility.SetDirty(canvasGO);
        activeScene = EditorSceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(activeScene);
        EditorSceneManager.SaveScene(activeScene);

        Debug.Log("<color=green>[HUD Setup]</color> Berhasil menyusun hierarki Canvas UI 1920x1080 dan mengaitkan seluruh referensi HUDController & PhoneUIController!");
    }

    [MenuItem("Game Debug/Build Smartphone UI")]
    public static void BuildSmartphoneUIMenu()
    {
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("[PhoneUI Setup] Canvas tidak ditemukan!");
            return;
        }

        HUDController hud = canvas.GetComponent<HUDController>();
        BuildPhoneUI(canvas.gameObject, hud);

        var scene = EditorSceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("<color=green>[PhoneUI Setup]</color> Berhasil menyusun hierarki Smartphone UI dan mengaitkannya ke scene!");
    }

    public static void BuildPhoneUI(GameObject canvasGO, HUDController hud)
    {
        // 1. Bersihkan elemen lama jika ada
        Transform existingBtnPhone = canvasGO.transform.Find("Btn_OpenPhone");
        if (existingBtnPhone != null) Object.DestroyImmediate(existingBtnPhone.gameObject);

        Transform existingPanelPhone = canvasGO.transform.Find("Panel_Phone");
        if (existingPanelPhone != null) Object.DestroyImmediate(existingPanelPhone.gameObject);

        // 2. Buat Btn_OpenPhone di pojok kanan bawah HUD
        Button btnOpenPhone = CreateButton("Btn_OpenPhone", canvasGO.transform, "📱 Smartphone", new Color(0.16f, 0.28f, 0.48f));
        RectTransform rtBtnPhone = btnOpenPhone.GetComponent<RectTransform>();
        rtBtnPhone.anchorMin = new Vector2(1f, 0f);
        rtBtnPhone.anchorMax = new Vector2(1f, 0f);
        rtBtnPhone.pivot = new Vector2(1f, 0f);
        rtBtnPhone.anchoredPosition = new Vector2(-40f, 30f);
        rtBtnPhone.sizeDelta = new Vector2(220f, 60f);

        if (hud != null)
        {
            hud.btnOpenPhone = btnOpenPhone;
            EditorUtility.SetDirty(hud);
        }

        // 3. Buat Panel_Phone (Container Smartphone Pop-up)
        GameObject panelPhone = CreateUIObject("Panel_Phone", canvasGO.transform);
        RectTransform rtPhone = panelPhone.GetComponent<RectTransform>();
        rtPhone.anchorMin = new Vector2(0.5f, 0.5f);
        rtPhone.anchorMax = new Vector2(0.5f, 0.5f);
        rtPhone.pivot = new Vector2(0.5f, 0.5f);
        rtPhone.anchoredPosition = new Vector2(0f, 0f);
        rtPhone.sizeDelta = new Vector2(440f, 660f);

        Image imgPhoneBg = panelPhone.AddComponent<Image>();
        imgPhoneBg.color = new Color(0.07f, 0.08f, 0.14f, 0.98f);

        PhoneUIController phoneUI = panelPhone.AddComponent<PhoneUIController>();

        // 4. Panel_MainMenu
        GameObject panelMainMenu = CreateUIObject("Panel_MainMenu", panelPhone.transform);
        RectTransform rtMainMenu = panelMainMenu.GetComponent<RectTransform>();
        rtMainMenu.anchorMin = Vector2.zero;
        rtMainMenu.anchorMax = Vector2.one;
        rtMainMenu.sizeDelta = Vector2.zero;

        VerticalLayoutGroup vlgMain = panelMainMenu.AddComponent<VerticalLayoutGroup>();
        vlgMain.padding = new RectOffset(25, 25, 30, 25);
        vlgMain.spacing = 14;
        vlgMain.childAlignment = TextAnchor.UpperCenter;
        vlgMain.childControlWidth = true;
        vlgMain.childControlHeight = false;
        vlgMain.childForceExpandWidth = true;
        vlgMain.childForceExpandHeight = false;

        TextMeshProUGUI txtTitleMain = CreateText("Txt_PhoneTitle", panelMainMenu.transform, "📱 SMARTPHONE DEVANO", 22, new Color(0.3f, 0.85f, 1f), true);
        txtTitleMain.alignment = TextAlignmentOptions.Center;

        TextMeshProUGUI txtSubMain = CreateText("Txt_PhoneSubtitle", panelMainMenu.transform, "Radar Intelijen & Janjian Akhir Pekan", 14, new Color(0.7f, 0.75f, 0.85f));
        txtSubMain.alignment = TextAlignmentOptions.Center;

        Button btnRadarHaoran = CreateButton("Btn_CallEdelweiss_Haoran", panelMainMenu.transform, "Tanya Radar: Li Haoran", new Color(0.18f, 0.42f, 0.55f));
        Button btnRadarXiangBai = CreateButton("Btn_CallEdelweiss_XiangBai", panelMainMenu.transform, "Tanya Radar: Dosen Xiang Bai", new Color(0.24f, 0.35f, 0.58f));
        Button btnRadarYangMei = CreateButton("Btn_CallEdelweiss_YangMei", panelMainMenu.transform, "Tanya Radar: Yang Mei", new Color(0.50f, 0.25f, 0.42f));
        Button btnAjakJalan = CreateButton("Btn_AjakJalan", panelMainMenu.transform, "Ajak Hangout Akhir Pekan", new Color(0.65f, 0.45f, 0.18f));
        Button btnClosePhone = CreateButton("Btn_ClosePhone", panelMainMenu.transform, "Tutup HP", new Color(0.42f, 0.20f, 0.24f));

        UnityEditor.Events.UnityEventTools.AddIntPersistentListener(btnRadarHaoran.onClick, phoneUI.OnClick_TanyaEdelweiss, 102);
        UnityEditor.Events.UnityEventTools.AddIntPersistentListener(btnRadarXiangBai.onClick, phoneUI.OnClick_TanyaEdelweiss, 101);
        UnityEditor.Events.UnityEventTools.AddIntPersistentListener(btnRadarYangMei.onClick, phoneUI.OnClick_TanyaEdelweiss, 103);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(btnAjakJalan.onClick, phoneUI.OnClick_BukaMenuHangout);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(btnClosePhone.onClick, phoneUI.TutupPhone);

        // 5. Panel_SelectContact
        GameObject panelSelectContact = CreateUIObject("Panel_SelectContact", panelPhone.transform);
        RectTransform rtSelectContact = panelSelectContact.GetComponent<RectTransform>();
        rtSelectContact.anchorMin = Vector2.zero;
        rtSelectContact.anchorMax = Vector2.one;
        rtSelectContact.sizeDelta = Vector2.zero;

        VerticalLayoutGroup vlgContact = panelSelectContact.AddComponent<VerticalLayoutGroup>();
        vlgContact.padding = new RectOffset(25, 25, 30, 25);
        vlgContact.spacing = 14;
        vlgContact.childAlignment = TextAnchor.UpperCenter;
        vlgContact.childControlWidth = true;
        vlgContact.childControlHeight = false;
        vlgContact.childForceExpandWidth = true;
        vlgContact.childForceExpandHeight = false;

        TextMeshProUGUI txtTitleContact = CreateText("Txt_ContactTitle", panelSelectContact.transform, "👥 PILIH TEMAN HANGOUT", 22, new Color(1f, 0.85f, 0.3f), true);
        txtTitleContact.alignment = TextAlignmentOptions.Center;

        TextMeshProUGUI txtSubContact = CreateText("Txt_ContactSubtitle", panelSelectContact.transform, "Pilih target sosialisasi akhir pekan:", 14, new Color(0.7f, 0.75f, 0.85f));
        txtSubContact.alignment = TextAlignmentOptions.Center;

        Button btnContactHaoran = CreateButton("Btn_Contact_Haoran", panelSelectContact.transform, "Li Haoran", new Color(0.18f, 0.42f, 0.55f));
        Button btnContactXiangBai = CreateButton("Btn_Contact_XiangBai", panelSelectContact.transform, "Dosen Xiang Bai", new Color(0.24f, 0.35f, 0.58f));
        Button btnContactYangMei = CreateButton("Btn_Contact_YangMei", panelSelectContact.transform, "Yang Mei", new Color(0.50f, 0.25f, 0.42f));
        Button btnBackContact = CreateButton("Btn_BackContact", panelSelectContact.transform, "Kembali", new Color(0.35f, 0.35f, 0.40f));

        UnityEditor.Events.UnityEventTools.AddIntPersistentListener(btnContactHaoran.onClick, phoneUI.OnSelectNpcForHangout, 102);
        UnityEditor.Events.UnityEventTools.AddIntPersistentListener(btnContactXiangBai.onClick, phoneUI.OnSelectNpcForHangout, 101);
        UnityEditor.Events.UnityEventTools.AddIntPersistentListener(btnContactYangMei.onClick, phoneUI.OnSelectNpcForHangout, 103);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(btnBackContact.onClick, phoneUI.OnClick_KembaliKeMenuUtama);

        // 6. Panel_SelectVenue
        GameObject panelSelectVenue = CreateUIObject("Panel_SelectVenue", panelPhone.transform);
        RectTransform rtSelectVenue = panelSelectVenue.GetComponent<RectTransform>();
        rtSelectVenue.anchorMin = Vector2.zero;
        rtSelectVenue.anchorMax = Vector2.one;
        rtSelectVenue.sizeDelta = Vector2.zero;

        VerticalLayoutGroup vlgVenue = panelSelectVenue.AddComponent<VerticalLayoutGroup>();
        vlgVenue.padding = new RectOffset(25, 25, 30, 25);
        vlgVenue.spacing = 12;
        vlgVenue.childAlignment = TextAnchor.UpperCenter;
        vlgVenue.childControlWidth = true;
        vlgVenue.childControlHeight = false;
        vlgVenue.childForceExpandWidth = true;
        vlgVenue.childForceExpandHeight = false;

        TextMeshProUGUI txtTitleVenue = CreateText("Txt_VenueTitle", panelSelectVenue.transform, "📍 PILIH LOKASI HANGOUT", 22, new Color(0.4f, 0.95f, 0.6f), true);
        txtTitleVenue.alignment = TextAlignmentOptions.Center;

        TextMeshProUGUI txtSubVenue = CreateText("Txt_VenueSubtitle", panelSelectVenue.transform, "Perhatikan preferensi & syarat etika venue:", 14, new Color(0.7f, 0.75f, 0.85f));
        txtSubVenue.alignment = TextAlignmentOptions.Center;

        Button btnVenue1 = CreateButton("Btn_Venue1", panelSelectVenue.transform, "Kantin Muslim / Halal Street", new Color(0.20f, 0.50f, 0.35f));
        Button btnVenue2 = CreateButton("Btn_Venue2", panelSelectVenue.transform, "Distrik Elektronik", new Color(0.20f, 0.42f, 0.62f));
        Button btnVenue3 = CreateButton("Btn_Venue3", panelSelectVenue.transform, "Kedai Teh Tradisional", new Color(0.55f, 0.38f, 0.20f));
        Button btnVenue4 = CreateButton("Btn_Venue4", panelSelectVenue.transform, "Perpustakaan Kota", new Color(0.35f, 0.30f, 0.55f));
        Button btnBackVenue = CreateButton("Btn_BackVenue", panelSelectVenue.transform, "Kembali", new Color(0.35f, 0.35f, 0.40f));

        UnityEditor.Events.UnityEventTools.AddIntPersistentListener(btnVenue1.onClick, phoneUI.OnSelectVenueForHangout, 1);
        UnityEditor.Events.UnityEventTools.AddIntPersistentListener(btnVenue2.onClick, phoneUI.OnSelectVenueForHangout, 2);
        UnityEditor.Events.UnityEventTools.AddIntPersistentListener(btnVenue3.onClick, phoneUI.OnSelectVenueForHangout, 3);
        UnityEditor.Events.UnityEventTools.AddIntPersistentListener(btnVenue4.onClick, phoneUI.OnSelectVenueForHangout, 4);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(btnBackVenue.onClick, phoneUI.OnClick_KembaliKePilihKontak);

        // 7. Sambungkan referensi ke PhoneUIController
        phoneUI.panelPhoneApp = panelMainMenu;
        phoneUI.panelSelectContact = panelSelectContact;
        phoneUI.panelSelectVenue = panelSelectVenue;

        // 8. Hubungkan tombol Open Phone ke PhoneUIController
        UnityEditor.Events.UnityEventTools.AddPersistentListener(btnOpenPhone.onClick, phoneUI.BukaPhone);

        // 9. Konfigurasi State Awal: Matikan panel anak dan nonaktifkan Panel_Phone di awal
        panelMainMenu.SetActive(false);
        panelSelectContact.SetActive(false);
        panelSelectVenue.SetActive(false);
        panelPhone.SetActive(false);

        EditorUtility.SetDirty(panelPhone);
        EditorUtility.SetDirty(phoneUI);
        EditorUtility.SetDirty(btnOpenPhone);
    }

    [MenuItem("Game Debug/Attach TelemetryLogger to GAME_CORE")]
    public static void AttachTelemetryLogger()
    {
        GameObject gameCore = GameObject.Find("GAME_CORE") ?? GameObject.Find("[GAME_CORE]");
        if (gameCore == null)
        {
            var gm = Object.FindFirstObjectByType<GameManager>();
            if (gm != null) gameCore = gm.gameObject;
        }

        if (gameCore != null)
        {
            if (gameCore.GetComponent<TelemetryLogger>() == null)
            {
                gameCore.AddComponent<TelemetryLogger>();
                EditorUtility.SetDirty(gameCore);
                var scene = EditorSceneManager.GetActiveScene();
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                Debug.Log("<color=green>[TelemetryLogger]</color> TelemetryLogger berhasil dipasang pada " + gameCore.name);
            }
            else
            {
                Debug.Log("<color=yellow>[TelemetryLogger]</color> TelemetryLogger sudah terpasang pada " + gameCore.name);
            }
        }
    }

    [MenuItem("Game Debug/Migrate Kenzo To Devano In Databases")]
    public static void MigrateKenzoToDevano()
    {
        string[] paths = new string[]
        {
            Path.Combine(Application.streamingAssetsPath, "game_database.db"),
            Path.Combine(Application.persistentDataPath, "game_database.db")
        };

        foreach (var path in paths)
        {
            if (File.Exists(path))
            {
                try
                {
                    using (var conn = new SQLiteConnection($"Data Source={path};Version=3;"))
                    {
                        conn.Open();
                        using (var cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = @"
                                UPDATE tbl_player_profile SET player_name = 'Devano Baskara Pratama' WHERE player_id = 1;
                                UPDATE tbl_dialogue_nodes SET dialogue_text = REPLACE(dialogue_text, 'Kenzo Pratama', 'Devano Baskara Pratama');
                                UPDATE tbl_dialogue_nodes SET dialogue_text = REPLACE(dialogue_text, 'Kenzo', 'Devano');
                            ";
                            int affected = cmd.ExecuteNonQuery();
                            Debug.Log($"<color=green>[Migration]</color> Berhasil migrasi nama Kenzo -> Devano di: {path}. Rows affected: {affected}");
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"<color=red>[Migration Error]</color> Gagal migrasi di {path}: {ex.Message}");
                }
            }
            else
            {
                Debug.LogWarning($"<color=yellow>[Migration]</color> File tidak ditemukan: {path}");
            }
        }
    }

    private static GameObject CreateUIObject(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    private static TextMeshProUGUI CreateText(string name, Transform parent, string text, float size, Color color, bool bold = false)
    {
        GameObject go = CreateUIObject(name, parent);
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.color = color;
        if (bold) tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.MidlineLeft;
        tmp.raycastTarget = false;
        return tmp;
    }

    private static Button CreateButton(string name, Transform parent, string label, Color bgColor)
    {
        GameObject btnGO = CreateUIObject(name, parent);
        RectTransform rt = btnGO.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(300f, 60f);

        Image img = btnGO.AddComponent<Image>();
        img.color = bgColor;

        Button btn = btnGO.AddComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.normalColor = bgColor;
        cb.highlightedColor = bgColor * 1.2f;
        cb.pressedColor = bgColor * 0.8f;
        cb.disabledColor = new Color(bgColor.r * 0.5f, bgColor.g * 0.5f, bgColor.b * 0.5f, 0.5f);
        btn.colors = cb;

        GameObject textGO = CreateUIObject("Text (TMP)", btnGO.transform);
        RectTransform rtText = textGO.GetComponent<RectTransform>();
        rtText.anchorMin = Vector2.zero;
        rtText.anchorMax = Vector2.one;
        rtText.sizeDelta = Vector2.zero;

        TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 18;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.raycastTarget = false;

        return btn;
    }

    private static void AddHoverEvents(GameObject target, UnityEngine.Events.UnityAction onEnter, UnityEngine.Events.UnityAction onExit)
    {
        EventTrigger trigger = target.GetComponent<EventTrigger>();
        if (trigger == null) trigger = target.AddComponent<EventTrigger>();

        EventTrigger.Entry entryEnter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        entryEnter.callback.AddListener((data) => onEnter?.Invoke());
        trigger.triggers.Add(entryEnter);

        EventTrigger.Entry entryExit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        entryExit.callback.AddListener((data) => onExit?.Invoke());
        trigger.triggers.Add(entryExit);
    }

    public static GameObject CreateOrLoadOptionButtonPrefab()
    {
        string folder = "Assets/Prefabs";
        if (!AssetDatabase.IsValidFolder(folder))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }

        string prefabPath = $"{folder}/Btn_DialogueOption_Prefab.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab != null) return prefab;

        GameObject btnGO = new GameObject("Btn_DialogueOption_Prefab", typeof(RectTransform));
        RectTransform rt = btnGO.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(800f, 50f);

        Image img = btnGO.AddComponent<Image>();
        img.color = new Color(0.14f, 0.17f, 0.25f, 0.95f);

        Button btn = btnGO.AddComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.normalColor = new Color(0.14f, 0.17f, 0.25f, 0.95f);
        cb.highlightedColor = new Color(0.25f, 0.38f, 0.6f, 1f);
        cb.pressedColor = new Color(0.09f, 0.11f, 0.18f, 1f);
        cb.selectedColor = new Color(0.25f, 0.38f, 0.6f, 1f);
        btn.colors = cb;

        GameObject textGO = new GameObject("Text (TMP)", typeof(RectTransform));
        textGO.transform.SetParent(btnGO.transform, false);
        RectTransform rtText = textGO.GetComponent<RectTransform>();
        rtText.anchorMin = Vector2.zero;
        rtText.anchorMax = Vector2.one;
        rtText.sizeDelta = Vector2.zero;

        TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text = "Opsi Respon Dialog";
        tmp.fontSize = 18;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.raycastTarget = false;

        prefab = PrefabUtility.SaveAsPrefabAsset(btnGO, prefabPath);
        Object.DestroyImmediate(btnGO);
        AssetDatabase.SaveAssets();

        return prefab;
    }

    [MenuItem("Game Debug/Build Social Status Window UI")]
    public static void BuildSocialStatusWindowUIMenu()
    {
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("[SocialUI Setup] Canvas tidak ditemukan!");
            return;
        }

        HUDController hud = canvas.GetComponent<HUDController>();
        Transform topStats = canvas.transform.Find("Panel_TopStats");
        Button btnSocial = null;
        if (topStats != null)
        {
            Transform existingBtn = topStats.Find("Btn_OpenSocial");
            if (existingBtn != null) btnSocial = existingBtn.GetComponent<Button>();
        }

        BuildSocialStatusWindowUI(canvas.gameObject, hud, btnSocial);

        var scene = EditorSceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("<color=green>[SocialUI Setup]</color> Berhasil menyusun hierarki Social Status Window UI dan mengaitkannya ke scene!");
    }

    public static void BuildSocialStatusWindowUI(GameObject canvasGO, HUDController hud, Button btnOpenSocial = null)
    {
        // 1. Bersihkan elemen jendela lama jika ada
        Transform existingWindow = canvasGO.transform.Find("Panel_SocialStatusWindow");
        if (existingWindow != null) Object.DestroyImmediate(existingWindow.gameObject);

        // 2. Siapkan Prefab Kartu NPC
        GameObject cardPrefab = CreateOrLoadNPCRelationCardPrefab();

        // 3. Buat Panel_SocialStatusWindow (Set Inactive di awal)
        GameObject panelSocial = CreateUIObject("Panel_SocialStatusWindow", canvasGO.transform);
        RectTransform rtSocial = panelSocial.GetComponent<RectTransform>();
        rtSocial.anchorMin = Vector2.zero;
        rtSocial.anchorMax = Vector2.one;
        rtSocial.sizeDelta = Vector2.zero;

        // Background Dim
        GameObject bgDim = CreateUIObject("BackgroundDim", panelSocial.transform);
        RectTransform rtDim = bgDim.GetComponent<RectTransform>();
        rtDim.anchorMin = Vector2.zero;
        rtDim.anchorMax = Vector2.one;
        rtDim.sizeDelta = Vector2.zero;
        Image imgDim = bgDim.AddComponent<Image>();
        imgDim.color = new Color(0.03f, 0.04f, 0.07f, 0.88f);

        // Window Frame (Container Popup di tengah)
        GameObject windowFrame = CreateUIObject("WindowFrame", panelSocial.transform);
        RectTransform rtFrame = windowFrame.GetComponent<RectTransform>();
        rtFrame.anchorMin = new Vector2(0.5f, 0.5f);
        rtFrame.anchorMax = new Vector2(0.5f, 0.5f);
        rtFrame.pivot = new Vector2(0.5f, 0.5f);
        rtFrame.anchoredPosition = Vector2.zero;
        rtFrame.sizeDelta = new Vector2(1160f, 640f);

        Image imgFrame = windowFrame.AddComponent<Image>();
        imgFrame.color = new Color(0.07f, 0.08f, 0.14f, 0.98f);

        VerticalLayoutGroup vlgFrame = windowFrame.AddComponent<VerticalLayoutGroup>();
        vlgFrame.padding = new RectOffset(25, 25, 20, 20);
        vlgFrame.spacing = 10;
        vlgFrame.childAlignment = TextAnchor.UpperCenter;
        vlgFrame.childControlWidth = true;
        vlgFrame.childControlHeight = false;
        vlgFrame.childForceExpandWidth = true;
        vlgFrame.childForceExpandHeight = false;

        // Header Title
        TextMeshProUGUI txtTitle = CreateText("Txt_Title", windowFrame.transform, "🌸 STATUS HUBUNGAN SOSIAL & BAKUDAN RADAR", 24, new Color(1f, 0.45f, 0.75f), true);
        txtTitle.alignment = TextAlignmentOptions.Center;

        TextMeshProUGUI txtSubtitle = CreateText("Txt_Subtitle", windowFrame.transform, "Pantau Nilai Guanxi, Tingkat Afeksi Emosional, dan Risiko Ledakan Bom Rumor (Bakudan)", 13, new Color(0.75f, 0.8f, 0.9f));
        txtSubtitle.alignment = TextAlignmentOptions.Center;

        TextMeshProUGUI txtRumor = CreateText("Txt_RumorLevel", windowFrame.transform, "Level Rumor Kampus: Level 0 (Aman)", 14, new Color(1f, 0.85f, 0.2f), true);
        txtRumor.alignment = TextAlignmentOptions.Center;

        // Cards Container
        GameObject cardsContainer = CreateUIObject("CardsContainer", windowFrame.transform);
        RectTransform rtCards = cardsContainer.GetComponent<RectTransform>();
        rtCards.sizeDelta = new Vector2(1100f, 440f);
        LayoutElement leCards = cardsContainer.AddComponent<LayoutElement>();
        leCards.preferredHeight = 440f;

        HorizontalLayoutGroup hlgCards = cardsContainer.AddComponent<HorizontalLayoutGroup>();
        hlgCards.padding = new RectOffset(10, 10, 10, 10);
        hlgCards.spacing = 16;
        hlgCards.childAlignment = TextAnchor.MiddleCenter;
        hlgCards.childControlWidth = false;
        hlgCards.childControlHeight = false;
        hlgCards.childForceExpandWidth = false;
        hlgCards.childForceExpandHeight = false;

        // Tombol Tutup Jendela
        Button btnClose = CreateButton("Btn_CloseWindow", windowFrame.transform, "✕ Tutup Jendela", new Color(0.42f, 0.20f, 0.25f));
        RectTransform rtBtnClose = btnClose.GetComponent<RectTransform>();
        rtBtnClose.sizeDelta = new Vector2(220f, 46f);
        LayoutElement leClose = btnClose.gameObject.AddComponent<LayoutElement>();
        leClose.preferredWidth = 220f;
        leClose.preferredHeight = 46f;

        // Pasang Script SocialStatusWindowUI pada panel
        SocialStatusWindowUI socialUI = panelSocial.AddComponent<SocialStatusWindowUI>();
        socialUI.panelRoot = panelSocial;
        socialUI.cardsContainer = cardsContainer.transform;
        socialUI.npcCardPrefab = cardPrefab;
        socialUI.btnClose = btnClose;
        socialUI.txtRumorLevelHeader = txtRumor;

        UnityEditor.Events.UnityEventTools.AddPersistentListener(btnClose.onClick, socialUI.CloseWindow);

        // 4. Hubungkan tombol Btn_OpenSocial di Panel_TopStats
        if (btnOpenSocial == null)
        {
            Transform topStats = canvasGO.transform.Find("Panel_TopStats");
            if (topStats != null)
            {
                Transform foundBtn = topStats.Find("Btn_OpenSocial");
                if (foundBtn != null) btnOpenSocial = foundBtn.GetComponent<Button>();
            }
        }

        if (btnOpenSocial != null)
        {
            UnityEditor.Events.UnityEventTools.AddPersistentListener(btnOpenSocial.onClick, socialUI.OpenWindow);
            if (hud != null)
            {
                hud.btnOpenSocialWindow = btnOpenSocial;
                EditorUtility.SetDirty(hud);
            }
        }

        // Set Inactive di awal
        panelSocial.SetActive(false);

        EditorUtility.SetDirty(panelSocial);
        EditorUtility.SetDirty(socialUI);
    }

    public static GameObject CreateOrLoadNPCRelationCardPrefab()
    {
        string folder = "Assets/Prefabs";
        if (!AssetDatabase.IsValidFolder(folder))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }

        string prefabPath = $"{folder}/NPCRelationCard_Prefab.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab != null) return prefab;

        // Buat GameObject Card
        GameObject cardGO = new GameObject("NPCRelationCard_Prefab", typeof(RectTransform));
        RectTransform rtCard = cardGO.GetComponent<RectTransform>();
        rtCard.sizeDelta = new Vector2(250f, 430f);

        Image imgCardBg = cardGO.AddComponent<Image>();
        imgCardBg.color = new Color(0.10f, 0.12f, 0.19f, 0.95f);

        NPCRelationCardUI cardUI = cardGO.AddComponent<NPCRelationCardUI>();

        VerticalLayoutGroup vlgCard = cardGO.AddComponent<VerticalLayoutGroup>();
        vlgCard.padding = new RectOffset(14, 14, 14, 14);
        vlgCard.spacing = 8;
        vlgCard.childAlignment = TextAnchor.UpperCenter;
        vlgCard.childControlWidth = true;
        vlgCard.childControlHeight = false;
        vlgCard.childForceExpandWidth = true;
        vlgCard.childForceExpandHeight = false;

        // 1. Portrait Container (Portrait + Affection Heart)
        GameObject portCont = CreateUIObject("PortraitContainer", cardGO.transform);
        RectTransform rtPortCont = portCont.GetComponent<RectTransform>();
        rtPortCont.sizeDelta = new Vector2(220f, 95f);
        LayoutElement lePort = portCont.AddComponent<LayoutElement>();
        lePort.preferredHeight = 95f;

        GameObject portraitGO = CreateUIObject("Img_Portrait", portCont.transform);
        RectTransform rtPortrait = portraitGO.GetComponent<RectTransform>();
        rtPortrait.anchorMin = new Vector2(0.5f, 0.5f);
        rtPortrait.anchorMax = new Vector2(0.5f, 0.5f);
        rtPortrait.pivot = new Vector2(0.5f, 0.5f);
        rtPortrait.anchoredPosition = Vector2.zero;
        rtPortrait.sizeDelta = new Vector2(85f, 85f);
        Image imgPort = portraitGO.AddComponent<Image>();
        imgPort.color = new Color(0.25f, 0.35f, 0.55f);

        // Ikon Afeksi (Heart Icon di pojok atas portrait)
        GameObject affIconGO = CreateUIObject("Img_AffectionIcon", portCont.transform);
        RectTransform rtAff = affIconGO.GetComponent<RectTransform>();
        rtAff.anchorMin = new Vector2(1f, 1f);
        rtAff.anchorMax = new Vector2(1f, 1f);
        rtAff.pivot = new Vector2(1f, 1f);
        rtAff.anchoredPosition = new Vector2(-12f, -4f);
        rtAff.sizeDelta = new Vector2(32f, 32f);
        Image imgAff = affIconGO.AddComponent<Image>();
        imgAff.color = new Color(1f, 0.35f, 0.65f);

        TextMeshProUGUI txtHeart = CreateText("Txt_Heart", affIconGO.transform, "♥", 20, Color.white, true);
        txtHeart.alignment = TextAlignmentOptions.Center;
        RectTransform rtTxtHeart = txtHeart.GetComponent<RectTransform>();
        rtTxtHeart.anchorMin = Vector2.zero;
        rtTxtHeart.anchorMax = Vector2.one;
        rtTxtHeart.sizeDelta = Vector2.zero;

        // 2. Info Nama, Role & Label Afeksi
        TextMeshProUGUI txtName = CreateText("Txt_NpcName", cardGO.transform, "Nama Karakter", 18, Color.white, true);
        txtName.alignment = TextAlignmentOptions.Center;
        LayoutElement leName = txtName.gameObject.AddComponent<LayoutElement>();
        leName.preferredHeight = 24f;

        TextMeshProUGUI txtRole = CreateText("Txt_NpcRole", cardGO.transform, "Mahasiswa Lokal", 11, new Color(0.75f, 0.80f, 0.90f));
        txtRole.alignment = TextAlignmentOptions.Center;
        LayoutElement leRole = txtRole.gameObject.AddComponent<LayoutElement>();
        leRole.preferredHeight = 18f;

        TextMeshProUGUI txtAffLabel = CreateText("Txt_AffectionLabel", cardGO.transform, "Tokimeki (Inti)", 12, new Color(1f, 0.40f, 0.70f), true);
        txtAffLabel.alignment = TextAlignmentOptions.Center;
        LayoutElement leAff = txtAffLabel.gameObject.AddComponent<LayoutElement>();
        leAff.preferredHeight = 18f;

        // 3. Guanxi Slider Bar
        Slider sliderGuanxi = CreateSlider("Slider_Guanxi", cardGO.transform, new Color(0.25f, 0.75f, 1f));
        LayoutElement leSlider = sliderGuanxi.gameObject.AddComponent<LayoutElement>();
        leSlider.preferredHeight = 14f;

        TextMeshProUGUI txtGuanxiVal = CreateText("Txt_GuanxiValue", cardGO.transform, "100 / 100", 12, Color.white, true);
        txtGuanxiVal.alignment = TextAlignmentOptions.Center;
        LayoutElement leGval = txtGuanxiVal.gameObject.AddComponent<LayoutElement>();
        leGval.preferredHeight = 18f;

        // 4. Panel Bakudan (Emotional Bomb Warning)
        GameObject panelBakudan = CreateUIObject("Panel_BakudanWarning", cardGO.transform);
        RectTransform rtBakudan = panelBakudan.GetComponent<RectTransform>();
        rtBakudan.sizeDelta = new Vector2(220f, 44f);
        LayoutElement leBakudan = panelBakudan.AddComponent<LayoutElement>();
        leBakudan.preferredHeight = 44f;

        Image imgBakudanBg = panelBakudan.AddComponent<Image>();
        imgBakudanBg.color = new Color(0.28f, 0.08f, 0.10f, 0.92f);

        HorizontalLayoutGroup hlgBakudan = panelBakudan.AddComponent<HorizontalLayoutGroup>();
        hlgBakudan.padding = new RectOffset(10, 10, 4, 4);
        hlgBakudan.spacing = 8;
        hlgBakudan.childAlignment = TextAnchor.MiddleCenter;
        hlgBakudan.childControlWidth = false;
        hlgBakudan.childControlHeight = true;
        hlgBakudan.childForceExpandWidth = false;
        hlgBakudan.childForceExpandHeight = true;

        GameObject bakudanIconGO = CreateUIObject("Img_BakudanIcon", panelBakudan.transform);
        RectTransform rtBakIcon = bakudanIconGO.GetComponent<RectTransform>();
        rtBakIcon.sizeDelta = new Vector2(26f, 26f);
        Image imgBakIcon = bakudanIconGO.AddComponent<Image>();
        imgBakIcon.color = new Color(1f, 0.20f, 0.25f);

        TextMeshProUGUI txtBombSymbol = CreateText("Txt_BombSymbol", bakudanIconGO.transform, "💣", 16, Color.white);
        txtBombSymbol.alignment = TextAlignmentOptions.Center;
        RectTransform rtBombSym = txtBombSymbol.GetComponent<RectTransform>();
        rtBombSym.anchorMin = Vector2.zero;
        rtBombSym.anchorMax = Vector2.one;
        rtBombSym.sizeDelta = Vector2.zero;

        TextMeshProUGUI txtBakudanStatus = CreateText("Txt_BakudanStatus", panelBakudan.transform, "KRITIS! (85/100)", 11, new Color(1f, 0.35f, 0.35f), true);
        txtBakudanStatus.alignment = TextAlignmentOptions.MidlineLeft;
        RectTransform rtBakStat = txtBakudanStatus.GetComponent<RectTransform>();
        rtBakStat.sizeDelta = new Vector2(150f, 36f);

        // Sambungkan komponen ke NPCRelationCardUI
        cardUI.imgPortrait = imgPort;
        cardUI.txtNpcName = txtName;
        cardUI.txtNpcRole = txtRole;
        cardUI.sliderGuanxi = sliderGuanxi;
        cardUI.txtGuanxiValue = txtGuanxiVal;
        cardUI.imgAffectionIcon = imgAff;
        cardUI.txtAffectionLabel = txtAffLabel;
        cardUI.panelBakudanWarning = panelBakudan;
        cardUI.imgBakudanIcon = imgBakIcon;
        cardUI.txtBakudanStatus = txtBakudanStatus;

        prefab = PrefabUtility.SaveAsPrefabAsset(cardGO, prefabPath);
        Object.DestroyImmediate(cardGO);
        AssetDatabase.SaveAssets();

        return prefab;
    }

    private static Slider CreateSlider(string name, Transform parent, Color fillColor)
    {
        GameObject sliderGO = CreateUIObject(name, parent);
        RectTransform rtSlider = sliderGO.GetComponent<RectTransform>();
        rtSlider.sizeDelta = new Vector2(180f, 14f);

        Slider slider = sliderGO.AddComponent<Slider>();
        slider.interactable = false;
        slider.transition = Selectable.Transition.None;

        // Background
        GameObject bgGO = CreateUIObject("Background", sliderGO.transform);
        RectTransform rtBg = bgGO.GetComponent<RectTransform>();
        rtBg.anchorMin = Vector2.zero;
        rtBg.anchorMax = Vector2.one;
        rtBg.sizeDelta = Vector2.zero;
        Image imgBg = bgGO.AddComponent<Image>();
        imgBg.color = new Color(0.16f, 0.18f, 0.25f, 1f);

        // Fill Area
        GameObject fillArea = CreateUIObject("Fill Area", sliderGO.transform);
        RectTransform rtFillArea = fillArea.GetComponent<RectTransform>();
        rtFillArea.anchorMin = Vector2.zero;
        rtFillArea.anchorMax = Vector2.one;
        rtFillArea.sizeDelta = Vector2.zero;

        // Fill
        GameObject fillGO = CreateUIObject("Fill", fillArea.transform);
        RectTransform rtFill = fillGO.GetComponent<RectTransform>();
        rtFill.anchorMin = Vector2.zero;
        rtFill.anchorMax = Vector2.one;
        rtFill.sizeDelta = Vector2.zero;
        Image imgFill = fillGO.AddComponent<Image>();
        imgFill.color = fillColor;

        slider.fillRect = rtFill;
        slider.targetGraphic = imgFill;
        slider.direction = Slider.Direction.LeftToRight;
        slider.minValue = 0;
        slider.maxValue = 100;
        slider.value = 50;

        return slider;
    }
}
