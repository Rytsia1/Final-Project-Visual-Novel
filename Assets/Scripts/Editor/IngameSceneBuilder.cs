#if UNITY_EDITOR
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

public static class IngameSceneBuilder
{
    private const string SCENE_PATH = "Assets/Scenes/SampleScene.unity";
    private const string PREFAB_OPTION_PATH = "Assets/Prefabs/OptionButtonPrefab.prefab";
    private const string PREFAB_OPTION_ALT_PATH = "Assets/Prefabs/Btn_DialogueOption_Prefab.prefab";

    private const string BG_BEDROOM_PATH = "Assets/Textures/ingame_bedroom_bg.jpg";
    private const string BG_HALLWAY_PATH = "Assets/Textures/dialogue_hallway_sunset_bg.jpg";
    private const string CHARA_GIRL_PATH = "Assets/Textures/chara_girl_sprite.png";

    private const string SPRITE_HOME_FRONT = "Assets/Textures/UI_Ingame/btn_home_front.png";
    private const string SPRITE_HOME_SHADOW = "Assets/Textures/UI_Ingame/btn_home_shadow.png";
    private const string SPRITE_BACK_FRONT = "Assets/Textures/UI_Ingame/btn_back_front.png";
    private const string SPRITE_BACK_SHADOW = "Assets/Textures/UI_Ingame/btn_back_shadow.png";

    private const string SPRITE_HUD_FRAME = "Assets/Textures/UI_Ingame/hud_stats_frame.png";
    private const string SPRITE_HUD_CELL = "Assets/Textures/UI_Ingame/hud_cell_bg.png";
    private const string SPRITE_CAL_BG = "Assets/Textures/UI_Ingame/calendar_badge_bg.png";
    private const string SPRITE_CAL_TAB = "Assets/Textures/UI_Ingame/calendar_header_tab.png";
    private const string SPRITE_ACT_BG = "Assets/Textures/UI_Ingame/activity_grid_bg.png";
    private const string SPRITE_ACT_TAB = "Assets/Textures/UI_Ingame/activity_accent_tab.png";
    private const string SPRITE_ACT_SLOT = "Assets/Textures/UI_Ingame/activity_slot_btn.png";
    private const string SPRITE_DIA_FRAME = "Assets/Textures/UI_Ingame/dialogue_frame_bg.png";
    private const string SPRITE_DIA_DARK = "Assets/Textures/UI_Ingame/dialogue_inner_dark.png";
    private const string SPRITE_NAME_TAG = "Assets/Textures/UI_Ingame/name_tag_bg.png";
    private const string SPRITE_CTRL_FRONT = "Assets/Textures/UI_Ingame/btn_control_front.png";
    private const string SPRITE_CTRL_SHADOW = "Assets/Textures/UI_Ingame/btn_control_shadow.png";
    private const string SPRITE_PHONE = "Assets/Textures/UI_Ingame/phone_widget.png";
    private const string SPRITE_VIG_TOP = "Assets/Textures/UI_Ingame/vignette_ingame_top.png";
    private const string SPRITE_VIG_BOT = "Assets/Textures/UI_Ingame/vignette_ingame_bot.png";
    private const string SPRITE_STAT_DIVIDER = "Assets/Textures/UI_Ingame/stat_divider_meter.png";

    private const string SPRITE_CHOICE_NORMAL = "Assets/Textures/UI_Ingame/btn_choice_normal.png";
    private const string SPRITE_CHOICE_SELECTED = "Assets/Textures/UI_Ingame/btn_choice_selected.png";
    private const string SPRITE_POINTER_HAND = "Assets/Textures/UI_Ingame/pointer_hand_left.png";

    private const string FONT_PATRICK_SDF = "Assets/Fonts/PatrickHand-Regular SDF.asset";
    private const string FONT_CAVEAT_SDF = "Assets/Fonts/CaveatBrush-Regular SDF.asset";
    private const string FONT_POPPINS_SDF = "Assets/Fonts/Poppins-Bold SDF.asset";

    [MenuItem("Game Debug/Build Ingame Home Scene (Ver 1)")]
    public static void BuildHomeMenu()
    {
        BuildIngameScene();
    }

    [MenuItem("Game Debug/Build Dialog Screen (Ver 2)")]
    public static void BuildDialogMenu()
    {
        BuildDialogueScreen(false);
    }

    [MenuItem("Game Debug/Build Branching Choices Screen (Ver 2)")]
    public static void BuildChoicesMenu()
    {
        BuildDialogueScreen(true);
    }

