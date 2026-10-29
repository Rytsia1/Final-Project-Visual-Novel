using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Layar loading overlay yang dibangun di runtime (tanpa prefab). Fade in, memuat scene secara async
/// dengan progress bar, lalu fade out setelah scene tujuan selesai Start — sehingga transisi ke prolog
/// tidak memperlihatkan frame kosong. Bertahan lintas scene via DontDestroyOnLoad selama transisi.
/// </summary>
public class LoadingScreen : MonoBehaviour
{
    private const float FADE_DURATION = 0.35f;
    private const float MIN_DISPLAY_TIME = 1.2f;

    private static bool _isLoading;

    private CanvasGroup _group;
    private RectTransform _barFill;
    private TextMeshProUGUI _txtPercent;

    public static void Load(string sceneName)
    {
        if (_isLoading) return;
        _isLoading = true;

        GameObject go = new GameObject("LoadingScreen");
        DontDestroyOnLoad(go);
        LoadingScreen screen = go.AddComponent<LoadingScreen>();
        screen.BuildUI();
        screen.StartCoroutine(screen.LoadRoutine(sceneName));
    }

    private void BuildUI()
    {
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;
        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        gameObject.AddComponent<GraphicRaycaster>();

        _group = gameObject.AddComponent<CanvasGroup>();
        _group.alpha = 0f;
        _group.blocksRaycasts = true; // Cegah klik ganda pada menu selama transisi

        Image bg = CreateRect<Image>("Img_Background", transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        bg.color = new Color(0.12f, 0.14f, 0.25f, 1f); // #1E2440
        bg.raycastTarget = true;

        TextMeshProUGUI title = CreateRect<TextMeshProUGUI>("Txt_Title", transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 60f), new Vector2(900f, 90f));
        title.text = "Memuat...";
        title.fontSize = 56;
        title.alignment = TextAlignmentOptions.Center;
        title.color = new Color(0.965f, 0.973f, 1f, 1f); // #F6F8FF

        Image barBg = CreateRect<Image>("Img_BarBg", transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -20f), new Vector2(640f, 14f));
        barBg.color = new Color(0.965f, 0.973f, 1f, 0.15f);

        // Fill diskalakan lewat anchorMax.x agar tidak butuh sprite (Image.Type.Filled perlu sprite)
        Image fill = CreateRect<Image>("Img_BarFill", barBg.transform, Vector2.zero, new Vector2(0f, 1f), Vector2.zero, Vector2.zero);
        fill.color = new Color(1f, 0.82f, 0.89f, 1f); // pink aksen tombol menu
        _barFill = fill.rectTransform;

        _txtPercent = CreateRect<TextMeshProUGUI>("Txt_Percent", transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -60f), new Vector2(640f, 40f));
        _txtPercent.fontSize = 24;
        _txtPercent.alignment = TextAlignmentOptions.Center;
        _txtPercent.color = new Color(0.965f, 0.973f, 1f, 0.7f);
    }

    private static T CreateRect<T>(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pos, Vector2 size) where T : Graphic
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        RectTransform rt = (RectTransform)go.transform;
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        T graphic = go.AddComponent<T>();
        graphic.raycastTarget = false;
        return graphic;
    }

    private void SetProgress(float p)
    {
        _barFill.anchorMax = new Vector2(p, 1f);
        _txtPercent.text = $"{Mathf.RoundToInt(p * 100f)}%";
    }

    private IEnumerator LoadRoutine(string sceneName)
    {
        SetProgress(0f);
        yield return Fade(0f, 1f);

        float startTime = Time.unscaledTime;
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        if (op == null)
        {
            Debug.LogError($"<color=red>[LoadingScreen]</color> Scene '{sceneName}' tidak ditemukan di Build Settings.");
            _isLoading = false;
            Destroy(gameObject);
            yield break;
        }
        op.allowSceneActivation = false;

        // Unity melaporkan progress 0..0.9 sebelum aktivasi; bar juga dibatasi waktu tampil minimum
        float shown = 0f;
        while (shown < 1f)
        {
            float loadP = Mathf.Clamp01(op.progress / 0.9f);
            float timeP = Mathf.Clamp01((Time.unscaledTime - startTime) / MIN_DISPLAY_TIME);
            shown = Mathf.MoveTowards(shown, Mathf.Min(loadP, timeP), Time.unscaledDeltaTime * 2f);
            SetProgress(shown);
            yield return null;
        }

        op.allowSceneActivation = true;
        while (!op.isDone) yield return null;

        // Tunggu satu frame agar Start() di scene gameplay (GameManager -> prolog) sudah berjalan
        yield return null;
        _group.blocksRaycasts = false;
        yield return Fade(1f, 0f);

        _isLoading = false;
        Destroy(gameObject);
    }

    private IEnumerator Fade(float from, float to)
    {
        for (float t = 0f; t < FADE_DURATION; t += Time.unscaledDeltaTime)
        {
            _group.alpha = Mathf.Lerp(from, to, t / FADE_DURATION);
            yield return null;
        }
        _group.alpha = to;
    }
}
