using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

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
    public Button btnDialogueBoxClick; // Area klik seluruh kotak dialog untuk advance/finish

    [Header("Komponen Teks")]
    public TextMeshProUGUI txtSpeakerName;
    public TextMeshProUGUI txtDialogueContent;

    [Header("Container Opsi")]
    public Transform optionsContainer;
    public GameObject optionButtonPrefab;

    [Header("Reader Control Suite Buttons")]
    public Button btnBacklog;
    public Button btnAuto;
    public Button btnSkip;
    public Button btnConfig;

    [Header("Typewriter & Speed Settings")]
    [Tooltip("Waktu jeda per karakter (detik)")]
    public float textSpeed = 0.03f;
    [Tooltip("Waktu jeda sebelum otomatis lanjut di Auto Mode (detik)")]
    public float autoWaitDelay = 1.8f;
    [Tooltip("Waktu jeda antar baris di Skip Mode (detik)")]
    public float skipStepDelay = 0.12f;

    [Header("Runtime State")]
    public bool isTyping { get; private set; } = false;
    public bool isAutoMode { get; private set; } = false;
    public bool isSkipMode { get; private set; } = false;
    public bool isWaitingForChoice { get; private set; } = false;

    private List<GameObject> activeOptionButtons = new List<GameObject>();
    private Coroutine _typewriterCoroutine;
    private Coroutine _advanceWaitCoroutine;
    private string _currentFullText = "";
    private Action activeCloseCallback = null;

    private Color _btnNormalColor = new Color(0.18f, 0.22f, 0.32f);
    private Color _btnAutoActiveColor = new Color(0.15f, 0.55f, 0.35f);
    private Color _btnSkipActiveColor = new Color(0.70f, 0.40f, 0.15f);

    void Awake()
    {
        if (_instance == null) _instance = this;
        else if (_instance != this) Destroy(gameObject);
    }

    void Start()
    {
        BindControlBarButtons();

        if (btnDialogueBoxClick != null)
        {
            btnDialogueBoxClick.onClick.RemoveAllListeners();
            btnDialogueBoxClick.onClick.AddListener(OnDialogueBoxClicked);
        }

        CloseDialoguePanel();
    }

    void Update()
    {
        if (dialoguePanel == null || !dialoguePanel.activeSelf) return;

        // Jika panel backlog sedang terbuka, abaikan input dialog biasa
        if (DialogueBacklogUI.Instance != null && DialogueBacklogUI.Instance.IsOpen) return;

        // 1. Deteksi scroll wheel up untuk membuka Backlog
        if (IsMouseScrollUp())
        {
            if (DialogueBacklogUI.Instance != null)
            {
                DialogueBacklogUI.Instance.OpenBacklog();
            }
        }

        // 2. Deteksi Space / Enter untuk advance / reveal dialog
        if (IsAdvanceKeyPressed())
        {
            OnDialogueBoxClicked();
        }
    }

    private bool IsMouseScrollUp()
    {
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null && Mouse.current.scroll.ReadValue().y > 0.1f) return true;
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
        if (Input.mouseScrollDelta.y > 0.1f) return true;