    // =========================================================================
    // 1. DIALOGUE / BRANCHING CHOICES SCREEN BUILDER (VER 2)
    // =========================================================================
    public static void BuildDialogueScreen(bool showBranchingChoices)
    {
        string modeTitle = showBranchingChoices ? "BRANCHING CHOICES SCREEN (VER 2)" : "DIALOG SCREEN (VER 2)";
        Debug.Log($"<color=cyan>=== MEMULAI PEMBANGUNAN {modeTitle} ===</color>");

        // 1. Konfigurasi semua sprite UI
        ConfigureAllTextures();

        // 2. Load Fonts & Sprites
        TMP_FontAsset fontPatrick = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FONT_PATRICK_SDF);
        TMP_FontAsset fontCaveat = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FONT_CAVEAT_SDF);
        TMP_FontAsset fontPoppins = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FONT_POPPINS_SDF);

        Sprite spBgHallway = AssetDatabase.LoadAssetAtPath<Sprite>(BG_HALLWAY_PATH);
        Sprite spChara = AssetDatabase.LoadAssetAtPath<Sprite>(CHARA_GIRL_PATH);
        Sprite spBackFront = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITE_BACK_FRONT);
        Sprite spBackShadow = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITE_BACK_SHADOW);
        Sprite spCalBg = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITE_CAL_BG);
        Sprite spCalTab = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITE_CAL_TAB);
        Sprite spDiaFrame = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITE_DIA_FRAME);
        Sprite spDiaDark = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITE_DIA_DARK);
        Sprite spNameTag = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITE_NAME_TAG);
        Sprite spCtrlFront = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITE_CTRL_FRONT);
        Sprite spCtrlShadow = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITE_CTRL_SHADOW);
        Sprite spVigBot = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITE_VIG_BOT);

        Sprite spChoiceNormal = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITE_CHOICE_NORMAL);
        Sprite spChoiceSelected = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITE_CHOICE_SELECTED);
        Sprite spPointerHand = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITE_POINTER_HAND);

        // 3. Buat / Perbarui Prefab Option Button
        GameObject optPrefab = CreateOrUpdateOptionPrefab(spChoiceNormal, spChoiceSelected, spPointerHand, fontPatrick);

        // 4. Buka Scene
        var scene = EditorSceneManager.OpenScene(SCENE_PATH, OpenSceneMode.Single);

        // 5. Pastikan Kamera Tersedia
        Camera cam = EnsureCamera();

        // 6. Temukan atau Buat Canvas
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        GameObject canvasGO = canvas != null ? canvas.gameObject : new GameObject("Canvas");
        if (canvas == null)
        {
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<GraphicRaycaster>();
        }

        CanvasScaler scaler = canvasGO.GetComponent<CanvasScaler>();
        if (scaler == null) scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        // 7. Bersihkan Objek UI Lama
        string[] oldPanels = { "Img_Background", "Img_TopGradient", "Img_BottomGradient", "Btn_Home", "Btn_Back",
                               "Img_Character", "Panel_TopStats", "Panel_Calendar", "Panel_ActivityGrid", "Btn_OpenPhone",
                               "Panel_DialogueBox", "Panel_PredictiveTooltip" };
        foreach (var name in oldPanels)
        {
            Transform t = canvasGO.transform.Find(name);
            if (t != null) Object.DestroyImmediate(t.gameObject);
        }

        // 8. Background Hallway Sunset Fullscreen
        GameObject bgGO = CreateUIObject("Img_Background", canvasGO.transform);
        bgGO.transform.SetAsFirstSibling();
        StretchFull(bgGO.GetComponent<RectTransform>());
        Image imgBg = bgGO.AddComponent<Image>();
        imgBg.sprite = spBgHallway;
        imgBg.color = Color.white;
        imgBg.raycastTarget = false;

        // 9. Character Sprite (Centered, 750 x 1040, grounded, chest and peace sign above dialogue box)
        GameObject charaGO = CreateUIObject("Img_Character", canvasGO.transform);
        charaGO.transform.SetSiblingIndex(1);
        RectTransform rtChara = charaGO.GetComponent<RectTransform>();
        rtChara.anchorMin = new Vector2(0.5f, 0f);
        rtChara.anchorMax = new Vector2(0.5f, 0f);
        rtChara.pivot = new Vector2(0.5f, 0f);
        rtChara.anchoredPosition = new Vector2(0f, 50f);
        rtChara.sizeDelta = new Vector2(750f, 1040f);
        Image imgChara = charaGO.AddComponent<Image>();
        imgChara.sprite = spChara;
        imgChara.preserveAspect = true;
        imgChara.raycastTarget = false;

        // 10. Bottom Vignette (Rectangle 66: height 360)
        GameObject botVigGO = CreateUIObject("Img_BottomGradient", canvasGO.transform);
        botVigGO.transform.SetSiblingIndex(2);
        RectTransform rtBotVig = botVigGO.GetComponent<RectTransform>();
        rtBotVig.anchorMin = new Vector2(0f, 0f);
        rtBotVig.anchorMax = new Vector2(1f, 0f);
        rtBotVig.pivot = new Vector2(0.5f, 0f);
        rtBotVig.anchoredPosition = Vector2.zero;
        rtBotVig.sizeDelta = new Vector2(0f, 360f);
        Image imgBotVig = botVigGO.AddComponent<Image>();
        imgBotVig.sprite = spVigBot;
        imgBotVig.raycastTarget = false;

        // 11. Back Button (Top Left: x=61, y=-40, w=184, h=62)
        Button btnBack = BuildBackButton(canvasGO.transform, spBackFront, spBackShadow, fontPatrick);

        // 12. Calendar Badge (Top Right: x=-60, y=-31, w=291, h=157, "[ Rabu ]", "16 / 09 / 2026")
        TextMeshProUGUI txtDayBadge, txtDateBadge;
        GameObject panelCalendar = BuildCalendarBadge(canvasGO.transform, spCalBg, spCalTab, fontPatrick, out txtDayBadge, out txtDateBadge);
        txtDayBadge.text = "[ Rabu ]";
        txtDateBadge.text = "16 / 09 / 2026";

        // 13. Dialogue Box (Bottom: w=1685, h=320, x=0, y=65)
        Button btnDialogueClick, btnAuto, btnSkip, btnConfig, btnBacklog;
        TextMeshProUGUI txtSpeaker, txtContent;
        GameObject nextIndicator;
        Transform optionsContainer;
        GameObject panelDialogue = BuildDialogueBox(canvasGO.transform, spDiaFrame, spDiaDark, spNameTag, spCtrlFront, spCtrlShadow, fontPatrick,
            out btnDialogueClick, out txtSpeaker, out txtContent, out nextIndicator, out optionsContainer,
            out btnAuto, out btnSkip, out btnConfig, out btnBacklog);

        panelDialogue.SetActive(true);

        // 14. Konfigurasi Mode Teks vs Mode Pilihan Percabangan (Branching Choices)
        if (showBranchingChoices)
        {
            txtContent.gameObject.SetActive(false);
            nextIndicator.SetActive(false);

            // Bersihkan opsi lama jika ada
            for (int i = optionsContainer.childCount - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(optionsContainer.GetChild(i).gameObject);
            }

            // Bangun 3 Opsi Respon (Option 1, Option 2, Option 3)
            GameObject opt1 = Object.Instantiate(optPrefab, optionsContainer);
            opt1.name = "Option_1";
            var ui1 = opt1.GetComponent<DialogueOptionButtonUI>();
            if (ui1 != null) { ui1.txtOption.text = "Option 1"; ui1.SetSelected(false); }

            GameObject opt2 = Object.Instantiate(optPrefab, optionsContainer);
            opt2.name = "Option_2";
            var ui2 = opt2.GetComponent<DialogueOptionButtonUI>();
            if (ui2 != null) { ui2.txtOption.text = "Option 2"; ui2.SetSelected(false); }

            GameObject opt3 = Object.Instantiate(optPrefab, optionsContainer);
            opt3.name = "Option_3";
            var ui3 = opt3.GetComponent<DialogueOptionButtonUI>();
            if (ui3 != null) { ui3.txtOption.text = "Option 3"; ui3.SetSelected(true); } // Selected state dengan pointer hand
        }
        else
        {
            txtContent.gameObject.SetActive(true);
            txtContent.text = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Etiam aliquam justo at eros sollicitudin, at consectetur odio tempus. Vivamus blandit pretium leo ac tristique.";
            nextIndicator.SetActive(true);

            // Bersihkan opsi container
            for (int i = optionsContainer.childCount - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(optionsContainer.GetChild(i).gameObject);
            }
        }

        // 15. Hubungkan DialogueUIController
        DialogueUIController dlg = panelDialogue.GetComponent<DialogueUIController>();
        if (dlg == null) dlg = panelDialogue.AddComponent<DialogueUIController>();

        dlg.dialoguePanel = panelDialogue;
        dlg.btnDialogueBoxClick = btnDialogueClick;
        dlg.btnBack = btnBack;
        dlg.txtSpeakerName = txtSpeaker;
        dlg.txtDialogueContent = txtContent;
        dlg.nextIndicator = nextIndicator;
        dlg.optionsContainer = optionsContainer;
        dlg.optionButtonPrefab = optPrefab;
        dlg.btnAuto = btnAuto;
        dlg.btnSkip = btnSkip;
        dlg.btnConfig = btnConfig;
        dlg.btnBacklog = btnBacklog;

        // 16. Hubungkan HUDController jika ada
        HUDController hud = canvasGO.GetComponent<HUDController>();
        if (hud != null)
        {
            hud.txtDayBadge = txtDayBadge;
            hud.txtDateBadge = txtDateBadge;
            EditorUtility.SetDirty(hud);
        }

        EditorUtility.SetDirty(dlg);
        EditorUtility.SetDirty(canvasGO);

        // 17. Simpan Scene
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, SCENE_PATH);
        AssetDatabase.SaveAssets();

        // 18. Tangkap Snapshot Render 1080p
        string renderFilename = showBranchingChoices ? "test_render_choices_1080p.png" : "test_render_dialog_1080p.png";
        CaptureScene1080p(canvas, cam, renderFilename);

        Debug.Log($"<color=green>=== PEMBANGUNAN {modeTitle} SELESAI ===</color>");
    }

    // =========================================================================
    // 2. INGAME HOME SCENE BUILDER (VER 1)
    // =========================================================================
    public static void BuildIngameScene()
    {
        Debug.Log("<color=cyan>=== MEMULAI PEMBANGUNAN INGAME HOME SCENE (VER 1) ===</color>");

        ConfigureAllTextures();

        TMP_FontAsset fontPatrick = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FONT_PATRICK_SDF);
        TMP_FontAsset fontCaveat = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FONT_CAVEAT_SDF);
        TMP_FontAsset fontPoppins = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FONT_POPPINS_SDF);

        Sprite spBg = AssetDatabase.LoadAssetAtPath<Sprite>(BG_BEDROOM_PATH);
        Sprite spHomeFront = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITE_HOME_FRONT);
        Sprite spHomeShadow = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITE_HOME_SHADOW);
        Sprite spHudFrame = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITE_HUD_FRAME);
        Sprite spHudCell = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITE_HUD_CELL);
        Sprite spCalBg = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITE_CAL_BG);
        Sprite spCalTab = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITE_CAL_TAB);
        Sprite spActBg = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITE_ACT_BG);
        Sprite spActTab = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITE_ACT_TAB);
        Sprite spActSlot = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITE_ACT_SLOT);
        Sprite spDiaFrame = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITE_DIA_FRAME);
        Sprite spDiaDark = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITE_DIA_DARK);
        Sprite spNameTag = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITE_NAME_TAG);
        Sprite spCtrlFront = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITE_CTRL_FRONT);
        Sprite spCtrlShadow = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITE_CTRL_SHADOW);
        Sprite spPhone = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITE_PHONE);
        Sprite spVigTop = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITE_VIG_TOP);
        Sprite spVigBot = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITE_VIG_BOT);
        Sprite spStatDiv = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITE_STAT_DIVIDER);

        Sprite spChoiceNormal = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITE_CHOICE_NORMAL);
        Sprite spChoiceSelected = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITE_CHOICE_SELECTED);
        Sprite spPointerHand = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITE_POINTER_HAND);

        GameObject optPrefab = CreateOrUpdateOptionPrefab(spChoiceNormal, spChoiceSelected, spPointerHand, fontPatrick);

        var scene = EditorSceneManager.OpenScene(SCENE_PATH, OpenSceneMode.Single);
        Camera cam = EnsureCamera();

        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        GameObject canvasGO = canvas != null ? canvas.gameObject : new GameObject("Canvas");
        if (canvas == null)
        {
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<GraphicRaycaster>();
        }

        CanvasScaler scaler = canvasGO.GetComponent<CanvasScaler>();
        if (scaler == null) scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        string[] oldPanels = { "Img_Background", "Img_TopGradient", "Img_BottomGradient", "Btn_Home", "Btn_Back",
                               "Img_Character", "Panel_TopStats", "Panel_Calendar", "Panel_ActivityGrid", "Btn_OpenPhone",
                               "Panel_DialogueBox", "Panel_PredictiveTooltip" };
        foreach (var name in oldPanels)
        {
            Transform t = canvasGO.transform.Find(name);
            if (t != null) Object.DestroyImmediate(t.gameObject);
        }

        // Background Bedroom Fullscreen
        GameObject bgGO = CreateUIObject("Img_Background", canvasGO.transform);
        bgGO.transform.SetAsFirstSibling();
        StretchFull(bgGO.GetComponent<RectTransform>());
        Image imgBg = bgGO.AddComponent<Image>();
        imgBg.sprite = spBg;
        imgBg.color = Color.white;
        imgBg.raycastTarget = false;

        // Top Vignette
        GameObject topVigGO = CreateUIObject("Img_TopGradient", canvasGO.transform);
        topVigGO.transform.SetSiblingIndex(1);
        RectTransform rtTopVig = topVigGO.GetComponent<RectTransform>();
        rtTopVig.anchorMin = new Vector2(0f, 1f);
        rtTopVig.anchorMax = new Vector2(1f, 1f);
        rtTopVig.pivot = new Vector2(0.5f, 1f);
        rtTopVig.anchoredPosition = Vector2.zero;
        rtTopVig.sizeDelta = new Vector2(0f, 180f);
        Image imgTopVig = topVigGO.AddComponent<Image>();
        imgTopVig.sprite = spVigTop;
        imgTopVig.raycastTarget = false;

        // Bottom Vignette
        GameObject botVigGO = CreateUIObject("Img_BottomGradient", canvasGO.transform);
        botVigGO.transform.SetSiblingIndex(2);
        RectTransform rtBotVig = botVigGO.GetComponent<RectTransform>();
        rtBotVig.anchorMin = new Vector2(0f, 0f);
        rtBotVig.anchorMax = new Vector2(1f, 0f);
        rtBotVig.pivot = new Vector2(0.5f, 0f);
        rtBotVig.anchoredPosition = Vector2.zero;
        rtBotVig.sizeDelta = new Vector2(0f, 360f);
        Image imgBotVig = botVigGO.AddComponent<Image>();
        imgBotVig.sprite = spVigBot;
        imgBotVig.raycastTarget = false;

        // Home Button
        Button btnHome = BuildHomeButton(canvasGO.transform, spHomeFront, spHomeShadow, fontCaveat);

        // Stats Header HUD Bar
        TextMeshProUGUI txtLang, txtEtiq, txtMH, txtPH, txtTheo, txtPrac;
        GameObject panelTopStats = BuildStatsHUD(canvasGO.transform, spHudFrame, spHudCell, spStatDiv, fontPatrick,
            out txtLang, out txtEtiq, out txtMH, out txtPH, out txtTheo, out txtPrac);

        // Calendar Badge
        TextMeshProUGUI txtDayBadge, txtDateBadge;
        GameObject panelCalendar = BuildCalendarBadge(canvasGO.transform, spCalBg, spCalTab, fontPatrick, out txtDayBadge, out txtDateBadge);

        // Smartphone Widget
        TextMeshProUGUI txtClock, txtPhoneDate;
        Button btnOpenPhone = BuildPhoneWidget(canvasGO.transform, spPhone, fontPoppins, out txtClock, out txtPhoneDate);

        // Activity Grid Panel
        Button btnStudy, btnLunch, btnLecturer, btnSleep, btnSocial;
        GameObject panelActivity = BuildActivityGrid(canvasGO.transform, spActBg, spActTab, spActSlot, fontPatrick,
            out btnStudy, out btnLunch, out btnLecturer, out btnSleep, out btnSocial);

        // Predictive Tooltip
        TextMeshProUGUI txtTooltipDesc, txtTooltipCost;
        GameObject panelTooltip = BuildPredictiveTooltip(canvasGO.transform, fontPatrick, out txtTooltipDesc, out txtTooltipCost);
        panelTooltip.SetActive(false);

        // Dialogue Box
        Button btnDialogueClick, btnAuto, btnSkip, btnConfig, btnBacklog;
        TextMeshProUGUI txtSpeaker, txtContent;
        GameObject nextIndicator;
        Transform optionsContainer;
        GameObject panelDialogue = BuildDialogueBox(canvasGO.transform, spDiaFrame, spDiaDark, spNameTag, spCtrlFront, spCtrlShadow, fontPatrick,
            out btnDialogueClick, out txtSpeaker, out txtContent, out nextIndicator, out optionsContainer,
            out btnAuto, out btnSkip, out btnConfig, out btnBacklog);

        panelDialogue.SetActive(true);

        // Hubungkan HUDController
        HUDController hud = canvasGO.GetComponent<HUDController>();
        if (hud == null) hud = canvasGO.AddComponent<HUDController>();

        hud.btnHome = btnHome;
        hud.txtPhysicalHealth = txtPH;
        hud.txtMentalHealth = txtMH;
        hud.txtLanguage = txtLang;
        hud.txtEtiquette = txtEtiq;
        hud.txtTheoretical = txtTheo;
        hud.txtPractical = txtPrac;

        hud.txtDayBadge = txtDayBadge;
        hud.txtDateBadge = txtDateBadge;
        hud.txtPhoneClock = txtClock;
        hud.txtPhoneDate = txtPhoneDate;

        hud.btnOpenPhone = btnOpenPhone;
        hud.btnOpenSocialWindow = btnSocial;

        hud.panelTooltip = panelTooltip;
        hud.txtActivityDesc = txtTooltipDesc;
        hud.txtCostGainPreview = txtTooltipCost;

        hud.btnStudyLanguage = btnStudy;
        hud.btnLunchWithNPC = btnLunch;
        hud.btnMeetLecturer = btnLecturer;
        hud.btnSleep = btnSleep;

        ActivityButtonHandler actHandler = canvasGO.GetComponent<ActivityButtonHandler>();
        if (actHandler == null) actHandler = canvasGO.AddComponent<ActivityButtonHandler>();

        UnityEditor.Events.UnityEventTools.RemovePersistentListener(btnStudy.onClick, actHandler.OnClick_StudyLanguage);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(btnStudy.onClick, actHandler.OnClick_StudyLanguage);

        UnityEditor.Events.UnityEventTools.RemovePersistentListener(btnLunch.onClick, actHandler.OnClick_LunchWithLiHaoran);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(btnLunch.onClick, actHandler.OnClick_LunchWithLiHaoran);

        UnityEditor.Events.UnityEventTools.RemovePersistentListener(btnLecturer.onClick, actHandler.OnClick_ReportToLecturer);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(btnLecturer.onClick, actHandler.OnClick_ReportToLecturer);

        UnityEditor.Events.UnityEventTools.RemovePersistentListener(btnSleep.onClick, actHandler.OnClick_Sleep);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(btnSleep.onClick, actHandler.OnClick_Sleep);

        if (btnSocial != null)
        {
            UnityEditor.Events.UnityEventTools.RemovePersistentListener(btnSocial.onClick, hud.OnClick_OpenSocialWindow);
            UnityEditor.Events.UnityEventTools.AddPersistentListener(btnSocial.onClick, hud.OnClick_OpenSocialWindow);
        }

        // Hubungkan DialogueUIController
        DialogueUIController dlg = panelDialogue.GetComponent<DialogueUIController>();
        if (dlg == null) dlg = panelDialogue.AddComponent<DialogueUIController>();

        dlg.dialoguePanel = panelDialogue;
        dlg.btnDialogueBoxClick = btnDialogueClick;
        dlg.txtSpeakerName = txtSpeaker;
        dlg.txtDialogueContent = txtContent;
        dlg.nextIndicator = nextIndicator;
        dlg.optionsContainer = optionsContainer;
        dlg.optionButtonPrefab = optPrefab;
        dlg.btnAuto = btnAuto;
        dlg.btnSkip = btnSkip;
        dlg.btnConfig = btnConfig;
        dlg.btnBacklog = btnBacklog;

        EditorUtility.SetDirty(hud);
        EditorUtility.SetDirty(dlg);
        EditorUtility.SetDirty(canvasGO);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, SCENE_PATH);
        AssetDatabase.SaveAssets();

        CaptureScene1080p(canvas, cam, "test_render_ingame_1080p.png");

        Debug.Log("<color=green>=== PEMBANGUNAN INGAME HOME SCENE (VER 1) SELESAI ===</color>");
    }

    // =========================================================================
    // 3. PREFAB CREATOR: OPTION BUTTON PREFAB
    // =========================================================================
    public static GameObject CreateOrUpdateOptionPrefab(Sprite spNormal, Sprite spSelected, Sprite spHand, TMP_FontAsset font)
    {
        GameObject root = new GameObject("Btn_DialogueOption_Prefab", typeof(RectTransform));
        RectTransform rt = root.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(936f, 54f);

        Image img = root.AddComponent<Image>();
        img.sprite = spNormal;
        img.type = Image.Type.Sliced;
        img.color = Color.white;

        Button btn = root.AddComponent<Button>();
        btn.targetGraphic = img;
        btn.transition = Selectable.Transition.None;

        // Text Option
        GameObject textGO = CreateUIObject("Txt_OptionText", root.transform);
        RectTransform rtText = textGO.GetComponent<RectTransform>();
        rtText.anchorMin = new Vector2(0f, 0f);
        rtText.anchorMax = new Vector2(1f, 1f);
        rtText.offsetMin = new Vector2(28f, 0f);
        rtText.offsetMax = new Vector2(-28f, 0f);

        TextMeshProUGUI txt = textGO.AddComponent<TextMeshProUGUI>();
        txt.text = "Option Text";
        if (font != null) txt.font = font;
        txt.fontSize = 34;
        txt.color = new Color(0.965f, 0.973f, 1f, 1f);
        txt.alignment = TextAlignmentOptions.MidlineLeft;
        txt.enableWordWrapping = false;
        txt.raycastTarget = false;

        // Pointer Hand Icon (👈)
        GameObject handGO = CreateUIObject("Img_PointerHand", root.transform);
        RectTransform rtHand = handGO.GetComponent<RectTransform>();
        rtHand.anchorMin = new Vector2(1f, 0.5f);
        rtHand.anchorMax = new Vector2(1f, 0.5f);
        rtHand.pivot = new Vector2(0f, 0.5f);
        rtHand.anchoredPosition = new Vector2(18f, 0f);
        rtHand.sizeDelta = new Vector2(56f, 56f);

        Image imgHand = handGO.AddComponent<Image>();
        imgHand.sprite = spHand;
        imgHand.preserveAspect = true;
        imgHand.raycastTarget = false;
        handGO.SetActive(false); // Default sembunyi

        // DialogueOptionButtonUI Component
        DialogueOptionButtonUI optUI = root.AddComponent<DialogueOptionButtonUI>();
        optUI.imgBackground = img;
        optUI.txtOption = txt;
        optUI.pointerHand = handGO;
        optUI.spriteNormal = spNormal;
        optUI.spriteSelected = spSelected;
        optUI.colorNormalText = new Color(0.965f, 0.973f, 1f, 1f);
        optUI.colorSelectedText = new Color(0.118f, 0.141f, 0.251f, 1f);

        // Save Prefabs
        string dir = Path.GetDirectoryName(PREFAB_OPTION_PATH);
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        GameObject saved1 = PrefabUtility.SaveAsPrefabAsset(root, PREFAB_OPTION_PATH);
        GameObject saved2 = PrefabUtility.SaveAsPrefabAsset(root, PREFAB_OPTION_ALT_PATH);

        Object.DestroyImmediate(root);
        Debug.Log("<color=green>[OptionPrefab]</color> Prefab tombol opsi berhasil dibuat & disimpan.");
        return saved1 != null ? saved1 : saved2;
    }

    // =========================================================================
    // 4. SUB-BUILDERS
    // =========================================================================

    public static Button BuildBackButton(Transform parent, Sprite spFront, Sprite spShadow, TMP_FontAsset font)
    {
        GameObject root = CreateUIObject("Btn_Back", parent);
        RectTransform rt = root.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = new Vector2(61f, -40f);
        rt.sizeDelta = new Vector2(184f, 62f);

        // Shadow
        GameObject shadowGO = CreateUIObject("Img_Shadow", root.transform);
        RectTransform rtSh = shadowGO.GetComponent<RectTransform>();
        StretchFull(rtSh);
        rtSh.anchoredPosition = new Vector2(3f, -3f);
        Image imgSh = shadowGO.AddComponent<Image>();
        imgSh.sprite = spShadow;
        imgSh.type = Image.Type.Sliced;
        imgSh.raycastTarget = false;

        // Front
        GameObject frontGO = CreateUIObject("Img_Front", root.transform);
        StretchFull(frontGO.GetComponent<RectTransform>());
        Image imgFront = frontGO.AddComponent<Image>();
        imgFront.sprite = spFront;
        imgFront.type = Image.Type.Sliced;
        imgFront.raycastTarget = true;

        Button btn = root.AddComponent<Button>();
        btn.targetGraphic = imgFront;

        // Content Container (Icon + Text)
        GameObject contentGO = CreateUIObject("Content", frontGO.transform);
        StretchFull(contentGO.GetComponent<RectTransform>());
        HorizontalLayoutGroup hlg = contentGO.AddComponent<HorizontalLayoutGroup>();
        hlg.padding = new RectOffset(20, 16, 8, 8);
        hlg.spacing = 10;
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.childControlWidth = false;
        hlg.childControlHeight = false;

        // Back Arrow Icon (↩ or ←)
        TextMeshProUGUI txtIcon = CreateText("Txt_Icon", contentGO.transform, "↩", 34, Color.white, true);
        txtIcon.rectTransform.sizeDelta = new Vector2(36f, 40f);
        txtIcon.alignment = TextAlignmentOptions.Center;

        TextMeshProUGUI txtLabel = CreateText("Txt_Label", contentGO.transform, "Back", 36, Color.white);
        if (font != null) txtLabel.font = font;
        txtLabel.rectTransform.sizeDelta = new Vector2(85f, 40f);
        txtLabel.alignment = TextAlignmentOptions.MidlineLeft;

        return btn;
    }

    private static Button BuildHomeButton(Transform parent, Sprite spFront, Sprite spShadow, TMP_FontAsset font)
    {
        GameObject root = CreateUIObject("Btn_Home", parent);
        RectTransform rt = root.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = new Vector2(115f, -44f);
        rt.sizeDelta = new Vector2(304f, 78f);

        GameObject shadowGO = CreateUIObject("Img_Shadow", root.transform);
        RectTransform rtSh = shadowGO.GetComponent<RectTransform>();
        StretchFull(rtSh);
        rtSh.anchoredPosition = new Vector2(4f, -4f);
        Image imgSh = shadowGO.AddComponent<Image>();
        imgSh.sprite = spShadow;
        imgSh.type = Image.Type.Sliced;
        imgSh.raycastTarget = false;

        GameObject frontGO = CreateUIObject("Img_Front", root.transform);
        StretchFull(frontGO.GetComponent<RectTransform>());
        Image imgFront = frontGO.AddComponent<Image>();
        imgFront.sprite = spFront;
        imgFront.type = Image.Type.Sliced;
        imgFront.raycastTarget = true;

        Button btn = root.AddComponent<Button>();
        btn.targetGraphic = imgFront;

        GameObject contentGO = CreateUIObject("Content", frontGO.transform);
        StretchFull(contentGO.GetComponent<RectTransform>());
        HorizontalLayoutGroup hlg = contentGO.AddComponent<HorizontalLayoutGroup>();
        hlg.padding = new RectOffset(30, 20, 10, 10);
        hlg.spacing = 14;
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.childControlWidth = false;
        hlg.childControlHeight = false;

        TextMeshProUGUI txtIcon = CreateText("Txt_Icon", contentGO.transform, "⌂", 38, new Color(0.965f, 0.973f, 1f, 1f), true);
        txtIcon.rectTransform.sizeDelta = new Vector2(40f, 44f);
        txtIcon.alignment = TextAlignmentOptions.Center;

        TextMeshProUGUI txtLabel = CreateText("Txt_Label", contentGO.transform, "Home", 42, new Color(0.965f, 0.973f, 1f, 1f));
        if (font != null) txtLabel.font = font;
        txtLabel.rectTransform.sizeDelta = new Vector2(140f, 44f);
        txtLabel.alignment = TextAlignmentOptions.MidlineLeft;

        return btn;
    }

    private static GameObject BuildStatsHUD(
        Transform parent,
        Sprite spFrame,
        Sprite spCell,
        Sprite spDivider,
        TMP_FontAsset font,
        out TextMeshProUGUI txtLang,
        out TextMeshProUGUI txtEtiq,
        out TextMeshProUGUI txtMH,
        out TextMeshProUGUI txtPH,
        out TextMeshProUGUI txtTheo,
        out TextMeshProUGUI txtPrac)
    {
        GameObject root = CreateUIObject("Panel_TopStats", parent);
        RectTransform rt = root.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = new Vector2(832f, -28f);
        rt.sizeDelta = new Vector2(970f, 146f);

        Image imgFrame = root.AddComponent<Image>();
        imgFrame.sprite = spFrame;
        imgFrame.type = Image.Type.Sliced;
        imgFrame.raycastTarget = false;

        GameObject cont = CreateUIObject("CellsContainer", root.transform);
        RectTransform rtCont = cont.GetComponent<RectTransform>();
        rtCont.anchorMin = new Vector2(0f, 0f);
        rtCont.anchorMax = new Vector2(1f, 1f);
        rtCont.offsetMin = new Vector2(24f, 18f);
        rtCont.offsetMax = new Vector2(-24f, -18f);

        HorizontalLayoutGroup hlg = cont.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 16;
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.childControlWidth = true;
        hlg.childControlHeight = true;
        hlg.childForceExpandWidth = true;
        hlg.childForceExpandHeight = true;

        BuildStatCell(cont.transform, spCell, spDivider, font, "LP", "089", out txtLang, "CE", "026", out txtEtiq);
        BuildStatCell(cont.transform, spCell, spDivider, font, "MH", "089", out txtMH, "PH", "026", out txtPH);
        BuildStatCell(cont.transform, spCell, spDivider, font, "AT", "089", out txtTheo, "PS", "026", out txtPrac);

        return root;
    }

    private static void BuildStatCell(
        Transform parent,
        Sprite spCell,
        Sprite spDivider,
        TMP_FontAsset font,
        string label1, string val1, out TextMeshProUGUI txtVal1,
        string label2, string val2, out TextMeshProUGUI txtVal2)
    {
        GameObject cell = CreateUIObject($"Cell_{label1}_{label2}", parent);
        Image imgCell = cell.AddComponent<Image>();
        imgCell.sprite = spCell;
        imgCell.type = Image.Type.Sliced;

        VerticalLayoutGroup vlg = cell.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(18, 18, 10, 10);
        vlg.spacing = 6;
        vlg.childAlignment = TextAnchor.MiddleCenter;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = true;

        BuildStatRow(cell.transform, spDivider, font, label1, val1, out txtVal1);
        BuildStatRow(cell.transform, spDivider, font, label2, val2, out txtVal2);
    }

    private static void BuildStatRow(
        Transform parent,
        Sprite spDivider,
        TMP_FontAsset font,
        string label,
        string val,
        out TextMeshProUGUI txtVal)
    {
        GameObject row = CreateUIObject($"Row_{label}", parent);
        HorizontalLayoutGroup hlg = row.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 10;
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.childControlWidth = false;
        hlg.childControlHeight = false;

        TextMeshProUGUI txtLbl = CreateText("Txt_Label", row.transform, label, 36, Color.white);
        if (font != null) txtLbl.font = font;
        txtLbl.enableWordWrapping = false;
        txtLbl.rectTransform.sizeDelta = new Vector2(65f, 40f);
        txtLbl.alignment = TextAlignmentOptions.MidlineLeft;

        GameObject divGO = CreateUIObject("Img_Divider", row.transform);
        RectTransform rtDiv = divGO.GetComponent<RectTransform>();
        rtDiv.sizeDelta = new Vector2(40f, 10f);
        Image imgDiv = divGO.AddComponent<Image>();
        imgDiv.sprite = spDivider;
        imgDiv.color = Color.white;
        imgDiv.raycastTarget = false;

        txtVal = CreateText("Txt_Value", row.transform, val, 36, Color.white);
        if (font != null) txtVal.font = font;
        txtVal.enableWordWrapping = false;
        txtVal.rectTransform.sizeDelta = new Vector2(75f, 40f);
        txtVal.alignment = TextAlignmentOptions.MidlineRight;
    }

    public static GameObject BuildCalendarBadge(
        Transform parent,
        Sprite spBg,
        Sprite spTab,
        TMP_FontAsset font,
        out TextMeshProUGUI txtDayBadge,
        out TextMeshProUGUI txtDateBadge)
    {
        GameObject root = CreateUIObject("Panel_Calendar", parent);
        RectTransform rt = root.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(1f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(1f, 1f);
        rt.anchoredPosition = new Vector2(-60f, -31f);
        rt.sizeDelta = new Vector2(291f, 157f);

        Image imgBg = root.AddComponent<Image>();
        imgBg.sprite = spBg;
        imgBg.type = Image.Type.Sliced;
        imgBg.raycastTarget = false;

        // Orange Header Tab
        GameObject tabGO = CreateUIObject("HeaderTab", root.transform);
        RectTransform rtTab = tabGO.GetComponent<RectTransform>();
        rtTab.anchorMin = new Vector2(0.5f, 1f);
        rtTab.anchorMax = new Vector2(0.5f, 1f);
        rtTab.pivot = new Vector2(0.5f, 1f);
        rtTab.anchoredPosition = new Vector2(0f, -8f);
        rtTab.sizeDelta = new Vector2(265f, 56f);

        Image imgTab = tabGO.AddComponent<Image>();
        imgTab.sprite = spTab;
        imgTab.type = Image.Type.Sliced;
        imgTab.raycastTarget = false;

        txtDayBadge = CreateText("Txt_DayBadge", tabGO.transform, "[ Rabu ]", 36, new Color(0.965f, 0.973f, 1f, 1f));
        if (font != null) txtDayBadge.font = font;
        StretchFull(txtDayBadge.rectTransform);
        txtDayBadge.alignment = TextAlignmentOptions.Center;

        txtDateBadge = CreateText("Txt_DateBadge", root.transform, "16 / 09 / 2026", 36, new Color(0.965f, 0.973f, 1f, 1f));
        if (font != null) txtDateBadge.font = font;
        RectTransform rtDate = txtDateBadge.rectTransform;
        rtDate.anchorMin = new Vector2(0f, 0f);
        rtDate.anchorMax = new Vector2(1f, 0f);
        rtDate.pivot = new Vector2(0.5f, 0f);
        rtDate.anchoredPosition = new Vector2(0f, 20f);
        rtDate.sizeDelta = new Vector2(0f, 50f);
        txtDateBadge.alignment = TextAlignmentOptions.Center;

        return root;
    }

    private static Button BuildPhoneWidget(
        Transform parent,
        Sprite spPhone,
        TMP_FontAsset font,
        out TextMeshProUGUI txtClock,
        out TextMeshProUGUI txtPhoneDate)
    {
        GameObject root = CreateUIObject("Btn_OpenPhone", parent);
        RectTransform rt = root.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = new Vector2(1627f, -425f);
        rt.sizeDelta = new Vector2(167f, 260f);

        Image img = root.AddComponent<Image>();
        img.sprite = spPhone;
        img.type = Image.Type.Simple;
        img.preserveAspect = true;

        Button btn = root.AddComponent<Button>();
        btn.targetGraphic = img;

        txtClock = CreateText("Txt_PhoneClock", root.transform, "11.30", 26, Color.white, true);
        if (font != null) txtClock.font = font;
        RectTransform rtClock = txtClock.rectTransform;
        rtClock.anchorMin = new Vector2(0.5f, 1f);
        rtClock.anchorMax = new Vector2(0.5f, 1f);
        rtClock.pivot = new Vector2(0.5f, 1f);
        rtClock.anchoredPosition = new Vector2(0f, -44f);
        rtClock.sizeDelta = new Vector2(120f, 32f);
        txtClock.alignment = TextAlignmentOptions.Center;

        txtPhoneDate = CreateText("Txt_PhoneDate", root.transform, "Selasa, 15 Sep", 11, Color.white);
        if (font != null) txtPhoneDate.font = font;
        RectTransform rtDate = txtPhoneDate.rectTransform;
        rtDate.anchorMin = new Vector2(0.5f, 1f);
        rtDate.anchorMax = new Vector2(0.5f, 1f);
        rtDate.pivot = new Vector2(0.5f, 1f);
        rtDate.anchoredPosition = new Vector2(0f, -76f);
        rtDate.sizeDelta = new Vector2(120f, 18f);
        txtPhoneDate.alignment = TextAlignmentOptions.Center;

        return btn;
    }

    private static GameObject BuildActivityGrid(
        Transform parent,
        Sprite spBg,
        Sprite spTab,
        Sprite spSlot,
        TMP_FontAsset font,
        out Button btnStudy,
        out Button btnLunch,
        out Button btnLecturer,
        out Button btnSleep,
        out Button btnSocial)
    {
        GameObject root = CreateUIObject("Panel_ActivityGrid", parent);
        RectTransform rt = root.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = new Vector2(103f, -199f);
        rt.sizeDelta = new Vector2(328f, 418f);

        GameObject tabTop = CreateUIObject("TabTop", root.transform);
        RectTransform rtTabTop = tabTop.GetComponent<RectTransform>();
        rtTabTop.anchorMin = new Vector2(0.5f, 1f);
        rtTabTop.anchorMax = new Vector2(0.5f, 1f);
        rtTabTop.pivot = new Vector2(0.5f, 1f);
        rtTabTop.anchoredPosition = Vector2.zero;
        rtTabTop.sizeDelta = new Vector2(328f, 48f);
        Image imgTabTop = tabTop.AddComponent<Image>();
        imgTabTop.sprite = spTab;
        imgTabTop.type = Image.Type.Sliced;
        imgTabTop.raycastTarget = false;

        GameObject tabBot = CreateUIObject("TabBot", root.transform);
        RectTransform rtTabBot = tabBot.GetComponent<RectTransform>();
        rtTabBot.anchorMin = new Vector2(0.5f, 0f);
        rtTabBot.anchorMax = new Vector2(0.5f, 0f);
        rtTabBot.pivot = new Vector2(0.5f, 0f);
        rtTabBot.anchoredPosition = Vector2.zero;
        rtTabBot.sizeDelta = new Vector2(328f, 48f);
        Image imgTabBot = tabBot.AddComponent<Image>();
        imgTabBot.sprite = spTab;
        imgTabBot.type = Image.Type.Sliced;
        imgTabBot.raycastTarget = false;

        GameObject cardGO = CreateUIObject("CardBody", root.transform);
        RectTransform rtCard = cardGO.GetComponent<RectTransform>();
        rtCard.anchorMin = new Vector2(0f, 0f);
        rtCard.anchorMax = new Vector2(1f, 1f);
        rtCard.offsetMin = new Vector2(14f, 16f);
        rtCard.offsetMax = new Vector2(-14f, -16f);
        Image imgCard = cardGO.AddComponent<Image>();
        imgCard.sprite = spBg;
        imgCard.type = Image.Type.Sliced;
        imgCard.raycastTarget = false;

        float[] colX = { 18f, 107f, 196f };
        float[] rowY = { -16f, -107f, -198f, -289f };

        btnStudy = CreateSlotButton(cardGO.transform, "Btn_StudyLanguage", spSlot, colX[0], rowY[0], "📖", "Belajar Kosakata Mandarin");
        btnLunch = CreateSlotButton(cardGO.transform, "Btn_LunchWithNPC", spSlot, colX[1], rowY[0], "🍱", "Makan Siang Li Haoran");
        btnLecturer = CreateSlotButton(cardGO.transform, "Btn_MeetLecturer", spSlot, colX[2], rowY[0], "🎓", "Laporan Progres Dosen");

        btnSleep = CreateSlotButton(cardGO.transform, "Btn_SleepEarly", spSlot, colX[0], rowY[1], "🌙", "Istirahat / Tidur Penuh");
        CreateSlotButton(cardGO.transform, "Btn_Library", spSlot, colX[2], rowY[1], "📚", "Studi Literatur Perpustakaan");

        CreateSlotButton(cardGO.transform, "Btn_Fitness", spSlot, colX[0], rowY[2], "🏃", "Olahraga & Kebugaran Fisik");
        CreateSlotButton(cardGO.transform, "Btn_PartTime", spSlot, colX[2], rowY[2], "💼", "Kerja Part-Time");

        btnSocial = CreateSlotButton(cardGO.transform, "Btn_OpenSocial", spSlot, colX[0], rowY[3], "🌸", "Status Hubungan Sosial");
        CreateSlotButton(cardGO.transform, "Btn_Hangout", spSlot, colX[1], rowY[3], "👥", "Jalan-jalan & Hangout");
        CreateSlotButton(cardGO.transform, "Btn_Cafe", spSlot, colX[2], rowY[3], "☕", "Relaksasi di Kafe");

        return root;
    }

    private static Button CreateSlotButton(Transform parent, string name, Sprite spSlot, float posX, float posY, string icon, string tooltip)
    {
        GameObject btnGO = CreateUIObject(name, parent);
        RectTransform rt = btnGO.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = new Vector2(posX, posY);
        rt.sizeDelta = new Vector2(85f, 85f);

        Image img = btnGO.AddComponent<Image>();
        img.sprite = spSlot;
        img.type = Image.Type.Sliced;
        img.color = Color.white;

        Button btn = btnGO.AddComponent<Button>();
        btn.targetGraphic = img;

        TextMeshProUGUI txtIcon = CreateText("Txt_Icon", btnGO.transform, icon, 42, new Color(0.12f, 0.14f, 0.25f, 1f));
        StretchFull(txtIcon.rectTransform);
        txtIcon.alignment = TextAlignmentOptions.Center;

        ActivityTooltipTrigger tt = btnGO.AddComponent<ActivityTooltipTrigger>();
        tt.activityDescription = tooltip;
        tt.costGainPreview = "";

        return btn;
    }

    private static GameObject BuildPredictiveTooltip(Transform parent, TMP_FontAsset font, out TextMeshProUGUI txtDesc, out TextMeshProUGUI txtCost)
    {
        GameObject root = CreateUIObject("Panel_PredictiveTooltip", parent);
        RectTransform rt = root.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0f);
        rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.anchoredPosition = new Vector2(0f, 395f);
        rt.sizeDelta = new Vector2(1100f, 95f);

        Image img = root.AddComponent<Image>();
        img.color = new Color(0.08f, 0.09f, 0.16f, 0.94f);

        Outline outl = root.AddComponent<Outline>();
        outl.effectColor = new Color(0.784f, 0.820f, 1f, 0.8f);
        outl.effectDistance = new Vector2(2f, -2f);

        VerticalLayoutGroup vlg = root.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(25, 25, 10, 10);
        vlg.spacing = 4;
        vlg.childAlignment = TextAnchor.MiddleCenter;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = true;

        txtDesc = CreateText("Txt_ActivityDescription", root.transform, "Pilih aktivitas harian Devano Baskara Pratama.", 24, Color.white);
        if (font != null) txtDesc.font = font;
        txtDesc.alignment = TextAlignmentOptions.Center;

        txtCost = CreateText("Txt_CostGainPreview", root.transform, "Biaya & Efek", 20, new Color(1f, 0.85f, 0.3f), true);
        if (font != null) txtCost.font = font;
        txtCost.alignment = TextAlignmentOptions.Center;

        return root;
    }

    public static GameObject BuildDialogueBox(
        Transform parent,
        Sprite spFrame,
        Sprite spDark,
        Sprite spNameTag,
        Sprite spCtrlFront,
        Sprite spCtrlShadow,
        TMP_FontAsset font,
        out Button btnClickArea,
        out TextMeshProUGUI txtSpeaker,
        out TextMeshProUGUI txtContent,
        out GameObject nextIndicator,
        out Transform optionsContainer,
        out Button btnAuto,
        out Button btnSkip,
        out Button btnConfig,
        out Button btnBacklog)
    {
        GameObject root = CreateUIObject("Panel_DialogueBox", parent);
        RectTransform rt = root.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0f);
        rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.anchoredPosition = new Vector2(0f, 65f);
        rt.sizeDelta = new Vector2(1685f, 320f);

        // Outer Periwinkle Frame
        Image imgFrame = root.AddComponent<Image>();
        imgFrame.sprite = spFrame;
        imgFrame.type = Image.Type.Sliced;
        imgFrame.raycastTarget = false;

        // Inner Dark Box (offset 18px around)
        GameObject darkGO = CreateUIObject("InnerDark", root.transform);
        RectTransform rtDark = darkGO.GetComponent<RectTransform>();
        rtDark.anchorMin = Vector2.zero;
        rtDark.anchorMax = Vector2.one;
        rtDark.offsetMin = new Vector2(18f, 18f);
        rtDark.offsetMax = new Vector2(-18f, -18f);

        Image imgDark = darkGO.AddComponent<Image>();
        imgDark.sprite = spDark;
        imgDark.type = Image.Type.Sliced;
        imgDark.raycastTarget = true;

        btnClickArea = darkGO.AddComponent<Button>();

        // Character Name Tag (Top Left overlapping frame)
        GameObject nameGO = CreateUIObject("NameTagBadge", root.transform);
        RectTransform rtName = nameGO.GetComponent<RectTransform>();
        rtName.anchorMin = new Vector2(0f, 1f);
        rtName.anchorMax = new Vector2(0f, 1f);
        rtName.pivot = new Vector2(0f, 1f);
        rtName.anchoredPosition = new Vector2(16f, 26f);
        rtName.sizeDelta = new Vector2(390f, 68f);

        Image imgName = nameGO.AddComponent<Image>();
        imgName.sprite = spNameTag;
        imgName.type = Image.Type.Sliced;
        imgName.raycastTarget = false;

        txtSpeaker = CreateText("Txt_SpeakerName", nameGO.transform, "Character Name", 38, new Color(0.965f, 0.973f, 1f, 1f));
        if (font != null) txtSpeaker.font = font;
        StretchFull(txtSpeaker.rectTransform);
        txtSpeaker.alignment = TextAlignmentOptions.Center;

        // Dialogue Content Text
        GameObject contentGO = CreateUIObject("Txt_DialogueContent", darkGO.transform);
        RectTransform rtContent = contentGO.GetComponent<RectTransform>();
        rtContent.anchorMin = Vector2.zero;
        rtContent.anchorMax = Vector2.one;
        rtContent.offsetMin = new Vector2(60f, 75f);
        rtContent.offsetMax = new Vector2(-120f, -35f);

        txtContent = contentGO.AddComponent<TextMeshProUGUI>();
        txtContent.text = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Etiam aliquam justo at eros sollicitudin, at consectetur odio tempus. Vivamus blandit pretium leo ac tristique.";
        if (font != null) txtContent.font = font;
        txtContent.fontSize = 36;
        txtContent.color = Color.white;
        txtContent.enableWordWrapping = true;
        txtContent.raycastTarget = false;

        // Next Chevron Indicator >>>
        GameObject nextGO = CreateUIObject("NextIndicator", darkGO.transform);
        RectTransform rtNext = nextGO.GetComponent<RectTransform>();
        rtNext.anchorMin = new Vector2(1f, 0f);
        rtNext.anchorMax = new Vector2(1f, 0f);
        rtNext.pivot = new Vector2(1f, 0f);
        rtNext.anchoredPosition = new Vector2(-40f, 40f);
        rtNext.sizeDelta = new Vector2(80f, 40f);

        TextMeshProUGUI txtNext = nextGO.AddComponent<TextMeshProUGUI>();
        txtNext.text = ">>>";
        if (font != null) txtNext.font = font;
        txtNext.fontSize = 36;
        txtNext.fontStyle = FontStyles.Bold;
        txtNext.color = new Color(0.965f, 0.973f, 1f, 1f);
        txtNext.alignment = TextAlignmentOptions.MidlineRight;
        txtNext.raycastTarget = false;
        nextIndicator = nextGO;

        // Options Container (Centered in dialogue box)
        GameObject optsGO = CreateUIObject("OptionsContainer", darkGO.transform);
        RectTransform rtOpts = optsGO.GetComponent<RectTransform>();
        rtOpts.anchorMin = new Vector2(0.5f, 0.5f);
        rtOpts.anchorMax = new Vector2(0.5f, 0.5f);
        rtOpts.pivot = new Vector2(0.5f, 0.5f);
        rtOpts.anchoredPosition = new Vector2(90f, -8f);
        rtOpts.sizeDelta = new Vector2(960f, 210f);

        VerticalLayoutGroup vlgOpts = optsGO.AddComponent<VerticalLayoutGroup>();
        vlgOpts.spacing = 14;
        vlgOpts.childAlignment = TextAnchor.MiddleCenter;
        vlgOpts.childControlWidth = false;
        vlgOpts.childControlHeight = false;
        optionsContainer = optsGO.transform;

        // Reader Control Buttons Suite (Auto, Skip, Config, Log)
        GameObject ctrlSuite = CreateUIObject("ControlSuite", root.transform);
        RectTransform rtSuite = ctrlSuite.GetComponent<RectTransform>();
        rtSuite.anchorMin = new Vector2(1f, 0f);
        rtSuite.anchorMax = new Vector2(1f, 0f);
        rtSuite.pivot = new Vector2(1f, 1f);
        rtSuite.anchoredPosition = new Vector2(-20f, -8f);
        rtSuite.sizeDelta = new Vector2(760f, 54f);

        HorizontalLayoutGroup hlgSuite = ctrlSuite.AddComponent<HorizontalLayoutGroup>();
        hlgSuite.spacing = 14;
        hlgSuite.childAlignment = TextAnchor.MiddleRight;
        hlgSuite.childControlWidth = false;
        hlgSuite.childControlHeight = false;

        btnAuto = BuildControlButton(ctrlSuite.transform, "Btn_Auto", "▶ Auto", spCtrlFront, spCtrlShadow, font);
        btnSkip = BuildControlButton(ctrlSuite.transform, "Btn_Skip", "⏭ Skip", spCtrlFront, spCtrlShadow, font);
        btnConfig = BuildControlButton(ctrlSuite.transform, "Btn_Config", "⚙ Config", spCtrlFront, spCtrlShadow, font);
        btnBacklog = BuildControlButton(ctrlSuite.transform, "Btn_Backlog", "📋 Log", spCtrlFront, spCtrlShadow, font);

        return root;
    }

    private static Button BuildControlButton(Transform parent, string name, string label, Sprite spFront, Sprite spShadow, TMP_FontAsset font)
    {
        GameObject root = CreateUIObject(name, parent);
        RectTransform rt = root.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(170f, 48f);

        GameObject shadowGO = CreateUIObject("Img_Shadow", root.transform);
        RectTransform rtSh = shadowGO.GetComponent<RectTransform>();
        StretchFull(rtSh);
        rtSh.anchoredPosition = new Vector2(3f, -3f);
        Image imgSh = shadowGO.AddComponent<Image>();
        imgSh.sprite = spShadow;
        imgSh.type = Image.Type.Sliced;
        imgSh.raycastTarget = false;

        GameObject frontGO = CreateUIObject("Img_Front", root.transform);
        StretchFull(frontGO.GetComponent<RectTransform>());
        Image imgFront = frontGO.AddComponent<Image>();
        imgFront.sprite = spFront;
        imgFront.type = Image.Type.Sliced;
        imgFront.raycastTarget = true;

        Button btn = root.AddComponent<Button>();
        btn.targetGraphic = imgFront;

        TextMeshProUGUI txt = CreateText("Txt_Label", frontGO.transform, label, 28, Color.white);
        if (font != null) txt.font = font;
        StretchFull(txt.rectTransform);
        txt.alignment = TextAlignmentOptions.Center;

        return btn;
    }

    // =========================================================================
    // 5. UTILITY & TEXTURE IMPORTER HELPERS
    // =========================================================================

    private static GameObject CreateUIObject(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    private static TextMeshProUGUI CreateText(string name, Transform parent, string text, float fontSize, Color color, bool bold = false)
    {
        GameObject go = CreateUIObject(name, parent);
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        if (bold) tmp.fontStyle = FontStyles.Bold;
        tmp.raycastTarget = false;
        return tmp;
    }

    private static void StretchFull(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private static Camera EnsureCamera()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            GameObject camGO = GameObject.Find("Main Camera");
            if (camGO == null) camGO = new GameObject("Main Camera");
            cam = camGO.GetComponent<Camera>();
            if (cam == null) cam = camGO.AddComponent<Camera>();
            cam.tag = "MainCamera";
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.965f, 0.973f, 1f, 1f);
        }
        return cam;
    }

    private static void ConfigureAllTextures()
    {
        ConfigureSpriteTexture(BG_BEDROOM_PATH, Vector4.zero);
        ConfigureSpriteTexture(BG_HALLWAY_PATH, Vector4.zero);
        ConfigureSpriteTexture(CHARA_GIRL_PATH, Vector4.zero);

        ConfigureSpriteTexture(SPRITE_HOME_FRONT, new Vector4(26, 26, 26, 26));
        ConfigureSpriteTexture(SPRITE_HOME_SHADOW, new Vector4(26, 26, 26, 26));
        ConfigureSpriteTexture(SPRITE_BACK_FRONT, new Vector4(14, 14, 14, 14));
        ConfigureSpriteTexture(SPRITE_BACK_SHADOW, new Vector4(14, 14, 14, 14));

        ConfigureSpriteTexture(SPRITE_HUD_FRAME, new Vector4(28, 28, 28, 28));
        ConfigureSpriteTexture(SPRITE_HUD_CELL, new Vector4(16, 16, 16, 16));
        ConfigureSpriteTexture(SPRITE_CAL_BG, new Vector4(14, 14, 14, 14));
        ConfigureSpriteTexture(SPRITE_CAL_TAB, new Vector4(12, 12, 12, 12));
        ConfigureSpriteTexture(SPRITE_ACT_BG, new Vector4(16, 16, 16, 16));
        ConfigureSpriteTexture(SPRITE_ACT_TAB, new Vector4(12, 12, 12, 12));
        ConfigureSpriteTexture(SPRITE_ACT_SLOT, new Vector4(14, 14, 14, 14));
        ConfigureSpriteTexture(SPRITE_DIA_FRAME, new Vector4(26, 26, 26, 26));
        ConfigureSpriteTexture(SPRITE_DIA_DARK, new Vector4(26, 26, 26, 26));
        ConfigureSpriteTexture(SPRITE_NAME_TAG, new Vector4(12, 12, 12, 12));
        ConfigureSpriteTexture(SPRITE_CTRL_FRONT, new Vector4(12, 12, 12, 12));
        ConfigureSpriteTexture(SPRITE_CTRL_SHADOW, new Vector4(12, 12, 12, 12));
        ConfigureSpriteTexture(SPRITE_PHONE, Vector4.zero);
        ConfigureSpriteTexture(SPRITE_VIG_TOP, Vector4.zero);
        ConfigureSpriteTexture(SPRITE_VIG_BOT, Vector4.zero);
        ConfigureSpriteTexture(SPRITE_STAT_DIVIDER, Vector4.zero);

        ConfigureSpriteTexture(SPRITE_CHOICE_NORMAL, new Vector4(16, 16, 16, 16));
        ConfigureSpriteTexture(SPRITE_CHOICE_SELECTED, new Vector4(16, 16, 16, 16));
        ConfigureSpriteTexture(SPRITE_POINTER_HAND, Vector4.zero);
    }

    private static void ConfigureSpriteTexture(string path, Vector4 borders)
    {
        TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
        if (ti != null)
        {
            bool changed = false;
            if (ti.textureType != TextureImporterType.Sprite)
            {
                ti.textureType = TextureImporterType.Sprite;
                changed = true;
            }
            if (ti.spriteImportMode != SpriteImportMode.Single)
            {
                ti.spriteImportMode = SpriteImportMode.Single;
                changed = true;
            }
            if (ti.mipmapEnabled)
            {
                ti.mipmapEnabled = false;
                changed = true;
            }
            if (borders != Vector4.zero && ti.spriteBorder != borders)
            {
                ti.spriteBorder = borders;
                changed = true;
            }
            if (changed)
            {
                ti.SaveAndReimport();
            }
        }
    }

    // =========================================================================
    // 6. 1080P SCREENSHOT CAPTURE HELPER
    // =========================================================================
    public static void CaptureScene1080p(Canvas canvas, Camera cam, string filename)
    {
        if (canvas == null || cam == null) return;

        RenderMode prevMode = canvas.renderMode;
        Camera prevCam = canvas.worldCamera;

        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = cam;

        RenderTexture rt = new RenderTexture(1920, 1080, 24, RenderTextureFormat.ARGB32);
        RenderTexture prevActive = RenderTexture.active;
        RenderTexture prevTarget = cam.targetTexture;

        cam.targetTexture = rt;
        RenderTexture.active = rt;
        cam.Render();

        Texture2D tex = new Texture2D(1920, 1080, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, 1920, 1080), 0, 0);
        tex.Apply();

        string outPath = Path.Combine("Assets/Textures/UI_Ingame", filename);
        File.WriteAllBytes(outPath, tex.EncodeToPNG());

        cam.targetTexture = prevTarget;
        RenderTexture.active = prevActive;
        Object.DestroyImmediate(rt);
        Object.DestroyImmediate(tex);

        canvas.renderMode = prevMode;
        canvas.worldCamera = prevCam;

        AssetDatabase.ImportAsset(outPath);
        Debug.Log($"<color=yellow>[Render 1080p]</color> Snapshot berhasil disimpan ke: {outPath}");
    }
}
#endif
