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

        DialogueUIController dialogueUI = panelDialogue.GetComponent<DialogueUIController>();
        if (dialogueUI == null) dialogueUI = panelDialogue.AddComponent<DialogueUIController>();

        // Area klik transparan di seluruh kotak dialog untuk advance / finish typewriter
        GameObject goClickArea = CreateUIObject("Btn_DialogueBoxClick", panelDialogue.transform);
        RectTransform rtClick = goClickArea.GetComponent<RectTransform>();
        rtClick.anchorMin = Vector2.zero;
        rtClick.anchorMax = Vector2.one;
        rtClick.sizeDelta = Vector2.zero;
        Image imgClick = goClickArea.AddComponent<Image>();
        imgClick.color = Color.clear;
        Button btnBoxClick = goClickArea.AddComponent<Button>();
        btnBoxClick.transition = Selectable.Transition.None;
        dialogueUI.btnDialogueBoxClick = btnBoxClick;

        // Pembicara
        GameObject goSpeaker = CreateUIObject("Txt_SpeakerName", panelDialogue.transform);
        RectTransform rtSpeaker = goSpeaker.GetComponent<RectTransform>();
        rtSpeaker.anchorMin = new Vector2(0f, 1f);
        rtSpeaker.anchorMax = new Vector2(0.5f, 1f);
        rtSpeaker.pivot = new Vector2(0f, 1f);
        rtSpeaker.anchoredPosition = new Vector2(30f, -15f);
        rtSpeaker.sizeDelta = new Vector2(350f, 40f);
        TextMeshProUGUI txtSpeaker = goSpeaker.AddComponent<TextMeshProUGUI>();
        txtSpeaker.text = "Xiang Bai";
        txtSpeaker.fontSize = 24;
        txtSpeaker.fontStyle = FontStyles.Bold;
        txtSpeaker.color = new Color(0.3f, 0.85f, 1f);

        // Control Bar di pojok kanan atas kotak dialog
        GameObject controlBar = CreateUIObject("DialogueControlBar", panelDialogue.transform);
        RectTransform rtCB = controlBar.GetComponent<RectTransform>();
        rtCB.anchorMin = new Vector2(1f, 1f);
        rtCB.anchorMax = new Vector2(1f, 1f);
        rtCB.pivot = new Vector2(1f, 1f);
        rtCB.anchoredPosition = new Vector2(-25f, -14f);
        rtCB.sizeDelta = new Vector2(340f, 34f);

        HorizontalLayoutGroup hlgCB = controlBar.AddComponent<HorizontalLayoutGroup>();
        hlgCB.spacing = 6;
        hlgCB.childControlWidth = true;
        hlgCB.childControlHeight = true;
        hlgCB.childForceExpandWidth = true;
        hlgCB.childForceExpandHeight = true;

        Button btnLog = CreateButton("Btn_Backlog", controlBar.transform, "LOG", new Color(0.18f, 0.22f, 0.32f), 12f);
        Button btnAuto = CreateButton("Btn_Auto", controlBar.transform, "AUTO", new Color(0.18f, 0.22f, 0.32f), 12f);
        Button btnSkip = CreateButton("Btn_Skip", controlBar.transform, "SKIP", new Color(0.18f, 0.22f, 0.32f), 12f);
        Button btnConf = CreateButton("Btn_Config", controlBar.transform, "CONF", new Color(0.18f, 0.22f, 0.32f), 12f);

        dialogueUI.btnBacklog = btnLog;
        dialogueUI.btnAuto = btnAuto;
        dialogueUI.btnSkip = btnSkip;
        dialogueUI.btnConfig = btnConf;

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

        dialogueUI.dialoguePanel = panelDialogue;
        dialogueUI.txtSpeakerName = txtSpeaker;
        dialogueUI.txtDialogueContent = txtContent;
        dialogueUI.optionsContainer = goOptions.transform;
        dialogueUI.optionButtonPrefab = optionPrefab;

        // Set Inactive di awal sesuai spesifikasi
        panelDialogue.SetActive(false);

        // 8. Bangun DialogueBacklogModal (History Modal Overlay)
        BuildDialogueBacklogModal(canvasGO, dialogueUI);

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

        // Toast Notification Panel (Feedback F5/F6 Quick Save & Quick Load)
        GameObject panelToast = CreateUIObject("Panel_ToastNotification", canvasGO.transform);
        RectTransform rtToast = panelToast.GetComponent<RectTransform>();
        rtToast.anchorMin = new Vector2(0.5f, 0.88f);
        rtToast.anchorMax = new Vector2(0.5f, 0.88f);
        rtToast.pivot = new Vector2(0.5f, 0.5f);
        rtToast.sizeDelta = new Vector2(520f, 48f);
        Image imgToast = panelToast.AddComponent<Image>();
        imgToast.color = new Color(0.12f, 0.15f, 0.24f, 0.95f);

        TextMeshProUGUI txtToast = CreateText("Txt_Toast", panelToast.transform, "", 15, new Color(1f, 0.85f, 0.35f), true);
        txtToast.alignment = TextAlignmentOptions.Center;
        panelToast.SetActive(false);

        hud.panelToast = panelToast;
        hud.txtToastMessage = txtToast;

        // 9. Bangun Antarmuka Smartphone (PhoneUIController)
        BuildPhoneUI(canvasGO, hud);

        // 10. Bangun Jendela Status Hubungan & Sistem Bakudan (SocialStatusWindowUI)
        BuildSocialStatusWindowUI(canvasGO, hud, btnOpenSocial);

        // Pastikan PhoneOutingManager, TelemetryLogger, dan SaveManager terpasang di GameObject GAME_CORE
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

            SaveManager sm = gameCore.GetComponent<SaveManager>();
            if (sm == null)
            {
                sm = gameCore.AddComponent<SaveManager>();
                EditorUtility.SetDirty(gameCore);
            }

            DialogueBacklogManager backlogMgr = gameCore.GetComponent<DialogueBacklogManager>();
            if (backlogMgr == null)
            {
                backlogMgr = gameCore.AddComponent<DialogueBacklogManager>();
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

        Transform existingSmartphoneRoot = canvasGO.transform.Find("Panel_SmartphoneRoot");
        if (existingSmartphoneRoot != null) Object.DestroyImmediate(existingSmartphoneRoot.gameObject);

        // 2. Buat Btn_OpenPhone di pojok kanan bawah HUD
        Button btnOpenPhone = CreateButton("Btn_OpenPhone", canvasGO.transform, "📱 WeTalk / HP", new Color(0.08f, 0.48f, 0.38f));
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

        // 3. Buat Panel_SmartphoneRoot (Full screen backdrop container)
        GameObject panelRoot = CreateUIObject("Panel_SmartphoneRoot", canvasGO.transform);
        RectTransform rtRoot = panelRoot.GetComponent<RectTransform>();
        rtRoot.anchorMin = Vector2.zero;
        rtRoot.anchorMax = Vector2.one;
        rtRoot.sizeDelta = Vector2.zero;

        Image imgRootDim = panelRoot.AddComponent<Image>();
        imgRootDim.color = new Color(0.02f, 0.03f, 0.06f, 0.72f); // Semi-transparent dark overlay

        PhoneUIController phoneUI = panelRoot.AddComponent<PhoneUIController>();

        // 4. Buat Phone_Body_Frame (Bezel & Layar HP di tengah layar)
        GameObject phoneBody = CreateUIObject("Phone_Body_Frame", panelRoot.transform);
        RectTransform rtBody = phoneBody.GetComponent<RectTransform>();
        rtBody.anchorMin = new Vector2(0.5f, 0.5f);
        rtBody.anchorMax = new Vector2(0.5f, 0.5f);
        rtBody.pivot = new Vector2(0.5f, 0.5f);
        rtBody.anchoredPosition = Vector2.zero;
        rtBody.sizeDelta = new Vector2(430f, 780f);

        Image imgBody = phoneBody.AddComponent<Image>();
        imgBody.color = new Color(0.10f, 0.11f, 0.16f, 1f); // Metallic bezel frame

        // Speaker / Camera Notch atas
        GameObject notch = CreateUIObject("SpeakerNotch", phoneBody.transform);
        RectTransform rtNotch = notch.GetComponent<RectTransform>();
        rtNotch.anchorMin = new Vector2(0.5f, 1f);
        rtNotch.anchorMax = new Vector2(0.5f, 1f);
        rtNotch.pivot = new Vector2(0.5f, 1f);
        rtNotch.anchoredPosition = new Vector2(0f, -6f);
        rtNotch.sizeDelta = new Vector2(90f, 5f);
        Image imgNotch = notch.AddComponent<Image>();
        imgNotch.color = new Color(0.04f, 0.05f, 0.07f, 1f);

        // 5. TopStatusBar (Jam, Hari/Tanggal, Sinyal/Baterai)
        GameObject statusBar = CreateUIObject("TopStatusBar", phoneBody.transform);
        RectTransform rtStatus = statusBar.GetComponent<RectTransform>();
        rtStatus.anchorMin = new Vector2(0f, 1f);
        rtStatus.anchorMax = new Vector2(1f, 1f);
        rtStatus.pivot = new Vector2(0.5f, 1f);
        rtStatus.anchoredPosition = Vector2.zero;
        rtStatus.sizeDelta = new Vector2(0f, 36f);

        Image imgStatus = statusBar.AddComponent<Image>();
        imgStatus.color = new Color(0.06f, 0.07f, 0.11f, 0.95f);

        TextMeshProUGUI txtTime = CreateText("Txt_StatusBarTime", statusBar.transform, "08:30 (Pagi)", 12, Color.white, true);
        RectTransform rtTime = txtTime.GetComponent<RectTransform>();
        rtTime.anchorMin = new Vector2(0f, 0f);
        rtTime.anchorMax = new Vector2(0.35f, 1f);
        rtTime.offsetMin = new Vector2(14f, 0f);
        rtTime.offsetMax = Vector2.zero;
        txtTime.alignment = TextAlignmentOptions.MidlineLeft;

        TextMeshProUGUI txtDay = CreateText("Txt_StatusBarDay", statusBar.transform, "Hari 1 (Hari Kerja)", 12, new Color(0.35f, 0.85f, 1f), true);
        RectTransform rtDay = txtDay.GetComponent<RectTransform>();
        rtDay.anchorMin = new Vector2(0.35f, 0f);
        rtDay.anchorMax = new Vector2(0.72f, 1f);
        rtDay.offsetMin = Vector2.zero;
        rtDay.offsetMax = Vector2.zero;
        txtDay.alignment = TextAlignmentOptions.Center;

        TextMeshProUGUI txtBattery = CreateText("Txt_StatusBarBattery", statusBar.transform, "100% 5G", 11, new Color(0.75f, 0.95f, 0.75f));
        RectTransform rtBattery = txtBattery.GetComponent<RectTransform>();
        rtBattery.anchorMin = new Vector2(0.72f, 0f);
        rtBattery.anchorMax = new Vector2(1f, 1f);
        rtBattery.offsetMin = Vector2.zero;
        rtBattery.offsetMax = new Vector2(-14f, 0f);
        txtBattery.alignment = TextAlignmentOptions.MidlineRight;

        phoneUI.txtStatusBarTime = txtTime;
        phoneUI.txtStatusBarDay = txtDay;
        phoneUI.txtStatusBarBattery = txtBattery;

        // 6. BottomNavBar (Back, Home, Close)
        GameObject bottomNav = CreateUIObject("BottomNavBar", phoneBody.transform);
        RectTransform rtBottom = bottomNav.GetComponent<RectTransform>();
        rtBottom.anchorMin = new Vector2(0f, 0f);
        rtBottom.anchorMax = new Vector2(1f, 0f);
        rtBottom.pivot = new Vector2(0.5f, 0f);
        rtBottom.anchoredPosition = Vector2.zero;
        rtBottom.sizeDelta = new Vector2(0f, 48f);

        Image imgBottom = bottomNav.AddComponent<Image>();
        imgBottom.color = new Color(0.06f, 0.07f, 0.11f, 0.95f);

        HorizontalLayoutGroup hlgNav = bottomNav.AddComponent<HorizontalLayoutGroup>();
        hlgNav.padding = new RectOffset(16, 16, 6, 6);
        hlgNav.spacing = 10;
        hlgNav.childAlignment = TextAnchor.MiddleCenter;
        hlgNav.childControlWidth = true;
        hlgNav.childControlHeight = true;
        hlgNav.childForceExpandWidth = true;
        hlgNav.childForceExpandHeight = true;

        Button btnNavBack = CreateButton("Btn_NavBack", bottomNav.transform, "< Back", new Color(0.18f, 0.22f, 0.32f));
        Button btnNavHome = CreateButton("Btn_NavHome", bottomNav.transform, "Home", new Color(0.20f, 0.32f, 0.46f));
        Button btnNavClose = CreateButton("Btn_NavClose", bottomNav.transform, "Tutup HP", new Color(0.48f, 0.18f, 0.22f));

        UnityEditor.Events.UnityEventTools.AddPersistentListener(btnNavBack.onClick, phoneUI.OnClick_NavBack);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(btnNavHome.onClick, phoneUI.OnClick_NavHome);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(btnNavClose.onClick, phoneUI.OnClick_NavClose);

        phoneUI.btnNavBack = btnNavBack;
        phoneUI.btnNavHome = btnNavHome;
        phoneUI.btnNavClose = btnNavClose;

        // 7. ScreenContainer (Area tengah ponsel)
        GameObject screenContainer = CreateUIObject("ScreenContainer", phoneBody.transform);
        RectTransform rtScreen = screenContainer.GetComponent<RectTransform>();
        rtScreen.anchorMin = Vector2.zero;
        rtScreen.anchorMax = Vector2.one;
        rtScreen.offsetMin = new Vector2(0f, 48f); // Di atas bottom nav
        rtScreen.offsetMax = new Vector2(0f, -36f); // Di bawah status bar

        Image imgScreen = screenContainer.AddComponent<Image>();
        imgScreen.color = new Color(0.07f, 0.08f, 0.13f, 1f);

        // =========================================================
        // 8. SCREEN: HOME SCREEN (Menu Utama Smartphone)
        // =========================================================
        GameObject screenHome = CreateUIObject("Screen_HomeScreen", screenContainer.transform);
        RectTransform rtHome = screenHome.GetComponent<RectTransform>();
        rtHome.anchorMin = Vector2.zero;
        rtHome.anchorMax = Vector2.one;
        rtHome.sizeDelta = Vector2.zero;

        VerticalLayoutGroup vlgHome = screenHome.AddComponent<VerticalLayoutGroup>();
        vlgHome.padding = new RectOffset(20, 20, 24, 20);
        vlgHome.spacing = 16;
        vlgHome.childAlignment = TextAnchor.UpperCenter;
        vlgHome.childControlWidth = true;
        vlgHome.childControlHeight = false;
        vlgHome.childForceExpandWidth = true;
        vlgHome.childForceExpandHeight = false;

        // Widget Jam Besar
        GameObject widgetTime = CreateUIObject("Widget_TimeCard", screenHome.transform);
        RectTransform rtWidgetTime = widgetTime.GetComponent<RectTransform>();
        rtWidgetTime.sizeDelta = new Vector2(0f, 105f);
        Image imgWidgetTime = widgetTime.AddComponent<Image>();
        imgWidgetTime.color = new Color(0.11f, 0.14f, 0.22f, 0.9f);
        LayoutElement leWidgetTime = widgetTime.AddComponent<LayoutElement>();
        leWidgetTime.preferredHeight = 105f;

        VerticalLayoutGroup vlgW = widgetTime.AddComponent<VerticalLayoutGroup>();
        vlgW.padding = new RectOffset(15, 15, 12, 12);
        vlgW.spacing = 4;
        vlgW.childAlignment = TextAnchor.MiddleCenter;
        vlgW.childControlWidth = true;
        vlgW.childControlHeight = false;
        vlgW.childForceExpandWidth = true;

        TextMeshProUGUI txtBigClock = CreateText("Txt_BigClock", widgetTime.transform, "08:30", 30, Color.white, true);
        txtBigClock.alignment = TextAlignmentOptions.Center;
        TextMeshProUGUI txtWidgetSub = CreateText("Txt_WidgetSub", widgetTime.transform, "Devano Baskara Pratama - Shanghai Jiaotong", 11, new Color(0.6f, 0.8f, 1f));
        txtWidgetSub.alignment = TextAlignmentOptions.Center;

        // Label Section
        TextMeshProUGUI txtSection = CreateText("Txt_SectionTitle", screenHome.transform, "[APLIKASI KAMPUS]", 13, new Color(0.75f, 0.82f, 0.95f), true);
        txtSection.alignment = TextAlignmentOptions.Center;

        // AppBtn_WeTalk
        Button btnWeTalk = CreateButton("AppBtn_WeTalk", screenHome.transform, "WeTalk (Pesan & Intel Edelweiss)", new Color(0.06f, 0.50f, 0.35f));
        RectTransform rtWeTalk = btnWeTalk.GetComponent<RectTransform>();
        rtWeTalk.sizeDelta = new Vector2(0f, 62f);
        LayoutElement leWeTalk = btnWeTalk.gameObject.AddComponent<LayoutElement>();
        leWeTalk.preferredHeight = 62f;
        UnityEditor.Events.UnityEventTools.AddPersistentListener(btnWeTalk.onClick, phoneUI.OnClick_AppWeTalk);

        // AppBtn_Outing
        Button btnOuting = CreateButton("AppBtn_Outing", screenHome.transform, "Campus Outing (Weekend Hangout)", new Color(0.70f, 0.40f, 0.16f));
        RectTransform rtOuting = btnOuting.GetComponent<RectTransform>();
        rtOuting.sizeDelta = new Vector2(0f, 62f);
        LayoutElement leOuting = btnOuting.gameObject.AddComponent<LayoutElement>();
        leOuting.preferredHeight = 62f;
        UnityEditor.Events.UnityEventTools.AddPersistentListener(btnOuting.onClick, phoneUI.OnClick_AppOuting);

        // AppBtn_Save (Cloud Save / Simpan Cerita / 云存档)
        Button btnSaveApp = CreateButton("AppBtn_Save", screenHome.transform, "Cloud Save (Simpan Cerita / 云存档)", new Color(0.26f, 0.32f, 0.55f));
        RectTransform rtSaveApp = btnSaveApp.GetComponent<RectTransform>();
        rtSaveApp.sizeDelta = new Vector2(0f, 62f);
        LayoutElement leSaveApp = btnSaveApp.gameObject.AddComponent<LayoutElement>();
        leSaveApp.preferredHeight = 62f;
        UnityEditor.Events.UnityEventTools.AddPersistentListener(btnSaveApp.onClick, phoneUI.OnClick_AppSave);

        // Petunjuk Smartphone
        GameObject infoCard = CreateUIObject("Card_HomeHelp", screenHome.transform);
        RectTransform rtInfo = infoCard.GetComponent<RectTransform>();
        rtInfo.sizeDelta = new Vector2(0f, 155f);
        Image imgInfo = infoCard.AddComponent<Image>();
        imgInfo.color = new Color(0.09f, 0.11f, 0.17f, 0.85f);
        LayoutElement leInfo = infoCard.AddComponent<LayoutElement>();
        leInfo.preferredHeight = 155f;

        VerticalLayoutGroup vlgInfo = infoCard.AddComponent<VerticalLayoutGroup>();
        vlgInfo.padding = new RectOffset(16, 16, 14, 14);
        vlgInfo.childAlignment = TextAnchor.UpperLeft;
        vlgInfo.childControlWidth = true;
        vlgInfo.childControlHeight = true;

        TextMeshProUGUI txtHelp = CreateText("Txt_HelpBody", infoCard.transform,
            "<b>Panduan Smartphone Devano:</b>\n" +
            "- <b>WeTalk:</b> Chat dengan Edelweiss untuk pantau rumor dan preferensi teman.\n" +
            "- <b>Campus Outing:</b> Ajak teman jalan-jalan saat akhir pekan (Sabtu & Minggu).\n" +
            "- <b>Cloud Save:</b> Simpan/muat permainan (atau gunakan shortcut F5 Quick Save / F6 Quick Load).", 10.5f, new Color(0.78f, 0.82f, 0.9f));

        phoneUI.screenHome = screenHome;

        // =========================================================
        // 9. SCREEN: WETALK APP (Aplikasi Chat & Intel Edelweiss)
        // =========================================================
        GameObject screenWeTalk = CreateUIObject("Screen_WeTalkApp", screenContainer.transform);
        RectTransform rtWeTalkScreen = screenWeTalk.GetComponent<RectTransform>();
        rtWeTalkScreen.anchorMin = Vector2.zero;
        rtWeTalkScreen.anchorMax = Vector2.one;
        rtWeTalkScreen.sizeDelta = Vector2.zero;

        phoneUI.screenWeTalkApp = screenWeTalk;

        // Sub-Panel A: ChatListPanel
        GameObject chatListPanel = CreateUIObject("ChatListPanel", screenWeTalk.transform);
        RectTransform rtChatList = chatListPanel.GetComponent<RectTransform>();
        rtChatList.anchorMin = Vector2.zero;
        rtChatList.anchorMax = Vector2.one;
        rtChatList.sizeDelta = Vector2.zero;

        // Header WeTalk List
        GameObject headerList = CreateUIObject("HeaderBar_WeTalk", chatListPanel.transform);
        RectTransform rtHeaderList = headerList.GetComponent<RectTransform>();
        rtHeaderList.anchorMin = new Vector2(0f, 1f);
        rtHeaderList.anchorMax = new Vector2(1f, 1f);
        rtHeaderList.pivot = new Vector2(0.5f, 1f);
        rtHeaderList.anchoredPosition = Vector2.zero;
        rtHeaderList.sizeDelta = new Vector2(0f, 44f);
        Image imgHeaderList = headerList.AddComponent<Image>();
        imgHeaderList.color = new Color(0.05f, 0.38f, 0.28f, 1f); // WeChat Green
        TextMeshProUGUI txtHeaderList = CreateText("Txt_TitleWeTalk", headerList.transform, "WeTalk - Obrolan", 16, Color.white, true);
        txtHeaderList.alignment = TextAlignmentOptions.Center;

        // Daftar Kontak Chat
        GameObject contactsContainer = CreateUIObject("ContactsContainer", chatListPanel.transform);
        RectTransform rtContacts = contactsContainer.GetComponent<RectTransform>();
        rtContacts.anchorMin = Vector2.zero;
        rtContacts.anchorMax = Vector2.one;
        rtContacts.offsetMin = Vector2.zero;
        rtContacts.offsetMax = new Vector2(0f, -44f);

        VerticalLayoutGroup vlgContacts = contactsContainer.AddComponent<VerticalLayoutGroup>();
        vlgContacts.padding = new RectOffset(12, 12, 14, 12);
        vlgContacts.spacing = 8;
        vlgContacts.childAlignment = TextAnchor.UpperCenter;
        vlgContacts.childControlWidth = true;
        vlgContacts.childControlHeight = false;
        vlgContacts.childForceExpandWidth = true;
        vlgContacts.childForceExpandHeight = false;

        Button chatItemEdel = CreateButton("ChatItem_Edelweiss", contactsContainer.transform, "<b>Edelweiss Mayori</b> <color=#55FF88>[Online]</color>\n<size=10><color=#AABBDD>Info Broker: Tanya rumor dan preferensi teman...</color></size>", new Color(0.14f, 0.18f, 0.26f));
        Button chatItemHaoran = CreateButton("ChatItem_Haoran", contactsContainer.transform, "<b>Li Haoran</b> <color=#888888>[Offline]</color>\n<size=10><color=#8899AA>Bro, tugas praktikum kemarin sudah selesai?</color></size>", new Color(0.11f, 0.14f, 0.20f));
        Button chatItemYangMei = CreateButton("ChatItem_YangMei", contactsContainer.transform, "<b>Yang Mei</b> <color=#888888>[Offline]</color>\n<size=10><color=#8899AA>Terima kasih atas bantuan kaligrafi tadi...</color></size>", new Color(0.11f, 0.14f, 0.20f));
        Button chatItemXiangBai = CreateButton("ChatItem_XiangBai", contactsContainer.transform, "<b>Dosen Xiang Bai</b> <color=#888888>[Offline]</color>\n<size=10><color=#8899AA>Jadwal asistensi praktikum tetap hari Kamis.</color></size>", new Color(0.11f, 0.14f, 0.20f));

        chatItemEdel.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 62f);
        chatItemHaoran.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 62f);
        chatItemYangMei.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 62f);
        chatItemXiangBai.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 62f);

        UnityEditor.Events.UnityEventTools.AddPersistentListener(chatItemEdel.onClick, phoneUI.OnClick_OpenEdelweissChat);

        phoneUI.panelChatList = chatListPanel;

        // Sub-Panel B: ChatRoomPanel (Room Edelweiss)
        GameObject chatRoomPanel = CreateUIObject("ChatRoomPanel", screenWeTalk.transform);
        RectTransform rtChatRoom = chatRoomPanel.GetComponent<RectTransform>();
        rtChatRoom.anchorMin = Vector2.zero;
        rtChatRoom.anchorMax = Vector2.one;
        rtChatRoom.sizeDelta = Vector2.zero;

        // Header Chat Room
        GameObject headerRoom = CreateUIObject("HeaderChatRoom", chatRoomPanel.transform);
        RectTransform rtHeaderRoom = headerRoom.GetComponent<RectTransform>();
        rtHeaderRoom.anchorMin = new Vector2(0f, 1f);
        rtHeaderRoom.anchorMax = new Vector2(1f, 1f);
        rtHeaderRoom.pivot = new Vector2(0.5f, 1f);
        rtHeaderRoom.anchoredPosition = Vector2.zero;
        rtHeaderRoom.sizeDelta = new Vector2(0f, 48f);
        Image imgHeaderRoom = headerRoom.AddComponent<Image>();
        imgHeaderRoom.color = new Color(0.08f, 0.11f, 0.18f, 1f);

        VerticalLayoutGroup vlgHR = headerRoom.AddComponent<VerticalLayoutGroup>();
        vlgHR.padding = new RectOffset(14, 14, 6, 6);
        vlgHR.spacing = 1;
        vlgHR.childAlignment = TextAnchor.MiddleCenter;
        vlgHR.childControlWidth = true;
        vlgHR.childControlHeight = false;
        vlgHR.childForceExpandWidth = true;

        TextMeshProUGUI txtContactName = CreateText("Txt_ChatRoomContactName", headerRoom.transform, "Edelweiss Mayori", 15, new Color(1f, 0.6f, 0.85f), true);
        txtContactName.alignment = TextAlignmentOptions.Center;
        TextMeshProUGUI txtContactStatus = CreateText("Txt_ChatRoomStatus", headerRoom.transform, "Online - Info Broker", 10, new Color(0.45f, 0.95f, 0.65f));
        txtContactStatus.alignment = TextAlignmentOptions.Center;

        phoneUI.txtChatRoomContactName = txtContactName;
        phoneUI.txtChatRoomStatus = txtContactStatus;

        // Chat Input Area (Pertanyaan Cepat) di bagian bawah chat room
        GameObject inputArea = CreateUIObject("ChatInputArea", chatRoomPanel.transform);
        RectTransform rtInput = inputArea.GetComponent<RectTransform>();
        rtInput.anchorMin = new Vector2(0f, 0f);
        rtInput.anchorMax = new Vector2(1f, 0f);
        rtInput.pivot = new Vector2(0.5f, 0f);
        rtInput.anchoredPosition = Vector2.zero;
        rtInput.sizeDelta = new Vector2(0f, 114f);
        Image imgInput = inputArea.AddComponent<Image>();
        imgInput.color = new Color(0.07f, 0.09f, 0.14f, 0.98f);

        VerticalLayoutGroup vlgInput = inputArea.AddComponent<VerticalLayoutGroup>();
        vlgInput.padding = new RectOffset(10, 10, 8, 8);
        vlgInput.spacing = 6;
        vlgInput.childAlignment = TextAnchor.MiddleCenter;
        vlgInput.childControlWidth = true;
        vlgInput.childControlHeight = true;
        vlgInput.childForceExpandWidth = true;
        vlgInput.childForceExpandHeight = true;

        // Row 1 Pertanyaan
        GameObject rowInput1 = CreateUIObject("Row1", inputArea.transform);
        HorizontalLayoutGroup hlgRow1 = rowInput1.AddComponent<HorizontalLayoutGroup>();
        hlgRow1.spacing = 6;
        hlgRow1.childControlWidth = true;
        hlgRow1.childControlHeight = true;
        hlgRow1.childForceExpandWidth = true;
        hlgRow1.childForceExpandHeight = true;

        Button btnAskHaoran = CreateButton("Btn_AskHaoran", rowInput1.transform, "Tanya: Li Haoran", new Color(0.14f, 0.32f, 0.44f));
        Button btnAskXiangBai = CreateButton("Btn_AskXiangBai", rowInput1.transform, "Tanya: Dosen Xiang Bai", new Color(0.20f, 0.28f, 0.46f));

        // Row 2 Pertanyaan
        GameObject rowInput2 = CreateUIObject("Row2", inputArea.transform);
        HorizontalLayoutGroup hlgRow2 = rowInput2.AddComponent<HorizontalLayoutGroup>();
        hlgRow2.spacing = 6;
        hlgRow2.childControlWidth = true;
        hlgRow2.childControlHeight = true;
        hlgRow2.childForceExpandWidth = true;
        hlgRow2.childForceExpandHeight = true;

        Button btnAskYangMei = CreateButton("Btn_AskYangMei", rowInput2.transform, "Tanya: Yang Mei", new Color(0.38f, 0.20f, 0.36f));
        Button btnAskRumor = CreateButton("Btn_AskRumor", rowInput2.transform, "Cek Rumor Kampus", new Color(0.55f, 0.38f, 0.14f));

        UnityEditor.Events.UnityEventTools.AddIntPersistentListener(btnAskHaoran.onClick, phoneUI.SendEdelweissInquiry, 102);
        UnityEditor.Events.UnityEventTools.AddIntPersistentListener(btnAskXiangBai.onClick, phoneUI.SendEdelweissInquiry, 101);
        UnityEditor.Events.UnityEventTools.AddIntPersistentListener(btnAskYangMei.onClick, phoneUI.SendEdelweissInquiry, 103);
        UnityEditor.Events.UnityEventTools.AddIntPersistentListener(btnAskRumor.onClick, phoneUI.SendEdelweissInquiry, 0);

        phoneUI.chatInputArea = inputArea;

        // MessageScrollView (Di antara Header dan Input Area)
        GameObject scrollGO = CreateUIObject("MessageScrollView", chatRoomPanel.transform);
        RectTransform rtScroll = scrollGO.GetComponent<RectTransform>();
        rtScroll.anchorMin = Vector2.zero;
        rtScroll.anchorMax = Vector2.one;
        rtScroll.offsetMin = new Vector2(0f, 114f);
        rtScroll.offsetMax = new Vector2(0f, -48f);

        ScrollRect scrollRect = scrollGO.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;

        // Viewport
        GameObject viewportGO = CreateUIObject("Viewport", scrollGO.transform);
        RectTransform rtViewport = viewportGO.GetComponent<RectTransform>();
        rtViewport.anchorMin = Vector2.zero;
        rtViewport.anchorMax = Vector2.one;
        rtViewport.sizeDelta = Vector2.zero;
        Image imgViewport = viewportGO.AddComponent<Image>();
        imgViewport.color = new Color(0f, 0f, 0f, 0.01f);
        Mask mask = viewportGO.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        // Content
        GameObject contentGO = CreateUIObject("Content", viewportGO.transform);
        RectTransform rtContent = contentGO.GetComponent<RectTransform>();
        rtContent.anchorMin = new Vector2(0f, 1f);
        rtContent.anchorMax = new Vector2(1f, 1f);
        rtContent.pivot = new Vector2(0.5f, 1f);
        rtContent.anchoredPosition = Vector2.zero;
        rtContent.sizeDelta = new Vector2(0f, 0f);

        VerticalLayoutGroup vlgMsg = contentGO.AddComponent<VerticalLayoutGroup>();
        vlgMsg.padding = new RectOffset(8, 8, 8, 8);
        vlgMsg.spacing = 8;
        vlgMsg.childAlignment = TextAnchor.UpperCenter;
        vlgMsg.childControlWidth = true;
        vlgMsg.childControlHeight = false;
        vlgMsg.childForceExpandWidth = true;
        vlgMsg.childForceExpandHeight = false;

        ContentSizeFitter csf = contentGO.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollRect.viewport = rtViewport;
        scrollRect.content = rtContent;

        phoneUI.messageScrollRect = scrollRect;
        phoneUI.messageContainer = contentGO.transform;
        phoneUI.panelChatRoom = chatRoomPanel;

        // =========================================================
        // 10. SCREEN: CAMPUS OUTING APP (Weekend Outing Planner)
        // =========================================================
        GameObject screenOuting = CreateUIObject("Screen_OutingApp", screenContainer.transform);
        RectTransform rtOutingScreen = screenOuting.GetComponent<RectTransform>();
        rtOutingScreen.anchorMin = Vector2.zero;
        rtOutingScreen.anchorMax = Vector2.one;
        rtOutingScreen.sizeDelta = Vector2.zero;

        // Header Outing
        GameObject headerOuting = CreateUIObject("HeaderOuting", screenOuting.transform);
        RectTransform rtHeaderOuting = headerOuting.GetComponent<RectTransform>();
        rtHeaderOuting.anchorMin = new Vector2(0f, 1f);
        rtHeaderOuting.anchorMax = new Vector2(1f, 1f);
        rtHeaderOuting.pivot = new Vector2(0.5f, 1f);
        rtHeaderOuting.anchoredPosition = Vector2.zero;
        rtHeaderOuting.sizeDelta = new Vector2(0f, 44f);
        Image imgHeaderOuting = headerOuting.AddComponent<Image>();
        imgHeaderOuting.color = new Color(0.65f, 0.35f, 0.14f, 1f);
        TextMeshProUGUI txtHeaderOuting = CreateText("Txt_TitleOuting", headerOuting.transform, "Weekend Campus Outing", 15, Color.white, true);
        txtHeaderOuting.alignment = TextAlignmentOptions.Center;

        // Kontainer Konten Outing
        GameObject outingBody = CreateUIObject("OutingBodyContainer", screenOuting.transform);
        RectTransform rtOutingBody = outingBody.GetComponent<RectTransform>();
        rtOutingBody.anchorMin = Vector2.zero;
        rtOutingBody.anchorMax = Vector2.one;
        rtOutingBody.offsetMin = Vector2.zero;
        rtOutingBody.offsetMax = new Vector2(0f, -44f);

        // Warning Text jika hari kerja
        GameObject warningGO = CreateUIObject("Txt_WeekendWarning", outingBody.transform);
        RectTransform rtWarn = warningGO.GetComponent<RectTransform>();
        rtWarn.anchorMin = new Vector2(0.05f, 0.4f);
        rtWarn.anchorMax = new Vector2(0.95f, 0.65f);
        rtWarn.sizeDelta = Vector2.zero;
        Image imgWarn = warningGO.AddComponent<Image>();
        imgWarn.color = new Color(0.40f, 0.12f, 0.14f, 0.95f);
        TextMeshProUGUI txtWarn = CreateText("Txt_WarnContent", warningGO.transform, "[Terkunci] <b>Aplikasi Weekend Outing</b>\nJanjian hangout hanya dapat dilakukan di akhir pekan (Sabtu & Minggu).", 13, new Color(1f, 0.85f, 0.85f), true);
        txtWarn.alignment = TextAlignmentOptions.Center;
        warningGO.SetActive(false);
        phoneUI.txtWeekendWarning = txtWarn;

        // Sub-Panel A: Target Selection Section (Pilih Teman)
        GameObject panelSelectContact = CreateUIObject("TargetSelectionSection", outingBody.transform);
        RectTransform rtSC = panelSelectContact.GetComponent<RectTransform>();
        rtSC.anchorMin = Vector2.zero;
        rtSC.anchorMax = Vector2.one;
        rtSC.sizeDelta = Vector2.zero;

        VerticalLayoutGroup vlgSC = panelSelectContact.AddComponent<VerticalLayoutGroup>();
        vlgSC.padding = new RectOffset(16, 16, 16, 16);
        vlgSC.spacing = 10;
        vlgSC.childAlignment = TextAnchor.UpperCenter;
        vlgSC.childControlWidth = true;
        vlgSC.childControlHeight = false;
        vlgSC.childForceExpandWidth = true;
        vlgSC.childForceExpandHeight = false;

        TextMeshProUGUI txtSCTitle = CreateText("Txt_SelectContactTitle", panelSelectContact.transform, "[PILIH TEMAN HANGOUT]", 16, new Color(1f, 0.85f, 0.3f), true);
        txtSCTitle.alignment = TextAlignmentOptions.Center;
        TextMeshProUGUI txtSCSub = CreateText("Txt_SelectContactSub", panelSelectContact.transform, "Pilih karakter yang ingin diajak keluar:", 12, new Color(0.75f, 0.8f, 0.9f));
        txtSCSub.alignment = TextAlignmentOptions.Center;

        Button btnContactHaoran = CreateButton("Btn_Contact_Haoran", panelSelectContact.transform, "Li Haoran", new Color(0.18f, 0.42f, 0.55f));
        Button btnContactXiangBai = CreateButton("Btn_Contact_XiangBai", panelSelectContact.transform, "Dosen Xiang Bai", new Color(0.24f, 0.35f, 0.58f));
        Button btnContactYangMei = CreateButton("Btn_Contact_YangMei", panelSelectContact.transform, "Yang Mei", new Color(0.50f, 0.25f, 0.42f));

        btnContactHaoran.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 55f);
        btnContactXiangBai.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 55f);
        btnContactYangMei.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 55f);

        UnityEditor.Events.UnityEventTools.AddIntPersistentListener(btnContactHaoran.onClick, phoneUI.OnSelectNpcForHangout, 102);
        UnityEditor.Events.UnityEventTools.AddIntPersistentListener(btnContactXiangBai.onClick, phoneUI.OnSelectNpcForHangout, 101);
        UnityEditor.Events.UnityEventTools.AddIntPersistentListener(btnContactYangMei.onClick, phoneUI.OnSelectNpcForHangout, 103);

        phoneUI.panelOutingSelectContact = panelSelectContact;

        // Sub-Panel B: Venue Selection Section (Daftar VenueCardPrefab)
        GameObject panelSelectVenue = CreateUIObject("VenueScrollView", outingBody.transform);
        RectTransform rtSV = panelSelectVenue.GetComponent<RectTransform>();
        rtSV.anchorMin = Vector2.zero;
        rtSV.anchorMax = Vector2.one;
        rtSV.sizeDelta = Vector2.zero;

        VerticalLayoutGroup vlgSV = panelSelectVenue.AddComponent<VerticalLayoutGroup>();
        vlgSV.padding = new RectOffset(14, 14, 14, 14);
        vlgSV.spacing = 8;
        vlgSV.childAlignment = TextAnchor.UpperCenter;
        vlgSV.childControlWidth = true;
        vlgSV.childControlHeight = false;
        vlgSV.childForceExpandWidth = true;
        vlgSV.childForceExpandHeight = false;

        TextMeshProUGUI txtSVTitle = CreateText("Txt_SelectVenueTitle", panelSelectVenue.transform, "[PILIH DESTINASI VENUE]", 16, new Color(0.4f, 0.95f, 0.6f), true);
        txtSVTitle.alignment = TextAlignmentOptions.Center;

        Button btnVenue1 = CreateButton("Btn_Venue1", panelSelectVenue.transform, "Kantin Muslim / Halal Street\n<size=10>Biaya: PH -10, MH +20 | Syarat: Etika >= 20</size>", new Color(0.20f, 0.50f, 0.35f));
        Button btnVenue2 = CreateButton("Btn_Venue2", panelSelectVenue.transform, "Distrik Elektronik\n<size=10>Biaya: PH -15, MH +15 | Syarat: Bahasa >= 30</size>", new Color(0.20f, 0.42f, 0.62f));
        Button btnVenue3 = CreateButton("Btn_Venue3", panelSelectVenue.transform, "Kedai Teh Tradisional\n<size=10>Biaya: PH -10, MH +25 | Syarat: Etika >= 40</size>", new Color(0.55f, 0.38f, 0.20f));
        Button btnVenue4 = CreateButton("Btn_Venue4", panelSelectVenue.transform, "Perpustakaan Kota\n<size=10>Biaya: PH -10, MH +10 | Syarat: Bahasa >= 25</size>", new Color(0.35f, 0.30f, 0.55f));
        Button btnBackVenue = CreateButton("Btn_BackVenue", panelSelectVenue.transform, "< Ganti Pilihan Teman", new Color(0.30f, 0.32f, 0.40f));

        btnVenue1.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 52f);
        btnVenue2.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 52f);
        btnVenue3.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 52f);
        btnVenue4.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 52f);
        btnBackVenue.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 44f);

        UnityEditor.Events.UnityEventTools.AddIntPersistentListener(btnVenue1.onClick, phoneUI.OnSelectVenueForHangout, 1);
        UnityEditor.Events.UnityEventTools.AddIntPersistentListener(btnVenue2.onClick, phoneUI.OnSelectVenueForHangout, 2);
        UnityEditor.Events.UnityEventTools.AddIntPersistentListener(btnVenue3.onClick, phoneUI.OnSelectVenueForHangout, 3);
        UnityEditor.Events.UnityEventTools.AddIntPersistentListener(btnVenue4.onClick, phoneUI.OnSelectVenueForHangout, 4);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(btnBackVenue.onClick, phoneUI.OnClick_KembaliKePilihKontak);

        phoneUI.panelOutingSelectVenue = panelSelectVenue;
        phoneUI.screenOutingApp = screenOuting;

        // =========================================================
        // 11. SCREEN: CLOUD SAVE APP (Simpan & Muat Permainan)
        // =========================================================
        GameObject screenSave = CreateUIObject("Screen_SaveApp", screenContainer.transform);
        RectTransform rtSaveScreen = screenSave.GetComponent<RectTransform>();
        rtSaveScreen.anchorMin = Vector2.zero;
        rtSaveScreen.anchorMax = Vector2.one;
        rtSaveScreen.sizeDelta = Vector2.zero;

        // Top App Header
        GameObject saveHeader = CreateUIObject("SaveAppHeader", screenSave.transform);
        RectTransform rtSH = saveHeader.GetComponent<RectTransform>();
        rtSH.anchorMin = new Vector2(0f, 1f);
        rtSH.anchorMax = new Vector2(1f, 1f);
        rtSH.pivot = new Vector2(0.5f, 1f);
        rtSH.sizeDelta = new Vector2(0f, 48f);
        Image imgSH = saveHeader.AddComponent<Image>();
        imgSH.color = new Color(0.18f, 0.22f, 0.38f, 1f);

        TextMeshProUGUI txtSaveHeaderTitle = CreateText("Txt_Title", saveHeader.transform, "Cloud Save & Load (云存档)", 15, Color.white, true);
        txtSaveHeaderTitle.alignment = TextAlignmentOptions.Center;

        // Scroll Container / Body
        GameObject saveScrollObj = CreateUIObject("SaveAppScrollView", screenSave.transform);
        RectTransform rtSS = saveScrollObj.GetComponent<RectTransform>();
        rtSS.anchorMin = Vector2.zero;
        rtSS.anchorMax = Vector2.one;
        rtSS.offsetMax = new Vector2(0f, -48f);
        rtSS.offsetMin = Vector2.zero;

        ScrollRect srSave = saveScrollObj.AddComponent<ScrollRect>();
        srSave.horizontal = false;
        srSave.vertical = true;
        srSave.movementType = ScrollRect.MovementType.Clamped;

        GameObject saveViewport = CreateUIObject("Viewport", saveScrollObj.transform);
        RectTransform rtSVp = saveViewport.GetComponent<RectTransform>();
        rtSVp.anchorMin = Vector2.zero;
        rtSVp.anchorMax = Vector2.one;
        rtSVp.sizeDelta = Vector2.zero;
        saveViewport.AddComponent<RectMask2D>();
        srSave.viewport = rtSVp;

        GameObject saveContent = CreateUIObject("Content", saveViewport.transform);
        RectTransform rtSContent = saveContent.GetComponent<RectTransform>();
        rtSContent.anchorMin = new Vector2(0f, 1f);
        rtSContent.anchorMax = new Vector2(1f, 1f);
        rtSContent.pivot = new Vector2(0.5f, 1f);
        rtSContent.sizeDelta = new Vector2(0f, 580f);
        srSave.content = rtSContent;

        VerticalLayoutGroup vlgSave = saveContent.AddComponent<VerticalLayoutGroup>();
        vlgSave.padding = new RectOffset(10, 10, 8, 12);
        vlgSave.spacing = 8;
        vlgSave.childAlignment = TextAnchor.UpperCenter;
        vlgSave.childControlWidth = true;
        vlgSave.childControlHeight = false;
        vlgSave.childForceExpandWidth = true;
        vlgSave.childForceExpandHeight = false;
        ContentSizeFitter csfSave = saveContent.AddComponent<ContentSizeFitter>();
        csfSave.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // A. Card Quick Save (Slot 0)
        GameObject cardQuick = CreateUIObject("Card_QuickSave", saveContent.transform);
        RectTransform rtCQ = cardQuick.GetComponent<RectTransform>();
        rtCQ.sizeDelta = new Vector2(0f, 92f);
        Image imgCQ = cardQuick.AddComponent<Image>();
        imgCQ.color = new Color(0.12f, 0.16f, 0.28f, 0.95f);
        LayoutElement leCQ = cardQuick.AddComponent<LayoutElement>();
        leCQ.preferredHeight = 92f;

        VerticalLayoutGroup vlgCQ = cardQuick.AddComponent<VerticalLayoutGroup>();
        vlgCQ.padding = new RectOffset(10, 10, 8, 8);
        vlgCQ.spacing = 6;
        vlgCQ.childControlWidth = true;
        vlgCQ.childControlHeight = false;
        vlgCQ.childForceExpandWidth = true;

        TextMeshProUGUI txtQuickInfo = CreateText("Txt_QuickInfo", cardQuick.transform, "<b>[F5/F6] Quick Save:</b> <i>Belum ada data tersimpan.</i>", 11, new Color(0.85f, 0.9f, 1f));
        phoneUI.txtQuickSaveInfo = txtQuickInfo;

        GameObject quickBtnRow = CreateUIObject("QuickBtnRow", cardQuick.transform);
        HorizontalLayoutGroup hlgQ = quickBtnRow.AddComponent<HorizontalLayoutGroup>();
        hlgQ.spacing = 8;
        hlgQ.childControlWidth = true;
        hlgQ.childControlHeight = false;
        hlgQ.childForceExpandWidth = true;

        Button btnQuickSave = CreateButton("Btn_QuickSavePhone", quickBtnRow.transform, "Quick Save (F5)", new Color(0.15f, 0.50f, 0.35f));
        btnQuickSave.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 34f);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(btnQuickSave.onClick, phoneUI.OnClick_PhoneQuickSave);
        phoneUI.btnQuickSaveApp = btnQuickSave;

        Button btnQuickLoad = CreateButton("Btn_QuickLoadPhone", quickBtnRow.transform, "Quick Load (F6)", new Color(0.55f, 0.35f, 0.20f));
        btnQuickLoad.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 34f);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(btnQuickLoad.onClick, phoneUI.OnClick_PhoneQuickLoad);
        phoneUI.btnQuickLoadApp = btnQuickLoad;

        // B. Section Title Manual Slots
        TextMeshProUGUI txtManualTitle = CreateText("Txt_ManualSlotsTitle", saveContent.transform, "[MANUAL SAVE SLOTS (1 - 5)]", 12, new Color(0.7f, 0.8f, 0.95f), true);
        txtManualTitle.alignment = TextAlignmentOptions.Center;

        // C. Slots 1 to 5
        PhoneSaveSlotItemUI[] slotItems = new PhoneSaveSlotItemUI[5];
        for (int i = 0; i < 5; i++)
        {
            int sId = i + 1;
            GameObject slotCard = CreateUIObject($"Card_Slot_{sId}", saveContent.transform);
            RectTransform rtSCard = slotCard.GetComponent<RectTransform>();
            rtSCard.sizeDelta = new Vector2(0f, 74f);
            Image imgSlotCard = slotCard.AddComponent<Image>();
            imgSlotCard.color = new Color(0.10f, 0.12f, 0.20f, 0.95f);
            LayoutElement leSC = slotCard.AddComponent<LayoutElement>();
            leSC.preferredHeight = 74f;

            HorizontalLayoutGroup hlgSlot = slotCard.AddComponent<HorizontalLayoutGroup>();
            hlgSlot.padding = new RectOffset(10, 10, 6, 6);
            hlgSlot.spacing = 8;
            hlgSlot.childControlWidth = false;
            hlgSlot.childControlHeight = true;
            hlgSlot.childForceExpandWidth = false;
            hlgSlot.childForceExpandHeight = true;

            // Info Column
            GameObject infoCol = CreateUIObject("InfoCol", slotCard.transform);
            RectTransform rtInfoCol = infoCol.GetComponent<RectTransform>();
            rtInfoCol.sizeDelta = new Vector2(210f, 0f);
            VerticalLayoutGroup vlgInfoCol = infoCol.AddComponent<VerticalLayoutGroup>();
            vlgInfoCol.spacing = 2;
            vlgInfoCol.childControlWidth = true;
            vlgInfoCol.childControlHeight = false;
            vlgInfoCol.childForceExpandWidth = true;

            TextMeshProUGUI txtTitle = CreateText("Txt_Title", infoCol.transform, $"SLOT {sId}", 11, Color.white, true);
            TextMeshProUGUI txtDetails = CreateText("Txt_Details", infoCol.transform, "Slot Kosong", 9.5f, new Color(0.7f, 0.8f, 0.9f));
            TextMeshProUGUI txtDate = CreateText("Txt_Date", infoCol.transform, "--/--/----", 9, new Color(0.55f, 0.65f, 0.75f));

            // Buttons Column
            GameObject btnCol = CreateUIObject("BtnCol", slotCard.transform);
            RectTransform rtBtnCol = btnCol.GetComponent<RectTransform>();
            rtBtnCol.sizeDelta = new Vector2(95f, 0f);
            VerticalLayoutGroup vlgBtnCol = btnCol.AddComponent<VerticalLayoutGroup>();
            vlgBtnCol.spacing = 4;
            vlgBtnCol.childControlWidth = true;
            vlgBtnCol.childControlHeight = false;
            vlgBtnCol.childForceExpandWidth = true;

            Button btnSave = CreateButton("Btn_Save", btnCol.transform, "Simpan / 保存", new Color(0.16f, 0.45f, 0.35f));
            btnSave.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 28f);
            Button btnLoad = CreateButton("Btn_Load", btnCol.transform, "Muat / 读取", new Color(0.35f, 0.32f, 0.55f));
            btnLoad.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 28f);

            PhoneSaveSlotItemUI item = slotCard.AddComponent<PhoneSaveSlotItemUI>();
            item.slotId = sId;
            item.txtSlotTitle = txtTitle;
            item.txtSlotDetails = txtDetails;
            item.txtSlotDate = txtDate;
            item.btnSave = btnSave;
            item.btnLoad = btnLoad;

            slotItems[i] = item;
        }
        phoneUI.manualSlotItems = slotItems;

        // D. Status feedback text
        TextMeshProUGUI txtStatus = CreateText("Txt_SaveAppStatus", saveContent.transform, "Pilih slot untuk menyimpan progres ceritamu.", 11, new Color(0.75f, 0.85f, 0.75f));
        txtStatus.alignment = TextAlignmentOptions.Center;
        phoneUI.txtSaveAppStatus = txtStatus;

        phoneUI.screenSaveApp = screenSave;

        // 12. Hubungkan tombol Open Phone ke PhoneUIController
        UnityEditor.Events.UnityEventTools.AddPersistentListener(btnOpenPhone.onClick, phoneUI.BukaPhone);

        // 13. Konfigurasi State Awal: Matikan panel ponsel
        panelSelectVenue.SetActive(false);
        screenWeTalk.SetActive(false);
        screenOuting.SetActive(false);
        screenSave.SetActive(false);
        screenHome.SetActive(true);
        panelRoot.SetActive(false);

        EditorUtility.SetDirty(panelRoot);
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

    private static Button CreateButton(string name, Transform parent, string label, Color bgColor, float fontSize = 18f)
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
        tmp.fontSize = fontSize;
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

    public static GameObject CreateOrLoadBacklogItemPrefab()
    {
        string folder = "Assets/Prefabs";
        if (!AssetDatabase.IsValidFolder(folder))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }

        string prefabPath = $"{folder}/BacklogItemPrefab.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab != null) return prefab;

        GameObject itemGO = new GameObject("BacklogItemPrefab", typeof(RectTransform));
        RectTransform rt = itemGO.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(1040f, 65f);

        Image img = itemGO.AddComponent<Image>();
        img.color = new Color(0.10f, 0.13f, 0.20f, 0.85f);

        VerticalLayoutGroup vlg = itemGO.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(16, 16, 8, 10);
        vlg.spacing = 4;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        ContentSizeFitter csf = itemGO.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Speaker Name
        GameObject spkGO = new GameObject("Txt_Speaker", typeof(RectTransform));
        spkGO.transform.SetParent(itemGO.transform, false);
        TextMeshProUGUI tmpSpk = spkGO.AddComponent<TextMeshProUGUI>();
        tmpSpk.text = "Speaker";
        tmpSpk.fontSize = 16;
        tmpSpk.fontStyle = FontStyles.Bold;
        tmpSpk.color = new Color(0.35f, 0.85f, 1f);

        // Dialogue Text
        GameObject diaGO = new GameObject("Txt_Dialogue", typeof(RectTransform));
        diaGO.transform.SetParent(itemGO.transform, false);
        TextMeshProUGUI tmpDia = diaGO.AddComponent<TextMeshProUGUI>();
        tmpDia.text = "Teks percakapan dialog yang pernah diucapkan...";
        tmpDia.fontSize = 15;
        tmpDia.color = new Color(0.92f, 0.94f, 0.98f);
        tmpDia.textWrappingMode = TextWrappingModes.Normal;

        BacklogItemUI itemUI = itemGO.AddComponent<BacklogItemUI>();
        itemUI.txtSpeaker = tmpSpk;
        itemUI.txtDialogue = tmpDia;

        prefab = PrefabUtility.SaveAsPrefabAsset(itemGO, prefabPath);
        Object.DestroyImmediate(itemGO);
        AssetDatabase.SaveAssets();

        return prefab;
    }

    public static void BuildDialogueBacklogModal(GameObject canvasGO, DialogueUIController dialogueUI)
    {
        Transform existing = canvasGO.transform.Find("Panel_BacklogModal");
        if (existing != null)
        {
            Object.DestroyImmediate(existing.gameObject);
        }

        GameObject modal = CreateUIObject("Panel_BacklogModal", canvasGO.transform);
        RectTransform rtModal = modal.GetComponent<RectTransform>();
        rtModal.anchorMin = Vector2.zero;
        rtModal.anchorMax = Vector2.one;
        rtModal.sizeDelta = Vector2.zero;

        Image imgDim = modal.AddComponent<Image>();
        imgDim.color = new Color(0.04f, 0.05f, 0.08f, 0.88f);

        // Window Frame di tengah layar
        GameObject frame = CreateUIObject("WindowFrame", modal.transform);
        RectTransform rtFrame = frame.GetComponent<RectTransform>();
        rtFrame.anchorMin = new Vector2(0.5f, 0.5f);
        rtFrame.anchorMax = new Vector2(0.5f, 0.5f);
        rtFrame.pivot = new Vector2(0.5f, 0.5f);
        rtFrame.anchoredPosition = Vector2.zero;
        rtFrame.sizeDelta = new Vector2(1120f, 680f);

        Image imgFrame = frame.AddComponent<Image>();
        imgFrame.color = new Color(0.08f, 0.10f, 0.16f, 0.98f);

        VerticalLayoutGroup vlgFrame = frame.AddComponent<VerticalLayoutGroup>();
        vlgFrame.padding = new RectOffset(20, 20, 16, 16);
        vlgFrame.spacing = 10;
        vlgFrame.childAlignment = TextAnchor.UpperCenter;
        vlgFrame.childControlWidth = true;
        vlgFrame.childControlHeight = false;
        vlgFrame.childForceExpandWidth = true;

        // Header Title
        TextMeshProUGUI txtTitle = CreateText("Txt_Title", frame.transform, "DIALOGUE HISTORY (对话回放 / 历史记录)", 22, new Color(0.4f, 0.85f, 1f), true);
        txtTitle.alignment = TextAlignmentOptions.Center;

        TextMeshProUGUI txtSub = CreateText("Txt_Subtitle", frame.transform, "Gunakan scroll mouse atau tombol di bawah untuk kembali ke percakapan aktif.", 12, new Color(0.7f, 0.78f, 0.88f));
        txtSub.alignment = TextAlignmentOptions.Center;

        // Scroll View Container
        GameObject scrollObj = CreateUIObject("ScrollView_Backlog", frame.transform);
        RectTransform rtScroll = scrollObj.GetComponent<RectTransform>();
        rtScroll.sizeDelta = new Vector2(1080f, 520f);
        LayoutElement leScroll = scrollObj.AddComponent<LayoutElement>();
        leScroll.preferredHeight = 520f;

        ScrollRect sr = scrollObj.AddComponent<ScrollRect>();
        sr.horizontal = false;
        sr.vertical = true;
        sr.movementType = ScrollRect.MovementType.Clamped;

        GameObject viewport = CreateUIObject("Viewport", scrollObj.transform);
        RectTransform rtVp = viewport.GetComponent<RectTransform>();
        rtVp.anchorMin = Vector2.zero;
        rtVp.anchorMax = Vector2.one;
        rtVp.sizeDelta = Vector2.zero;
        viewport.AddComponent<RectMask2D>();
        sr.viewport = rtVp;

        GameObject content = CreateUIObject("Content", viewport.transform);
        RectTransform rtContent = content.GetComponent<RectTransform>();
        rtContent.anchorMin = new Vector2(0f, 1f);
        rtContent.anchorMax = new Vector2(1f, 1f);
        rtContent.pivot = new Vector2(0.5f, 1f);
        rtContent.sizeDelta = new Vector2(0f, 500f);
        sr.content = rtContent;

        VerticalLayoutGroup vlgContent = content.AddComponent<VerticalLayoutGroup>();
        vlgContent.padding = new RectOffset(10, 10, 8, 8);
        vlgContent.spacing = 8;
        vlgContent.childAlignment = TextAnchor.UpperCenter;
        vlgContent.childControlWidth = true;
        vlgContent.childControlHeight = false;
        vlgContent.childForceExpandWidth = true;

        ContentSizeFitter csfContent = content.AddComponent<ContentSizeFitter>();
        csfContent.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        TextMeshProUGUI txtEmpty = CreateText("Txt_EmptyBacklog", content.transform, "Belum ada riwayat dialog tersimpan.", 15, new Color(0.6f, 0.7f, 0.8f));
        txtEmpty.alignment = TextAlignmentOptions.Center;
        txtEmpty.gameObject.SetActive(false);

        // Close Button di bagian bawah
        Button btnClose = CreateButton("Btn_CloseBacklog", frame.transform, "✕ KEMBALI KE DIALOG (ESC / 返回)", new Color(0.42f, 0.20f, 0.25f), 15f);
        btnClose.GetComponent<RectTransform>().sizeDelta = new Vector2(340f, 44f);
        LayoutElement leClose = btnClose.gameObject.AddComponent<LayoutElement>();
        leClose.preferredHeight = 44f;

        // DialogueBacklogUI component
        DialogueBacklogUI backlogUI = modal.AddComponent<DialogueBacklogUI>();
        backlogUI.panelBacklogRoot = modal;
        backlogUI.scrollRect = sr;
        backlogUI.backlogContentContainer = content.transform;
        backlogUI.backlogItemPrefab = CreateOrLoadBacklogItemPrefab();
        backlogUI.btnClose = btnClose;
        backlogUI.txtEmptyBacklog = txtEmpty;

        modal.SetActive(false);
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
