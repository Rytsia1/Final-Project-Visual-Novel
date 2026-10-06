#if UNITY_EDITOR
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

public static class PhoneSceneBuilder
{
    private const string SCENE_PATH = "Assets/Scenes/SampleScene.unity";
    private const string BG_BEDROOM_PATH = "Assets/Textures/ingame_bedroom_bg.jpg";

    private const string SPRITE_FRAME_OUTER = "Assets/Textures/UI_Phone/phone_frame_outer.png";
    private const string SPRITE_WALLPAPER = "Assets/Textures/UI_Phone/phone_wallpaper.png";
    private const string SPRITE_WIDGET_BG = "Assets/Textures/UI_Phone/widget_card_bg.png";
    private const string SPRITE_APP_BG = "Assets/Textures/UI_Phone/app_icon_bg.png";
    private const string SPRITE_VIGNETTE_SIDE = "Assets/Textures/UI_Phone/phone_vignette_side.png";

    private const string ICON_CALL = "Assets/Textures/UI_Phone/icon_phone_call.png";
    private const string ICON_CHAT = "Assets/Textures/UI_Phone/icon_phone_chat.png";
    private const string ICON_CAM = "Assets/Textures/UI_Phone/icon_phone_camera.png";
    private const string ICON_SETTING = "Assets/Textures/UI_Phone/icon_phone_setting.png";
    private const string ICON_RELATION = "Assets/Textures/UI_Phone/icon_relation_badge.png";

    private const string ICON_WIFI = "Assets/Textures/UI_Phone/icon_status_wifi.png";
    private const string ICON_SIGNAL = "Assets/Textures/UI_Phone/icon_status_signal.png";
    private const string ICON_BATTERY = "Assets/Textures/UI_Phone/icon_status_battery.png";

    private const string FONT_POPPINS_SDF = "Assets/Fonts/Poppins-Bold SDF.asset";
    private const string FONT_PATRICK_SDF = "Assets/Fonts/PatrickHand-Regular SDF.asset";

    [MenuItem("Game Debug/Build Phone Screen")]
    public static void BuildPhoneScreenMenu()
    {
        BuildPhoneScreen();
    }

