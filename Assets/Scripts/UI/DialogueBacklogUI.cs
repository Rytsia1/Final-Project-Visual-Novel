using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class DialogueBacklogUI : MonoBehaviour
{
    private static DialogueBacklogUI _instance;
    public static DialogueBacklogUI Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<DialogueBacklogUI>(FindObjectsInactive.Include);
            }
            return _instance;
        }
        private set => _instance = value;
    }

    [Header("Referensi UI")]
    public GameObject panelBacklogRoot;
    public ScrollRect scrollRect;
    public Transform backlogContentContainer;
    public GameObject backlogItemPrefab;
    public Button btnClose;
    public TextMeshProUGUI txtEmptyBacklog;

    public bool IsOpen => panelBacklogRoot != null && panelBacklogRoot.activeSelf;

    void Awake()
    {
        if (_instance == null) _instance = this;
        else if (_instance != this) Destroy(this);
    }

    void Start()
    {
        if (btnClose != null)
        {
            btnClose.onClick.RemoveAllListeners();
            btnClose.onClick.AddListener(CloseBacklog);
        }

        CloseBacklog();
    }

    void Update()
    {
        if (!IsOpen) return;

        // Tutup backlog dengan tombol Escape
        if (IsEscapePressed())
        {
            CloseBacklog();
        }
    }

    private bool IsEscapePressed()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) return true;
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
        if (Input.GetKeyDown(KeyCode.Escape)) return true;
#endif
        return false;
    }

    /// <summary>
    /// Membuka jendela dialog backlog dan men-scroll otomatis ke percakapan paling akhir.
    /// </summary>
    public void OpenBacklog()
    {
        if (panelBacklogRoot != null) panelBacklogRoot.SetActive(true);

        PopulateBacklog();

        Canvas.ForceUpdateCanvases();
        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(ScrollToBottomRoutine());
        }
    }

    /// <summary>
    /// Menutup jendela dialog backlog.
    /// </summary>
    public void CloseBacklog()
    {
        if (panelBacklogRoot != null) panelBacklogRoot.SetActive(false);
    }

    private void PopulateBacklog()
    {
        if (backlogContentContainer == null) return;

        // Bersihkan entri lama
        for (int i = backlogContentContainer.childCount - 1; i >= 0; i--)
        {
            Transform child = backlogContentContainer.GetChild(i);
#if UNITY_EDITOR
            if (!Application.isPlaying)
                DestroyImmediate(child.gameObject);
            else
                Destroy(child.gameObject);
#else
            Destroy(child.gameObject);
#endif
        }

        var history = DialogueBacklogManager.Instance != null 
            ? DialogueBacklogManager.Instance.historyList 
            : new List<BacklogEntry>();

        bool hasEntries = history.Count > 0;
        if (txtEmptyBacklog != null)
        {
            txtEmptyBacklog.gameObject.SetActive(!hasEntries);
        }

        if (!hasEntries) return;

        foreach (var entry in history)
        {
            Color speakerColor = GetSpeakerColor(entry.speakerName);

            if (backlogItemPrefab != null)
            {
                GameObject itemObj = Instantiate(backlogItemPrefab, backlogContentContainer);
                BacklogItemUI itemUI = itemObj.GetComponent<BacklogItemUI>();
                if (itemUI != null)
                {
                    itemUI.Setup(entry.speakerName, entry.dialogueText, speakerColor);
                }
                else
                {
                    // Fallback jika tidak memakai BacklogItemUI
                    TextMeshProUGUI[] tmps = itemObj.GetComponentsInChildren<TextMeshProUGUI>();
                    if (tmps.Length >= 2)
                    {
                        tmps[0].text = entry.speakerName;
                        tmps[0].color = speakerColor;
                        tmps[1].text = entry.dialogueText;
                    }
                    else if (tmps.Length == 1)
                    {
                        tmps[0].text = $"<b><color=#{ColorUtility.ToHtmlStringRGB(speakerColor)}>{entry.speakerName}</color></b>: {entry.dialogueText}";
                    }
                }
            }
        }
    }

    private IEnumerator ScrollToBottomRoutine()
    {
        yield return null; // Tunggu satu frame agar RectTransform dan LayoutGroups menghitung ukuran konten
        yield return new WaitForEndOfFrame();

        if (scrollRect != null)
        {
            scrollRect.verticalNormalizedPosition = 0f;
        }
    }

    private Color GetSpeakerColor(string speaker)
    {
        if (string.IsNullOrEmpty(speaker)) return new Color(0.7f, 0.8f, 0.9f);

        string s = speaker.ToLower();
        if (s.Contains("devano")) return new Color(1f, 0.55f, 0.35f); // #FF8C59 (Oranye Devano)
        if (s.Contains("xiang bai") || s.Contains("dosen")) return new Color(0.31f, 0.76f, 0.97f); // #4FC3F7 (Cyan Dosen)
        if (s.Contains("haoran")) return new Color(0.30f, 0.71f, 0.67f); // #4DB6AC (Teal Li Haoran)
        if (s.Contains("yang mei")) return new Color(0.94f, 0.45f, 0.45f); // #EF7373 (Merah Yang Mei)
        if (s.Contains("edelweiss") || s.Contains("edel")) return new Color(0.94f, 0.38f, 0.57f); // #F06292 (Pink Edelweiss)

        return new Color(0.85f, 0.88f, 0.95f);
    }
}
