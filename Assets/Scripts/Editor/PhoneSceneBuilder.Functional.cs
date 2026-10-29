#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using TMPro;

/// <summary>
/// Membangun ponsel in-game yang BERFUNGSI (Panel_SmartphoneRoot + PhoneUIController) dengan gaya visual
/// mockup PhoneSceneBuilder: Home, WeTalk (daftar & ruang chat), Akhir Pekan, dan Simpan/Muat.
/// Daftar kontak/venue/pertanyaan cepat diisi saat runtime dari SQLite lewat template tersembunyi.
/// Catatan: menu "Build Phone ... Screen" lain hanya mockup statis dan menghapus ponsel fungsional ini.
/// </summary>
public static partial class PhoneSceneBuilder
{
    private const string FONT_CAVEAT_SDF = "Assets/Fonts/CaveatBrush-Regular SDF.asset";

    private static readonly Color PH_SCREEN = new Color(0.10f, 0.12f, 0.24f, 1f);
    private static readonly Color PH_BAR = new Color(0.07f, 0.08f, 0.17f, 1f);
    private static readonly Color PH_TEXT = new Color(0.965f, 0.973f, 1f, 1f);
    private static readonly Color PH_TEXT_DIM = new Color(0.965f, 0.973f, 1f, 0.62f);
    private static readonly Color PH_NAVY = new Color(0.12f, 0.14f, 0.25f, 1f);
    private static readonly Color PH_ACCENT = new Color(1f, 0.78f, 0.86f, 1f);
    private static readonly Color PH_CHAT_BG = new Color(0.43f, 0.49f, 0.78f, 1f);
    private static readonly Color PH_GLASS = new Color(1f, 1f, 1f, 0.08f);

    private const float PH_SCREEN_W = 554f;
    private const float PH_STATUS_H = 52f;
    private const float PH_NAV_H = 76f;
    private const float PH_HEADER_H = 84f;

    private static TMP_FontAsset _phFont, _phFontChat, _phFontTitle;

    [MenuItem("Game Debug/Build Smartphone (Functional)")]
    public static void BuildFunctionalPhoneMenu()
    {
        BuildFunctionalPhone();
    }

