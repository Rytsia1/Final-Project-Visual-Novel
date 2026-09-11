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

        TextMeshProUGUI txtDesc = CreateText("Txt_ActivityDescription", panelTooltip.transform, "Pilih aktivitas harian Kenzo Pratama.", 18, Color.white);
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
        txtContent.text = "Kenzo, bagaimana progres analisis data untuk tugas mingguanmu?";
        txtContent.fontSize = 20;
        txtContent.color = Color.white;
        txtContent.enableWordWrapping = true;

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

        // Pastikan TelemetryLogger terpasang di GameObject GAME_CORE
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
        }

        // Simpan perubahan ke Scene
        EditorUtility.SetDirty(canvasGO);
        activeScene = EditorSceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(activeScene);
        EditorSceneManager.SaveScene(activeScene);

        Debug.Log("<color=green>[HUD Setup]</color> Berhasil menyusun hierarki Canvas UI 1920x1080 dan mengaitkan seluruh referensi HUDController!");
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
}
