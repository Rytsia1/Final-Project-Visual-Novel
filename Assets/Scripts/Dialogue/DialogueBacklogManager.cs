using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct BacklogEntry
{
    public string speakerName;
    public string dialogueText;

    public BacklogEntry(string speaker, string text)
    {
        speakerName = speaker;
        dialogueText = text;
    }
}

public class DialogueBacklogManager : MonoBehaviour
{
    private static DialogueBacklogManager _instance;
    public static DialogueBacklogManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<DialogueBacklogManager>();
                if (_instance == null)
                {
                    GameObject core = GameObject.Find("GAME_CORE") ?? GameObject.Find("[GAME_CORE]");
                    if (core != null)
                    {
                        _instance = core.AddComponent<DialogueBacklogManager>();
                    }
                    else
                    {
                        GameObject go = new GameObject("[DialogueBacklogManager]");
                        _instance = go.AddComponent<DialogueBacklogManager>();
                        DontDestroyOnLoad(go);
                    }
                }
            }
            return _instance;
        }
        private set => _instance = value;
    }

    public const int MAX_BACKLOG_ENTRIES = 100;
    public List<BacklogEntry> historyList = new List<BacklogEntry>();

    public event System.Action OnBacklogUpdated;

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(this);
        }
    }

    /// <summary>
    /// Menambahkan entri dialog ke riwayat backlog (maksimal 100 entri terbaru).
    /// </summary>
    public void AddEntry(string speaker, string text)
    {
        if (string.IsNullOrEmpty(text)) return;

        // Cegah duplikasi langsung dari node yang sama persis
        if (historyList.Count > 0)
        {
            BacklogEntry last = historyList[historyList.Count - 1];
            if (last.speakerName == speaker && last.dialogueText == text)
            {
                return;
            }
        }

        historyList.Add(new BacklogEntry(speaker, text));

        if (historyList.Count > MAX_BACKLOG_ENTRIES)
        {
            historyList.RemoveAt(0);
        }

        OnBacklogUpdated?.Invoke();
    }

    /// <summary>
    /// Menghapus seluruh riwayat saat New Game atau memuat save slot baru.
    /// </summary>
    public void ClearHistory()
    {
        historyList.Clear();
        OnBacklogUpdated?.Invoke();
    }
}