    public static void BuildFunctionalPhone()
    {
        Debug.Log("<color=cyan>=== MEMBANGUN SMARTPHONE FUNGSIONAL ===</color>");
        ConfigurePhoneTextures();

        _phFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FONT_POPPINS_SDF);
        _phFontChat = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FONT_PATRICK_SDF);
        _phFontTitle = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FONT_CAVEAT_SDF);

        var scene = EditorSceneManager.OpenScene(SCENE_PATH, OpenSceneMode.Single);
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("[PhoneSceneBuilder] Canvas tidak ditemukan di SampleScene.");
            return;
        }

        foreach (string old in new[] { "Panel_SmartphoneRoot", "Panel_PhoneScreen" })
        {
            Transform t = canvas.transform.Find(old);
            if (t != null) Object.DestroyImmediate(t.gameObject);
        }

        // Root: overlay redup layar penuh (menahan klik ke HUD) + PhoneUIController
        GameObject root = CreateUIObject("Panel_SmartphoneRoot", canvas.transform);
        StretchFull((RectTransform)root.transform);
        Image dim = root.AddComponent<Image>();
        dim.color = new Color(0.04f, 0.05f, 0.10f, 0.55f);
        PhoneUIController phone = root.AddComponent<PhoneUIController>();

        // Bodi ponsel (bingkai mockup) + layar bermask
        Image body = PhRect("Img_PhoneBody", root.transform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(600f, 975f), Color.white);
        body.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITE_FRAME_OUTER);
        body.type = Image.Type.Sliced;
        body.raycastTarget = true;
        PhRect("Img_Speaker", body.transform, new Vector2(0.5f, 1f), new Vector2(0f, -44f), new Vector2(125f, 10f), new Color(0.965f, 0.973f, 1f, 0.95f));

        Image screen = PhRect("ScreenDisplay", body.transform, new Vector2(0.5f, 0.5f), new Vector2(0f, -24f), new Vector2(PH_SCREEN_W, 840f), PH_SCREEN);
        screen.gameObject.AddComponent<RectMask2D>();

        // Area isi aplikasi (di antara status bar dan nav bar)
        GameObject content = CreateUIObject("ScreenContent", screen.transform);
        RectTransform rtContent = (RectTransform)content.transform;
        StretchFull(rtContent);
        rtContent.offsetMin = new Vector2(0f, PH_NAV_H);
        rtContent.offsetMax = new Vector2(0f, -PH_STATUS_H);

        // Status bar
        Image status = PhBand("StatusBar", screen.transform, true, PH_STATUS_H, PH_BAR);
        phone.txtStatusBarTime = PhText("Txt_StatusBarTime", status.transform, "08.30", 21f, PH_TEXT, TextAlignmentOptions.MidlineLeft);
        PhLeftColumn(phone.txtStatusBarTime.rectTransform, 28f, 200f);
        PhStatusIcon(status.transform, ICON_BATTERY, -28f);
        PhStatusIcon(status.transform, ICON_SIGNAL, -62f);
        PhStatusIcon(status.transform, ICON_WIFI, -96f);

        // Nav bar bawah
        Image nav = PhBand("BottomNav", screen.transform, false, PH_NAV_H, PH_BAR);
        HorizontalLayoutGroup navLayout = nav.gameObject.AddComponent<HorizontalLayoutGroup>();
        navLayout.padding = new RectOffset(40, 40, 12, 12);
        navLayout.childAlignment = TextAnchor.MiddleCenter;
        navLayout.childControlWidth = true;
        navLayout.childControlHeight = true;
        navLayout.childForceExpandWidth = true;
        phone.btnNavBack = PhNavButton(nav.transform, "Btn_NavBack", ICON_NAV_BACK, null);
        phone.btnNavHome = PhNavButton(nav.transform, "Btn_NavHome", ICON_NAV_HOME, null);
        phone.btnNavClose = PhNavButton(nav.transform, "Btn_NavClose", null, "Tutup");
        UnityEventTools.AddPersistentListener(phone.btnNavBack.onClick, phone.OnClick_NavBack);
        UnityEventTools.AddPersistentListener(phone.btnNavHome.onClick, phone.OnClick_NavHome);
        UnityEventTools.AddPersistentListener(phone.btnNavClose.onClick, phone.OnClick_NavClose);

        // Template runtime (di luar layar, induknya nonaktif)
        GameObject templates = CreateUIObject("Templates", root.transform);
        templates.SetActive(false);

        BuildPhoneHome(content.transform, phone);
        BuildPhoneWeTalk(content.transform, templates.transform, phone);
        BuildPhoneOuting(content.transform, phone);
        BuildPhoneSave(content.transform, phone);

        root.transform.SetAsLastSibling();
        root.SetActive(false); // dibuka lewat HUD -> PhoneUIController.BukaPhone

        EditorUtility.SetDirty(phone);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, SCENE_PATH);
        AssetDatabase.SaveAssets();
        Debug.Log("<color=green>=== SMARTPHONE FUNGSIONAL SELESAI ===</color>");
    }

    // -------------------------------------------------------------------------
    // LAYAR
    // -------------------------------------------------------------------------
    private static void BuildPhoneHome(Transform content, PhoneUIController phone)
    {
        GameObject scr = PhScreen("Screen_HomeScreen", content);
        Image wall = PhRect("Img_Wallpaper", scr.transform, Vector2.zero, Vector2.zero, Vector2.zero, Color.white);
        wall.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITE_WALLPAPER);
        StretchFull(wall.rectTransform);

        phone.txtHomeDay = PhText("Txt_HomeDay", scr.transform, "Senin", 52f, PH_TEXT, TextAlignmentOptions.Center, _phFontTitle);
        PhPlace(phone.txtHomeDay.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -40f), new Vector2(480f, 64f));
        phone.txtHomeClock = PhText("Txt_HomeClock", scr.transform, "08.30", 104f, PH_TEXT, TextAlignmentOptions.Center);
        PhPlace(phone.txtHomeClock.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -98f), new Vector2(480f, 120f));
        phone.txtHomeDate = PhText("Txt_HomeDate", scr.transform, "14 September  ·  Hari 1, Pagi", 19f, PH_TEXT_DIM, TextAlignmentOptions.Center);
        PhPlace(phone.txtHomeDate.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -222f), new Vector2(480f, 30f));

        // Ikon aplikasi
        GameObject grid = CreateUIObject("AppGrid", scr.transform);
        PhPlace((RectTransform)grid.transform, new Vector2(0.5f, 0f), new Vector2(0f, 150f), new Vector2(480f, 170f));
        HorizontalLayoutGroup hlg = grid.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 30f;
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.childControlWidth = false;
        hlg.childControlHeight = false;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;

        UnityEventTools.AddPersistentListener(PhAppIcon(grid.transform, "Btn_AppWeTalk", ICON_CHAT, "WeTalk").onClick, phone.OnClick_AppWeTalk);
        UnityEventTools.AddPersistentListener(PhAppIcon(grid.transform, "Btn_AppOuting", ICON_RELATION, "Akhir Pekan").onClick, phone.OnClick_AppOuting);
        UnityEventTools.AddPersistentListener(PhAppIcon(grid.transform, "Btn_AppSave", ICON_SETTING, "Simpan").onClick, phone.OnClick_AppSave);

        TextMeshProUGUI hint = PhText("Txt_Hint", scr.transform, "Esc untuk kembali", 15f, PH_TEXT_DIM, TextAlignmentOptions.Center);
        PhPlace(hint.rectTransform, new Vector2(0.5f, 0f), new Vector2(0f, 40f), new Vector2(480f, 24f));

        phone.screenHome = scr;
    }

    private static void BuildPhoneWeTalk(Transform content, Transform templates, PhoneUIController phone)
    {
        GameObject scr = PhScreen("Screen_WeTalkApp", content);
        phone.screenWeTalkApp = scr;

        // --- Daftar chat ---
        GameObject list = CreateUIObject("Panel_ChatList", scr.transform);
        StretchFull((RectTransform)list.transform);
        PhHeader(list.transform, "WeTalk", false, null);

        Image card = PhRect("Btn_ChatEdelweiss", list.transform, new Vector2(0.5f, 1f), new Vector2(0f, -PH_HEADER_H - 20f), new Vector2(PH_SCREEN_W - 48f, 100f), PH_GLASS);
        PhRounded(card, 0.3f);
        card.raycastTarget = true;
        PhAvatar(card.transform, new Vector2(22f, 0f), 64f);
        TextMeshProUGUI name = PhText("Txt_Name", card.transform, "Edelweiss Mayori", 22f, PH_TEXT, TextAlignmentOptions.TopLeft);
        PhPlace(name.rectTransform, new Vector2(0f, 1f), new Vector2(104f, -22f), new Vector2(360f, 30f));
        TextMeshProUGUI preview = PhText("Txt_Preview", card.transform, "Info broker kampus · tanya kabar teman", 15f, PH_TEXT_DIM, TextAlignmentOptions.TopLeft);
        PhPlace(preview.rectTransform, new Vector2(0f, 1f), new Vector2(104f, -56f), new Vector2(360f, 24f));
        Image dot = PhRect("Img_Online", card.transform, new Vector2(1f, 0.5f), new Vector2(-24f, 0f), new Vector2(14f, 14f), new Color(0.45f, 0.92f, 0.65f, 1f));
        PhRounded(dot, 1f);
        Button btnCard = card.gameObject.AddComponent<Button>();
        btnCard.targetGraphic = card;
        PhTint(btnCard, true);
        UnityEventTools.AddPersistentListener(btnCard.onClick, phone.OnClick_OpenEdelweissChat);
        phone.panelChatList = list;

        // --- Ruang chat ---
        GameObject room = CreateUIObject("Panel_ChatRoom", scr.transform);
        StretchFull((RectTransform)room.transform);

        const float replyAreaH = 156f;
        Image chatBg = PhRect("Img_ChatBg", room.transform, Vector2.zero, Vector2.zero, Vector2.zero, PH_CHAT_BG);
        StretchFull(chatBg.rectTransform);

        Image header = PhHeader(room.transform, "", true, phone);
        PhAvatar(header.transform, new Vector2(70f, 0f), 52f);
        phone.txtChatRoomContactName = PhText("Txt_ChatRoomContactName", header.transform, "Edelweiss Mayori", 22f, PH_TEXT, TextAlignmentOptions.TopLeft);
        PhPlace(phone.txtChatRoomContactName.rectTransform, new Vector2(0f, 1f), new Vector2(136f, -16f), new Vector2(380f, 30f));
        phone.txtChatRoomStatus = PhText("Txt_ChatRoomStatus", header.transform, "Online • Info Broker", 15f, new Color(0.62f, 0.95f, 0.76f, 1f), TextAlignmentOptions.TopLeft);
        PhPlace(phone.txtChatRoomStatus.rectTransform, new Vector2(0f, 1f), new Vector2(136f, -48f), new Vector2(380f, 24f));

        // Scroll pesan
        GameObject scrollGO = CreateUIObject("ScrollView_Messages", room.transform);
        RectTransform rtScroll = (RectTransform)scrollGO.transform;
        StretchFull(rtScroll);
        rtScroll.offsetMin = new Vector2(0f, replyAreaH);
        rtScroll.offsetMax = new Vector2(0f, -PH_HEADER_H);
        scrollGO.AddComponent<RectMask2D>();
        ScrollRect sr = scrollGO.AddComponent<ScrollRect>();
        sr.horizontal = false;
        sr.movementType = ScrollRect.MovementType.Clamped;
        sr.scrollSensitivity = 30f;
        sr.viewport = rtScroll;

        GameObject msgs = CreateUIObject("Content", scrollGO.transform);
        RectTransform rtMsgs = (RectTransform)msgs.transform;
        rtMsgs.anchorMin = new Vector2(0f, 1f);
        rtMsgs.anchorMax = new Vector2(1f, 1f);
        rtMsgs.pivot = new Vector2(0.5f, 1f);
        rtMsgs.sizeDelta = Vector2.zero;
        VerticalLayoutGroup vlg = msgs.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(16, 16, 16, 16);
        vlg.spacing = 10f;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        msgs.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        sr.content = rtMsgs;

        phone.messageScrollRect = sr;
        phone.messageContainer = msgs.transform;

        // Pertanyaan cepat (diisi runtime dari tbl_npc_preferences)
        Image replyArea = PhBand("QuickReplyArea", room.transform, false, replyAreaH, PH_BAR);
        GridLayoutGroup grid = replyArea.gameObject.AddComponent<GridLayoutGroup>();
        grid.padding = new RectOffset(18, 18, 20, 20);
        grid.cellSize = new Vector2((PH_SCREEN_W - 36f - 12f) / 2f, 52f);
        grid.spacing = new Vector2(12f, 12f);
        grid.childAlignment = TextAnchor.UpperCenter;
        phone.chatInputArea = replyArea.gameObject;
        phone.quickReplyContainer = replyArea.transform;

        Image chip = PhRect("Tpl_QuickReply", replyArea.transform, Vector2.zero, Vector2.zero, Vector2.zero, PH_TEXT);
        PhRounded(chip, 0.25f);
        chip.raycastTarget = true;
        TextMeshProUGUI chipTxt = PhText("Txt_Label", chip.transform, "Tanya soal ...", 16f, PH_NAVY, TextAlignmentOptions.Center);
        StretchFull(chipTxt.rectTransform);
        chipTxt.rectTransform.offsetMin = new Vector2(10f, 0f);
        chipTxt.rectTransform.offsetMax = new Vector2(-10f, 0f);
        chipTxt.enableAutoSizing = true;
        chipTxt.fontSizeMin = 12f;
        chipTxt.fontSizeMax = 16f;
        Button chipBtn = chip.gameObject.AddComponent<Button>();
        chipBtn.targetGraphic = chip;
        PhTint(chipBtn, false);
        chip.gameObject.SetActive(false);
        phone.quickReplyTemplate = chipBtn;

        // Template bubble
        phone.bubbleNpcPrefab = PhBubbleTemplate(templates, "Tpl_BubbleNpc", false);
        phone.bubblePlayerPrefab = PhBubbleTemplate(templates, "Tpl_BubblePlayer", true);

        room.SetActive(false);
        phone.panelChatRoom = room;
    }

    private static void BuildPhoneOuting(Transform content, PhoneUIController phone)
    {
        GameObject scr = PhScreen("Screen_OutingApp", content);
        PhHeader(scr.transform, "Akhir Pekan", false, null);

        phone.txtWeekendWarning = PhText("Txt_WeekendWarning", scr.transform, "<b>Belum akhir pekan</b>", 19f, PH_TEXT, TextAlignmentOptions.Top);
        phone.txtWeekendWarning.textWrappingMode = TextWrappingModes.Normal;
        PhPlace(phone.txtWeekendWarning.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -PH_HEADER_H - 60f), new Vector2(PH_SCREEN_W - 80f, 140f));

        // Template opsi (kontak & venue) disimpan nonaktif di container kontak
        phone.panelOutingSelectContact = PhListPanel("Panel_SelectContact", scr.transform, "Pilih teman", out Transform contacts, out _);
        phone.outingContactContainer = contacts;
        phone.outingOptionTemplate = PhOptionTemplate(contacts);

        phone.panelOutingSelectVenue = PhListPanel("Panel_SelectVenue", scr.transform, "Ajak ke mana?", out Transform venues, out TextMeshProUGUI venueTitle);
        phone.venueCardsContainer = venues;
        phone.txtVenueTitle = venueTitle;
        phone.panelOutingSelectVenue.SetActive(false);

        phone.screenOutingApp = scr;
    }

    private static void BuildPhoneSave(Transform content, PhoneUIController phone)
    {
        GameObject scr = PhScreen("Screen_SaveApp", content);
        PhHeader(scr.transform, "Simpan & Muat", false, null);

        // Quick save
        Image quick = PhRect("Card_QuickSave", scr.transform, new Vector2(0.5f, 1f), new Vector2(0f, -PH_HEADER_H - 10f), new Vector2(PH_SCREEN_W - 40f, 104f), PH_GLASS);
        PhRounded(quick, 0.3f);
        TextMeshProUGUI qLabel = PhText("Txt_Label", quick.transform, "QUICK SAVE", 14f, PH_TEXT_DIM, TextAlignmentOptions.TopLeft);
        qLabel.characterSpacing = 10f;
        PhPlace(qLabel.rectTransform, new Vector2(0f, 1f), new Vector2(20f, -12f), new Vector2(200f, 20f));
        phone.txtQuickSaveInfo = PhText("Txt_QuickInfo", quick.transform, "<i>Belum ada quick save.</i>", 16f, PH_TEXT, TextAlignmentOptions.TopLeft);
        PhPlace(phone.txtQuickSaveInfo.rectTransform, new Vector2(0f, 1f), new Vector2(20f, -36f), new Vector2(370f, 60f));
        phone.txtQuickSaveInfo.textWrappingMode = TextWrappingModes.Normal;
        phone.btnQuickSaveApp = PhPill(quick.transform, "Btn_QuickSave", "Simpan", new Vector2(-20f, -18f), PH_ACCENT, PH_NAVY);
        phone.btnQuickLoadApp = PhPill(quick.transform, "Btn_QuickLoad", "Muat", new Vector2(-20f, -60f), new Color(1f, 1f, 1f, 0.16f), PH_TEXT);
        UnityEventTools.AddPersistentListener(phone.btnQuickSaveApp.onClick, phone.OnClick_PhoneQuickSave);
        UnityEventTools.AddPersistentListener(phone.btnQuickLoadApp.onClick, phone.OnClick_PhoneQuickLoad);

        // Slot manual 1-5 (listener diisi PhoneSaveSlotItemUI.Setup saat runtime)
        phone.manualSlotItems = new PhoneSaveSlotItemUI[5];
        for (int i = 0; i < 5; i++)
        {
            float y = -PH_HEADER_H - 124f - i * 78f;
            Image row = PhRect($"Slot_{i + 1}", scr.transform, new Vector2(0.5f, 1f), new Vector2(0f, y), new Vector2(PH_SCREEN_W - 40f, 72f), new Color(1f, 1f, 1f, 0.06f));
            PhRounded(row, 0.3f);
            PhoneSaveSlotItemUI item = row.gameObject.AddComponent<PhoneSaveSlotItemUI>();
            item.slotId = i + 1;
            item.txtSlotTitle = PhText("Txt_Title", row.transform, $"Slot {i + 1}  ·  Kosong", 17f, PH_TEXT, TextAlignmentOptions.TopLeft);
            PhPlace(item.txtSlotTitle.rectTransform, new Vector2(0f, 1f), new Vector2(20f, -8f), new Vector2(370f, 24f));
            item.txtSlotDetails = PhText("Txt_Details", row.transform, "Belum ada cerita tersimpan.", 14f, PH_TEXT_DIM, TextAlignmentOptions.TopLeft);
            PhPlace(item.txtSlotDetails.rectTransform, new Vector2(0f, 1f), new Vector2(20f, -32f), new Vector2(370f, 20f));
            item.txtSlotDate = PhText("Txt_Date", row.transform, "", 12f, PH_TEXT_DIM, TextAlignmentOptions.TopLeft);
            PhPlace(item.txtSlotDate.rectTransform, new Vector2(0f, 1f), new Vector2(20f, -50f), new Vector2(370f, 18f));
            item.btnSave = PhPill(row.transform, "Btn_Save", "Simpan", new Vector2(-20f, -6f), PH_ACCENT, PH_NAVY, 30f);
            item.btnLoad = PhPill(row.transform, "Btn_Load", "Muat", new Vector2(-20f, -38f), new Color(1f, 1f, 1f, 0.16f), PH_TEXT, 30f);
            phone.manualSlotItems[i] = item;
        }

        phone.txtSaveAppStatus = PhText("Txt_SaveAppStatus", scr.transform, "Pilih slot untuk menyimpan atau memuat cerita.", 15f, PH_TEXT_DIM, TextAlignmentOptions.Center);
        PhPlace(phone.txtSaveAppStatus.rectTransform, new Vector2(0.5f, 0f), new Vector2(0f, 10f), new Vector2(PH_SCREEN_W - 40f, 26f));

        phone.screenSaveApp = scr;
    }

    // -------------------------------------------------------------------------
    // HELPER
    // -------------------------------------------------------------------------
    private static Image PhRect(string name, Transform parent, Vector2 anchor, Vector2 pos, Vector2 size, Color color)
    {
        GameObject go = CreateUIObject(name, parent);
        PhPlace((RectTransform)go.transform, anchor, pos, size);
        Image img = go.AddComponent<Image>();
        img.color = color;
        img.raycastTarget = false;
        return img;
    }

    private static void PhPlace(RectTransform rt, Vector2 anchor, Vector2 pos, Vector2 size)
    {
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = anchor;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
    }

    // Kolom setinggi induk, mulai dari 'left' px dengan lebar tetap
    private static void PhLeftColumn(RectTransform rt, float left, float width)
    {
        rt.anchorMin = new Vector2(0f, 0f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 0.5f);
        rt.offsetMin = new Vector2(left, 0f);
        rt.offsetMax = new Vector2(left + width, 0f);
    }

    private static void PhRounded(Image img, float roundness)
    {
        img.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        img.type = Image.Type.Sliced;
        img.pixelsPerUnitMultiplier = roundness;
    }

    // Pita selebar layar yang menempel di atas/bawah induk
    private static Image PhBand(string name, Transform parent, bool top, float height, Color color)
    {
        Image img = PhRect(name, parent, Vector2.zero, Vector2.zero, Vector2.zero, color);
        RectTransform rt = img.rectTransform;
        rt.anchorMin = new Vector2(0f, top ? 1f : 0f);
        rt.anchorMax = new Vector2(1f, top ? 1f : 0f);
        rt.pivot = new Vector2(0.5f, top ? 1f : 0f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(0f, height);
        return img;
    }

    private static TextMeshProUGUI PhText(string name, Transform parent, string text, float size, Color color, TextAlignmentOptions align, TMP_FontAsset font = null)
    {
        TextMeshProUGUI t = CreateText(name, parent, text, size, color);
        t.font = font != null ? font : _phFont;
        t.alignment = align;
        t.textWrappingMode = TextWrappingModes.NoWrap;
        return t;
    }

    // Tint tombol. Elemen transparan (kaca) harus MENERANG saat hover, jadi pakai colorMultiplier:
    // normal 0.4 x 2.5 = warna asli, hover 1.0 x 2.5 = alpha ~2.5x lebih terang.
    private static void PhTint(Button btn, bool translucent)
    {
        ColorBlock cb = btn.colors;
        if (translucent)
        {
            cb.normalColor = new Color(0.4f, 0.4f, 0.4f, 0.4f);
            cb.highlightedColor = new Color(1f, 1f, 1f, 1f);
            cb.pressedColor = new Color(0.8f, 0.8f, 0.8f, 0.8f);
            cb.selectedColor = cb.normalColor;
            cb.disabledColor = new Color(0.4f, 0.4f, 0.4f, 0.16f);
            cb.colorMultiplier = 2.5f;
        }
        else
        {
            cb.highlightedColor = new Color(0.88f, 0.90f, 1f, 1f);
            cb.pressedColor = new Color(0.75f, 0.78f, 0.92f, 1f);
            cb.selectedColor = Color.white;
            cb.disabledColor = new Color(1f, 1f, 1f, 0.35f);
        }
        cb.fadeDuration = 0.1f;
        btn.colors = cb;
    }

    private static GameObject PhScreen(string name, Transform content)
    {
        GameObject scr = CreateUIObject(name, content);
        StretchFull((RectTransform)scr.transform);
        return scr;
    }

    private static Image PhHeader(Transform parent, string title, bool withBack, PhoneUIController phone)
    {
        Image header = PhBand("Header", parent, true, PH_HEADER_H, PH_BAR);
        if (!string.IsNullOrEmpty(title))
        {
            TextMeshProUGUI t = PhText("Txt_Title", header.transform, title, 30f, PH_TEXT, TextAlignmentOptions.MidlineLeft);
            PhLeftColumn(t.rectTransform, withBack ? 70f : 28f, 420f);
        }
        if (withBack && phone != null)
        {
            Image chev = PhRect("Btn_Back", header.transform, new Vector2(0f, 0.5f), new Vector2(18f, 0f), new Vector2(40f, 40f), Color.white);
            chev.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(ICON_CHEV_LEFT);
            chev.preserveAspect = true;
            chev.raycastTarget = true;
            Button b = chev.gameObject.AddComponent<Button>();
            b.targetGraphic = chev;
            UnityEventTools.AddPersistentListener(b.onClick, phone.OnClick_NavBack);
        }
        return header;
    }

    private static void PhStatusIcon(Transform bar, string spritePath, float x)
    {
        Image img = PhRect("Img_" + System.IO.Path.GetFileNameWithoutExtension(spritePath), bar, new Vector2(1f, 0.5f), new Vector2(x, 0f), new Vector2(24f, 24f), Color.white);
        img.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
        img.preserveAspect = true;
    }

    private static Button PhNavButton(Transform nav, string name, string iconPath, string label)
    {
        GameObject go = CreateUIObject(name, nav);
        Image hit = go.AddComponent<Image>();
        hit.color = new Color(1f, 1f, 1f, 0f); // area klik transparan
        Button btn = go.AddComponent<Button>();
        btn.targetGraphic = hit;

        if (iconPath != null)
        {
            Image icon = PhRect("Img_Icon", go.transform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(32f, 32f), PH_TEXT);
            icon.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(iconPath);
            icon.preserveAspect = true;
        }
        else
        {
            TextMeshProUGUI t = PhText("Txt_Label", go.transform, label, 18f, PH_TEXT, TextAlignmentOptions.Center);
            StretchFull(t.rectTransform);
        }
        return btn;
    }

    private static Button PhAppIcon(Transform grid, string name, string iconPath, string label)
    {
        GameObject go = CreateUIObject(name, grid);
        ((RectTransform)go.transform).sizeDelta = new Vector2(130f, 160f);

        Image tile = PhRect("Img_Tile", go.transform, new Vector2(0.5f, 1f), Vector2.zero, new Vector2(112f, 112f), Color.white);
        tile.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITE_APP_BG);
        tile.type = Image.Type.Sliced;
        tile.raycastTarget = true;
        Image icon = PhRect("Img_Icon", tile.transform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(56f, 56f), PH_TEXT);
        icon.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(iconPath);
        icon.preserveAspect = true;

        TextMeshProUGUI t = PhText("Txt_Label", go.transform, label, 18f, PH_TEXT, TextAlignmentOptions.Center);
        PhPlace(t.rectTransform, new Vector2(0.5f, 0f), Vector2.zero, new Vector2(140f, 34f));

        Button btn = go.AddComponent<Button>();
        btn.targetGraphic = tile;
        PhTint(btn, false);
        return btn;
    }

    private static void PhAvatar(Transform parent, Vector2 leftPos, float size)
    {
        Image av = PhRect("Img_Avatar", parent, new Vector2(0f, 0.5f), leftPos, new Vector2(size, size), PH_ACCENT);
        av.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITE_CIRCLE);
        TextMeshProUGUI initial = PhText("Txt_Initial", av.transform, "E", size * 0.5f, PH_NAVY, TextAlignmentOptions.Center);
        StretchFull(initial.rectTransform);
    }

    private static Button PhPill(Transform parent, string name, string label, Vector2 topRightPos, Color bg, Color fg, float height = 34f)
    {
        Image img = PhRect(name, parent, new Vector2(1f, 1f), topRightPos, new Vector2(96f, height), bg);
        PhRounded(img, 0.25f);
        img.raycastTarget = true;
        TextMeshProUGUI t = PhText("Txt_Label", img.transform, label, 15f, fg, TextAlignmentOptions.Center);
        StretchFull(t.rectTransform);
        Button btn = img.gameObject.AddComponent<Button>();
        btn.targetGraphic = img;
        PhTint(btn, bg.a < 0.5f);
        return btn;
    }

    // Panel daftar (judul + VerticalLayout) di bawah header
    private static GameObject PhListPanel(string name, Transform parent, string title, out Transform list, out TextMeshProUGUI titleText)
    {
        GameObject panel = CreateUIObject(name, parent);
        RectTransform rt = (RectTransform)panel.transform;
        StretchFull(rt);
        rt.offsetMax = new Vector2(0f, -PH_HEADER_H);

        titleText = PhText("Txt_ListTitle", panel.transform, title, 20f, PH_TEXT_DIM, TextAlignmentOptions.MidlineLeft);
        PhPlace(titleText.rectTransform, new Vector2(0f, 1f), new Vector2(24f, -14f), new Vector2(PH_SCREEN_W - 48f, 30f));

        GameObject listGO = CreateUIObject("List", panel.transform);
        RectTransform rtList = (RectTransform)listGO.transform;
        StretchFull(rtList);
        rtList.offsetMin = new Vector2(20f, 10f);
        rtList.offsetMax = new Vector2(-20f, -56f);
        VerticalLayoutGroup vlg = listGO.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 10f;
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        list = listGO.transform;
        return panel;
    }

    private static Button PhOptionTemplate(Transform container)
    {
        Image bg = PhRect("Tpl_Option", container, Vector2.zero, Vector2.zero, Vector2.zero, PH_GLASS);
        PhRounded(bg, 0.3f);
        bg.raycastTarget = true;
        bg.gameObject.AddComponent<LayoutElement>().preferredHeight = 78f;

        PhRect("Img_Mark", bg.transform, new Vector2(0f, 0.5f), new Vector2(16f, 0f), new Vector2(4f, 30f), PH_ACCENT);
        TextMeshProUGUI title = PhText("Txt_Title", bg.transform, "Judul", 21f, PH_TEXT, TextAlignmentOptions.TopLeft);
        PhPlace(title.rectTransform, new Vector2(0f, 1f), new Vector2(34f, -12f), new Vector2(440f, 28f));
        TextMeshProUGUI sub = PhText("Txt_Sub", bg.transform, "Keterangan", 15f, PH_TEXT_DIM, TextAlignmentOptions.TopLeft);
        PhPlace(sub.rectTransform, new Vector2(0f, 1f), new Vector2(34f, -44f), new Vector2(440f, 22f));

        Button btn = bg.gameObject.AddComponent<Button>();
        btn.targetGraphic = bg;
        PhTint(btn, true);
        bg.gameObject.SetActive(false);
        return btn;
    }

    // Baris bubble: HorizontalLayout rata kiri/kanan berisi bubble (sprite mockup) yang selebar teksnya
    private static GameObject PhBubbleTemplate(Transform templates, string name, bool fromPlayer)
    {
        GameObject row = CreateUIObject(name, templates);
        HorizontalLayoutGroup hlg = row.AddComponent<HorizontalLayoutGroup>();
        hlg.childAlignment = fromPlayer ? TextAnchor.UpperRight : TextAnchor.UpperLeft;
        hlg.childControlWidth = true;
        hlg.childControlHeight = true;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;

        GameObject bubble = CreateUIObject("Bubble", row.transform);
        Image img = bubble.AddComponent<Image>();
        img.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(fromPlayer ? SPRITE_BUBBLE_OUTGOING : SPRITE_BUBBLE_INCOMING);
        img.type = Image.Type.Sliced;
        img.color = fromPlayer ? PH_NAVY : Color.white;
        img.raycastTarget = false;
        VerticalLayoutGroup vlg = bubble.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(18, 18, 12, 12);
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        vlg.childForceExpandWidth = false;
        vlg.childForceExpandHeight = false;

        TextMeshProUGUI body = PhText("Txt_Body", bubble.transform, "Pesan", 21f, fromPlayer ? PH_TEXT : PH_NAVY, TextAlignmentOptions.TopLeft, _phFontChat);
        body.textWrappingMode = TextWrappingModes.Normal;
        body.gameObject.AddComponent<LayoutElement>(); // lebar diisi PhoneUIController sesuai isi pesan
        return row;
    }
}
#endif