#endif
        return false;
    }

    private bool IsAdvanceKeyPressed()
    {
#if ENABLE_INPUT_SYSTEM
        var kb = Keyboard.current;
        if (kb != null)
        {
            if (kb.spaceKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame) return true;
        }
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)) return true;
#endif
        return false;
    }

    private void BindControlBarButtons()
    {
        if (btnBacklog != null)
        {
            btnBacklog.onClick.RemoveAllListeners();
            btnBacklog.onClick.AddListener(() =>
            {
                if (DialogueBacklogUI.Instance != null)
                    DialogueBacklogUI.Instance.OpenBacklog();
            });
        }

        if (btnAuto != null)
        {
            btnAuto.onClick.RemoveAllListeners();
            btnAuto.onClick.AddListener(ToggleAutoMode);
        }

        if (btnSkip != null)
        {
            btnSkip.onClick.RemoveAllListeners();
            btnSkip.onClick.AddListener(ToggleSkipMode);
        }

        if (btnConfig != null)
        {
            btnConfig.onClick.RemoveAllListeners();
            btnConfig.onClick.AddListener(() =>
            {
                // Opsional: Buka modal konfigurasi jika tersedia
                Debug.Log("<color=cyan>[Reader Control]</color> Tombol Config ditekan.");
            });
        }

        UpdateControlBarVisuals();
    }

    // =========================================================
    // RENDER & TYPEWRITER DIALOGUE
    // =========================================================
    public void DisplayDialogue(string speaker, string content)
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        if (txtSpeakerName != null) txtSpeakerName.text = speaker;

        // Catat ke DialogueBacklogManager
        if (DialogueBacklogManager.Instance != null)
        {
            DialogueBacklogManager.Instance.AddEntry(speaker, content);
        }

        ClearOptions();
        isWaitingForChoice = false;
        _currentFullText = content ?? "";

        if (_typewriterCoroutine != null) StopCoroutine(_typewriterCoroutine);
        if (_advanceWaitCoroutine != null) StopCoroutine(_advanceWaitCoroutine);

        if (isSkipMode || textSpeed <= 0f)
        {
            // Mode Skip: tampilkan kalimat penuh secara instan
            if (txtDialogueContent != null)
            {
                txtDialogueContent.text = _currentFullText;
                txtDialogueContent.maxVisibleCharacters = int.MaxValue;
            }
            isTyping = false;
            HandleTypingFinished();
        }
        else
        {
            _typewriterCoroutine = StartCoroutine(TypewriterRoutine(_currentFullText));
        }
    }

    private IEnumerator TypewriterRoutine(string fullText)
    {
        isTyping = true;

        if (txtDialogueContent != null)
        {
            txtDialogueContent.text = fullText;
            txtDialogueContent.maxVisibleCharacters = 0;
            txtDialogueContent.ForceMeshUpdate();

            int totalCharacters = txtDialogueContent.textInfo.characterCount;

            for (int i = 0; i <= totalCharacters; i++)
            {
                if (isSkipMode)
                {
                    txtDialogueContent.maxVisibleCharacters = int.MaxValue;
                    break;
                }

                txtDialogueContent.maxVisibleCharacters = i;
                yield return new WaitForSeconds(textSpeed);
            }

            txtDialogueContent.maxVisibleCharacters = int.MaxValue;
        }

        isTyping = false;
        _typewriterCoroutine = null;
        HandleTypingFinished();
    }

    private void HandleTypingFinished()
    {
        if (_advanceWaitCoroutine != null) StopCoroutine(_advanceWaitCoroutine);

        if (isSkipMode)
        {
            if (isWaitingForChoice)
            {
                // Berhenti otomatis saat tiba di pilihan percabangan
                SetSkipMode(false);
            }
            else if (gameObject.activeInHierarchy)
            {
                _advanceWaitCoroutine = StartCoroutine(RoutineSkipAdvance());
            }
        }
        else if (isAutoMode)
        {
            if (!isWaitingForChoice && gameObject.activeInHierarchy)
            {
                _advanceWaitCoroutine = StartCoroutine(RoutineAutoAdvance());
            }
        }
    }

    private IEnumerator RoutineAutoAdvance()
    {
        yield return new WaitForSeconds(autoWaitDelay);

        if (isAutoMode && !isWaitingForChoice && !isTyping)
        {
            AdvanceDialogue();
        }
    }

    private IEnumerator RoutineSkipAdvance()
    {
        yield return new WaitForSeconds(skipStepDelay);

        if (isSkipMode && !isWaitingForChoice)
        {
            AdvanceDialogue();
        }
    }

    // =========================================================
    // LOGIKA KLIK KOTAK DIALOG (CLICK TO ADVANCE / FINISH)
    // =========================================================
    public void OnDialogueBoxClicked()
    {
        // 1. Klik saat teks sedang mengetik -> langsung tampilkan kalimat penuh (instant reveal)
        if (isTyping)
        {
            if (_typewriterCoroutine != null)
            {
                StopCoroutine(_typewriterCoroutine);
                _typewriterCoroutine = null;
            }

            if (txtDialogueContent != null)
            {
                txtDialogueContent.maxVisibleCharacters = int.MaxValue;
            }

            isTyping = false;
            HandleTypingFinished();
            return;
        }

        // 2. Klik saat kalimat sudah penuh -> lanjut ke dialog berikutnya (jika bukan pilihan)
        if (!isWaitingForChoice)
        {
            AdvanceDialogue();
        }
    }

    public void AdvanceDialogue()
    {
        if (_advanceWaitCoroutine != null)
        {
            StopCoroutine(_advanceWaitCoroutine);
            _advanceWaitCoroutine = null;
        }

        if (activeCloseCallback != null)
        {
            OnCloseButtonClicked();
        }
        else if (activeOptionButtons.Count == 1 && !isWaitingForChoice)
        {
            // Jika hanya ada 1 tombol penutup/lanjut, klik tombol tersebut
            Button btn = activeOptionButtons[0].GetComponent<Button>();
            if (btn != null && btn.interactable)
            {
                btn.onClick.Invoke();
            }
        }
        else if (activeOptionButtons.Count == 0)
        {
            CloseDialoguePanel();
        }
    }

    // =========================================================
    // MODUS AUTO & SKIP
    // =========================================================
    public void ToggleAutoMode()
    {
        SetAutoMode(!isAutoMode);
    }

    public void SetAutoMode(bool active)
    {
        isAutoMode = active;
        if (isAutoMode && isSkipMode)
        {
            isSkipMode = false;
        }

        UpdateControlBarVisuals();

        if (isAutoMode && !isTyping && !isWaitingForChoice && gameObject.activeInHierarchy)
        {
            if (_advanceWaitCoroutine != null) StopCoroutine(_advanceWaitCoroutine);
            _advanceWaitCoroutine = StartCoroutine(RoutineAutoAdvance());
        }
    }

    public void ToggleSkipMode()
    {
        SetSkipMode(!isSkipMode);
    }

    public void SetSkipMode(bool active)
    {
        isSkipMode = active;
        if (isSkipMode && isAutoMode)
        {
            isAutoMode = false;
        }

        UpdateControlBarVisuals();

        if (isSkipMode)
        {
            // Jika sedang mengetik, langsung selesaikan
            if (isTyping)
            {
                if (_typewriterCoroutine != null) StopCoroutine(_typewriterCoroutine);
                if (txtDialogueContent != null) txtDialogueContent.maxVisibleCharacters = int.MaxValue;
                isTyping = false;
            }

            if (!isWaitingForChoice && gameObject.activeInHierarchy)
            {
                if (_advanceWaitCoroutine != null) StopCoroutine(_advanceWaitCoroutine);
                _advanceWaitCoroutine = StartCoroutine(RoutineSkipAdvance());
            }
        }
    }

    private void UpdateControlBarVisuals()
    {
        if (btnAuto != null)
        {
            Image img = btnAuto.GetComponent<Image>();
            if (img != null) img.color = isAutoMode ? _btnAutoActiveColor : _btnNormalColor;

            TextMeshProUGUI tmp = btnAuto.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null) tmp.text = isAutoMode ? "<b>AUTO</b>" : "AUTO";
        }

        if (btnSkip != null)
        {
            Image img = btnSkip.GetComponent<Image>();
            if (img != null) img.color = isSkipMode ? _btnSkipActiveColor : _btnNormalColor;

            TextMeshProUGUI tmp = btnSkip.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null) tmp.text = isSkipMode ? "<b>SKIP</b>" : "SKIP";
        }
    }

    // =========================================================
    // MANAJEMEN TOMBOL OPSI
    // =========================================================
    public void ClearOptions()
    {
        foreach (var btn in activeOptionButtons)
        {
            if (btn != null)
            {
                btn.SetActive(false);
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    DestroyImmediate(btn);
                else
                    Destroy(btn);
#else
                Destroy(btn);
#endif
            }
        }
        activeOptionButtons.Clear();
        isWaitingForChoice = false;
    }

    public void CreateOptionButton(int optionId, string optionText)
    {
        if (optionButtonPrefab == null || optionsContainer == null) return;

        isWaitingForChoice = true; // Menandai adanya pilihan respon aktif

        // Jika skip mode aktif saat tiba di pilihan, hentikan skip mode
        if (isSkipMode)
        {
            SetSkipMode(false);
        }

        GameObject newBtn = Instantiate(optionButtonPrefab, optionsContainer);
        activeOptionButtons.Add(newBtn);

        TextMeshProUGUI btnText = newBtn.GetComponentInChildren<TextMeshProUGUI>();
        if (btnText != null)
        {
            btnText.text = optionText;
        }

        Button btnComp = newBtn.GetComponent<Button>();
        if (btnComp != null)
        {
            btnComp.onClick.AddListener(() =>
            {
                isWaitingForChoice = false;
                DialogueManager.Instance.SelectOption(optionId);
            });
        }
    }

    public void ShowCloseButton(Action onCloseAction = null)
    {
        ClearOptions();
        activeCloseCallback = onCloseAction;
        isWaitingForChoice = false;

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

        // Jika auto atau skip mode aktif, proses trigger
        if (isSkipMode && gameObject.activeInHierarchy)
        {
            if (_advanceWaitCoroutine != null) StopCoroutine(_advanceWaitCoroutine);
            _advanceWaitCoroutine = StartCoroutine(RoutineSkipAdvance());
        }
        else if (isAutoMode && !isTyping && gameObject.activeInHierarchy)
        {
            if (_advanceWaitCoroutine != null) StopCoroutine(_advanceWaitCoroutine);
            _advanceWaitCoroutine = StartCoroutine(RoutineAutoAdvance());
        }
    }

    private void OnCloseButtonClicked()
    {
        Action callback = activeCloseCallback;
        activeCloseCallback = null;
        CloseDialoguePanel();
        callback?.Invoke();
    }

    public void CreateCustomActionButton(string buttonText, UnityEngine.Events.UnityAction onClickAction)
    {
        ClearOptions();
        activeCloseCallback = null;
        isWaitingForChoice = true;

        if (isSkipMode)
        {
            SetSkipMode(false);
        }

        if (optionButtonPrefab == null || optionsContainer == null) return;

        GameObject actionBtn = Instantiate(optionButtonPrefab, optionsContainer);
        activeOptionButtons.Add(actionBtn);

        TextMeshProUGUI btnText = actionBtn.GetComponentInChildren<TextMeshProUGUI>();
        if (btnText != null) btnText.text = buttonText;

        Button btnComp = actionBtn.GetComponent<Button>();
        if (btnComp != null)
        {
            btnComp.onClick.AddListener(() =>
            {
                isWaitingForChoice = false;
                onClickAction?.Invoke();
            });
        }
    }

    public void CloseDialoguePanel()
    {
        if (_typewriterCoroutine != null)
        {
            StopCoroutine(_typewriterCoroutine);
            _typewriterCoroutine = null;
        }
        if (_advanceWaitCoroutine != null)
        {
            StopCoroutine(_advanceWaitCoroutine);
            _advanceWaitCoroutine = null;
        }

        isTyping = false;
        isWaitingForChoice = false;
        SetAutoMode(false);
        SetSkipMode(false);

        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        ClearOptions();
        activeCloseCallback = null;

        if (DialogueManager.Instance != null && DialogueManager.Instance.isDialogueActive)
        {
            DialogueManager.Instance.EndDialogue();
        }
    }
}
