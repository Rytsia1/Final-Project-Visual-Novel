#if UNITY_EDITOR
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

public static class MainMenuSceneBuilder
{
    private const string SCENE_PATH = "Assets/Scenes/MainMenuScene.unity";
    private const string PREFAB_PATH = "Assets/Prefabs/LoadSlotItemPrefab.prefab";
    private const string BG_TEXTURE_PATH = "Assets/Textures/main_menu_bg.jpg";

    private const string GAME_TITLE = "Under the Same Sky";
    private const string SPRITE_LEFT_SHADE ="Assets/Textures/UI_MainMenu/menu_left_shade.png";

    private const string FONT_CAVEAT_SDF = "Assets/Fonts/CaveatBrush-Regular SDF.asset";
    private const string FONT_POPPINS_SDF = "Assets/Fonts/Poppins-Bold SDF.asset";
    private const string FONT_YAHEI_SDF = "Assets/Fonts/MicrosoftYaHei SDF.asset";

    private static readonly Color COLOR_ACCENT = new Color(1f, 0.78f, 0.86f, 1f); // pink sakura
    private const float MENU_ROW_HEIGHT = 62f;

    [MenuItem("Game Debug/Build Main Menu Scene (Model 3)")]
    public static void BuildSceneMenu()
    {
        BuildMainMenuScene();
    }

    public static void BuildMainMenuScene()
    {
        Debug.Log("<color=cyan>=== MEMULAI PEMBANGUNAN MAIN MENU SCENE (MODEL 3) ===</color>");

        // 1. Pastikan seluruh sprite di-import dengan benar
        ConfigureTextures();

        // 2. Load Font Assets
        TMP_FontAsset fontCaveat = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FONT_CAVEAT_SDF);
        TMP_FontAsset fontPoppins = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FONT_POPPINS_SDF);
        TMP_FontAsset fontYaHei = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FONT_YAHEI_SDF);
        Material matCaveat = EnsureShadowMaterial(fontCaveat, "Assets/Fonts/CaveatBrush-Regular SDF - MenuShadow.mat");
        Material matPoppins = EnsureShadowMaterial(fontPoppins, "Assets/Fonts/Poppins-Bold SDF - MenuShadow.mat");
        Material matYaHei = EnsureShadowMaterial(fontYaHei, "Assets/Fonts/MicrosoftYaHei SDF - MenuShadow.mat");
        Sprite spLeftShade = EnsureLeftShadeSprite();

        // 3. Buat atau perbarui Prefab Item Slot Simpanan
        GameObject slotPrefab = CreateOrUpdateSlotPrefab();

        // 4. Buat Scene Baru
        var newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // 4b. Buat Main Camera
        GameObject camGO = new GameObject("Main Camera");
        Camera cam = camGO.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.965f, 0.973f, 1f, 1f);
        cam.tag = "MainCamera";
        camGO.AddComponent<AudioListener>();

        // 5. Buat EventSystem
        GameObject esGO = new GameObject("EventSystem");
        esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
#if ENABLE_INPUT_SYSTEM
        esGO.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
        esGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