    public static void BuildPhoneScreen()
    {
        Debug.Log("<color=cyan>=== MEMULAI PEMBANGUNAN PHONE SCREEN (1080P) ===</color>");

        // 1. Konfigurasi semua sprite UI Phone
        ConfigurePhoneTextures();

        // 2. Load Fonts & Sprites
        TMP_FontAsset fontPoppins = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FONT_POPPINS_SDF);
        TMP_FontAsset fontPatrick = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FONT_PATRICK_SDF);

        Sprite spBgBedroom = AssetDatabase.LoadAssetAtPath<Sprite>(BG_BEDROOM_PATH);
        Sprite spFrameOuter = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITE_FRAME_OUTER);
        Sprite spWallpaper = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITE_WALLPAPER);
        Sprite spWidgetBg = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITE_WIDGET_BG);
        Sprite spAppBg = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITE_APP_BG);
        Sprite spVignetteSide = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITE_VIGNETTE_SIDE);

        Sprite spIconCall = AssetDatabase.LoadAssetAtPath<Sprite>(ICON_CALL);
        Sprite spIconChat = AssetDatabase.LoadAssetAtPath<Sprite>(ICON_CHAT);
        Sprite spIconCam = AssetDatabase.LoadAssetAtPath<Sprite>(ICON_CAM);
        Sprite spIconSetting = AssetDatabase.LoadAssetAtPath<Sprite>(ICON_SETTING);
        Sprite spIconRelation = AssetDatabase.LoadAssetAtPath<Sprite>(ICON_RELATION);

        Sprite spIconWifi = AssetDatabase.LoadAssetAtPath<Sprite>(ICON_WIFI);
        Sprite spIconSignal = AssetDatabase.LoadAssetAtPath<Sprite>(ICON_SIGNAL);
        Sprite spIconBattery = AssetDatabase.LoadAssetAtPath<Sprite>(ICON_BATTERY);

        // 3. Buka Scene
        var scene = EditorSceneManager.OpenScene(SCENE_PATH, OpenSceneMode.Single);
        Camera cam = EnsureCamera();

        // 4. Temukan atau Buat Canvas
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

        // 5. Bersihkan Objek Panel Phone lama jika ada
        Transform oldPhone = canvasGO.transform.Find("Panel_PhoneScreen");
        if (oldPhone != null) Object.DestroyImmediate(oldPhone.gameObject);

        // Bersihkan Objek Background / Screen lain sementara untuk rendering Phone Screen
        string[] oldPanels = { "Img_Background", "Img_TopGradient", "Img_BottomGradient", "Btn_Home", "Btn_Back",
                               "Img_Character", "Panel_TopStats", "Panel_Calendar", "Panel_ActivityGrid", "Btn_OpenPhone",
                               "Panel_DialogueBox", "Panel_PredictiveTooltip" };
        foreach (var name in oldPanels)
        {
            Transform t = canvasGO.transform.Find(name);
            if (t != null) Object.DestroyImmediate(t.gameObject);
        }

        // 6. Background Bedroom Fullscreen
        GameObject bgGO = CreateUIObject("Img_Background", canvasGO.transform);
        bgGO.transform.SetAsFirstSibling();
        StretchFull(bgGO.GetComponent<RectTransform>());
        Image imgBg = bgGO.AddComponent<Image>();
        imgBg.sprite = spBgBedroom;
        imgBg.color = new Color(0.85f, 0.88f, 0.95f, 1f); // slightly dimmed for focus
        imgBg.raycastTarget = false;

        // 7. Left Vignette (Rectangle 118: width 800)
        GameObject vigLeftGO = CreateUIObject("Img_VignetteLeft", canvasGO.transform);
        vigLeftGO.transform.SetSiblingIndex(1);
        RectTransform rtVigLeft = vigLeftGO.GetComponent<RectTransform>();
        rtVigLeft.anchorMin = new Vector2(0f, 0f);
        rtVigLeft.anchorMax = new Vector2(0f, 1f);
        rtVigLeft.pivot = new Vector2(0f, 0.5f);
        rtVigLeft.offsetMin = Vector2.zero;
        rtVigLeft.offsetMax = Vector2.zero;
        rtVigLeft.sizeDelta = new Vector2(800f, 0f);
        Image imgVigLeft = vigLeftGO.AddComponent<Image>();
        imgVigLeft.sprite = spVignetteSide;
        imgVigLeft.raycastTarget = false;

        // 8. Right Vignette (Rectangle 119: width 800, flipped horizontally)
        GameObject vigRightGO = CreateUIObject("Img_VignetteRight", canvasGO.transform);
        vigRightGO.transform.SetSiblingIndex(2);
        RectTransform rtVigRight = vigRightGO.GetComponent<RectTransform>();
        rtVigRight.anchorMin = new Vector2(1f, 0f);
        rtVigRight.anchorMax = new Vector2(1f, 1f);
        rtVigRight.pivot = new Vector2(1f, 0.5f);
        rtVigRight.offsetMin = Vector2.zero;
        rtVigRight.offsetMax = Vector2.zero;
        rtVigRight.sizeDelta = new Vector2(800f, 0f);
        rtVigRight.localScale = new Vector3(-1f, 1f, 1f);
        Image imgVigRight = vigRightGO.AddComponent<Image>();
        imgVigRight.sprite = spVignetteSide;
        imgVigRight.raycastTarget = false;

        // 9. Root Phone Screen Panel (Centered, 600 x 975)
        GameObject phoneRoot = CreateUIObject("Panel_PhoneScreen", canvasGO.transform);
        RectTransform rtPhone = phoneRoot.GetComponent<RectTransform>();
        rtPhone.anchorMin = new Vector2(0.5f, 0.5f);
        rtPhone.anchorMax = new Vector2(0.5f, 0.5f);
        rtPhone.pivot = new Vector2(0.5f, 0.5f);
        rtPhone.anchoredPosition = Vector2.zero;
        rtPhone.sizeDelta = new Vector2(600f, 975f);

        // Outer Phone Frame Body (Rectangle 106 & 107)
        Image imgPhoneFrame = phoneRoot.AddComponent<Image>();
        imgPhoneFrame.sprite = spFrameOuter;
        imgPhoneFrame.type = Image.Type.Sliced;
        imgPhoneFrame.raycastTarget = true;

        // Earpiece Speaker Grill (Rectangle 109: 125 x 10)
        GameObject speakerGO = CreateUIObject("Img_Speaker", phoneRoot.transform);
        RectTransform rtSpeaker = speakerGO.GetComponent<RectTransform>();
        rtSpeaker.anchorMin = new Vector2(0.5f, 1f);
        rtSpeaker.anchorMax = new Vector2(0.5f, 1f);
        rtSpeaker.pivot = new Vector2(0.5f, 1f);
        rtSpeaker.anchoredPosition = new Vector2(0f, -44f);
        rtSpeaker.sizeDelta = new Vector2(125f, 10f);
        Image imgSpeaker = speakerGO.AddComponent<Image>();
        imgSpeaker.color = new Color(0.965f, 0.973f, 1f, 0.95f);
        imgSpeaker.raycastTarget = false;

        // 10. Phone Screen Display Area (Rectangle 132: 554 x 840)
        GameObject screenDisplay = CreateUIObject("ScreenDisplay", phoneRoot.transform);
        RectTransform rtScreen = screenDisplay.GetComponent<RectTransform>();
        rtScreen.anchorMin = new Vector2(0.5f, 0.5f);
        rtScreen.anchorMax = new Vector2(0.5f, 0.5f);
        rtScreen.pivot = new Vector2(0.5f, 0.5f);
        rtScreen.anchoredPosition = new Vector2(0f, -24f);
        rtScreen.sizeDelta = new Vector2(554f, 840f);

        // Wallpaper
        GameObject wallGO = CreateUIObject("Img_Wallpaper", screenDisplay.transform);
        StretchFull(wallGO.GetComponent<RectTransform>());
        Image imgWall = wallGO.AddComponent<Image>();
        imgWall.sprite = spWallpaper;
        imgWall.raycastTarget = false;

        // 11. Status Bar (Top of Phone Screen)
        BuildStatusBar(screenDisplay.transform, spIconWifi, spIconSignal, spIconBattery, fontPoppins);

        // 12. Calendar Widget (KAlender / Rectangle 120: 450 x 180)
        BuildCalendarWidget(screenDisplay.transform, spWidgetBg, fontPoppins);

        // 13. Middle Row Widgets: Relation Widget & Digital Clock Widget
        BuildMiddleWidgets(screenDisplay.transform, spWidgetBg, spIconRelation, fontPoppins);

        // 14. Bottom App Dock (4 Apps: Call, Chat, Cam, Setting)
        BuildAppDock(screenDisplay.transform, spAppBg, spIconCall, spIconChat, spIconCam, spIconSetting);

        // 15. Simpan Scene & Tangkap Render 1080p
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, SCENE_PATH);
        AssetDatabase.SaveAssets();

        CapturePhoneScene1080p(canvas, cam, "test_render_phone_1080p.png");

        Debug.Log("<color=green>=== PEMBANGUNAN PHONE SCREEN SELESAI ===</color>");
    }

    // =========================================================================
    // SUB-COMPONENTS BUILDERS
    // =========================================================================

    private static void BuildStatusBar(Transform parent, Sprite spWifi, Sprite spSignal, Sprite spBattery, TMP_FontAsset font)
    {
        GameObject statusBar = CreateUIObject("StatusBar", parent);
        RectTransform rt = statusBar.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0f, -14f);
        rt.sizeDelta = new Vector2(0f, 32f);

        // Left Time (11.30)
        TextMeshProUGUI txtTime = CreateText("Txt_StatusTime", statusBar.transform, "11.30", 18, new Color(0.965f, 0.973f, 1f, 1f), true);
        if (font != null) txtTime.font = font;
        RectTransform rtTime = txtTime.rectTransform;
        rtTime.anchorMin = new Vector2(0f, 0.5f);
        rtTime.anchorMax = new Vector2(0f, 0.5f);
        rtTime.pivot = new Vector2(0f, 0.5f);
        rtTime.anchoredPosition = new Vector2(32f, 0f);
        rtTime.sizeDelta = new Vector2(80f, 26f);
        txtTime.alignment = TextAlignmentOptions.MidlineLeft;

        // Right Icons Container (Wifi, Signal, Battery)
        GameObject iconsCont = CreateUIObject("StatusIcons", statusBar.transform);
        RectTransform rtIcons = iconsCont.GetComponent<RectTransform>();
        rtIcons.anchorMin = new Vector2(1f, 0.5f);
        rtIcons.anchorMax = new Vector2(1f, 0.5f);
        rtIcons.pivot = new Vector2(1f, 0.5f);
        rtIcons.anchoredPosition = new Vector2(-30f, 0f);
        rtIcons.sizeDelta = new Vector2(110f, 24f);

        HorizontalLayoutGroup hlg = iconsCont.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 10;
        hlg.childAlignment = TextAnchor.MiddleRight;
        hlg.childControlWidth = false;
        hlg.childControlHeight = false;

        // Wifi
        GameObject wifiGO = CreateUIObject("Icon_Wifi", iconsCont.transform);
        wifiGO.GetComponent<RectTransform>().sizeDelta = new Vector2(20f, 16f);
        Image imgWifi = wifiGO.AddComponent<Image>();
        imgWifi.sprite = spWifi;
        imgWifi.preserveAspect = true;
        imgWifi.raycastTarget = false;

        // Signal
        GameObject sigGO = CreateUIObject("Icon_Signal", iconsCont.transform);
        sigGO.GetComponent<RectTransform>().sizeDelta = new Vector2(20f, 16f);
        Image imgSig = sigGO.AddComponent<Image>();
        imgSig.sprite = spSignal;
        imgSig.preserveAspect = true;
        imgSig.raycastTarget = false;

        // Battery
        GameObject batGO = CreateUIObject("Icon_Battery", iconsCont.transform);
        batGO.GetComponent<RectTransform>().sizeDelta = new Vector2(28f, 14f);
        Image imgBat = batGO.AddComponent<Image>();
        imgBat.sprite = spBattery;
        imgBat.preserveAspect = true;
        imgBat.raycastTarget = false;
    }

    private static void BuildCalendarWidget(Transform parent, Sprite spWidgetBg, TMP_FontAsset font)
    {
        GameObject root = CreateUIObject("Widget_Calendar", parent);
        RectTransform rt = root.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 1f);
        rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0f, -96f);
        rt.sizeDelta = new Vector2(450f, 180f);

        Image imgBg = root.AddComponent<Image>();
        imgBg.sprite = spWidgetBg;
        imgBg.type = Image.Type.Sliced;
        imgBg.raycastTarget = true;

        // Left Section: Date Badge (Sen 21)
        GameObject leftSec = CreateUIObject("DateBadge", root.transform);
        RectTransform rtLeft = leftSec.GetComponent<RectTransform>();
        rtLeft.anchorMin = new Vector2(0f, 0.5f);
        rtLeft.anchorMax = new Vector2(0f, 0.5f);
        rtLeft.pivot = new Vector2(0f, 0.5f);
        rtLeft.anchoredPosition = new Vector2(34f, 0f);
        rtLeft.sizeDelta = new Vector2(130f, 140f);

        VerticalLayoutGroup vlg = leftSec.AddComponent<VerticalLayoutGroup>();
        vlg.childAlignment = TextAnchor.MiddleCenter;
        vlg.spacing = 2;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;

        TextMeshProUGUI txtDayName = CreateText("Txt_DayName", leftSec.transform, "Sen", 34, new Color(0.965f, 0.973f, 1f, 1f), true);
        if (font != null) txtDayName.font = font;
        txtDayName.alignment = TextAlignmentOptions.Center;

        TextMeshProUGUI txtDayNum = CreateText("Txt_DayNum", leftSec.transform, "21", 52, new Color(0.965f, 0.973f, 1f, 1f), true);
        if (font != null) txtDayNum.font = font;
        txtDayNum.alignment = TextAlignmentOptions.Center;

        // Right Section: Calendar Grid
        GameObject rightSec = CreateUIObject("CalendarGridSection", root.transform);
        RectTransform rtRight = rightSec.GetComponent<RectTransform>();
        rtRight.anchorMin = new Vector2(1f, 0.5f);
        rtRight.anchorMax = new Vector2(1f, 0.5f);
        rtRight.pivot = new Vector2(1f, 0.5f);
        rtRight.anchoredPosition = new Vector2(-26f, 0f);
        rtRight.sizeDelta = new Vector2(240f, 150f);

        // Month Title (September)
        TextMeshProUGUI txtMonth = CreateText("Txt_Month", rightSec.transform, "September", 18, new Color(0.965f, 0.973f, 1f, 1f), true);
        if (font != null) txtMonth.font = font;
        RectTransform rtMonth = txtMonth.rectTransform;
        rtMonth.anchorMin = new Vector2(0f, 1f);
        rtMonth.anchorMax = new Vector2(1f, 1f);
        rtMonth.pivot = new Vector2(1f, 1f);
        rtMonth.anchoredPosition = new Vector2(0f, -8f);
        rtMonth.sizeDelta = new Vector2(0f, 24f);
        txtMonth.alignment = TextAlignmentOptions.MidlineRight;

        // Day of Week Header row
        TextMeshProUGUI txtDaysHeader = CreateText("Txt_DaysHeader", rightSec.transform, "Min  Sen  Sel  Rab  Kam  Jum  Sab", 10, new Color(0.965f, 0.973f, 1f, 0.85f), true);
        if (font != null) txtDaysHeader.font = font;
        RectTransform rtHeader = txtDaysHeader.rectTransform;
        rtHeader.anchorMin = new Vector2(0f, 1f);
        rtHeader.anchorMax = new Vector2(1f, 1f);
        rtHeader.pivot = new Vector2(0.5f, 1f);
        rtHeader.anchoredPosition = new Vector2(0f, -38f);
        rtHeader.sizeDelta = new Vector2(0f, 18f);
        txtDaysHeader.alignment = TextAlignmentOptions.Center;

        // Matrix Dot Grid Container (5 rows x 7 cols)
        GameObject gridGO = CreateUIObject("DotsGrid", rightSec.transform);
        RectTransform rtGrid = gridGO.GetComponent<RectTransform>();
        rtGrid.anchorMin = new Vector2(0.5f, 0f);
        rtGrid.anchorMax = new Vector2(0.5f, 0f);
        rtGrid.pivot = new Vector2(0.5f, 0f);
        rtGrid.anchoredPosition = new Vector2(0f, 10f);
        rtGrid.sizeDelta = new Vector2(230f, 78f);

        GridLayoutGroup glg = gridGO.AddComponent<GridLayoutGroup>();
        glg.cellSize = new Vector2(16f, 12f);
        glg.spacing = new Vector2(16f, 3f);
        glg.childAlignment = TextAnchor.MiddleCenter;

        // 35 dots (day 21 highlighted on row 4 col 2)
        int activeDotIndex = 22; // Sen 21
        for (int i = 0; i < 35; i++)
        {
            GameObject dot = CreateUIObject($"Dot_{i}", gridGO.transform);
            Image imgDot = dot.AddComponent<Image>();

            if (i == activeDotIndex)
            {
                // Active highlighted day
                imgDot.color = new Color(0.12f, 0.14f, 0.25f, 1f); // #1E2440
                Outline outl = dot.AddComponent<Outline>();
                outl.effectColor = new Color(0.965f, 0.973f, 1f, 1f); // #F6F8FF
                outl.effectDistance = new Vector2(2f, 2f);
            }
            else
            {
                imgDot.color = new Color(0.85f, 0.85f, 0.85f, 0.65f); // #D9D9D9
            }
            imgDot.raycastTarget = false;
        }
    }

    private static void BuildMiddleWidgets(Transform parent, Sprite spWidgetBg, Sprite spIconRelation, TMP_FontAsset font)
    {
        // 1. Relation Widget (Left: 214 x 180)
        GameObject relWidget = CreateUIObject("Widget_Relation", parent);
        RectTransform rtRel = relWidget.GetComponent<RectTransform>();
        rtRel.anchorMin = new Vector2(0.5f, 1f);
        rtRel.anchorMax = new Vector2(0.5f, 1f);
        rtRel.pivot = new Vector2(0.5f, 1f);
        rtRel.anchoredPosition = new Vector2(-118f, -310f);
        rtRel.sizeDelta = new Vector2(214f, 180f);

        Image imgRelBg = relWidget.AddComponent<Image>();
        imgRelBg.sprite = spWidgetBg;
        imgRelBg.type = Image.Type.Sliced;
        Button btnRel = relWidget.AddComponent<Button>();
        btnRel.targetGraphic = imgRelBg;

        // Relation Icon
        GameObject relIconGO = CreateUIObject("Icon_Relation", relWidget.transform);
        RectTransform rtRelIcon = relIconGO.GetComponent<RectTransform>();
        rtRelIcon.anchorMin = new Vector2(0.5f, 0.5f);
        rtRelIcon.anchorMax = new Vector2(0.5f, 0.5f);
        rtRelIcon.pivot = new Vector2(0.5f, 0.5f);
        rtRelIcon.anchoredPosition = new Vector2(0f, 18f);
        rtRelIcon.sizeDelta = new Vector2(84f, 84f);
        Image imgRelIcon = relIconGO.AddComponent<Image>();
        imgRelIcon.sprite = spIconRelation;
        imgRelIcon.preserveAspect = true;
        imgRelIcon.raycastTarget = false;

        // Relation Label
        TextMeshProUGUI txtRel = CreateText("Txt_RelationLabel", relWidget.transform, "Relation", 20, new Color(0.965f, 0.973f, 1f, 1f), true);
        if (font != null) txtRel.font = font;
        RectTransform rtTxtRel = txtRel.rectTransform;
        rtTxtRel.anchorMin = new Vector2(0f, 0f);
        rtTxtRel.anchorMax = new Vector2(1f, 0f);
        rtTxtRel.pivot = new Vector2(0.5f, 0f);
        rtTxtRel.anchoredPosition = new Vector2(0f, 16f);
        rtTxtRel.sizeDelta = new Vector2(0f, 26f);
        txtRel.alignment = TextAlignmentOptions.Center;

        // 2. Digital Clock Widget (Right: 214 x 180)
        GameObject clockWidget = CreateUIObject("Widget_Clock", parent);
        RectTransform rtClock = clockWidget.GetComponent<RectTransform>();
        rtClock.anchorMin = new Vector2(0.5f, 1f);
        rtClock.anchorMax = new Vector2(0.5f, 1f);
        rtClock.pivot = new Vector2(0.5f, 1f);
        rtClock.anchoredPosition = new Vector2(118f, -310f);
        rtClock.sizeDelta = new Vector2(214f, 180f);

        VerticalLayoutGroup vlgClock = clockWidget.AddComponent<VerticalLayoutGroup>();
        vlgClock.childAlignment = TextAnchor.MiddleCenter;
        vlgClock.spacing = 2;
        vlgClock.childControlWidth = true;
        vlgClock.childControlHeight = false;

        TextMeshProUGUI txtBigClock = CreateText("Txt_BigClock", clockWidget.transform, "11.30", 58, new Color(0.965f, 0.973f, 1f, 1f), true);
        if (font != null) txtBigClock.font = font;
        txtBigClock.alignment = TextAlignmentOptions.Center;

        TextMeshProUGUI txtTz = CreateText("Txt_TimeZone", clockWidget.transform, "WIB", 20, new Color(0.965f, 0.973f, 1f, 0.9f), true);
        if (font != null) txtTz.font = font;
        txtTz.alignment = TextAlignmentOptions.Center;
    }

    private static void BuildAppDock(
        Transform parent,
        Sprite spAppBg,
        Sprite spIconCall,
        Sprite spIconChat,
        Sprite spIconCam,
        Sprite spIconSetting)
    {
        GameObject dock = CreateUIObject("AppDock", parent);
        RectTransform rtDock = dock.GetComponent<RectTransform>();
        rtDock.anchorMin = new Vector2(0.5f, 0f);
        rtDock.anchorMax = new Vector2(0.5f, 0f);
        rtDock.pivot = new Vector2(0.5f, 0f);
        rtDock.anchoredPosition = new Vector2(0f, 68f);
        rtDock.sizeDelta = new Vector2(480f, 110f);

        HorizontalLayoutGroup hlg = dock.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 16;
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.childControlWidth = false;
        hlg.childControlHeight = false;

        BuildAppButton(dock.transform, "Btn_AppCall", spAppBg, spIconCall);
        BuildAppButton(dock.transform, "Btn_AppChat", spAppBg, spIconChat);
        BuildAppButton(dock.transform, "Btn_AppCam", spAppBg, spIconCam);
        BuildAppButton(dock.transform, "Btn_AppSetting", spAppBg, spIconSetting);
    }

    private static Button BuildAppButton(Transform parent, string name, Sprite spBg, Sprite spIcon)
    {
        GameObject btnGO = CreateUIObject(name, parent);
        RectTransform rt = btnGO.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(100f, 98f);

        Image imgBg = btnGO.AddComponent<Image>();
        imgBg.sprite = spBg;
        imgBg.type = Image.Type.Sliced;

        Button btn = btnGO.AddComponent<Button>();
        btn.targetGraphic = imgBg;

        // Icon
        GameObject iconGO = CreateUIObject("Icon", btnGO.transform);
        RectTransform rtIcon = iconGO.GetComponent<RectTransform>();
        rtIcon.anchorMin = new Vector2(0.5f, 0.5f);
        rtIcon.anchorMax = new Vector2(0.5f, 0.5f);
        rtIcon.pivot = new Vector2(0.5f, 0.5f);
        rtIcon.anchoredPosition = Vector2.zero;
        rtIcon.sizeDelta = new Vector2(48f, 48f);

        Image imgIcon = iconGO.AddComponent<Image>();
        imgIcon.sprite = spIcon;
        imgIcon.preserveAspect = true;
        imgIcon.raycastTarget = false;

        return btn;
    }

    // =========================================================================
    // UTILITIES & TEXTURE CONFIG
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

    private static void ConfigurePhoneTextures()
    {
        ConfigureSpriteTexture(BG_BEDROOM_PATH, Vector4.zero);
        ConfigureSpriteTexture(SPRITE_FRAME_OUTER, new Vector4(50, 50, 50, 50));
        ConfigureSpriteTexture(SPRITE_WALLPAPER, Vector4.zero);
        ConfigureSpriteTexture(SPRITE_WIDGET_BG, new Vector4(24, 24, 24, 24));
        ConfigureSpriteTexture(SPRITE_APP_BG, new Vector4(20, 20, 20, 20));
        ConfigureSpriteTexture(SPRITE_VIGNETTE_SIDE, Vector4.zero);

        ConfigureSpriteTexture(ICON_CALL, Vector4.zero);
        ConfigureSpriteTexture(ICON_CHAT, Vector4.zero);
        ConfigureSpriteTexture(ICON_CAM, Vector4.zero);
        ConfigureSpriteTexture(ICON_SETTING, Vector4.zero);
        ConfigureSpriteTexture(ICON_RELATION, Vector4.zero);

        ConfigureSpriteTexture(ICON_WIFI, Vector4.zero);
        ConfigureSpriteTexture(ICON_SIGNAL, Vector4.zero);
        ConfigureSpriteTexture(ICON_BATTERY, Vector4.zero);
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

    public static void CapturePhoneScene1080p(Canvas canvas, Camera cam, string filename)
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

        string outPath = Path.Combine("Assets/Textures/UI_Phone", filename);
        File.WriteAllBytes(outPath, tex.EncodeToPNG());

        cam.targetTexture = prevTarget;
        RenderTexture.active = prevActive;
        Object.DestroyImmediate(rt);
        Object.DestroyImmediate(tex);

        canvas.renderMode = prevMode;
        canvas.worldCamera = prevCam;

        AssetDatabase.ImportAsset(outPath);
        Debug.Log($"<color=yellow>[Render Phone 1080p]</color> Snapshot berhasil disimpan ke: {outPath}");
    }
}
#endif
