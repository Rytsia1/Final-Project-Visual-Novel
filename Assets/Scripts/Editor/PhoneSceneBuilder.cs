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

    // Calendar App Assets
    private const string SPRITE_CAL_BODY_BG = "Assets/Textures/UI_Phone/cal_body_bg.png";
    private const string SPRITE_CAL_HEADER_ARCH = "Assets/Textures/UI_Phone/cal_header_arch.png";
    private const string SPRITE_CAL_BOTTOM_NAV = "Assets/Textures/UI_Phone/cal_bottom_nav.png";
    private const string SPRITE_CAL_ACT_BAR = "Assets/Textures/UI_Phone/cal_activity_bar.png";
    private const string SPRITE_CIRCLE = "Assets/Textures/UI_Phone/circle_64.png";

    // Chat Contacts App Assets
    private const string SPRITE_CHAT_BG_GRADIENT = "Assets/Textures/UI_Phone/chat_bg_gradient.png";
    private const string SPRITE_SEARCH_BAR_PILL = "Assets/Textures/UI_Phone/search_bar_pill.png";
    private const string SPRITE_CHAT_CARD_BG = "Assets/Textures/UI_Phone/chat_card_bg.png";
    private const string ICON_SEARCH = "Assets/Textures/UI_Phone/icon_search.png";
    private const string SPRITE_AVATAR_USER = "Assets/Textures/UI_Phone/avatar_user_silhouette.png";

    // Chat Room App Assets (No Keypad)
    private const string SPRITE_CHAT_ROOM_BG = "Assets/Textures/UI_Phone/chat_room_bg.png";
    private const string SPRITE_CHAT_HEADER_BAR = "Assets/Textures/UI_Phone/chat_header_bar.png";
    private const string SPRITE_CHAT_BOTTOM_BAR = "Assets/Textures/UI_Phone/chat_bottom_bar.png";
    private const string SPRITE_BUBBLE_INCOMING = "Assets/Textures/UI_Phone/bubble_incoming.png";
    private const string SPRITE_BUBBLE_OUTGOING = "Assets/Textures/UI_Phone/bubble_outgoing.png";
    private const string SPRITE_BUBBLE_TAIL_L = "Assets/Textures/UI_Phone/bubble_tail_left.png";
    private const string SPRITE_BUBBLE_TAIL_R = "Assets/Textures/UI_Phone/bubble_tail_right.png";
    private const string SPRITE_CHAT_STICKER = "Assets/Textures/UI_Phone/chat_sticker_meme.png";
    private const string ICON_PLUS = "Assets/Textures/UI_Phone/icon_plus.png";
    private const string ICON_SEND_PLANE = "Assets/Textures/UI_Phone/icon_send_plane.png";

    private const string ICON_NAV_BACK = "Assets/Textures/UI_Phone/icon_nav_back.png";
    private const string ICON_NAV_HOME = "Assets/Textures/UI_Phone/icon_nav_home.png";
    private const string ICON_NAV_RECENTS = "Assets/Textures/UI_Phone/icon_nav_recents.png";
    private const string ICON_CHEV_LEFT = "Assets/Textures/UI_Phone/chevron_left.png";
    private const string ICON_CHEV_RIGHT = "Assets/Textures/UI_Phone/chevron_right.png";

    // Phone Home App Icons
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

    public enum PhoneScreenMode
    {
        Home,
        Calendar,
        ChatContacts,
        ChatRoom
    }

    [MenuItem("Game Debug/Build Phone Home Screen")]
    public static void BuildPhoneHomeMenu()
    {
        BuildPhoneScreen(PhoneScreenMode.Home);
    }

    [MenuItem("Game Debug/Build Phone Calendar Screen")]
    public static void BuildPhoneCalendarMenu()
    {
        BuildPhoneScreen(PhoneScreenMode.Calendar);
    }

    [MenuItem("Game Debug/Build Phone Chat Contacts Screen")]
    public static void BuildPhoneChatMenu()
    {
        BuildPhoneScreen(PhoneScreenMode.ChatContacts);
    }

    [MenuItem("Game Debug/Build Phone Chat Room Screen")]
    public static void BuildPhoneChatRoomMenu()
    {
        BuildPhoneScreen(PhoneScreenMode.ChatRoom);
    }

    public static void BuildPhoneScreen(bool showCalendarApp)
    {
        BuildPhoneScreen(showCalendarApp ? PhoneScreenMode.Calendar : PhoneScreenMode.Home);
    }

    public static void BuildPhoneScreen(PhoneScreenMode mode = PhoneScreenMode.Home)
    {
        string modeTitle = (mode == PhoneScreenMode.Calendar) ? "PHONE SCREEN - CALENDAR APP" :
                           (mode == PhoneScreenMode.ChatContacts ? "PHONE SCREEN - KONTAK CHATTING" :
                           (mode == PhoneScreenMode.ChatRoom ? "PHONE SCREEN - CHAT ROOM" : "PHONE SCREEN - HOME"));
        Debug.Log($"<color=cyan>=== MEMULAI PEMBANGUNAN {modeTitle} (1080P) ===</color>");

        // 1. Konfigurasi semua sprite UI Phone
        ConfigurePhoneTextures();

        // 2. Load Fonts & Sprites
        TMP_FontAsset fontPoppins = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FONT_POPPINS_SDF);

        Sprite spBgBedroom = AssetDatabase.LoadAssetAtPath<Sprite>(BG_BEDROOM_PATH);
        Sprite spFrameOuter = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITE_FRAME_OUTER);
        Sprite spWallpaper = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITE_WALLPAPER);
        Sprite spWidgetBg = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITE_WIDGET_BG);
        Sprite spAppBg = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITE_APP_BG);
        Sprite spVignetteSide = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITE_VIGNETTE_SIDE);

        // Calendar App Sprites
        Sprite spCalBodyBg = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITE_CAL_BODY_BG);
        Sprite spCalHeaderArch = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITE_CAL_HEADER_ARCH);
        Sprite spCalBottomNav = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITE_CAL_BOTTOM_NAV);
        Sprite spCalActBar = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITE_CAL_ACT_BAR);
        Sprite spCircle = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITE_CIRCLE);
        Sprite spNavBack = AssetDatabase.LoadAssetAtPath<Sprite>(ICON_NAV_BACK);
        Sprite spNavHome = AssetDatabase.LoadAssetAtPath<Sprite>(ICON_NAV_HOME);
        Sprite spNavRecents = AssetDatabase.LoadAssetAtPath<Sprite>(ICON_NAV_RECENTS);
        Sprite spChevLeft = AssetDatabase.LoadAssetAtPath<Sprite>(ICON_CHEV_LEFT);
        Sprite spChevRight = AssetDatabase.LoadAssetAtPath<Sprite>(ICON_CHEV_RIGHT);

        // Chat Contacts App Sprites
        Sprite spChatBg = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITE_CHAT_BG_GRADIENT);
        Sprite spSearchPill = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITE_SEARCH_BAR_PILL);
        Sprite spChatCardBg = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITE_CHAT_CARD_BG);
        Sprite spIconSearch = AssetDatabase.LoadAssetAtPath<Sprite>(ICON_SEARCH);
        Sprite spAvatarUser = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITE_AVATAR_USER);

        // Chat Room App Sprites
        TMP_FontAsset fontPatrick = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FONT_PATRICK_SDF);
        Sprite spChatRoomBg = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITE_CHAT_ROOM_BG);
        Sprite spChatHeaderBar = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITE_CHAT_HEADER_BAR);
        Sprite spChatBottomBar = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITE_CHAT_BOTTOM_BAR);
        Sprite spBubbleIncoming = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITE_BUBBLE_INCOMING);
        Sprite spBubbleOutgoing = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITE_BUBBLE_OUTGOING);
        Sprite spBubbleTailL = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITE_BUBBLE_TAIL_L);
        Sprite spBubbleTailR = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITE_BUBBLE_TAIL_R);
        Sprite spChatSticker = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITE_CHAT_STICKER);
        Sprite spIconPlus = AssetDatabase.LoadAssetAtPath<Sprite>(ICON_PLUS);
        Sprite spIconSendPlane = AssetDatabase.LoadAssetAtPath<Sprite>(ICON_SEND_PLANE);

        // Home App Sprites
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

        // 5. Bersihkan Objek UI Lama
        string[] oldPanels = { "Img_Background", "Img_TopGradient", "Img_BottomGradient", "Btn_Home", "Btn_Back",
                               "Img_Character", "Panel_TopStats", "Panel_Calendar", "Panel_ActivityGrid", "Btn_OpenPhone",
                               "Panel_DialogueBox", "Panel_PredictiveTooltip", "Panel_PhoneScreen",
                               "Img_VignetteLeft", "Img_VignetteRight" };
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
        imgBg.color = new Color(0.85f, 0.88f, 0.95f, 1f);
        imgBg.raycastTarget = false;

        // 7. Left Vignette (Rectangle 118: width 800)
        GameObject vigLeftGO = CreateUIObject("Img_VignetteLeft", canvasGO.transform);
        vigLeftGO.transform.SetSiblingIndex(1);
        RectTransform rtVigLeft = vigLeftGO.GetComponent<RectTransform>();
        rtVigLeft.anchorMin = new Vector2(0f, 0f);
        rtVigLeft.anchorMax = new Vector2(0f, 1f);
        rtVigLeft.pivot = new Vector2(0f, 0.5f);
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

        Image imgPhoneFrame = phoneRoot.AddComponent<Image>();
        imgPhoneFrame.sprite = spFrameOuter;
        imgPhoneFrame.type = Image.Type.Sliced;
        imgPhoneFrame.raycastTarget = true;

        // Earpiece Speaker Grill
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

        // 10. Phone Screen Display Area (554 x 840)
        GameObject screenDisplay = CreateUIObject("ScreenDisplay", phoneRoot.transform);
        RectTransform rtScreen = screenDisplay.GetComponent<RectTransform>();
        rtScreen.anchorMin = new Vector2(0.5f, 0.5f);
        rtScreen.anchorMax = new Vector2(0.5f, 0.5f);
        rtScreen.pivot = new Vector2(0.5f, 0.5f);
        rtScreen.anchoredPosition = new Vector2(0f, -24f);
        rtScreen.sizeDelta = new Vector2(554f, 840f);

        if (mode == PhoneScreenMode.Calendar)
        {
            // =========================================================
            // CALENDAR APP SCREEN
            // =========================================================
            // Background (Light gradient white-periwinkle)
            GameObject calBgGO = CreateUIObject("Img_CalBodyBg", screenDisplay.transform);
            StretchFull(calBgGO.GetComponent<RectTransform>());
            Image imgCalBg = calBgGO.AddComponent<Image>();
            imgCalBg.sprite = spCalBodyBg;
            imgCalBg.raycastTarget = false;

            // Arched Header Card (Top: w=554, h=130)
            BuildCalendarHeader(screenDisplay.transform, spCalHeaderArch, spChevLeft, spChevRight, fontPoppins);

            // Status Bar on top of header
            BuildStatusBar(screenDisplay.transform, spIconWifi, spIconSignal, spIconBattery, fontPoppins);

            // Calendar Month View (Days of Week + 5x7 Date Grid)
            BuildCalendarMonthView(screenDisplay.transform, spCircle, fontPoppins);

            // Activity Header Bar (Rectangle 202: 2 AKTIFITAS)
            BuildActivitySection(screenDisplay.transform, spCalActBar, spCircle, fontPoppins);

            // Android Bottom Navigation Bar
            BuildCalendarBottomNav(screenDisplay.transform, spCalBottomNav, spNavBack, spNavHome, spNavRecents);
        }
        else if (mode == PhoneScreenMode.ChatContacts)
        {
            // =========================================================
            // CHAT CONTACTS APP SCREEN (WeTalk)
            // =========================================================
            BuildChatContactsScreen(
                screenDisplay.transform,
                spChatBg,
                spSearchPill,
                spChatCardBg,
                spIconSearch,
                spAvatarUser,
                spCircle,
                spCalBottomNav,
                spNavBack,
                spNavHome,
                spNavRecents,
                spIconWifi,
                spIconSignal,
                spIconBattery,
                fontPoppins);
        }
        else if (mode == PhoneScreenMode.ChatRoom)
        {
            // =========================================================
            // CHAT ROOM APP SCREEN (No Keypad)
            // =========================================================
            BuildChatRoomScreen(
                screenDisplay.transform,
                spChatRoomBg,
                spChatHeaderBar,
                spChatBottomBar,
                spBubbleIncoming,
                spBubbleOutgoing,
                spBubbleTailL,
                spBubbleTailR,
                spChatSticker,
                spIconPlus,
                spIconSendPlane,
                spChevLeft,
                spAvatarUser,
                spCircle,
                spSearchPill,
                spIconWifi,
                spIconSignal,
                spIconBattery,
                fontPoppins,
                fontPatrick);
        }
        else
        {
            // =========================================================
            // HOME SCREEN
            // =========================================================
            // Wallpaper
            GameObject wallGO = CreateUIObject("Img_Wallpaper", screenDisplay.transform);
            StretchFull(wallGO.GetComponent<RectTransform>());
            Image imgWall = wallGO.AddComponent<Image>();
            imgWall.sprite = spWallpaper;
            imgWall.raycastTarget = false;

            // Status Bar
            BuildStatusBar(screenDisplay.transform, spIconWifi, spIconSignal, spIconBattery, fontPoppins);

            // Calendar Widget (Sen 21)
            BuildCalendarWidget(screenDisplay.transform, spWidgetBg, fontPoppins);

            // Middle Row Widgets (Relation & Clock)
            BuildMiddleWidgets(screenDisplay.transform, spWidgetBg, spIconRelation, fontPoppins);

            // Bottom App Dock
            BuildAppDock(screenDisplay.transform, spAppBg, spIconCall, spIconChat, spIconCam, spIconSetting);
        }

        // 11. Simpan Scene & Tangkap Snapshot
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, SCENE_PATH);
        AssetDatabase.SaveAssets();

        string renderFile = "test_render_phone_1080p.png";
        if (mode == PhoneScreenMode.Calendar) renderFile = "test_render_phone_calendar_1080p.png";
        else if (mode == PhoneScreenMode.ChatContacts) renderFile = "test_render_phone_chat_1080p.png";
        else if (mode == PhoneScreenMode.ChatRoom) renderFile = "test_render_phone_chatroom_1080p.png";
        CapturePhoneScene1080p(canvas, cam, renderFile);

        Debug.Log($"<color=green>=== PEMBANGUNAN {modeTitle} SELESAI ===</color>");
    }

    // =========================================================================
    // CALENDAR APP SUB-BUILDERS
    // =========================================================================

    private static void BuildCalendarHeader(Transform parent, Sprite spArch, Sprite spLeft, Sprite spRight, TMP_FontAsset font)
    {
        GameObject header = CreateUIObject("Header_CalendarArch", parent);
        RectTransform rt = header.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 1f);
        rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(554f, 130f);

        Image imgArch = header.AddComponent<Image>();
        imgArch.sprite = spArch;
        imgArch.raycastTarget = false;

        // Navigation Row (Chevron Left, Month Title, Chevron Right)
        GameObject navRow = CreateUIObject("NavRow", header.transform);
        RectTransform rtNav = navRow.GetComponent<RectTransform>();
        rtNav.anchorMin = new Vector2(0.5f, 0f);
        rtNav.anchorMax = new Vector2(0.5f, 0f);
        rtNav.pivot = new Vector2(0.5f, 0f);
        rtNav.anchoredPosition = new Vector2(0f, 26f);
        rtNav.sizeDelta = new Vector2(490f, 36f);

        // Chevron Left
        GameObject btnLeftGO = CreateUIObject("Btn_PrevMonth", navRow.transform);
        RectTransform rtLeft = btnLeftGO.GetComponent<RectTransform>();
        rtLeft.anchorMin = new Vector2(0f, 0.5f);
        rtLeft.anchorMax = new Vector2(0f, 0.5f);
        rtLeft.pivot = new Vector2(0f, 0.5f);
        rtLeft.anchoredPosition = new Vector2(30f, 0f);
        rtLeft.sizeDelta = new Vector2(20f, 28f);
        Image imgL = btnLeftGO.AddComponent<Image>();
        imgL.sprite = spLeft;
        imgL.preserveAspect = true;
        btnLeftGO.AddComponent<Button>();

        // Month Title: SEPTEMBER 2026
        TextMeshProUGUI txtTitle = CreateText("Txt_MonthYear", navRow.transform, "SEPTEMBER 2026", 21, new Color(0.965f, 0.973f, 1f, 1f), true);
        if (font != null) txtTitle.font = font;
        txtTitle.characterSpacing = 16f;
        RectTransform rtTitle = txtTitle.rectTransform;
        rtTitle.anchorMin = new Vector2(0.5f, 0.5f);
        rtTitle.anchorMax = new Vector2(0.5f, 0.5f);
        rtTitle.pivot = new Vector2(0.5f, 0.5f);
        rtTitle.anchoredPosition = Vector2.zero;
        rtTitle.sizeDelta = new Vector2(320f, 32f);
        txtTitle.alignment = TextAlignmentOptions.Center;

        // Chevron Right
        GameObject btnRightGO = CreateUIObject("Btn_NextMonth", navRow.transform);
        RectTransform rtRight = btnRightGO.GetComponent<RectTransform>();
        rtRight.anchorMin = new Vector2(1f, 0.5f);
        rtRight.anchorMax = new Vector2(1f, 0.5f);
        rtRight.pivot = new Vector2(1f, 0.5f);
        rtRight.anchoredPosition = new Vector2(-30f, 0f);
        rtRight.sizeDelta = new Vector2(20f, 28f);
        Image imgR = btnRightGO.AddComponent<Image>();
        imgR.sprite = spRight;
        imgR.preserveAspect = true;
        btnRightGO.AddComponent<Button>();
    }

    private static void BuildCalendarMonthView(Transform parent, Sprite spCircle, TMP_FontAsset font)
    {
        // 1. Days of Week Header (Min Sen Sel Rab Kam Jum Sab)
        GameObject dowGO = CreateUIObject("DaysOfWeekHeader", parent);
        RectTransform rtDow = dowGO.GetComponent<RectTransform>();
        rtDow.anchorMin = new Vector2(0.5f, 1f);
        rtDow.anchorMax = new Vector2(0.5f, 1f);
        rtDow.pivot = new Vector2(0.5f, 1f);
        rtDow.anchoredPosition = new Vector2(0f, -145f);
        rtDow.sizeDelta = new Vector2(490f, 32f);

        HorizontalLayoutGroup hlgDow = dowGO.AddComponent<HorizontalLayoutGroup>();
        hlgDow.spacing = 0;
        hlgDow.childAlignment = TextAnchor.MiddleCenter;
        hlgDow.childControlWidth = true;
        hlgDow.childControlHeight = true;
        hlgDow.childForceExpandWidth = true;

        string[] dows = { "Min", "Sen", "Sel", "Rab", "Kam", "Jum", "Sab" };
        for (int i = 0; i < 7; i++)
        {
            Color c = (i == 0) ? new Color(0.85f, 0.42f, 0f, 1f) : new Color(0.12f, 0.14f, 0.25f, 1f); // Orange for Sunday, Navy for others
            TextMeshProUGUI tmp = CreateText($"Txt_{dows[i]}", dowGO.transform, dows[i], 17, c, true);
            if (font != null) tmp.font = font;
            tmp.alignment = TextAlignmentOptions.Center;
        }

        // 2. Dates Matrix Grid (September 2026: starts on Tuesday / Sel)
        // Rows: 5 weeks
        GameObject gridGO = CreateUIObject("CalendarDatesGrid", parent);
        RectTransform rtGrid = gridGO.GetComponent<RectTransform>();
        rtGrid.anchorMin = new Vector2(0.5f, 1f);
        rtGrid.anchorMax = new Vector2(0.5f, 1f);
        rtGrid.pivot = new Vector2(0.5f, 1f);
        rtGrid.anchoredPosition = new Vector2(0f, -188f);
        rtGrid.sizeDelta = new Vector2(490f, 240f);

        GridLayoutGroup glg = gridGO.AddComponent<GridLayoutGroup>();
        glg.cellSize = new Vector2(70f, 44f);
        glg.spacing = Vector2.zero;
        glg.childAlignment = TextAnchor.MiddleCenter;

        // Day numbers mapping for September 2026:
        string[] days = {
            "", "", "1", "2", "3", "4", "5",
            "6", "7", "8", "9", "10", "11", "12",
            "13", "14", "15", "16", "17", "18", "19",
            "20", "21", "22", "23", "24", "25", "26",
            "27", "28", "29", "30", "", "", ""
        };

        for (int i = 0; i < 35; i++)
        {
            string numStr = days[i];
            int col = i % 7;
            bool isSunday = (col == 0);
            bool isActiveDay = (numStr == "21");

            GameObject cellGO = CreateUIObject($"Cell_{i}_{numStr}", gridGO.transform);

            if (string.IsNullOrEmpty(numStr)) continue;

            if (isActiveDay)
            {
                // Active Selected Circle Badge (Ellipse 52: #1E266D)
                GameObject badge = CreateUIObject("ActiveBadge", cellGO.transform);
                RectTransform rtBadge = badge.GetComponent<RectTransform>();
                rtBadge.anchorMin = new Vector2(0.5f, 0.5f);
                rtBadge.anchorMax = new Vector2(0.5f, 0.5f);
                rtBadge.pivot = new Vector2(0.5f, 0.5f);
                rtBadge.anchoredPosition = new Vector2(0f, 3f);
                rtBadge.sizeDelta = new Vector2(36f, 36f);

                Image imgBadge = badge.AddComponent<Image>();
                imgBadge.sprite = spCircle;
                imgBadge.color = new Color(0.12f, 0.15f, 0.43f, 1f); // #1E266D

                TextMeshProUGUI txtNum = CreateText("Txt_Num", badge.transform, numStr, 17, new Color(0.965f, 0.973f, 1f, 1f), true);
                if (font != null) txtNum.font = font;
                StretchFull(txtNum.rectTransform);
                txtNum.alignment = TextAlignmentOptions.Center;

                // Event Indicator dots (two small orange dots below)
                GameObject dotsHolder = CreateUIObject("EventDots", cellGO.transform);
                RectTransform rtDots = dotsHolder.GetComponent<RectTransform>();
                rtDots.anchorMin = new Vector2(0.5f, 0f);
                rtDots.anchorMax = new Vector2(0.5f, 0f);
                rtDots.pivot = new Vector2(0.5f, 0f);
                rtDots.anchoredPosition = new Vector2(0f, 0f);
                rtDots.sizeDelta = new Vector2(14f, 5f);

                HorizontalLayoutGroup hlgDots = dotsHolder.AddComponent<HorizontalLayoutGroup>();
                hlgDots.spacing = 3;
                hlgDots.childAlignment = TextAnchor.MiddleCenter;

                for (int d = 0; d < 2; d++)
                {
                    GameObject dot = CreateUIObject($"Dot_{d}", dotsHolder.transform);
                    dot.GetComponent<RectTransform>().sizeDelta = new Vector2(4f, 4f);
                    Image id = dot.AddComponent<Image>();
                    id.sprite = spCircle;
                    id.color = new Color(0.85f, 0.42f, 0f, 1f); // #D96B00
                }
            }
            else
            {
                Color numColor = isSunday ? new Color(0.85f, 0.42f, 0f, 1f) : new Color(0.12f, 0.14f, 0.25f, 1f);
                string displayText = (numStr == "9") ? $"<u>{numStr}</u>" : numStr;
                TextMeshProUGUI txtNum = CreateText("Txt_Num", cellGO.transform, displayText, 17, numColor, true);
                if (font != null) txtNum.font = font;
                StretchFull(txtNum.rectTransform);
                txtNum.alignment = TextAlignmentOptions.Center;

                if (numStr == "6")
                {
                    // Dot under 6 (navy)
                    GameObject dot = CreateUIObject("EventDot", cellGO.transform);
                    RectTransform rtDot = dot.GetComponent<RectTransform>();
                    rtDot.anchorMin = new Vector2(0.5f, 0f);
                    rtDot.anchorMax = new Vector2(0.5f, 0f);
                    rtDot.pivot = new Vector2(0.5f, 0f);
                    rtDot.anchoredPosition = new Vector2(0f, 6f);
                    rtDot.sizeDelta = new Vector2(4f, 4f);
                    Image id = dot.AddComponent<Image>();
                    id.sprite = spCircle;
                    id.color = new Color(0.12f, 0.14f, 0.25f, 0.9f);
                }
                else if (numStr == "14")
                {
                    // Dot under 14 (navy)
                    GameObject dot = CreateUIObject("EventDot", cellGO.transform);
                    RectTransform rtDot = dot.GetComponent<RectTransform>();
                    rtDot.anchorMin = new Vector2(0.5f, 0f);
                    rtDot.anchorMax = new Vector2(0.5f, 0f);
                    rtDot.pivot = new Vector2(0.5f, 0f);
                    rtDot.anchoredPosition = new Vector2(0f, 6f);
                    rtDot.sizeDelta = new Vector2(4f, 4f);
                    Image id = dot.AddComponent<Image>();
                    id.sprite = spCircle;
                    id.color = new Color(0.12f, 0.14f, 0.25f, 0.9f);
                }
            }
        }
    }

    private static void BuildActivitySection(Transform parent, Sprite spActBar, Sprite spCircle, TMP_FontAsset font)
    {
        // 1. Activity Bar Header (Rectangle 202: 2 AKTIFITAS)
        GameObject actBar = CreateUIObject("ActivitySectionBar", parent);
        RectTransform rtBar = actBar.GetComponent<RectTransform>();
        rtBar.anchorMin = new Vector2(0.5f, 1f);
        rtBar.anchorMax = new Vector2(0.5f, 1f);
        rtBar.pivot = new Vector2(0.5f, 1f);
        rtBar.anchoredPosition = new Vector2(0f, -444f);
        rtBar.sizeDelta = new Vector2(554f, 44f);

        Image imgBar = actBar.AddComponent<Image>();
        imgBar.sprite = spActBar;
        imgBar.raycastTarget = false;

        TextMeshProUGUI txtBarTitle = CreateText("Txt_ActTitle", actBar.transform, "2 AKTIFITAS", 15, new Color(0.965f, 0.973f, 1f, 1f), true);
        if (font != null) txtBarTitle.font = font;
        txtBarTitle.characterSpacing = 8f;
        RectTransform rtTitle = txtBarTitle.rectTransform;
        rtTitle.anchorMin = new Vector2(0f, 0.5f);
        rtTitle.anchorMax = new Vector2(0f, 0.5f);
        rtTitle.pivot = new Vector2(0f, 0.5f);
        rtTitle.anchoredPosition = new Vector2(36f, 0f);
        rtTitle.sizeDelta = new Vector2(200f, 26f);
        txtBarTitle.alignment = TextAlignmentOptions.MidlineLeft;

        // 2. Activity List (5 Schedule Rows with VerticalLayoutGroup)
        GameObject listGO = CreateUIObject("ActivityList", parent);
        RectTransform rtList = listGO.GetComponent<RectTransform>();
        rtList.anchorMin = new Vector2(0.5f, 1f);
        rtList.anchorMax = new Vector2(0.5f, 1f);
        rtList.pivot = new Vector2(0.5f, 1f);
        rtList.anchoredPosition = new Vector2(0f, -502f);
        rtList.sizeDelta = new Vector2(464f, 260f);

        VerticalLayoutGroup vlg = listGO.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 14f;
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        // Schedule Items:
        // Item 1: Ngapain ....
        BuildScheduleRow(listGO.transform, "Row_1", "Ngapain ....", true, spCircle, font);
        // Item 2: Ga Ngapa-ngapain
        BuildScheduleRow(listGO.transform, "Row_2", "Ga Ngapa-ngapain", true, spCircle, font);
        // Item 3 to 5: Empty slots
        BuildScheduleRow(listGO.transform, "Row_3", "", false, spCircle, font);
        BuildScheduleRow(listGO.transform, "Row_4", "", false, spCircle, font);
        BuildScheduleRow(listGO.transform, "Row_5", "", false, spCircle, font);
    }

    private static void BuildScheduleRow(Transform parent, string name, string label, bool hasEvent, Sprite spCircle, TMP_FontAsset font)
    {
        GameObject row = CreateUIObject(name, parent);
        RectTransform rtRow = row.GetComponent<RectTransform>();
        rtRow.sizeDelta = new Vector2(464f, 36f);

        // Bottom Underline (Rectangle 203..207)
        GameObject under = CreateUIObject("Underline", row.transform);
        RectTransform rtUnder = under.GetComponent<RectTransform>();
        rtUnder.anchorMin = new Vector2(0f, 0f);
        rtUnder.anchorMax = new Vector2(1f, 0f);
        rtUnder.pivot = new Vector2(0.5f, 0f);
        rtUnder.anchoredPosition = Vector2.zero;
        rtUnder.sizeDelta = new Vector2(0f, 2.5f);
        Image imgUnder = under.AddComponent<Image>();
        imgUnder.color = new Color(0.12f, 0.14f, 0.25f, 0.85f); // #1E2440

        // Left Vertical Marker Line connected to bottom-left corner
        GameObject leftLine = CreateUIObject("LeftMarker", row.transform);
        RectTransform rtLL = leftLine.GetComponent<RectTransform>();
        rtLL.anchorMin = new Vector2(0f, 0f);
        rtLL.anchorMax = new Vector2(0f, 0f);
        rtLL.pivot = new Vector2(0f, 0f);
        rtLL.anchoredPosition = Vector2.zero;
        rtLL.sizeDelta = new Vector2(2.5f, 22f);
        Image imgLL = leftLine.AddComponent<Image>();
        imgLL.color = new Color(0.12f, 0.14f, 0.25f, 0.85f); // #1E2440

        // Orange Bullet Dot
        if (hasEvent)
        {
            GameObject dot = CreateUIObject("BulletDot", row.transform);
            RectTransform rtDot = dot.GetComponent<RectTransform>();
            rtDot.anchorMin = new Vector2(0f, 0f);
            rtDot.anchorMax = new Vector2(0f, 0f);
            rtDot.pivot = new Vector2(0f, 0f);
            rtDot.anchoredPosition = new Vector2(16f, 10f);
            rtDot.sizeDelta = new Vector2(8f, 8f);
            Image imgDot = dot.AddComponent<Image>();
            imgDot.sprite = spCircle;
            imgDot.color = new Color(0.85f, 0.42f, 0f, 1f); // #D96B00
        }

        // Label Text
        if (!string.IsNullOrEmpty(label))
        {
            TextMeshProUGUI txtLabel = CreateText("Txt_Label", row.transform, label, 16, new Color(0.12f, 0.14f, 0.25f, 1f), true);
            if (font != null) txtLabel.font = font;
            RectTransform rtTxt = txtLabel.rectTransform;
            rtTxt.anchorMin = new Vector2(0f, 0f);
            rtTxt.anchorMax = new Vector2(1f, 0f);
            rtTxt.pivot = new Vector2(0f, 0f);
            rtTxt.anchoredPosition = new Vector2(34f, 4f);
            rtTxt.sizeDelta = new Vector2(-40f, 26f);
            txtLabel.alignment = TextAlignmentOptions.MidlineLeft;
        }
    }

    private static void BuildCalendarBottomNav(Transform parent, Sprite spNavBg, Sprite spBack, Sprite spHome, Sprite spRecents)
    {
        GameObject nav = CreateUIObject("BottomNav_Android", parent);
        RectTransform rt = nav.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0f);
        rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(554f, 56f);

        Image imgNav = nav.AddComponent<Image>();
        imgNav.sprite = spNavBg;
        imgNav.raycastTarget = false;

        // 3 Nav Icons
        // 1. Back
        GameObject btnBack = CreateUIObject("Btn_NavBack", nav.transform);
        RectTransform rtBack = btnBack.GetComponent<RectTransform>();
        rtBack.anchorMin = new Vector2(0.5f, 0.5f);
        rtBack.anchorMax = new Vector2(0.5f, 0.5f);
        rtBack.pivot = new Vector2(0.5f, 0.5f);
        rtBack.anchoredPosition = new Vector2(-130f, 0f);
        rtBack.sizeDelta = new Vector2(24f, 24f);
        Image imgB = btnBack.AddComponent<Image>();
        imgB.sprite = spBack;
        imgB.preserveAspect = true;
        btnBack.AddComponent<Button>();

        // 2. Home
        GameObject btnHome = CreateUIObject("Btn_NavHome", nav.transform);
        RectTransform rtHome = btnHome.GetComponent<RectTransform>();
        rtHome.anchorMin = new Vector2(0.5f, 0.5f);
        rtHome.anchorMax = new Vector2(0.5f, 0.5f);
        rtHome.pivot = new Vector2(0.5f, 0.5f);
        rtHome.anchoredPosition = Vector2.zero;
        rtHome.sizeDelta = new Vector2(24f, 24f);
        Image imgH = btnHome.AddComponent<Image>();
        imgH.sprite = spHome;
        imgH.preserveAspect = true;
        btnHome.AddComponent<Button>();

        // 3. Recents
        GameObject btnRec = CreateUIObject("Btn_NavRecents", nav.transform);
        RectTransform rtRec = btnRec.GetComponent<RectTransform>();
        rtRec.anchorMin = new Vector2(0.5f, 0.5f);
        rtRec.anchorMax = new Vector2(0.5f, 0.5f);
        rtRec.pivot = new Vector2(0.5f, 0.5f);
        rtRec.anchoredPosition = new Vector2(130f, 0f);
        rtRec.sizeDelta = new Vector2(24f, 24f);
        Image imgR = btnRec.AddComponent<Image>();
        imgR.sprite = spRecents;
        imgR.preserveAspect = true;
        btnRec.AddComponent<Button>();
    }

    // =========================================================================
    // CHAT CONTACTS (WETALK) SUB-BUILDERS
    // =========================================================================

    private static void BuildChatContactsScreen(
        Transform parent,
        Sprite spChatBg,
        Sprite spSearchPill,
        Sprite spChatCardBg,
        Sprite spIconSearch,
        Sprite spAvatarUser,
        Sprite spCircle,
        Sprite spBottomNav,
        Sprite spNavBack,
        Sprite spNavHome,
        Sprite spNavRecents,
        Sprite spIconWifi,
        Sprite spIconSignal,
        Sprite spIconBattery,
        TMP_FontAsset font)
    {
        // 1. Dark Gradient Background (Rectangle 133: #070918 to #1E266C)
        GameObject bgGO = CreateUIObject("Img_ChatBgGradient", parent);
        StretchFull(bgGO.GetComponent<RectTransform>());
        Image imgBg = bgGO.AddComponent<Image>();
        imgBg.sprite = spChatBg;
        imgBg.raycastTarget = false;

        // 2. Status Bar on top
        BuildStatusBar(parent, spIconWifi, spIconSignal, spIconBattery, font);

        // 3. Search Bar Pill (Rectangle 218)
        GameObject searchBar = CreateUIObject("SearchBar", parent);
        RectTransform rtSearch = searchBar.GetComponent<RectTransform>();
        rtSearch.anchorMin = new Vector2(0.5f, 1f);
        rtSearch.anchorMax = new Vector2(0.5f, 1f);
        rtSearch.pivot = new Vector2(0.5f, 1f);
        rtSearch.anchoredPosition = new Vector2(0f, -52f);
        rtSearch.sizeDelta = new Vector2(496f, 48f);

        Image imgSearchBg = searchBar.AddComponent<Image>();
        imgSearchBg.sprite = spSearchPill;
        imgSearchBg.type = Image.Type.Sliced;
        imgSearchBg.raycastTarget = true;

        // Search Icon (ant-design:search-outlined)
        GameObject iconSearchGO = CreateUIObject("Icon_Search", searchBar.transform);
        RectTransform rtIcon = iconSearchGO.GetComponent<RectTransform>();
        rtIcon.anchorMin = new Vector2(0f, 0.5f);
        rtIcon.anchorMax = new Vector2(0f, 0.5f);
        rtIcon.pivot = new Vector2(0f, 0.5f);
        rtIcon.anchoredPosition = new Vector2(18f, 0f);
        rtIcon.sizeDelta = new Vector2(24f, 24f);

        Image imgSearchIcon = iconSearchGO.AddComponent<Image>();
        imgSearchIcon.sprite = spIconSearch;
        imgSearchIcon.preserveAspect = true;
        imgSearchIcon.raycastTarget = false;

        // Placeholder Text "Cari..."
        TextMeshProUGUI txtSearchPlaceholder = CreateText("Txt_Placeholder", searchBar.transform, "Cari...", 16, new Color(0.12f, 0.15f, 0.43f, 0.55f), true);
        if (font != null) txtSearchPlaceholder.font = font;
        RectTransform rtTxtPl = txtSearchPlaceholder.rectTransform;
        rtTxtPl.anchorMin = new Vector2(0f, 0f);
        rtTxtPl.anchorMax = new Vector2(1f, 1f);
        rtTxtPl.offsetMin = new Vector2(52f, 0f);
        rtTxtPl.offsetMax = new Vector2(-18f, 0f);
        txtSearchPlaceholder.alignment = TextAlignmentOptions.MidlineLeft;

        // 4. Contact Card Container (Rectangle 219)
        GameObject cardGO = CreateUIObject("Panel_ContactCard", parent);
        RectTransform rtCard = cardGO.GetComponent<RectTransform>();
        rtCard.anchorMin = new Vector2(0.5f, 1f);
        rtCard.anchorMax = new Vector2(0.5f, 1f);
        rtCard.pivot = new Vector2(0.5f, 1f);
        rtCard.anchoredPosition = new Vector2(0f, -114f);
        rtCard.sizeDelta = new Vector2(514f, 650f);

        Image imgCard = cardGO.AddComponent<Image>();
        imgCard.sprite = spChatCardBg;
        imgCard.type = Image.Type.Sliced;
        imgCard.raycastTarget = true;

        // Mask for clean rounded list clipping
        RectMask2D mask = cardGO.AddComponent<RectMask2D>();
        mask.padding = new Vector4(2, 2, 2, 2);

        // Content List Holder
        GameObject listGO = CreateUIObject("ContactList", cardGO.transform);
        RectTransform rtList = listGO.GetComponent<RectTransform>();
        rtList.anchorMin = new Vector2(0f, 0f);
        rtList.anchorMax = new Vector2(1f, 1f);
        rtList.offsetMin = new Vector2(14f, 10f);
        rtList.offsetMax = new Vector2(-14f, -10f);

        VerticalLayoutGroup vlg = listGO.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 8f;
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        // 5. Contact Items (7 Rows according to Group 83 - 89)
        for (int i = 0; i < 7; i++)
        {
            BuildContactRow(
                listGO.transform,
                $"ContactRow_{i + 1}",
                "Nama Kontak",
                "Lorem Ipsum has been the in...",
                "1",
                spAvatarUser,
                spCircle,
                font);
        }

        // 6. Android Bottom Navigation Bar (Rectangle 202)
        BuildCalendarBottomNav(parent, spBottomNav, spNavBack, spNavHome, spNavRecents);
    }

    private static void BuildContactRow(
        Transform parent,
        string name,
        string contactName,
        string messagePreview,
        string unreadCount,
        Sprite spAvatar,
        Sprite spCircle,
        TMP_FontAsset font)
    {
        GameObject row = CreateUIObject(name, parent);
        RectTransform rtRow = row.GetComponent<RectTransform>();
        rtRow.sizeDelta = new Vector2(486f, 80f);

        Button btnRow = row.AddComponent<Button>();
        Image imgRowTarget = row.AddComponent<Image>();
        imgRowTarget.color = Color.clear;
        btnRow.targetGraphic = imgRowTarget;

        // Avatar Icon (Group 74)
        GameObject avatarGO = CreateUIObject("Avatar", row.transform);
        RectTransform rtAvatar = avatarGO.GetComponent<RectTransform>();
        rtAvatar.anchorMin = new Vector2(0f, 0.5f);
        rtAvatar.anchorMax = new Vector2(0f, 0.5f);
        rtAvatar.pivot = new Vector2(0f, 0.5f);
        rtAvatar.anchoredPosition = new Vector2(10f, 0f);
        rtAvatar.sizeDelta = new Vector2(56f, 56f);

        Image imgAvatar = avatarGO.AddComponent<Image>();
        imgAvatar.sprite = spAvatar;
        imgAvatar.preserveAspect = true;
        imgAvatar.raycastTarget = false;

        // Text Column
        GameObject textCol = CreateUIObject("TextCol", row.transform);
        RectTransform rtTextCol = textCol.GetComponent<RectTransform>();
        rtTextCol.anchorMin = new Vector2(0f, 0f);
        rtTextCol.anchorMax = new Vector2(1f, 1f);
        rtTextCol.offsetMin = new Vector2(78f, 0f);
        rtTextCol.offsetMax = new Vector2(-46f, 0f);

        // Name
        TextMeshProUGUI txtName = CreateText("Txt_Name", textCol.transform, contactName, 16, new Color(0.12f, 0.15f, 0.43f, 1f), true);
        if (font != null) txtName.font = font;
        RectTransform rtName = txtName.rectTransform;
        rtName.anchorMin = new Vector2(0f, 0.5f);
        rtName.anchorMax = new Vector2(1f, 0.5f);
        rtName.pivot = new Vector2(0f, 0.5f);
        rtName.anchoredPosition = new Vector2(0f, 13f);
        rtName.sizeDelta = new Vector2(0f, 26f);
        txtName.alignment = TextAlignmentOptions.MidlineLeft;

        // Message Preview
        TextMeshProUGUI txtMsg = CreateText("Txt_Message", textCol.transform, messagePreview, 13.5f, new Color(0.12f, 0.15f, 0.43f, 0.75f), false);
        if (font != null) txtMsg.font = font;
        RectTransform rtMsg = txtMsg.rectTransform;
        rtMsg.anchorMin = new Vector2(0f, 0.5f);
        rtMsg.anchorMax = new Vector2(1f, 0.5f);
        rtMsg.pivot = new Vector2(0f, 0.5f);
        rtMsg.anchoredPosition = new Vector2(0f, -13f);
        rtMsg.sizeDelta = new Vector2(0f, 22f);
        txtMsg.alignment = TextAlignmentOptions.MidlineLeft;

        // Unread Badge (Ellipse 71 + "1")
        if (!string.IsNullOrEmpty(unreadCount))
        {
            GameObject badgeGO = CreateUIObject("Badge_Unread", row.transform);
            RectTransform rtBadge = badgeGO.GetComponent<RectTransform>();
            rtBadge.anchorMin = new Vector2(1f, 0.5f);
            rtBadge.anchorMax = new Vector2(1f, 0.5f);
            rtBadge.pivot = new Vector2(1f, 0.5f);
            rtBadge.anchoredPosition = new Vector2(-10f, 0f);
            rtBadge.sizeDelta = new Vector2(22f, 22f);

            Image imgBadge = badgeGO.AddComponent<Image>();
            imgBadge.sprite = spCircle;
            imgBadge.color = new Color(0.43f, 0.49f, 0.75f, 1f); // #6E7CC0

            TextMeshProUGUI txtBadge = CreateText("Txt_Badge", badgeGO.transform, unreadCount, 11, new Color(0.965f, 0.973f, 1f, 1f), true);
            if (font != null) txtBadge.font = font;
            StretchFull(txtBadge.rectTransform);
            txtBadge.alignment = TextAlignmentOptions.Center;
        }

        // Divider Line (Line 7)
        GameObject lineGO = CreateUIObject("DividerLine", row.transform);
        RectTransform rtLine = lineGO.GetComponent<RectTransform>();
        rtLine.anchorMin = new Vector2(0f, 0f);
        rtLine.anchorMax = new Vector2(1f, 0f);
        rtLine.pivot = new Vector2(0.5f, 0f);
        rtLine.anchoredPosition = Vector2.zero;
        rtLine.sizeDelta = new Vector2(0f, 1.5f);

        Image imgLine = lineGO.AddComponent<Image>();
        imgLine.color = new Color(0.78f, 0.82f, 1f, 0.9f); // #C8D1FF
    }

    // =========================================================================
    // CHAT ROOM (NO KEYPAD) SUB-BUILDERS
    // =========================================================================

    private static void BuildChatRoomScreen(
        Transform parent,
        Sprite spChatRoomBg,
        Sprite spHeaderBar,
        Sprite spBottomBar,
        Sprite spBubbleIn,
        Sprite spBubbleOut,
        Sprite spTailL,
        Sprite spTailR,
        Sprite spSticker,
        Sprite spPlus,
        Sprite spSendPlane,
        Sprite spChevLeft,
        Sprite spAvatar,
        Sprite spCircle,
        Sprite spPill,
        Sprite spWifi,
        Sprite spSignal,
        Sprite spBattery,
        TMP_FontAsset fontPoppins,
        TMP_FontAsset fontPatrick)
    {
        // 1. Chat Room Body Background (Rectangle 132: #6E7CC0, 554 x 840)
        GameObject bgGO = CreateUIObject("Img_ChatRoomBg", parent);
        StretchFull(bgGO.GetComponent<RectTransform>());
        Image imgBg = bgGO.AddComponent<Image>();
        imgBg.sprite = spChatRoomBg;
        imgBg.raycastTarget = false;

        // 2. Chat Header Bar (Rectangle 134: #070918 to #1E266D, 554 x 105)
        GameObject headerGO = CreateUIObject("Header_ChatRoom", parent);
        RectTransform rtHeader = headerGO.GetComponent<RectTransform>();
        rtHeader.anchorMin = new Vector2(0f, 1f);
        rtHeader.anchorMax = new Vector2(1f, 1f);
        rtHeader.pivot = new Vector2(0.5f, 1f);
        rtHeader.anchoredPosition = Vector2.zero;
        rtHeader.sizeDelta = new Vector2(0f, 105f);

        Image imgHeader = headerGO.AddComponent<Image>();
        imgHeader.sprite = spHeaderBar;
        imgHeader.raycastTarget = false;

        // Top Status Bar inside Header
        BuildStatusBar(headerGO.transform, spWifi, spSignal, spBattery, fontPoppins);

        // Header Navigation & Profile Row
        GameObject navRow = CreateUIObject("NavRow", headerGO.transform);
        RectTransform rtNav = navRow.GetComponent<RectTransform>();
        rtNav.anchorMin = new Vector2(0f, 0f);
        rtNav.anchorMax = new Vector2(1f, 0f);
        rtNav.pivot = new Vector2(0.5f, 0f);
        rtNav.anchoredPosition = new Vector2(0f, 10f);
        rtNav.sizeDelta = new Vector2(0f, 50f);

        // Back Chevron (Vector 6)
        GameObject btnBackGO = CreateUIObject("Btn_Back", navRow.transform);
        RectTransform rtBack = btnBackGO.GetComponent<RectTransform>();
        rtBack.anchorMin = new Vector2(0f, 0.5f);
        rtBack.anchorMax = new Vector2(0f, 0.5f);
        rtBack.pivot = new Vector2(0f, 0.5f);
        rtBack.anchoredPosition = new Vector2(24f, 0f);
        rtBack.sizeDelta = new Vector2(14f, 24f);

        Image imgBack = btnBackGO.AddComponent<Image>();
        imgBack.sprite = spChevLeft;
        imgBack.color = new Color(0.965f, 0.973f, 1f, 1f);
        imgBack.preserveAspect = true;
        btnBackGO.AddComponent<Button>();

        // Avatar (Group 73: Profile silhouette circle)
        GameObject avatarGO = CreateUIObject("Avatar_SiAnu", navRow.transform);
        RectTransform rtAvatar = avatarGO.GetComponent<RectTransform>();
        rtAvatar.anchorMin = new Vector2(0f, 0.5f);
        rtAvatar.anchorMax = new Vector2(0f, 0.5f);
        rtAvatar.pivot = new Vector2(0f, 0.5f);
        rtAvatar.anchoredPosition = new Vector2(56f, 0f);
        rtAvatar.sizeDelta = new Vector2(46f, 46f);

        Image imgAvatar = avatarGO.AddComponent<Image>();
        imgAvatar.sprite = spAvatar;
        imgAvatar.preserveAspect = true;
        imgAvatar.raycastTarget = false;

        // Contact Name ("Si Anu")
        TextMeshProUGUI txtName = CreateText("Txt_ContactName", navRow.transform, "Si Anu", 20f, new Color(0.965f, 0.973f, 1f, 1f), true);
        if (fontPoppins != null) txtName.font = fontPoppins;
        RectTransform rtName = txtName.rectTransform;
        rtName.anchorMin = new Vector2(0f, 0.5f);
        rtName.anchorMax = new Vector2(0f, 0.5f);
        rtName.pivot = new Vector2(0f, 0.5f);
        rtName.anchoredPosition = new Vector2(114f, 0f);
        rtName.sizeDelta = new Vector2(180f, 30f);
        txtName.alignment = TextAlignmentOptions.MidlineLeft;

        // 3. Chat Messages Container
        GameObject msgsGO = CreateUIObject("Panel_ChatMessages", parent);
        RectTransform rtMsgs = msgsGO.GetComponent<RectTransform>();
        rtMsgs.anchorMin = new Vector2(0f, 0f);
        rtMsgs.anchorMax = new Vector2(1f, 1f);
        rtMsgs.offsetMin = new Vector2(0f, 105f); // Above bottom bar
        rtMsgs.offsetMax = new Vector2(0f, -105f); // Below header

        // Bubble 1: Incoming ("Lorem ipsum dolor sit amet...")
        BuildBubbleIncoming(
            msgsGO.transform,
            "Bubble_Incoming_1",
            "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Etiam aliquam justo at eros sollicitudin,",
            new Vector2(28f, -24f),
            new Vector2(330f, 76f),
            spBubbleIn,
            spTailL,
            fontPatrick);

        // Bubble 2: Incoming ("at consectetur odio tempus.")
        BuildBubbleIncoming(
            msgsGO.transform,
            "Bubble_Incoming_2",
            "at consectetur odio tempus.",
            new Vector2(28f, -108f),
            new Vector2(210f, 42f),
            spBubbleIn,
            spTailL,
            fontPatrick);

        // Bubble 3: Outgoing ("Vivamus blandit pretium leo ac tristique.")
        BuildBubbleOutgoing(
            msgsGO.transform,
            "Bubble_Outgoing_1",
            "Vivamus blandit pretium leo ac tristique.",
            new Vector2(-32f, -168f),
            new Vector2(300f, 42f),
            spBubbleOut,
            spTailR,
            fontPatrick);

        // Sticker / Meme Image (images 1)
        GameObject stickerGO = CreateUIObject("Img_StickerAttachment", msgsGO.transform);
        RectTransform rtSticker = stickerGO.GetComponent<RectTransform>();
        rtSticker.anchorMin = new Vector2(1f, 1f);
        rtSticker.anchorMax = new Vector2(1f, 1f);
        rtSticker.pivot = new Vector2(1f, 1f);
        rtSticker.anchoredPosition = new Vector2(-32f, -220f);
        rtSticker.sizeDelta = new Vector2(92f, 92f);

        Image imgSticker = stickerGO.AddComponent<Image>();
        imgSticker.sprite = spSticker;
        imgSticker.preserveAspect = true;
        imgSticker.raycastTarget = false;

        // Typing Indicator (Group 72)
        BuildTypingBubble(
            msgsGO.transform,
            "Bubble_TypingIndicator",
            new Vector2(28f, -330f),
            new Vector2(110f, 42f),
            spBubbleIn,
            spCircle);

        // 4. Bottom Input Bar (Rectangle 136: #070918 to #1E266D, 554 x 105)
        GameObject bottomBarGO = CreateUIObject("Bar_BottomInput", parent);
        RectTransform rtBottom = bottomBarGO.GetComponent<RectTransform>();
        rtBottom.anchorMin = new Vector2(0f, 0f);
        rtBottom.anchorMax = new Vector2(1f, 0f);
        rtBottom.pivot = new Vector2(0.5f, 0f);
        rtBottom.anchoredPosition = Vector2.zero;
        rtBottom.sizeDelta = new Vector2(0f, 105f);

        Image imgBottom = bottomBarGO.AddComponent<Image>();
        imgBottom.sprite = spBottomBar;
        imgBottom.raycastTarget = true;

        // Circular Plus Button (Ellipse 63: 46 x 46)
        GameObject btnPlusGO = CreateUIObject("Btn_Plus", bottomBarGO.transform);
        RectTransform rtPlus = btnPlusGO.GetComponent<RectTransform>();
        rtPlus.anchorMin = new Vector2(0f, 0.5f);
        rtPlus.anchorMax = new Vector2(0f, 0.5f);
        rtPlus.pivot = new Vector2(0.5f, 0.5f);
        rtPlus.anchoredPosition = new Vector2(46f, 0f);
        rtPlus.sizeDelta = new Vector2(46f, 46f);

        Image imgPlusBg = btnPlusGO.AddComponent<Image>();
        imgPlusBg.sprite = spCircle;
        imgPlusBg.color = new Color(0.965f, 0.973f, 1f, 1f); // #F6F8FF
        btnPlusGO.AddComponent<Button>();

        GameObject iconPlusGO = CreateUIObject("Icon_Plus", btnPlusGO.transform);
        RectTransform rtIconPlus = iconPlusGO.GetComponent<RectTransform>();
        rtIconPlus.anchorMin = new Vector2(0.5f, 0.5f);
        rtIconPlus.anchorMax = new Vector2(0.5f, 0.5f);
        rtIconPlus.pivot = new Vector2(0.5f, 0.5f);
        rtIconPlus.anchoredPosition = Vector2.zero;
        rtIconPlus.sizeDelta = new Vector2(24f, 24f);

        Image imgIconPlus = iconPlusGO.AddComponent<Image>();
        imgIconPlus.sprite = spPlus;
        imgIconPlus.color = new Color(0.12f, 0.14f, 0.25f, 1f); // #1E2440
        imgIconPlus.preserveAspect = true;
        imgIconPlus.raycastTarget = false;

        // Input Field Pill (Rectangle 212: #F6F8FF)
        GameObject pillGO = CreateUIObject("Pill_InputField", bottomBarGO.transform);
        RectTransform rtPill = pillGO.GetComponent<RectTransform>();
        rtPill.anchorMin = new Vector2(0f, 0.5f);
        rtPill.anchorMax = new Vector2(1f, 0.5f);
        rtPill.pivot = new Vector2(0.5f, 0.5f);
        rtPill.offsetMin = new Vector2(82f, -23f);
        rtPill.offsetMax = new Vector2(-22f, 23f);

        Image imgPill = pillGO.AddComponent<Image>();
        imgPill.sprite = spPill;
        imgPill.type = Image.Type.Sliced;
        imgPill.color = new Color(0.965f, 0.973f, 1f, 1f); // #F6F8FF

        // Send Airplane Icon inside Pill (Vector 8)
        GameObject btnSendGO = CreateUIObject("Btn_Send", pillGO.transform);
        RectTransform rtSend = btnSendGO.GetComponent<RectTransform>();
        rtSend.anchorMin = new Vector2(1f, 0.5f);
        rtSend.anchorMax = new Vector2(1f, 0.5f);
        rtSend.pivot = new Vector2(1f, 0.5f);
        rtSend.anchoredPosition = new Vector2(-12f, 0f);
        rtSend.sizeDelta = new Vector2(28f, 28f);

        Image imgSend = btnSendGO.AddComponent<Image>();
        imgSend.sprite = spSendPlane;
        imgSend.color = new Color(0.12f, 0.14f, 0.25f, 1f); // #1E2440
        imgSend.preserveAspect = true;
        btnSendGO.AddComponent<Button>();
    }

    private static void BuildBubbleIncoming(
        Transform parent,
        string name,
        string message,
        Vector2 pos,
        Vector2 size,
        Sprite spBubble,
        Sprite spTail,
        TMP_FontAsset font)
    {
        GameObject bubbleGO = CreateUIObject(name, parent);
        RectTransform rt = bubbleGO.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        Image imgBubble = bubbleGO.AddComponent<Image>();
        imgBubble.sprite = spBubble;
        imgBubble.type = Image.Type.Sliced;
        imgBubble.color = new Color(0.965f, 0.973f, 1f, 1f); // #F6F8FF

        // Tail
        if (spTail != null)
        {
            GameObject tailGO = CreateUIObject("Tail", bubbleGO.transform);
            RectTransform rtTail = tailGO.GetComponent<RectTransform>();
            rtTail.anchorMin = new Vector2(0f, 0f);
            rtTail.anchorMax = new Vector2(0f, 0f);
            rtTail.pivot = new Vector2(1f, 0f);
            rtTail.anchoredPosition = new Vector2(12f, 0f);
            rtTail.sizeDelta = new Vector2(30f, 22f);

            Image imgTail = tailGO.AddComponent<Image>();
            imgTail.sprite = spTail;
            imgTail.color = new Color(0.965f, 0.973f, 1f, 1f);
            imgTail.raycastTarget = false;
        }

        // Text
        TextMeshProUGUI txt = CreateText("Txt_Message", bubbleGO.transform, message, 15.5f, new Color(0.09f, 0.11f, 0.18f, 1f), false);
        if (font != null) txt.font = font;
        RectTransform rtTxt = txt.rectTransform;
        rtTxt.anchorMin = Vector2.zero;
        rtTxt.anchorMax = Vector2.one;
        rtTxt.offsetMin = new Vector2(16f, 8f);
        rtTxt.offsetMax = new Vector2(-16f, -8f);
        txt.enableWordWrapping = true;
        txt.alignment = TextAlignmentOptions.MidlineLeft;
    }

    private static void BuildBubbleOutgoing(
        Transform parent,
        string name,
        string message,
        Vector2 pos,
        Vector2 size,
        Sprite spBubble,
        Sprite spTail,
        TMP_FontAsset font)
    {
        GameObject bubbleGO = CreateUIObject(name, parent);
        RectTransform rt = bubbleGO.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(1f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(1f, 1f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        Image imgBubble = bubbleGO.AddComponent<Image>();
        imgBubble.sprite = spBubble;
        imgBubble.type = Image.Type.Sliced;
        imgBubble.color = new Color(0.12f, 0.14f, 0.25f, 1f); // #1E2440

        // Tail
        if (spTail != null)
        {
            GameObject tailGO = CreateUIObject("Tail", bubbleGO.transform);
            RectTransform rtTail = tailGO.GetComponent<RectTransform>();
            rtTail.anchorMin = new Vector2(1f, 0f);
            rtTail.anchorMax = new Vector2(1f, 0f);
            rtTail.pivot = new Vector2(0f, 0f);
            rtTail.anchoredPosition = new Vector2(-12f, 0f);
            rtTail.sizeDelta = new Vector2(30f, 22f);

            Image imgTail = tailGO.AddComponent<Image>();
            imgTail.sprite = spTail;
            imgTail.color = new Color(0.12f, 0.14f, 0.25f, 1f);
            imgTail.raycastTarget = false;
        }

        // Text
        TextMeshProUGUI txt = CreateText("Txt_Message", bubbleGO.transform, message, 15.5f, Color.white, false);
        if (font != null) txt.font = font;
        RectTransform rtTxt = txt.rectTransform;
        rtTxt.anchorMin = Vector2.zero;
        rtTxt.anchorMax = Vector2.one;
        rtTxt.offsetMin = new Vector2(16f, 8f);
        rtTxt.offsetMax = new Vector2(-16f, -8f);
        txt.enableWordWrapping = false;
        txt.alignment = TextAlignmentOptions.MidlineLeft;
    }

    private static void BuildTypingBubble(
        Transform parent,
        string name,
        Vector2 pos,
        Vector2 size,
        Sprite spBubble,
        Sprite spCircle)
    {
        GameObject bubbleGO = CreateUIObject(name, parent);
        RectTransform rt = bubbleGO.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        Image imgBubble = bubbleGO.AddComponent<Image>();
        imgBubble.sprite = spBubble;
        imgBubble.type = Image.Type.Sliced;
        imgBubble.color = new Color(0.965f, 0.973f, 1f, 1f); // #F6F8FF

        // 3 Dots (Ellipse 60, 61, 62 in #1E2440)
        float dotSize = 10f;
        float spacing = 22f;
        float[] offsets = { -spacing, 0f, spacing };

        for (int i = 0; i < 3; i++)
        {
            GameObject dotGO = CreateUIObject($"Dot_{i + 1}", bubbleGO.transform);
            RectTransform rtDot = dotGO.GetComponent<RectTransform>();
            rtDot.anchorMin = new Vector2(0.5f, 0.5f);
            rtDot.anchorMax = new Vector2(0.5f, 0.5f);
            rtDot.pivot = new Vector2(0.5f, 0.5f);
            rtDot.anchoredPosition = new Vector2(offsets[i], 0f);
            rtDot.sizeDelta = new Vector2(dotSize, dotSize);

            Image imgDot = dotGO.AddComponent<Image>();
            imgDot.sprite = spCircle;
            imgDot.color = new Color(0.12f, 0.14f, 0.25f, 1f); // #1E2440
            imgDot.raycastTarget = false;
        }
    }

    // =========================================================================
    // PHONE HOME SUB-BUILDERS
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

        GameObject wifiGO = CreateUIObject("Icon_Wifi", iconsCont.transform);
        wifiGO.GetComponent<RectTransform>().sizeDelta = new Vector2(20f, 16f);
        Image imgWifi = wifiGO.AddComponent<Image>();
        imgWifi.sprite = spWifi;
        imgWifi.preserveAspect = true;
        imgWifi.raycastTarget = false;

        GameObject sigGO = CreateUIObject("Icon_Signal", iconsCont.transform);
        sigGO.GetComponent<RectTransform>().sizeDelta = new Vector2(20f, 16f);
        Image imgSig = sigGO.AddComponent<Image>();
        imgSig.sprite = spSignal;
        imgSig.preserveAspect = true;
        imgSig.raycastTarget = false;

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

        GameObject rightSec = CreateUIObject("CalendarGridSection", root.transform);
        RectTransform rtRight = rightSec.GetComponent<RectTransform>();
        rtRight.anchorMin = new Vector2(1f, 0.5f);
        rtRight.anchorMax = new Vector2(1f, 0.5f);
        rtRight.pivot = new Vector2(1f, 0.5f);
        rtRight.anchoredPosition = new Vector2(-26f, 0f);
        rtRight.sizeDelta = new Vector2(240f, 150f);

        TextMeshProUGUI txtMonth = CreateText("Txt_Month", rightSec.transform, "September", 18, new Color(0.965f, 0.973f, 1f, 1f), true);
        if (font != null) txtMonth.font = font;
        RectTransform rtMonth = txtMonth.rectTransform;
        rtMonth.anchorMin = new Vector2(0f, 1f);
        rtMonth.anchorMax = new Vector2(1f, 1f);
        rtMonth.pivot = new Vector2(1f, 1f);
        rtMonth.anchoredPosition = new Vector2(0f, -8f);
        rtMonth.sizeDelta = new Vector2(0f, 24f);
        txtMonth.alignment = TextAlignmentOptions.MidlineRight;

        TextMeshProUGUI txtDaysHeader = CreateText("Txt_DaysHeader", rightSec.transform, "Min  Sen  Sel  Rab  Kam  Jum  Sab", 10, new Color(0.965f, 0.973f, 1f, 0.85f), true);
        if (font != null) txtDaysHeader.font = font;
        RectTransform rtHeader = txtDaysHeader.rectTransform;
        rtHeader.anchorMin = new Vector2(0f, 1f);
        rtHeader.anchorMax = new Vector2(1f, 1f);
        rtHeader.pivot = new Vector2(0.5f, 1f);
        rtHeader.anchoredPosition = new Vector2(0f, -38f);
        rtHeader.sizeDelta = new Vector2(0f, 18f);
        txtDaysHeader.alignment = TextAlignmentOptions.Center;

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

        int activeDotIndex = 22; // Sen 21
        for (int i = 0; i < 35; i++)
        {
            GameObject dot = CreateUIObject($"Dot_{i}", gridGO.transform);
            Image imgDot = dot.AddComponent<Image>();

            if (i == activeDotIndex)
            {
                imgDot.color = new Color(0.12f, 0.14f, 0.25f, 1f);
                Outline outl = dot.AddComponent<Outline>();
                outl.effectColor = new Color(0.965f, 0.973f, 1f, 1f);
                outl.effectDistance = new Vector2(2f, 2f);
            }
            else
            {
                imgDot.color = new Color(0.85f, 0.85f, 0.85f, 0.65f);
            }
            imgDot.raycastTarget = false;
        }
    }

    private static void BuildMiddleWidgets(Transform parent, Sprite spWidgetBg, Sprite spIconRelation, TMP_FontAsset font)
    {
        // 1. Relation Widget
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

        TextMeshProUGUI txtRel = CreateText("Txt_RelationLabel", relWidget.transform, "Relation", 20, new Color(0.965f, 0.973f, 1f, 1f), true);
        if (font != null) txtRel.font = font;
        RectTransform rtTxtRel = txtRel.rectTransform;
        rtTxtRel.anchorMin = new Vector2(0f, 0f);
        rtTxtRel.anchorMax = new Vector2(1f, 0f);
        rtTxtRel.pivot = new Vector2(0.5f, 0f);
        rtTxtRel.anchoredPosition = new Vector2(0f, 16f);
        rtTxtRel.sizeDelta = new Vector2(0f, 26f);
        txtRel.alignment = TextAlignmentOptions.Center;

        // 2. Digital Clock Widget
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

    // =========================================================
    // UTILITIES & TEXTURE CONFIG
    // =========================================================

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

        ConfigureSpriteTexture(SPRITE_CAL_BODY_BG, Vector4.zero);
        ConfigureSpriteTexture(SPRITE_CAL_HEADER_ARCH, Vector4.zero);
        ConfigureSpriteTexture(SPRITE_CAL_BOTTOM_NAV, Vector4.zero);
        ConfigureSpriteTexture(SPRITE_CAL_ACT_BAR, Vector4.zero);

        ConfigureSpriteTexture(ICON_NAV_BACK, Vector4.zero);
        ConfigureSpriteTexture(ICON_NAV_HOME, Vector4.zero);
        ConfigureSpriteTexture(ICON_NAV_RECENTS, Vector4.zero);
        ConfigureSpriteTexture(ICON_CHEV_LEFT, Vector4.zero);
        ConfigureSpriteTexture(ICON_CHEV_RIGHT, Vector4.zero);

        ConfigureSpriteTexture(ICON_CALL, Vector4.zero);
        ConfigureSpriteTexture(ICON_CHAT, Vector4.zero);
        ConfigureSpriteTexture(ICON_CAM, Vector4.zero);
        ConfigureSpriteTexture(ICON_SETTING, Vector4.zero);
        ConfigureSpriteTexture(ICON_RELATION, Vector4.zero);

        ConfigureSpriteTexture(ICON_WIFI, Vector4.zero);
        ConfigureSpriteTexture(ICON_SIGNAL, Vector4.zero);
        ConfigureSpriteTexture(ICON_BATTERY, Vector4.zero);
        ConfigureSpriteTexture(SPRITE_CIRCLE, Vector4.zero);

        ConfigureSpriteTexture(SPRITE_CHAT_BG_GRADIENT, Vector4.zero);
        ConfigureSpriteTexture(SPRITE_SEARCH_BAR_PILL, new Vector4(46, 46, 46, 46));
        ConfigureSpriteTexture(SPRITE_CHAT_CARD_BG, new Vector4(24, 24, 24, 24));
        ConfigureSpriteTexture(ICON_SEARCH, Vector4.zero);
        ConfigureSpriteTexture(SPRITE_AVATAR_USER, Vector4.zero);

        ConfigureSpriteTexture(SPRITE_CHAT_ROOM_BG, Vector4.zero);
        ConfigureSpriteTexture(SPRITE_CHAT_HEADER_BAR, Vector4.zero);
        ConfigureSpriteTexture(SPRITE_CHAT_BOTTOM_BAR, Vector4.zero);
        ConfigureSpriteTexture(SPRITE_BUBBLE_INCOMING, new Vector4(24, 24, 24, 24));
        ConfigureSpriteTexture(SPRITE_BUBBLE_OUTGOING, new Vector4(24, 24, 24, 24));
        ConfigureSpriteTexture(SPRITE_BUBBLE_TAIL_L, Vector4.zero);
        ConfigureSpriteTexture(SPRITE_BUBBLE_TAIL_R, Vector4.zero);
        ConfigureSpriteTexture(SPRITE_CHAT_STICKER, Vector4.zero);
        ConfigureSpriteTexture(ICON_PLUS, Vector4.zero);
        ConfigureSpriteTexture(ICON_SEND_PLANE, Vector4.zero);
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