#endif

        // 6. Buat Canvas (1440 x 840 Reference)
        GameObject canvasGO = new GameObject("Canvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<GraphicRaycaster>();

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        // 7. Img_Background (Full Screen, envelope agar tidak gepeng di rasio layar lain)
        GameObject bgGO = CreateUIObject("Img_Background", canvasGO.transform);
        StretchFull(bgGO.GetComponent<RectTransform>());
        Image imgBg = bgGO.AddComponent<Image>();
        Sprite bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>(BG_TEXTURE_PATH);
        if (bgSprite != null)
        {
            imgBg.sprite = bgSprite;
            imgBg.color = Color.white;
            AspectRatioFitter arf = bgGO.AddComponent<AspectRatioFitter>();
            arf.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            arf.aspectRatio = bgSprite.rect.width / bgSprite.rect.height;
        }
        else
        {
            imgBg.color = new Color(0.95f, 0.95f, 1f, 1f);
        }
        imgBg.raycastTarget = false;

        // 8. Bayangan navy lembut dari kiri agar judul & menu terbaca di atas langit terang
        GameObject shadeGO = CreateUIObject("Img_LeftShade", canvasGO.transform);
        RectTransform rtShade = shadeGO.GetComponent<RectTransform>();
        rtShade.anchorMin = new Vector2(0f, 0f);
        rtShade.anchorMax = new Vector2(0f, 1f);
        rtShade.pivot = new Vector2(0f, 0.5f);
        rtShade.anchoredPosition = Vector2.zero;
        rtShade.sizeDelta = new Vector2(1250f, 0f);
        Image imgShade = shadeGO.AddComponent<Image>();
        imgShade.sprite = spLeftShade;
        imgShade.raycastTarget = false;

        // 9. Judul: "Under the" / "Same Sky" + subjudul Mandarin
        GameObject titleGO = CreateUIObject("Container_Title", canvasGO.transform);
        RectTransform rtTitle = titleGO.GetComponent<RectTransform>();
        rtTitle.anchorMin = new Vector2(0f, 1f);
        rtTitle.anchorMax = new Vector2(0f, 1f);
        rtTitle.pivot = new Vector2(0f, 1f);
        rtTitle.anchoredPosition = new Vector2(140f, -110f);
        rtTitle.sizeDelta = new Vector2(1100f, 420f);

        CreateTitleText("Txt_TitleSmall", titleGO.transform, "Under the", fontCaveat, matCaveat, 104f, new Vector2(8f, 0f), 120f);
        CreateTitleText("Txt_TitleMain", titleGO.transform, "Same Sky", fontCaveat, matCaveat, 230f, new Vector2(0f, -76f), 230f);

        GameObject accentGO = CreateUIObject("Img_TitleAccent", titleGO.transform);
        RectTransform rtAccent = accentGO.GetComponent<RectTransform>();
        rtAccent.anchorMin = new Vector2(0f, 1f);
        rtAccent.anchorMax = new Vector2(0f, 1f);
        rtAccent.pivot = new Vector2(0f, 0.5f);
        rtAccent.anchoredPosition = new Vector2(14f, -338f);
        rtAccent.sizeDelta = new Vector2(72f, 5f);
        Image imgAccent = accentGO.AddComponent<Image>();
        imgAccent.color = COLOR_ACCENT;
        imgAccent.raycastTarget = false;

        TextMeshProUGUI txtTitleSub = CreateTitleText("Txt_TitleSub", titleGO.transform, "同一片天空下", fontYaHei, matYaHei, 34f, new Vector2(104f, -316f), 44f);
        txtTitleSub.characterSpacing = 40f;
        txtTitleSub.color = new Color(1f, 1f, 1f, 0.9f);

        // 10. Menu aksi (kiri bawah, di bawah judul)
        GameObject buttonsCont = CreateUIObject("Container_ActionButtons", canvasGO.transform);
        RectTransform rtBtnsCont = buttonsCont.GetComponent<RectTransform>();
        rtBtnsCont.anchorMin = new Vector2(0f, 0f);
        rtBtnsCont.anchorMax = new Vector2(0f, 0f);
        rtBtnsCont.pivot = new Vector2(0f, 0f);
        rtBtnsCont.anchoredPosition = new Vector2(126f, 110f);
        rtBtnsCont.sizeDelta = new Vector2(400f, 5 * MENU_ROW_HEIGHT + 4 * 6f);

        VerticalLayoutGroup vlgMenu = buttonsCont.AddComponent<VerticalLayoutGroup>();
        vlgMenu.spacing = 6f;
        vlgMenu.childAlignment = TextAnchor.LowerLeft;
        vlgMenu.childControlWidth = true;
        vlgMenu.childControlHeight = true;
        vlgMenu.childForceExpandWidth = true;
        vlgMenu.childForceExpandHeight = false;

        Sprite spRounded = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        Button btnNew = CreateMenuButton("Btn_NewStory", buttonsCont.transform, "New Story", spRounded, fontPoppins, matPoppins);
        Button btnContinue = CreateMenuButton("Btn_Continue", buttonsCont.transform, "Continue", spRounded, fontPoppins, matPoppins);
        Button btnLoad = CreateMenuButton("Btn_LoadStory", buttonsCont.transform, "Load Story", spRounded, fontPoppins, matPoppins);
        Button btnConfig = CreateMenuButton("Btn_Config", buttonsCont.transform, "Config", spRounded, fontPoppins, matPoppins);
        Button btnQuit = CreateMenuButton("Btn_Quit", buttonsCont.transform, "Quit", spRounded, fontPoppins, matPoppins);

        // 11. Label kaki kiri bawah (hak cipta + versi)
        TextMeshProUGUI txtFooter = CreateText("Txt_FooterLabel", canvasGO.transform, "© 2026 Your Studio. All Rights Reserved.   v" + Application.version, 18, new Color(1f, 1f, 1f, 0.85f));
        if (fontPoppins != null) txtFooter.font = fontPoppins;
        if (matPoppins != null) txtFooter.fontSharedMaterial = matPoppins;
        RectTransform rtFooter = txtFooter.GetComponent<RectTransform>();
        rtFooter.anchorMin = new Vector2(0f, 0f);
        rtFooter.anchorMax = new Vector2(0f, 0f);
        rtFooter.pivot = new Vector2(0f, 0f);
        rtFooter.anchoredPosition = new Vector2(146f, 40f);
        rtFooter.sizeDelta = new Vector2(900f, 30f);
        txtFooter.alignment = TextAlignmentOptions.BottomLeft;

        // 16. Modal_LoadStory (Default Inactive)
        Button btnCloseLoad = null;
        Transform slotsContent = null;
        TextMeshProUGUI txtEmptySaves = null;
        GameObject modalLoadStory = BuildModalLoadStory(canvasGO.transform, out btnCloseLoad, out slotsContent, out txtEmptySaves);

        // 17. Modal_Config (Default Inactive)
        Button btnCloseConfig = null;
        Slider slBgm = null, slSfx = null, slTextSpeed = null;
        GameObject modalConfig = BuildModalConfig(canvasGO.transform, out btnCloseConfig, out slBgm, out slSfx, out slTextSpeed);

        // 18. Modal_Extras (Default Inactive)
        Button btnCloseExtras = null;
        GameObject modalExtras = BuildModalExtras(canvasGO.transform, out btnCloseExtras);

        // 19. GameController Object
        GameObject ctrlGO = new GameObject("GameController");
        DatabaseManager dbMgr = ctrlGO.AddComponent<DatabaseManager>();
        SaveManager saveMgr = ctrlGO.AddComponent<SaveManager>();
        MainMenuController menuCtrl = ctrlGO.AddComponent<MainMenuController>();

        // Wire references ke MainMenuController
        menuCtrl.btnNewStory = btnNew;
        menuCtrl.btnContinue = btnContinue;
        menuCtrl.btnLoadStory = btnLoad;
        menuCtrl.btnConfig = btnConfig;
        menuCtrl.btnExtras = null;
        menuCtrl.btnQuit = btnQuit;

        menuCtrl.modalLoadStory = modalLoadStory;
        menuCtrl.modalConfig = modalConfig;
        menuCtrl.modalExtras = modalExtras;

        menuCtrl.slotsContainer = slotsContent;
        menuCtrl.slotItemPrefab = slotPrefab;
        menuCtrl.btnCloseLoadStory = btnCloseLoad;
        menuCtrl.txtEmptySaves = txtEmptySaves;

        menuCtrl.btnCloseConfig = btnCloseConfig;
        menuCtrl.sliderBgm = slBgm;
        menuCtrl.sliderSfx = slSfx;
        menuCtrl.sliderTextSpeed = slTextSpeed;

        menuCtrl.btnCloseExtras = btnCloseExtras;
        menuCtrl.gameplaySceneName = "SampleScene";

        EditorUtility.SetDirty(menuCtrl);
        EditorUtility.SetDirty(canvasGO);

        // 20. Simpan Scene
        EditorSceneManager.SaveScene(newScene, SCENE_PATH);
        Debug.Log($"<color=green>[MainMenuSceneBuilder]</color> Scene berhasil disimpan ke: {SCENE_PATH}");

        // 21. Konfigurasi Build Settings
        UpdateBuildSettings();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("<color=green>=== PEMBANGUNAN MAIN MENU SCENE MODEL 3 SELESAI ===</color>");
    }

    private static TextMeshProUGUI CreateTitleText(string name, Transform parent, string text, TMP_FontAsset font, Material mat, float size, Vector2 pos, float height)
    {
        TextMeshProUGUI tmp = CreateText(name, parent, text, size, Color.white);
        if (font != null) tmp.font = font;
        if (mat != null) tmp.fontSharedMaterial = mat;
        RectTransform rt = tmp.rectTransform;
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(1100f, height);
        tmp.alignment = TextAlignmentOptions.TopLeft;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;
        return tmp;
    }

    // Baris menu: pill putih transparan yang hanya muncul saat hover/pressed, label putih ber-bayangan.
    private static Button CreateMenuButton(string name, Transform parent, string label, Sprite rounded, TMP_FontAsset font, Material mat)
    {
        GameObject btnRoot = CreateUIObject(name, parent);
        LayoutElement le = btnRoot.AddComponent<LayoutElement>();
        le.preferredHeight = MENU_ROW_HEIGHT;

        Image imgHover = btnRoot.AddComponent<Image>();
        imgHover.sprite = rounded;
        imgHover.type = Image.Type.Sliced;
        imgHover.pixelsPerUnitMultiplier = 0.35f; // sudut lebih bulat
        imgHover.color = Color.white;

        GameObject markGO = CreateUIObject("Img_Mark", btnRoot.transform);
        RectTransform rtMark = markGO.GetComponent<RectTransform>();
        rtMark.anchorMin = new Vector2(0f, 0.5f);
        rtMark.anchorMax = new Vector2(0f, 0.5f);
        rtMark.pivot = new Vector2(0f, 0.5f);
        rtMark.anchoredPosition = new Vector2(20f, 0f);
        rtMark.sizeDelta = new Vector2(4f, 22f);
        Image imgMark = markGO.AddComponent<Image>();
        imgMark.color = COLOR_ACCENT;
        imgMark.raycastTarget = false;

        TextMeshProUGUI txt = CreateText("Txt_Label", btnRoot.transform, label, 34, Color.white);
        if (font != null) txt.font = font;
        if (mat != null) txt.fontSharedMaterial = mat;
        txt.characterSpacing = 3f;
        RectTransform rtText = txt.rectTransform;
        StretchFull(rtText);
        rtText.offsetMin = new Vector2(44f, 0f);

        Button btn = btnRoot.AddComponent<Button>();
        btn.targetGraphic = imgHover;
        ColorBlock cb = btn.colors;
        cb.normalColor = new Color(1f, 1f, 1f, 0f);
        cb.highlightedColor = new Color(1f, 1f, 1f, 0.24f);
        cb.pressedColor = new Color(1f, 1f, 1f, 0.38f);
        cb.selectedColor = new Color(1f, 1f, 1f, 0f); // jangan tertahan menyala setelah diklik
        cb.disabledColor = new Color(1f, 1f, 1f, 0f);
        cb.fadeDuration = 0.12f;
        btn.colors = cb;

        btnRoot.AddComponent<CanvasGroup>(); // MainMenuController meredupkan Continue lewat alpha ini

        return btn;
    }

    // Preset material TMP dengan underlay (bayangan lembut navy), disimpan berdampingan dengan font-nya.
    private static Material EnsureShadowMaterial(TMP_FontAsset font, string assetPath)
    {
        if (font == null) return null;

        Material mat = new Material(font.material);
        mat.name = Path.GetFileNameWithoutExtension(assetPath);
        mat.EnableKeyword("UNDERLAY_ON");
        mat.SetColor("_UnderlayColor", new Color(0.12f, 0.14f, 0.25f, 0.55f));
        mat.SetFloat("_UnderlayOffsetX", 0.5f);
        mat.SetFloat("_UnderlayOffsetY", -0.6f);
        mat.SetFloat("_UnderlayDilate", 0.15f);
        mat.SetFloat("_UnderlaySoftness", 0.55f);

        Material existing = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
        if (existing == null)
        {
            AssetDatabase.CreateAsset(mat, assetPath);
            return mat;
        }
        EditorUtility.CopySerialized(mat, existing);
        return existing;
    }

    // Gradasi horizontal navy -> transparan, digenerate sekali ke PNG lalu di-import sebagai sprite.
    private static Sprite EnsureLeftShadeSprite()
    {
        if (!File.Exists(SPRITE_LEFT_SHADE))
        {
            const int w = 256;
            Texture2D tex = new Texture2D(w, 4, TextureFormat.RGBA32, false);
            for (int x = 0; x < w; x++)
            {
                float a = 0.6f * (1f - Mathf.SmoothStep(0f, 1f, x / (w - 1f)));
                Color c = new Color(0.12f, 0.14f, 0.25f, a);
                for (int y = 0; y < 4; y++) tex.SetPixel(x, y, c);
            }
            File.WriteAllBytes(SPRITE_LEFT_SHADE, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(SPRITE_LEFT_SHADE);
        }

        ConfigureSpriteTexture(SPRITE_LEFT_SHADE);
        TextureImporter ti = AssetImporter.GetAtPath(SPRITE_LEFT_SHADE) as TextureImporter;
        if (ti != null && ti.wrapMode != TextureWrapMode.Clamp)
        {
            ti.wrapMode = TextureWrapMode.Clamp;
            ti.SaveAndReimport();
        }
        return AssetDatabase.LoadAssetAtPath<Sprite>(SPRITE_LEFT_SHADE);
    }

    private static void ConfigureTextures()
    {
        ConfigureSpriteTexture(BG_TEXTURE_PATH);
    }

    private static void ConfigureSpriteTexture(string path)
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
            if (changed)
            {
                ti.SaveAndReimport();
            }
        }
    }

    private static Button CreatePillMenuButton(string name, Transform parent, string title, string subtitle, TMP_FontAsset subFont = null)
    {
        GameObject btnGO = CreateUIObject(name, parent);
        RectTransform rt = btnGO.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(360f, 74f);

        LayoutElement le = btnGO.AddComponent<LayoutElement>();
        le.preferredWidth = 360f;
        le.preferredHeight = 74f;

        Image img = btnGO.AddComponent<Image>();
        img.color = new Color(1f, 0.94f, 0.96f, 0.96f);

        Outline outline = btnGO.AddComponent<Outline>();
        outline.effectColor = new Color(0.96f, 0.65f, 0.78f, 0.9f);
        outline.effectDistance = new Vector2(2f, -2f);

        Shadow shadow = btnGO.AddComponent<Shadow>();
        shadow.effectColor = new Color(0.4f, 0.15f, 0.25f, 0.25f);
        shadow.effectDistance = new Vector2(3f, -4f);

        Button btn = btnGO.AddComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.normalColor = new Color(1f, 0.94f, 0.96f, 0.96f);
        cb.highlightedColor = new Color(1f, 0.82f, 0.89f, 1f);
        cb.pressedColor = new Color(0.94f, 0.68f, 0.80f, 1f);
        cb.selectedColor = new Color(1f, 0.86f, 0.92f, 1f);
        btn.colors = cb;

        // Container vertikal untuk teks utama dan subteks
        VerticalLayoutGroup vlg = btnGO.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(20, 20, 10, 10);
        vlg.spacing = 2;
        vlg.childAlignment = TextAnchor.MiddleCenter;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        TextMeshProUGUI txtTitle = CreateText("Txt_Title", btnGO.transform, title, 20, new Color(0.48f, 0.10f, 0.30f, 1f), true);
        txtTitle.alignment = TextAlignmentOptions.Center;

        TextMeshProUGUI txtSub = CreateText("Txt_Sub", btnGO.transform, subtitle, 12, new Color(0.68f, 0.25f, 0.48f, 0.95f), true);
        txtSub.alignment = TextAlignmentOptions.Center;
        if (subFont != null) txtSub.font = subFont;

        return btn;
    }

    private static GameObject BuildModalLoadStory(Transform parent, out Button btnClose, out Transform contentContainer, out TextMeshProUGUI txtEmpty)
    {
        GameObject modalGO = CreateUIObject("Modal_LoadStory", parent);
        StretchFull(modalGO.GetComponent<RectTransform>());

        // Dim background
        Image dim = modalGO.AddComponent<Image>();
        dim.color = new Color(0.08f, 0.04f, 0.10f, 0.76f);

        // Panel Window
        GameObject winGO = CreateUIObject("Panel_Window", modalGO.transform);
        RectTransform rtWin = winGO.GetComponent<RectTransform>();
        rtWin.anchorMin = new Vector2(0.5f, 0.5f);
        rtWin.anchorMax = new Vector2(0.5f, 0.5f);
        rtWin.pivot = new Vector2(0.5f, 0.5f);
        rtWin.anchoredPosition = Vector2.zero;
        rtWin.sizeDelta = new Vector2(860f, 640f);

        Image imgWin = winGO.AddComponent<Image>();
        imgWin.color = new Color(0.965f, 0.973f, 1f, 0.98f);
        Outline outWin = winGO.AddComponent<Outline>();
        outWin.effectColor = new Color(0.784f, 0.820f, 1f, 1f);
        outWin.effectDistance = new Vector2(3f, -3f);

        // Window Title
        TextMeshProUGUI txtTitle = CreateText("Txt_ModalTitle", winGO.transform, "LOAD STORY", 26, new Color(0.12f, 0.14f, 0.25f, 1f), true);
        RectTransform rtTitle = txtTitle.GetComponent<RectTransform>();
        rtTitle.anchorMin = new Vector2(0.5f, 1f);
        rtTitle.anchorMax = new Vector2(0.5f, 1f);
        rtTitle.pivot = new Vector2(0.5f, 1f);
        rtTitle.anchoredPosition = new Vector2(0f, -24f);
        rtTitle.sizeDelta = new Vector2(600f, 36f);
        txtTitle.alignment = TextAlignmentOptions.Center;

        TextMeshProUGUI txtSub = CreateText("Txt_ModalSubtitle", winGO.transform, "Pilih riwayat simpanan untuk melanjutkan studi & relasi Devano", 13, new Color(0.35f, 0.40f, 0.55f, 0.95f));
        RectTransform rtSub = txtSub.GetComponent<RectTransform>();
        rtSub.anchorMin = new Vector2(0.5f, 1f);
        rtSub.anchorMax = new Vector2(0.5f, 1f);
        rtSub.pivot = new Vector2(0.5f, 1f);
        rtSub.anchoredPosition = new Vector2(0f, -62f);
        rtSub.sizeDelta = new Vector2(600f, 24f);
        txtSub.alignment = TextAlignmentOptions.Center;

        // ScrollView
        GameObject scrollGO = CreateUIObject("ScrollView_Slots", winGO.transform);
        RectTransform rtScroll = scrollGO.GetComponent<RectTransform>();
        rtScroll.anchorMin = new Vector2(0.5f, 0.5f);
        rtScroll.anchorMax = new Vector2(0.5f, 0.5f);
        rtScroll.pivot = new Vector2(0.5f, 0.5f);
        rtScroll.anchoredPosition = new Vector2(0f, -10f);
        rtScroll.sizeDelta = new Vector2(790f, 440f);

        ScrollRect scrollRect = scrollGO.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;

        // Viewport
        GameObject viewportGO = CreateUIObject("Viewport", scrollGO.transform);
        StretchFull(viewportGO.GetComponent<RectTransform>());
        Image imgVP = viewportGO.AddComponent<Image>();
        imgVP.color = new Color(0.90f, 0.92f, 0.98f, 0.5f);
        Mask mask = viewportGO.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        // Content
        GameObject contentGO = CreateUIObject("Content", viewportGO.transform);
        RectTransform rtContent = contentGO.GetComponent<RectTransform>();
        rtContent.anchorMin = new Vector2(0f, 1f);
        rtContent.anchorMax = new Vector2(1f, 1f);
        rtContent.pivot = new Vector2(0.5f, 1f);
        rtContent.anchoredPosition = Vector2.zero;
        rtContent.sizeDelta = new Vector2(0f, 300f);

        VerticalLayoutGroup vlgContent = contentGO.AddComponent<VerticalLayoutGroup>();
        vlgContent.padding = new RectOffset(12, 12, 12, 12);
        vlgContent.spacing = 12;
        vlgContent.childAlignment = TextAnchor.UpperCenter;
        vlgContent.childControlWidth = true;
        vlgContent.childControlHeight = false;
        vlgContent.childForceExpandWidth = true;
        vlgContent.childForceExpandHeight = false;

        ContentSizeFitter csf = contentGO.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollRect.viewport = viewportGO.GetComponent<RectTransform>();
        scrollRect.content = rtContent;

        // Empty placeholder
        txtEmpty = CreateText("Txt_EmptySaves", winGO.transform, "Belum ada file simpanan. Mulai cerita baru melalui 'NEW STORY'.", 15, new Color(0.45f, 0.48f, 0.60f, 0.9f));
        RectTransform rtEmpty = txtEmpty.GetComponent<RectTransform>();
        rtEmpty.anchorMin = new Vector2(0.5f, 0.5f);
        rtEmpty.anchorMax = new Vector2(0.5f, 0.5f);
        rtEmpty.pivot = new Vector2(0.5f, 0.5f);
        rtEmpty.anchoredPosition = new Vector2(0f, -10f);
        rtEmpty.sizeDelta = new Vector2(600f, 40f);
        txtEmpty.alignment = TextAlignmentOptions.Center;
        txtEmpty.gameObject.SetActive(false);

        // Close Button
        btnClose = CreateSimpleButton("Btn_CloseModal", winGO.transform, "✕ TUTUP", new Color(0.12f, 0.14f, 0.25f, 1f));
        RectTransform rtClose = btnClose.GetComponent<RectTransform>();
        rtClose.anchorMin = new Vector2(0.5f, 0f);
        rtClose.anchorMax = new Vector2(0.5f, 0f);
        rtClose.pivot = new Vector2(0.5f, 0f);
        rtClose.anchoredPosition = new Vector2(0f, 18f);
        rtClose.sizeDelta = new Vector2(180f, 44f);

        contentContainer = contentGO.transform;
        modalGO.SetActive(false);
        return modalGO;
    }

    private static GameObject BuildModalConfig(Transform parent, out Button btnClose, out Slider slBgm, out Slider slSfx, out Slider slSpeed)
    {
        GameObject modalGO = CreateUIObject("Modal_Config", parent);
        StretchFull(modalGO.GetComponent<RectTransform>());

        Image dim = modalGO.AddComponent<Image>();
        dim.color = new Color(0.08f, 0.04f, 0.10f, 0.76f);

        GameObject winGO = CreateUIObject("Panel_Window", modalGO.transform);
        RectTransform rtWin = winGO.GetComponent<RectTransform>();
        rtWin.anchorMin = new Vector2(0.5f, 0.5f);
        rtWin.anchorMax = new Vector2(0.5f, 0.5f);
        rtWin.pivot = new Vector2(0.5f, 0.5f);
        rtWin.anchoredPosition = Vector2.zero;
        rtWin.sizeDelta = new Vector2(720f, 540f);

        Image imgWin = winGO.AddComponent<Image>();
        imgWin.color = new Color(0.965f, 0.973f, 1f, 0.98f);
        Outline outWin = winGO.AddComponent<Outline>();
        outWin.effectColor = new Color(0.784f, 0.820f, 1f, 1f);
        outWin.effectDistance = new Vector2(3f, -3f);

        TextMeshProUGUI txtTitle = CreateText("Txt_ModalTitle", winGO.transform, "CONFIG / PENGATURAN", 24, new Color(0.12f, 0.14f, 0.25f, 1f), true);
        RectTransform rtTitle = txtTitle.GetComponent<RectTransform>();
        rtTitle.anchorMin = new Vector2(0.5f, 1f);
        rtTitle.anchorMax = new Vector2(0.5f, 1f);
        rtTitle.pivot = new Vector2(0.5f, 1f);
        rtTitle.anchoredPosition = new Vector2(0f, -28f);
        rtTitle.sizeDelta = new Vector2(500f, 36f);
        txtTitle.alignment = TextAlignmentOptions.Center;

        // Container Pengaturan
        GameObject settingsCont = CreateUIObject("SettingsContainer", winGO.transform);
        RectTransform rtSettings = settingsCont.GetComponent<RectTransform>();
        rtSettings.anchorMin = new Vector2(0.5f, 0.5f);
        rtSettings.anchorMax = new Vector2(0.5f, 0.5f);
        rtSettings.pivot = new Vector2(0.5f, 0.5f);
        rtSettings.anchoredPosition = new Vector2(0f, 0f);
        rtSettings.sizeDelta = new Vector2(600f, 320f);

        VerticalLayoutGroup vlg = settingsCont.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(20, 20, 15, 15);
        vlg.spacing = 24;
        vlg.childAlignment = TextAnchor.MiddleCenter;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;

        slBgm = CreateSettingSlider("Slider_BGM", settingsCont.transform, "Volume Musik Latar (BGM)", 0f, 1f, 0.8f);
        slSfx = CreateSettingSlider("Slider_SFX", settingsCont.transform, "Volume Efek Suara (SFX)", 0f, 1f, 1.0f);
        slSpeed = CreateSettingSlider("Slider_TextSpeed", settingsCont.transform, "Kecepatan Teks Dialog", 0.01f, 0.08f, 0.03f);

        btnClose = CreateSimpleButton("Btn_CloseModal", winGO.transform, "✕ SIMPAN & TUTUP", new Color(0.12f, 0.14f, 0.25f, 1f));
        RectTransform rtClose = btnClose.GetComponent<RectTransform>();
        rtClose.anchorMin = new Vector2(0.5f, 0f);
        rtClose.anchorMax = new Vector2(0.5f, 0f);
        rtClose.pivot = new Vector2(0.5f, 0f);
        rtClose.anchoredPosition = new Vector2(0f, 22f);
        rtClose.sizeDelta = new Vector2(220f, 44f);

        modalGO.SetActive(false);
        return modalGO;
    }

    private static GameObject BuildModalExtras(Transform parent, out Button btnClose)
    {
        GameObject modalGO = CreateUIObject("Modal_Extras", parent);
        StretchFull(modalGO.GetComponent<RectTransform>());

        Image dim = modalGO.AddComponent<Image>();
        dim.color = new Color(0.08f, 0.04f, 0.10f, 0.76f);

        GameObject winGO = CreateUIObject("Panel_Window", modalGO.transform);
        RectTransform rtWin = winGO.GetComponent<RectTransform>();
        rtWin.anchorMin = new Vector2(0.5f, 0.5f);
        rtWin.anchorMax = new Vector2(0.5f, 0.5f);
        rtWin.pivot = new Vector2(0.5f, 0.5f);
        rtWin.anchoredPosition = Vector2.zero;
        rtWin.sizeDelta = new Vector2(980f, 640f);

        Image imgWin = winGO.AddComponent<Image>();
        imgWin.color = new Color(0.965f, 0.973f, 1f, 0.98f);
        Outline outWin = winGO.AddComponent<Outline>();
        outWin.effectColor = new Color(0.784f, 0.820f, 1f, 1f);
        outWin.effectDistance = new Vector2(3f, -3f);

        TextMeshProUGUI txtTitle = CreateText("Txt_ModalTitle", winGO.transform, "EXTRAS / BIODATA KARAKTER", 24, new Color(0.12f, 0.14f, 0.25f, 1f), true);
        RectTransform rtTitle = txtTitle.GetComponent<RectTransform>();
        rtTitle.anchorMin = new Vector2(0.5f, 1f);
        rtTitle.anchorMax = new Vector2(0.5f, 1f);
        rtTitle.pivot = new Vector2(0.5f, 1f);
        rtTitle.anchoredPosition = new Vector2(0f, -24f);
        rtTitle.sizeDelta = new Vector2(600f, 36f);
        txtTitle.alignment = TextAlignmentOptions.Center;

        // Container Biodata Grid
        GameObject gridGO = CreateUIObject("Grid_Characters", winGO.transform);
        RectTransform rtGrid = gridGO.GetComponent<RectTransform>();
        rtGrid.anchorMin = new Vector2(0.5f, 0.5f);
        rtGrid.anchorMax = new Vector2(0.5f, 0.5f);
        rtGrid.pivot = new Vector2(0.5f, 0.5f);
        rtGrid.anchoredPosition = new Vector2(0f, -10f);
        rtGrid.sizeDelta = new Vector2(900f, 440f);

        GridLayoutGroup glg = gridGO.AddComponent<GridLayoutGroup>();
        glg.cellSize = new Vector2(170f, 420f);
        glg.spacing = new Vector2(12f, 12f);
        glg.childAlignment = TextAnchor.MiddleCenter;

        // 5 Karakter
        CreateCharacterCard(gridGO.transform, "Devano", "Protagonis", "Mahasiswa Pertukaran IT", "Tekad beradaptasi di kampus baru.", new Color(0.85f, 0.42f, 0.0f));
        CreateCharacterCard(gridGO.transform, "Xiang Bai", "Dosen IT", "Pembimbing Akademik", "Menghargai ketepatan logika & tata krama.", new Color(0.11f, 0.16f, 0.28f));
        CreateCharacterCard(gridGO.transform, "Li Haoran", "Senior Lab", "Spesialis Embedded System", "Fokus riset & dedikasi tinggi di lab.", new Color(0.09f, 0.63f, 0.52f));
        CreateCharacterCard(gridGO.transform, "Yang Mei", "Mahasiswi Seni", "Fakultas Seni & Sains Terapan", "Penuh ekspresi & menyukai eksplorasi rasa.", new Color(0.75f, 0.22f, 0.17f));
        CreateCharacterCard(gridGO.transform, "Edelweiss", "Double Degree", "Information Broker WeTalk", "Memiliki jaringan sosial luas di kampus.", new Color(0.784f, 0.820f, 1f));

        btnClose = CreateSimpleButton("Btn_CloseModal", winGO.transform, "✕ TUTUP", new Color(0.12f, 0.14f, 0.25f, 1f));
        RectTransform rtClose = btnClose.GetComponent<RectTransform>();
        rtClose.anchorMin = new Vector2(0.5f, 0f);
        rtClose.anchorMax = new Vector2(0.5f, 0f);
        rtClose.pivot = new Vector2(0.5f, 0f);
        rtClose.anchoredPosition = new Vector2(0f, 18f);
        rtClose.sizeDelta = new Vector2(180f, 44f);

        modalGO.SetActive(false);
        return modalGO;
    }

    private static void CreateCharacterCard(Transform parent, string name, string role, string dept, string bio, Color themeColor)
    {
        GameObject cardGO = CreateUIObject("Card_" + name, parent);
        Image img = cardGO.AddComponent<Image>();
        img.color = new Color(1f, 0.98f, 1f, 0.98f);
        Outline outline = cardGO.AddComponent<Outline>();
        outline.effectColor = themeColor;
        outline.effectDistance = new Vector2(2f, -2f);

        VerticalLayoutGroup vlg = cardGO.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(10, 10, 14, 14);
        vlg.spacing = 8;
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;

        // Avatar placeholder
        GameObject avatarGO = CreateUIObject("Avatar", cardGO.transform);
        RectTransform rtAv = avatarGO.GetComponent<RectTransform>();
        rtAv.sizeDelta = new Vector2(90f, 90f);
        LayoutElement leAv = avatarGO.AddComponent<LayoutElement>();
        leAv.preferredWidth = 90f;
        leAv.preferredHeight = 90f;
        Image imgAv = avatarGO.AddComponent<Image>();
        imgAv.color = themeColor;

        TextMeshProUGUI txtInitial = CreateText("Txt_Initial", avatarGO.transform, name.Substring(0, 1), 36, Color.white, true);
        StretchFull(txtInitial.GetComponent<RectTransform>());
        txtInitial.alignment = TextAlignmentOptions.Center;

        TextMeshProUGUI txtName = CreateText("Txt_Name", cardGO.transform, name, 16, themeColor, true);
        txtName.alignment = TextAlignmentOptions.Center;

        TextMeshProUGUI txtRole = CreateText("Txt_Role", cardGO.transform, role, 12, new Color(0.4f, 0.35f, 0.45f, 1f), true);
        txtRole.alignment = TextAlignmentOptions.Center;

        TextMeshProUGUI txtDept = CreateText("Txt_Dept", cardGO.transform, dept, 11, new Color(0.55f, 0.5f, 0.6f, 1f));
        txtDept.alignment = TextAlignmentOptions.Center;

        TextMeshProUGUI txtBio = CreateText("Txt_Bio", cardGO.transform, bio, 11, new Color(0.35f, 0.25f, 0.35f, 1f));
        txtBio.alignment = TextAlignmentOptions.Center;
    }

    private static Slider CreateSettingSlider(string name, Transform parent, string label, float min, float max, float val)
    {
        GameObject rowGO = CreateUIObject(name + "_Row", parent);
        HorizontalLayoutGroup hlg = rowGO.AddComponent<HorizontalLayoutGroup>();
        hlg.padding = new RectOffset(10, 10, 5, 5);
        hlg.spacing = 20;
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.childControlWidth = true;
        hlg.childControlHeight = true;

        LayoutElement leRow = rowGO.AddComponent<LayoutElement>();
        leRow.preferredHeight = 46f;

        TextMeshProUGUI txtLbl = CreateText("Label", rowGO.transform, label, 15, new Color(0.12f, 0.14f, 0.25f, 1f), true);
        LayoutElement leLbl = txtLbl.gameObject.AddComponent<LayoutElement>();
        leLbl.preferredWidth = 260f;

        GameObject sliderGO = CreateUIObject(name, rowGO.transform);
        LayoutElement leSlider = sliderGO.AddComponent<LayoutElement>();
        leSlider.flexibleWidth = 1f;
        leSlider.preferredHeight = 24f;

        Slider slider = sliderGO.AddComponent<Slider>();
        slider.minValue = min;
        slider.maxValue = max;
        slider.value = val;

        // Background
        GameObject bgGO = CreateUIObject("Background", sliderGO.transform);
        StretchFull(bgGO.GetComponent<RectTransform>());
        Image imgBg = bgGO.AddComponent<Image>();
        imgBg.color = new Color(0.85f, 0.88f, 0.96f, 1f);

        // Fill Area
        GameObject fillArea = CreateUIObject("Fill Area", sliderGO.transform);
        StretchFull(fillArea.GetComponent<RectTransform>());
        GameObject fillGO = CreateUIObject("Fill", fillArea.transform);
        StretchFull(fillGO.GetComponent<RectTransform>());
        Image imgFill = fillGO.AddComponent<Image>();
        imgFill.color = new Color(0.85f, 0.42f, 0.0f, 1f); // #D96B00 orange accent

        slider.fillRect = fillGO.GetComponent<RectTransform>();
        slider.targetGraphic = imgFill;

        return slider;
    }

    public static GameObject CreateOrUpdateSlotPrefab()
    {
        string folder = "Assets/Prefabs";
        if (!AssetDatabase.IsValidFolder(folder))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }

        GameObject itemGO = new GameObject("LoadSlotItemPrefab", typeof(RectTransform));
        RectTransform rt = itemGO.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(760f, 96f);

        LayoutElement le = itemGO.AddComponent<LayoutElement>();
        le.preferredWidth = 760f;
        le.preferredHeight = 96f;

        Image imgBg = itemGO.AddComponent<Image>();
        imgBg.color = new Color(0.965f, 0.973f, 1f, 0.98f);

        Outline outline = itemGO.AddComponent<Outline>();
        outline.effectColor = new Color(0.784f, 0.820f, 1f, 1f);
        outline.effectDistance = new Vector2(2f, -2f);

        HorizontalLayoutGroup hlg = itemGO.AddComponent<HorizontalLayoutGroup>();
        hlg.padding = new RectOffset(20, 20, 12, 12);
        hlg.spacing = 16;
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.childControlWidth = false;
        hlg.childControlHeight = true;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = true;

        // Bagian Kiri: Judul & Hari/Waktu
        GameObject leftCol = CreateUIObject("LeftColumn", itemGO.transform);
        RectTransform rtLeft = leftCol.GetComponent<RectTransform>();
        rtLeft.sizeDelta = new Vector2(380f, 72f);
        LayoutElement leLeft = leftCol.AddComponent<LayoutElement>();
        leLeft.preferredWidth = 380f;

        VerticalLayoutGroup vlgLeft = leftCol.AddComponent<VerticalLayoutGroup>();
        vlgLeft.padding = new RectOffset(0, 0, 2, 2);
        vlgLeft.spacing = 4;
        vlgLeft.childAlignment = TextAnchor.MiddleLeft;
        vlgLeft.childControlWidth = true;
        vlgLeft.childControlHeight = false;

        TextMeshProUGUI txtTitle = CreateText("Txt_SlotTitle", leftCol.transform, "[SLOT 1] Sesi Permainan", 17, new Color(0.12f, 0.14f, 0.25f, 1f), true);
        TextMeshProUGUI txtDetails = CreateText("Txt_SlotDetails", leftCol.transform, "Hari 1 • Waktu: Pagi", 13, new Color(0.35f, 0.40f, 0.55f, 0.95f));

        // Bagian Tengah: Info Stats & Tanggal Simpan
        GameObject midCol = CreateUIObject("MidColumn", itemGO.transform);
        RectTransform rtMid = midCol.GetComponent<RectTransform>();
        rtMid.sizeDelta = new Vector2(200f, 72f);
        LayoutElement leMid = midCol.AddComponent<LayoutElement>();
        leMid.preferredWidth = 200f;

        VerticalLayoutGroup vlgMid = midCol.AddComponent<VerticalLayoutGroup>();
        vlgMid.padding = new RectOffset(0, 0, 4, 4);
        vlgMid.spacing = 4;
        vlgMid.childAlignment = TextAnchor.MiddleLeft;
        vlgMid.childControlWidth = true;
        vlgMid.childControlHeight = false;

        TextMeshProUGUI txtStats = CreateText("Txt_Stats", midCol.transform, "PH: 100 | MH: 80", 13, new Color(0.15f, 0.50f, 0.35f, 1f), true);
        TextMeshProUGUI txtDate = CreateText("Txt_Date", midCol.transform, "Simpan: 2026-09-13 18:00", 11, new Color(0.45f, 0.48f, 0.60f, 0.85f));

        // Bagian Kanan: Tombol Load
        Button btnLoad = CreateSimpleButton("Btn_Load", itemGO.transform, "MUAT ▶", new Color(0.85f, 0.42f, 0.0f, 1f));
        RectTransform rtLoad = btnLoad.GetComponent<RectTransform>();
        rtLoad.sizeDelta = new Vector2(110f, 50f);
        LayoutElement leLoad = btnLoad.gameObject.AddComponent<LayoutElement>();
        leLoad.preferredWidth = 110f;
        leLoad.preferredHeight = 50f;

        // Pasang Script LoadSlotItemUI
        LoadSlotItemUI itemUI = itemGO.AddComponent<LoadSlotItemUI>();
        itemUI.txtTitle = txtTitle;
        itemUI.txtDetails = txtDetails;
        itemUI.txtDate = txtDate;
        itemUI.txtStats = txtStats;
        itemUI.btnLoad = btnLoad;

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(itemGO, PREFAB_PATH);
        Object.DestroyImmediate(itemGO);
        AssetDatabase.SaveAssets();

        return prefab;
    }

    private static Button CreateSimpleButton(string name, Transform parent, string label, Color bgColor)
    {
        GameObject btnGO = CreateUIObject(name, parent);
        RectTransform rt = btnGO.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(140f, 44f);

        Image img = btnGO.AddComponent<Image>();
        img.color = bgColor;

        Button btn = btnGO.AddComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.normalColor = bgColor;
        cb.highlightedColor = bgColor * 1.15f;
        cb.pressedColor = bgColor * 0.85f;
        btn.colors = cb;

        TextMeshProUGUI tmp = CreateText("Text (TMP)", btnGO.transform, label, 16, Color.white, true);
        StretchFull(tmp.GetComponent<RectTransform>());
        tmp.alignment = TextAlignmentOptions.Center;

        return btn;
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

    private static void StretchFull(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    public static void UpdateBuildSettings()
    {
        string mainMenuPath = "Assets/Scenes/MainMenuScene.unity";
        string sampleScenePath = "Assets/Scenes/SampleScene.unity";

        EditorBuildSettingsScene[] scenes = new EditorBuildSettingsScene[]
        {
            new EditorBuildSettingsScene(mainMenuPath, true),
            new EditorBuildSettingsScene(sampleScenePath, true)
        };

        EditorBuildSettings.scenes = scenes;

        // Nama produk = judul game (judul jendela build & folder persistentDataPath/PlayerPrefs)
        PlayerSettings.productName = GAME_TITLE;
        Debug.Log("<color=green>[BuildSettings]</color> Build Settings diperbarui: [0] MainMenuScene.unity, [1] SampleScene.unity");
    }
}
#endif
