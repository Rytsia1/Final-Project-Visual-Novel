using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueUIController : MonoBehaviour
{
    private static DialogueUIController _instance;
    public static DialogueUIController Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<DialogueUIController>(FindObjectsInactive.Include);
            }
            return _instance;
        }
        private set => _instance = value;
    }

    [Header("Panel Utama")]
    public GameObject dialoguePanel; // Panel_DialogueBox

    [Header("Komponen Teks")]
    public TextMeshProUGUI txtSpeakerName;
    public TextMeshProUGUI txtDialogueContent;

    [Header("Container Opsi")]
    public Transform optionsContainer; // Objek yang memiliki Vertical Layout Group
    public GameObject optionButtonPrefab;

    private List<GameObject> activeOptionButtons = new List<GameObject>();

    void Awake()
    {
        if (_instance == null) _instance = this;
        else if (_instance != this) Destroy(gameObject);
    }

    void Start()
    {
        // Pastikan panel dialog tertutup saat awal permainan
        CloseDialoguePanel();
    }

    // Membuka panel dan menampilkan teks pembicara
    public void DisplayDialogue(string speaker, string content)
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        if (txtSpeakerName != null) txtSpeakerName.text = speaker;
        if (txtDialogueContent != null) txtDialogueContent.text = content;
        ClearOptions();
    }

    // Menghapus tombol opsi sebelumnya
    public void ClearOptions()
    {
        foreach (var btn in activeOptionButtons)
        {
            if (btn != null)
            {
                btn.SetActive(false);
                Destroy(btn);
            }
        }
        activeOptionButtons.Clear();
    }

    // Men-generate tombol pilihan respon secara dinamis
    public void CreateOptionButton(int optionId, string optionText)
    {
        if (optionButtonPrefab == null || optionsContainer == null) return;

        GameObject newBtn = Instantiate(optionButtonPrefab, optionsContainer);
        activeOptionButtons.Add(newBtn);

        // Pasang teks pada tombol
        TextMeshProUGUI btnText = newBtn.GetComponentInChildren<TextMeshProUGUI>();
        if (btnText != null)
        {
            btnText.text = optionText;
        }

        // Pasang aksi klik tombol
        Button btnComp = newBtn.GetComponent<Button>();
        if (btnComp != null)
        {
            btnComp.onClick.AddListener(() =>
            {
                DialogueManager.Instance.SelectOption(optionId);
            });
        }
    }

    private System.Action activeCloseCallback = null;

    // Tombol penutup dialog jika tidak ada opsi lanjutan
    public void ShowCloseButton(System.Action onCloseAction = null)
    {
        ClearOptions();
        activeCloseCallback = onCloseAction;

        if (optionButtonPrefab == null || optionsContainer == null) return;

        GameObject closeBtn = Instantiate(optionButtonPrefab, optionsContainer);
        activeOptionButtons.Add(closeBtn);

        TextMeshProUGUI btnText = closeBtn.GetComponentInChildren<TextMeshProUGUI>();
        if (btnText != null) btnText.text = "[Lanjut / Selesai]";

        Button btnComp = closeBtn.GetComponent<Button>();
        if (btnComp != null)
        {
            btnComp.onClick.AddListener(OnCloseButtonClicked);
        }
    }

    private void OnCloseButtonClicked()
    {
        System.Action callback = activeCloseCallback;
        activeCloseCallback = null;
        CloseDialoguePanel();
        callback?.Invoke();
    }

    // Tombol respon aksi kustom (misal: Sapa balik dengan senyuman pada sapaan pagi)
    public void CreateCustomActionButton(string buttonText, UnityEngine.Events.UnityAction onClickAction)
    {
        ClearOptions();
        activeCloseCallback = null;
        if (optionButtonPrefab == null || optionsContainer == null) return;

        GameObject actionBtn = Instantiate(optionButtonPrefab, optionsContainer);
        activeOptionButtons.Add(actionBtn);

        TextMeshProUGUI btnText = actionBtn.GetComponentInChildren<TextMeshProUGUI>();
        if (btnText != null) btnText.text = buttonText;

        Button btnComp = actionBtn.GetComponent<Button>();
        if (btnComp != null)
        {
            btnComp.onClick.AddListener(onClickAction);
        }
    }

    public void CloseDialoguePanel()
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        ClearOptions();
        activeCloseCallback = null;
        if (DialogueManager.Instance != null && DialogueManager.Instance.isDialogueActive)
        {
            DialogueManager.Instance.EndDialogue();
        }
    }
}
