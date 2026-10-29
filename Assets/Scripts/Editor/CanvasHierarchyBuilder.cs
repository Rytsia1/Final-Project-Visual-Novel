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
        vlg.childControlHeight = true; // tinggi baris mengikuti teks yang dibungkus
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        ContentSizeFitter csf = itemGO.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Speaker Name
        GameObject spkGO = new GameObject("Txt_Speaker", typeof(RectTransform));
        spkGO.transform.SetParent(itemGO.transform, false);
        TextMeshProUGUI tmpSpk = spkGO.AddComponent<TextMeshProUGUI>();
        tmpSpk.text = "Speaker";
        tmpSpk.fontSize = 26;
        tmpSpk.fontStyle = FontStyles.Bold;
        tmpSpk.color = new Color(0.35f, 0.85f, 1f);

        // Dialogue Text
        GameObject diaGO = new GameObject("Txt_Dialogue", typeof(RectTransform));
        diaGO.transform.SetParent(itemGO.transform, false);
        TextMeshProUGUI tmpDia = diaGO.AddComponent<TextMeshProUGUI>();
        tmpDia.text = "Teks percakapan dialog yang pernah diucapkan...";
        tmpDia.fontSize = 24;
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
        BuildSocialStatusWindowUI(canvas.gameObject, hud, hud != null ? hud.btnOpenSocialWindow : null);

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

        // 4. Hubungkan tombol Btn_OpenSocial (di HUD baru ada di Panel_ActivityGrid; cari di seluruh canvas)
        if (btnOpenSocial == null)
        {
            foreach (Button b in canvasGO.GetComponentsInChildren<Button>(true))
            {
                if (b.name == "Btn_OpenSocial") { btnOpenSocial = b; break; }
            }
        }

        // Klik dipasang saat runtime oleh HUDController.Start -> OnClick_OpenSocialWindow.
        // Tidak menambah persistent listener di sini supaya jendela tidak terbuka dua kali per klik.
        if (btnOpenSocial != null)
        {
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
