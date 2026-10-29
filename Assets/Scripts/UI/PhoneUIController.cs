using System;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public enum PhoneScreenState
{
    Home,
    WeTalk_List,
    WeTalk_Room,
    OutingApp,
    SaveApp
}

/// <summary>
/// Ponsel in-game: Home, WeTalk (chat intel dengan Edelweiss), Akhir Pekan (ajak hangout) dan Simpan/Muat.
/// Daftar kontak, pertanyaan cepat, dan venue dibaca dari SQLite (tbl_npc_preferences, tbl_npc_list,
/// tbl_venues) lalu diisi dari template tombol yang disembunyikan di scene (dibangun PhoneSceneBuilder).
/// </summary>
public class PhoneUIController : MonoBehaviour
{
    private static PhoneUIController _instance;
    public static PhoneUIController Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<PhoneUIController>(FindObjectsInactive.Include);
            }
            return _instance;
        }
        private set { _instance = value; }
    }

    private const int EDELWEISS_NPC_ID = 104;
    private const int RUMOR_TOPIC_ID = 0;
    private const float MAX_BUBBLE_TEXT_WIDTH = 360f;
    private const float REPLY_DELAY = 0.8f;
    private const float OVERWRITE_CONFIRM_WINDOW = 4f;

    [Header("Status Navigasi")]
    public PhoneScreenState currentScreenState = PhoneScreenState.Home;

    [Header("Top Status Bar")]
    public TextMeshProUGUI txtStatusBarTime;
    public TextMeshProUGUI txtStatusBarDay;
    public TextMeshProUGUI txtStatusBarBattery;

    [Header("Home Widget")]
    public TextMeshProUGUI txtHomeClock;
    public TextMeshProUGUI txtHomeDay;
    public TextMeshProUGUI txtHomeDate;

    [Header("Layar Utama (Screens)")]
    public GameObject screenHome;
    public GameObject screenWeTalkApp;
    public GameObject screenOutingApp;
    public GameObject screenSaveApp;

    [Header("Sub-Panel Cloud Save")]
    public TextMeshProUGUI txtQuickSaveInfo;
    public Button btnQuickSaveApp;
    public Button btnQuickLoadApp;
    public PhoneSaveSlotItemUI[] manualSlotItems;
    public TextMeshProUGUI txtSaveAppStatus;

    [Header("Sub-Panel WeTalk")]
    public GameObject panelChatList;
    public GameObject panelChatRoom;
    public TextMeshProUGUI txtChatRoomContactName;
    public TextMeshProUGUI txtChatRoomStatus;
    public ScrollRect messageScrollRect;
    public Transform messageContainer;
    public GameObject bubblePlayerPrefab; // template baris bubble (boleh objek scene nonaktif)
    public GameObject bubbleNpcPrefab;
    public GameObject chatInputArea;
    public Transform quickReplyContainer;
    public Button quickReplyTemplate;

    [Header("Sub-Panel Campus Outing")]
    public GameObject panelOutingSelectContact;
    public GameObject panelOutingSelectVenue;
    public TextMeshProUGUI txtWeekendWarning;
    public TextMeshProUGUI txtVenueTitle;
    public Transform outingContactContainer;
    public Transform venueCardsContainer;
    public Button outingOptionTemplate; // anak: Txt_Title, Txt_Sub

    [Header("Bottom Nav Bar")]
    public Button btnNavBack;
    public Button btnNavHome;
    public Button btnNavClose;

    [Header("State Runtime")]
    public int selectedNpcId = 0;
    private bool openedOnce = false;
    private bool hasGreetedEdelweiss = false;
    private bool isWaitingForReply = false;
    private int pendingOverwriteSlot = -1;
    private float pendingOverwriteTime;
    private readonly List<GameObject> quickReplyButtons = new List<GameObject>();

    private struct Contact
    {
        public int id;
        public string name;
    }

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else if (_instance != this)
        {
            Destroy(this);
        }
    }

    void Start()
    {
        // Root yang disimpan nonaktif baru menjalankan Start saat pertama dibuka; jangan langsung ditutup lagi
        if (!openedOnce) TutupPhone();
    }

    void Update()
    {
        if (IsEscapePressed()) OnClick_NavBack();
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

    // =========================================================
    // KONTROL DASAR PONSEL
    // =========================================================
    public void BukaPhone()
    {
        openedOnce = true;
        gameObject.SetActive(true);
        transform.SetAsLastSibling();
        if (HUDController.Instance != null) HUDController.Instance.HidePredictiveTooltip();
        SetScreenState(PhoneScreenState.Home);
    }

    public void TutupPhone()
    {
        isWaitingForReply = false;
        gameObject.SetActive(false);
    }

    public void UpdateStatusBar()
    {
        GameManager gm = GameManager.Instance;
        if (gm != null)
        {
            if (txtStatusBarTime != null) txtStatusBarTime.text = gm.GetClockText();
            if (txtStatusBarDay != null)
            {
                string workdayStr = gm.IsWorkday() ? "Hari Kerja" : "Akhir Pekan";
                txtStatusBarDay.text = $"Hari {gm.currentDay} ({workdayStr})";
            }
            if (txtHomeClock != null) txtHomeClock.text = gm.GetClockText();
            if (txtHomeDay != null) txtHomeDay.text = gm.GetDayName();
            if (txtHomeDate != null) txtHomeDate.text = $"{gm.GetDateText(true)}  ·  Hari {gm.currentDay}, {gm.currentTimeBlock}";
        }
        if (txtStatusBarBattery != null)
        {
            txtStatusBarBattery.text = "100% 5G";
        }
    }

    // =========================================================
    // MANAJER PERPINDAHAN LAYAR (STATE NAVIGATION)
    // =========================================================
    public void SetScreenState(PhoneScreenState newState)
    {
        currentScreenState = newState;
        UpdateStatusBar();

        if (screenHome != null) screenHome.SetActive(newState == PhoneScreenState.Home);
        if (screenWeTalkApp != null) screenWeTalkApp.SetActive(newState == PhoneScreenState.WeTalk_List || newState == PhoneScreenState.WeTalk_Room);
        if (screenOutingApp != null) screenOutingApp.SetActive(newState == PhoneScreenState.OutingApp);
        if (screenSaveApp != null) screenSaveApp.SetActive(newState == PhoneScreenState.SaveApp);

        if (newState == PhoneScreenState.WeTalk_List)
        {
            if (panelChatList != null) panelChatList.SetActive(true);
            if (panelChatRoom != null) panelChatRoom.SetActive(false);
        }
        else if (newState == PhoneScreenState.WeTalk_Room)
        {
            if (panelChatList != null) panelChatList.SetActive(false);
            if (panelChatRoom != null) panelChatRoom.SetActive(true);
            InitEdelweissChatRoom();
        }
        else if (newState == PhoneScreenState.OutingApp)
        {
            RefreshOutingApp();
        }
        else if (newState == PhoneScreenState.SaveApp)
        {
            pendingOverwriteSlot = -1;
            if (txtSaveAppStatus != null) txtSaveAppStatus.text = "Pilih slot untuk menyimpan atau memuat cerita.";
            RefreshSaveApp();
        }
    }

    // =========================================================
    // DATA KONTAK (SQLite)
    // =========================================================
    private List<Contact> LoadContacts()
    {
        var contacts = new List<Contact>();
        if (DatabaseManager.Instance == null) return contacts;

        DataTable dt = DatabaseManager.Instance.ExecuteQuery(
            "SELECT n.npc_id, n.npc_name FROM tbl_npc_preferences p JOIN tbl_npc_list n ON n.npc_id = p.npc_id ORDER BY n.npc_id;");
        if (dt == null) return contacts;

        foreach (DataRow row in dt.Rows)
        {
            contacts.Add(new Contact { id = Convert.ToInt32(row["npc_id"]), name = ShortName(row["npc_name"].ToString()) });
        }
        return contacts;
    }

    // "Li Haoran (李浩然)" -> "Li Haoran", "Edelweiss Mayori Lenathea" tetap utuh
    private static string ShortName(string npcName)
    {
        int paren = npcName.IndexOf(" (", StringComparison.Ordinal);
        return paren > 0 ? npcName.Substring(0, paren) : npcName;
    }

    private string GetContactName(int npcId)
    {
        if (npcId == RUMOR_TOPIC_ID) return "Kampus";
        foreach (Contact c in LoadContacts())
        {
            if (c.id == npcId) return c.name;
        }
        return "dia";
    }

    // Mengosongkan container yang diisi dari template (template-nya sendiri dibiarkan)
    private static void ClearGenerated(Transform container, Component template)
    {
        if (container == null) return;
        for (int i = container.childCount - 1; i >= 0; i--)
        {
            GameObject child = container.GetChild(i).gameObject;
            if (template != null && child == template.gameObject) continue;
            Destroy(child);
        }
    }

    private Button SpawnOption(Button template, Transform container, string title, string sub)
    {
        Button btn = Instantiate(template, container);
        btn.gameObject.SetActive(true);
        btn.onClick.RemoveAllListeners();
        foreach (TextMeshProUGUI t in btn.GetComponentsInChildren<TextMeshProUGUI>(true))
        {
            if (t.name == "Txt_Title") t.text = title;
            else if (t.name == "Txt_Sub") t.text = sub;
        }
        return btn;
    }

    // =========================================================
    // MODUL WETALK (MESSAGING APP & INFORMATION BROKER)
    // =========================================================
    public void OnClick_AppWeTalk()
    {
        SetScreenState(PhoneScreenState.WeTalk_List);
    }

    public void OnClick_OpenEdelweissChat()
    {
        SetScreenState(PhoneScreenState.WeTalk_Room);
    }

    private void InitEdelweissChatRoom()
    {
        if (txtChatRoomContactName != null) txtChatRoomContactName.text = GetContactName(EDELWEISS_NPC_ID);
        if (txtChatRoomStatus != null) txtChatRoomStatus.text = "Online • Info Broker";

        BuildQuickReplies();

        if (!hasGreetedEdelweiss)
        {
            hasGreetedEdelweiss = true;
            AddEdelweissMessage("Hai Devano! Selamat datang di WeTalk. Ada kabar teman atau dosen yang ingin kamu tanyakan hari ini? 😉");
        }
        ScrollToBottom();
    }

    private void BuildQuickReplies()
    {
        if (quickReplyContainer == null || quickReplyTemplate == null) return; // scene lama: tombol tanya statis

        ClearGenerated(quickReplyContainer, quickReplyTemplate);
        quickReplyButtons.Clear();

        foreach (Contact c in LoadContacts())
        {
            if (c.id == EDELWEISS_NPC_ID) continue;
            AddQuickReply($"Tanya soal {c.name}", c.id);
        }
        AddQuickReply("Cek rumor kampus", RUMOR_TOPIC_ID);
        SetQuickRepliesInteractable(!isWaitingForReply);
    }

    private void AddQuickReply(string label, int topicId)
    {
        Button btn = Instantiate(quickReplyTemplate, quickReplyContainer);
        btn.gameObject.SetActive(true);
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => SendEdelweissInquiry(topicId));
        TextMeshProUGUI t = btn.GetComponentInChildren<TextMeshProUGUI>(true);
        if (t != null) t.text = label;
        quickReplyButtons.Add(btn.gameObject);
    }

    private void SetQuickRepliesInteractable(bool interactable)
    {
        foreach (GameObject go in quickReplyButtons)
        {
            if (go != null) go.GetComponent<Button>().interactable = interactable;
        }
    }

    /// <summary>
    /// Mengirimkan pertanyaan teks dari Devano ke Edelweiss untuk mendapatkan bocoran intel sosial.
    /// </summary>
    public void SendEdelweissInquiry(int targetNpcId)
    {
        if (isWaitingForReply) return; // satu pertanyaan sekali jalan

        string targetName = GetContactName(targetNpcId);
        if (targetNpcId == RUMOR_TOPIC_ID)
            AddPlayerMessage("Edel, bagaimana kondisi kampus belakangan ini? Ada gosip atau rumor aneh nggak?");
        else
            AddPlayerMessage($"Edel, kamu tahu kabar soal {targetName} belakangan ini?");

        if (Application.isPlaying && gameObject.activeInHierarchy)
            StartCoroutine(CoroutineEdelweissReply(targetNpcId, targetName));
        else
            ProcessEdelweissReply(targetNpcId, targetName);
    }

    private IEnumerator CoroutineEdelweissReply(int targetNpcId, string targetName)
    {
        isWaitingForReply = true;
        SetQuickRepliesInteractable(false);
        GameObject typing = CreateBubble(false, "• • •");
        ScrollToBottom();

        yield return new WaitForSeconds(REPLY_DELAY);

        if (typing != null) Destroy(typing);
        ProcessEdelweissReply(targetNpcId, targetName);
        isWaitingForReply = false;
        SetQuickRepliesInteractable(true);
    }

    private void ProcessEdelweissReply(int targetNpcId, string targetName)
    {
        if (targetNpcId == RUMOR_TOPIC_ID)
        {
            // Intel rumor kampus global
            int rumorLevel = (SocialManager.Instance != null) ? SocialManager.Instance.globalRumorLevel : 0;
            string rumorReply;
            if (rumorLevel == 0)
                rumorReply = "Kampus lagi tenang banget kok, Devano! Nama kamu bersih dan nggak ada desas-desus aneh. Lanjutkan fokus belajarmu ya!";
            else if (rumorLevel == 1)
                rumorReply = "Hmm, aku dengar bisik-bisik kecil tentangmu di sekitar kantin. Tapi masih taraf wajar kok. Jangan lupa rajin sapa teman-teman lokal biar suasana tetap cair!";
            else if (rumorLevel == 2)
                rumorReply = "Devano, hati-hati ya! Rumor tentangmu mulai menyebar di fakultas. Beberapa mahasiswa mulai membicarakanmu di lorong. Coba perbaiki hubungan sosialmu sebelum Dosen turun tangan!";
            else
                rumorReply = "GAWAT DEVANO! Terjadi ledakan rumor di seluruh kampus! Namamu sedang jadi sorotan negatif dan Dosen Xiang Bai mungkin akan segera memanggilmu ke ruangannya!";
            AddEdelweissMessage(rumorReply);
            return;
        }

        // Kueri preferensi dan status relasi karakter dari SQLite
        string query = "SELECT p.favorite_topic, p.sensitive_topic, p.intel_hint, " +
                       "v_fav.venue_name AS fav_venue, v_hate.venue_name AS hate_venue, " +
                       "r.guanxi_score, r.loneliness_meter, r.affection_state, n.npc_name " +
                       "FROM tbl_npc_preferences p " +
                       "JOIN tbl_npc_list n ON p.npc_id = n.npc_id " +
                       "LEFT JOIN tbl_venues v_fav ON p.favorite_venue_id = v_fav.venue_id " +
                       "LEFT JOIN tbl_venues v_hate ON p.hated_venue_id = v_hate.venue_id " +
                       "LEFT JOIN tbl_npc_relations r ON p.npc_id = r.npc_id AND r.player_id = 1 " +
                       $"WHERE p.npc_id = {targetNpcId};";

        DataTable dt = DatabaseManager.Instance.ExecuteQuery(query);
        if (dt == null || dt.Rows.Count == 0)
        {
            AddEdelweissMessage($"Hmm, aneh... aku belum punya banyak catatan tentang {targetName}. Coba kenali dia lebih dekat di kampus dulu ya!");
            return;
        }

        DataRow row = dt.Rows[0];
        string npcName = ShortName(row["npc_name"].ToString());
        int guanxi = row["guanxi_score"] != DBNull.Value ? Convert.ToInt32(row["guanxi_score"]) : 0;
        int loneliness = row["loneliness_meter"] != DBNull.Value ? Convert.ToInt32(row["loneliness_meter"]) : 0;
        int affState = row["affection_state"] != DBNull.Value ? Convert.ToInt32(row["affection_state"]) : 0;
        string favVenue = row["fav_venue"] != DBNull.Value ? row["fav_venue"].ToString() : "tempat santai";
        string sensitiveTopic = row["sensitive_topic"] != DBNull.Value ? row["sensitive_topic"].ToString() : "topik sensitif";
        string intelHint = row["intel_hint"] != DBNull.Value ? row["intel_hint"].ToString() : "";

        // 1. Indikator Terselubung Loneliness (Bakudan Radar)
        string lonelinessText;
        if (loneliness >= 80)
            lonelinessText = $"<b>Duh Devano, gawat banget...</b> {npcName} lagi sensi parah dan merasa kamu cuekin belakangan ini. Kalau diabaikan terus, bisa memicu ledakan rumor kampus! Mending buruan sapa atau ajak ngobrol!";
        else if (loneliness >= 50)
            lonelinessText = $"Kayaknya {npcName} mulai merasa kamu jarang nyapa dia deh. Jangan kelamaan dicuekin ya, nanti hubungannya makin renggang.";
        else
            lonelinessText = $"Hubunganmu dengan {npcName} aman banget kok, santai aja! Dia senang temenan sama kamu.";

        // 2. Status Kedekatan Emosional (Affection & Guanxi)
        string affLabel = "Formal / Kenal Biasa";
        if (affState == 3) affLabel = "Lingkaran Inti / Tokimeki";
        else if (affState == 2) affLabel = "Sahabat Akrab";
        else if (affState == 1) affLabel = "Teman Bicara";

        // Pesan dipecah jadi beberapa bubble pendek, seperti orang mengetik di chat
        AddEdelweissMessage(lonelinessText);
        AddEdelweissMessage($"Guanxi kalian <b>{guanxi}/100</b> ({affLabel}). Dia suka banget kalau diajak ke <i>{favVenue}</i>, tapi jangan pernah singgung soal <i>{sensitiveTopic}</i> ya.");
        if (!string.IsNullOrEmpty(intelHint)) AddEdelweissMessage($"<i>Catatan Edel: \"{intelHint}\"</i>");

        if (TelemetryLogger.Instance != null)
        {
            TelemetryLogger.Instance.RecordCriticalEvent("PHONE_INTEL", $"WeTalk intel Edelweiss: {npcName} (Guanxi: {guanxi}, Loneliness: {loneliness})");
        }
    }

    public void AddPlayerMessage(string messageText)
    {
        CreateBubble(true, messageText);
        ScrollToBottom();
    }

    public void AddEdelweissMessage(string messageText)
    {
        CreateBubble(false, messageText);
        ScrollToBottom();
    }

    private GameObject CreateBubble(bool fromPlayer, string bodyText)
    {
        GameObject template = fromPlayer ? bubblePlayerPrefab : bubbleNpcPrefab;
        if (template == null)
        {
            // Scene lama tanpa template bubble
            return fromPlayer
                ? CreateBubbleObject("Bubble_Player", TextAnchor.MiddleRight, new Color(0.05f, 0.45f, 0.38f, 0.95f), "Devano", new Color(0.4f, 0.95f, 0.7f), bodyText)
                : CreateBubbleObject("Bubble_Edelweiss", TextAnchor.MiddleLeft, new Color(0.16f, 0.20f, 0.26f, 0.95f), "Edelweiss (Info Broker)", new Color(1f, 0.55f, 0.8f), bodyText);
        }
        if (messageContainer == null) return null;

        GameObject row = Instantiate(template, messageContainer);
        row.SetActive(true);

        TextMeshProUGUI body = null;
        foreach (TextMeshProUGUI t in row.GetComponentsInChildren<TextMeshProUGUI>(true))
        {
            if (t.name == "Txt_Body") body = t;
        }
        if (body != null)
        {
            body.text = bodyText;
            // Bubble selebar isinya, maksimal MAX_BUBBLE_TEXT_WIDTH lalu membungkus ke baris berikutnya
            LayoutElement le = body.GetComponent<LayoutElement>();
            if (le == null) le = body.gameObject.AddComponent<LayoutElement>();
            le.preferredWidth = Mathf.Min(body.GetPreferredValues(bodyText).x + 2f, MAX_BUBBLE_TEXT_WIDTH);
        }
        return row;
    }

    private GameObject CreateBubbleObject(string bubbleName, TextAnchor alignment, Color bgColor, string senderName, Color senderColor, string bodyText)
    {
        if (messageContainer == null) return null;

        // Container pembungkus untuk penataan rata kiri/kanan
        GameObject rowGO = new GameObject(bubbleName, typeof(RectTransform));
        rowGO.transform.SetParent(messageContainer, false);

        HorizontalLayoutGroup hlg = rowGO.AddComponent<HorizontalLayoutGroup>();
        hlg.childAlignment = alignment;
        hlg.childControlWidth = false;
        hlg.childControlHeight = false;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;
        hlg.padding = new RectOffset(10, 10, 4, 4);

        // Bubble fisik di dalam row
        GameObject bubbleGO = new GameObject("BubbleBody", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        bubbleGO.transform.SetParent(rowGO.transform, false);
        bubbleGO.GetComponent<Image>().color = bgColor;

        VerticalLayoutGroup vlg = bubbleGO.GetComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(14, 14, 10, 10);
        vlg.spacing = 4;
        vlg.childAlignment = TextAnchor.UpperLeft;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        ContentSizeFitter csf = bubbleGO.GetComponent<ContentSizeFitter>();
        csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        bubbleGO.AddComponent<LayoutElement>().preferredWidth = 320f;

        TextMeshProUGUI tmpSender = new GameObject("Txt_Sender", typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
        tmpSender.transform.SetParent(bubbleGO.transform, false);
        tmpSender.text = senderName;
        tmpSender.fontSize = 11;
        tmpSender.fontStyle = FontStyles.Bold;
        tmpSender.color = senderColor;

        TextMeshProUGUI tmpBody = new GameObject("Txt_Body", typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
        tmpBody.transform.SetParent(bubbleGO.transform, false);
        tmpBody.text = bodyText;
        tmpBody.fontSize = 13;
        tmpBody.color = Color.white;
        tmpBody.textWrappingMode = TextWrappingModes.Normal;

        return rowGO;
    }

    private void ScrollToBottom()
    {
        if (messageScrollRect == null) return;
        if (Application.isPlaying && gameObject.activeInHierarchy) StartCoroutine(ScrollToBottomRoutine());
        else messageScrollRect.verticalNormalizedPosition = 0f;
    }

    // Tunggu layout bubble baru dihitung dulu; kalau langsung, scroll berhenti satu bubble terlalu cepat
    private IEnumerator ScrollToBottomRoutine()
    {
        yield return null;
        Canvas.ForceUpdateCanvases();
        if (messageScrollRect.content != null) LayoutRebuilder.ForceRebuildLayoutImmediate(messageScrollRect.content);
        messageScrollRect.verticalNormalizedPosition = 0f;
    }

    // =========================================================
    // MODUL CAMPUS OUTING (WEEKEND PLANNER APP)
    // =========================================================
    public void OnClick_AppOuting()
    {
        SetScreenState(PhoneScreenState.OutingApp);
    }

    public void RefreshOutingApp()
    {
        bool isWorkday = GameManager.Instance != null && GameManager.Instance.IsWorkday();

        if (txtWeekendWarning != null)
        {
            txtWeekendWarning.gameObject.SetActive(isWorkday);
            if (isWorkday)
            {
                txtWeekendWarning.text = "<b>Belum akhir pekan</b>\nJanjian hangout hanya bisa di hari Sabtu & Minggu. Hari kerja fokus kuliah dulu ya!";
            }
        }

        if (panelOutingSelectContact != null) panelOutingSelectContact.SetActive(!isWorkday);
        if (panelOutingSelectVenue != null) panelOutingSelectVenue.SetActive(false);

        if (!isWorkday && outingOptionTemplate != null && outingContactContainer != null)
        {
            ClearGenerated(outingContactContainer, outingOptionTemplate);
            foreach (Contact c in LoadContacts())
            {
                int npcId = c.id;
                Button btn = SpawnOption(outingOptionTemplate, outingContactContainer, c.name, "Ajak jalan akhir pekan ini");
                btn.onClick.AddListener(() => OnSelectNpcForHangout(npcId));
            }
        }
    }

    public void OnSelectNpcForHangout(int npcId)
    {
        selectedNpcId = npcId;
        if (panelOutingSelectContact != null) panelOutingSelectContact.SetActive(false);
        if (panelOutingSelectVenue != null) panelOutingSelectVenue.SetActive(true);
        if (txtVenueTitle != null) txtVenueTitle.text = $"Ajak {GetContactName(npcId)} ke mana?";
        BuildVenueList();
    }

    private void BuildVenueList()
    {
        if (outingOptionTemplate == null || venueCardsContainer == null || DatabaseManager.Instance == null) return; // scene lama: kartu venue statis

        ClearGenerated(venueCardsContainer, outingOptionTemplate);
        DataTable dt = DatabaseManager.Instance.ExecuteQuery(
            "SELECT venue_id, venue_name, cost_ph, min_etiquette_req FROM tbl_venues ORDER BY venue_id;");
        if (dt == null) return;

        int etiquette = PlayerStats.Instance != null ? PlayerStats.Instance.culturalEtiquette : 0;
        foreach (DataRow row in dt.Rows)
        {
            int venueId = Convert.ToInt32(row["venue_id"]);
            int costPh = Convert.ToInt32(row["cost_ph"]);
            int minEtiq = Convert.ToInt32(row["min_etiquette_req"]);
            bool allowed = etiquette >= minEtiq;

            string sub = $"Fisik -{costPh}  ·  Etika min. {minEtiq}" + (allowed ? "" : $"  ·  <color=#FFB3C8>Etikamu {etiquette}</color>");
            Button btn = SpawnOption(outingOptionTemplate, venueCardsContainer, row["venue_name"].ToString(), sub);
            btn.interactable = allowed;
            btn.onClick.AddListener(() => OnSelectVenueForHangout(venueId));
        }
    }

    public void OnSelectVenueForHangout(int venueId)
    {
        TutupPhone();
        if (PhoneOutingManager.Instance != null)
        {
            PhoneOutingManager.Instance.AjakHangout(selectedNpcId, venueId);
        }
        else
        {
            Debug.LogError("<color=red>[PhoneUIController]</color> PhoneOutingManager.Instance tidak ditemukan!");
        }
    }

    public void OnClick_KembaliKePilihKontak()
    {
        if (panelOutingSelectVenue != null) panelOutingSelectVenue.SetActive(false);
        if (panelOutingSelectContact != null) panelOutingSelectContact.SetActive(true);
    }

    // =========================================================
    // BOTTOM NAV BAR HANDLERS
    // =========================================================
    public void OnClick_NavBack()
    {
        if (currentScreenState == PhoneScreenState.WeTalk_Room)
        {
            SetScreenState(PhoneScreenState.WeTalk_List);
        }
        else if (currentScreenState == PhoneScreenState.OutingApp && panelOutingSelectVenue != null && panelOutingSelectVenue.activeSelf)
        {
            OnClick_KembaliKePilihKontak();
        }
        else if (currentScreenState != PhoneScreenState.Home)
        {
            SetScreenState(PhoneScreenState.Home);
        }
        else
        {
            TutupPhone();
        }
    }

    public void OnClick_NavHome()
    {
        SetScreenState(PhoneScreenState.Home);
    }

    public void OnClick_NavClose()
    {
        TutupPhone();
    }

    // =========================================================
    // MODUL CLOUD SAVE & LOAD (HP / IN-GAME SMARTPHONE)
    // =========================================================
    public void OnClick_AppSave()
    {
        SetScreenState(PhoneScreenState.SaveApp);
    }

    private Dictionary<int, DataRow> LoadSaveMetadata()
    {
        var map = new Dictionary<int, DataRow>();
        DataTable dt = DatabaseManager.Instance.ExecuteQuery("SELECT * FROM tbl_save_metadata WHERE slot_id >= 0 AND slot_id <= 5;");
        if (dt != null)
        {
            foreach (DataRow row in dt.Rows) map[Convert.ToInt32(row["slot_id"])] = row;
        }
        return map;
    }

    public void RefreshSaveApp()
    {
        if (DatabaseManager.Instance == null) return;
        Dictionary<int, DataRow> saves = LoadSaveMetadata();

        // 1. Quick Save (Slot 0)
        bool hasQuick = saves.TryGetValue(SaveManager.QUICK_SAVE_SLOT, out DataRow q);
        if (txtQuickSaveInfo != null)
        {
            txtQuickSaveInfo.text = hasQuick
                ? $"<b>Hari {q["current_day"]} · {q["time_block"]}</b>   Fisik {q["ph_snapshot"]} · Mental {q["mh_snapshot"]}\n<size=80%><color=#B9C2E0>{q["saved_at"]}</color></size>"
                : "<i>Belum ada quick save.</i>";
        }
        if (btnQuickLoadApp != null) btnQuickLoadApp.interactable = hasQuick;

        // 2. Manual Slots (1-5)
        if (manualSlotItems == null) return;
        for (int i = 0; i < manualSlotItems.Length; i++)
        {
            int slotId = i + 1;
            PhoneSaveSlotItemUI item = manualSlotItems[i];
            if (item == null) continue;

            if (saves.TryGetValue(slotId, out DataRow r))
            {
                item.Setup(slotId, r["slot_title"].ToString(), Convert.ToInt32(r["current_day"]), r["time_block"].ToString(),
                    r["saved_at"].ToString(), Convert.ToInt32(r["ph_snapshot"]), Convert.ToInt32(r["mh_snapshot"]), true, HandleSaveSlot, HandleLoadSlot);
            }
            else
            {
                item.Setup(slotId, "Slot Kosong", 0, "-", "-", 0, 0, false, HandleSaveSlot, HandleLoadSlot);
            }
        }
    }

    public void HandleSaveSlot(int slotId)
    {
        if (SaveManager.Instance == null) return;

        // Slot terisi: minta tekan sekali lagi sebelum menimpa
        bool confirmed = pendingOverwriteSlot == slotId && Time.unscaledTime - pendingOverwriteTime <= OVERWRITE_CONFIRM_WINDOW;
        if (SaveManager.Instance.HasSaveData(slotId) && !confirmed)
        {
            pendingOverwriteSlot = slotId;
            pendingOverwriteTime = Time.unscaledTime;
            SetSaveStatus($"<color=#FFD27A>Slot {slotId} sudah terisi.</color> Tekan <b>Simpan</b> sekali lagi untuk menimpa.");
            return;
        }

        pendingOverwriteSlot = -1;
        SaveManager.Instance.SaveGame(slotId, $"Manual Slot {slotId}");
        SetSaveStatus($"<color=#8FE3B0>Tersimpan di Slot {slotId}.</color>");
        RefreshSaveApp();
    }

    public void HandleLoadSlot(int slotId)
    {
        if (SaveManager.Instance == null || !SaveManager.Instance.HasSaveData(slotId)) return;
        SaveManager.Instance.LoadGame(slotId);
        FinishLoad(slotId == SaveManager.QUICK_SAVE_SLOT ? "Quick save dimuat." : $"Slot {slotId} dimuat.");
    }

    public void OnClick_PhoneQuickSave()
    {
        if (SaveManager.Instance == null) return;
        SaveManager.Instance.QuickSave();
        SetSaveStatus("<color=#8FE3B0>Quick save tersimpan.</color>");
        RefreshSaveApp();
    }

    public void OnClick_PhoneQuickLoad()
    {
        if (SaveManager.Instance == null) return;
        if (!SaveManager.Instance.HasSaveData(SaveManager.QUICK_SAVE_SLOT))
        {
            SetSaveStatus("<color=#FF9AA8>Belum ada quick save.</color>");
            return;
        }
        SaveManager.Instance.QuickLoad();
        FinishLoad("Quick save dimuat.");
    }

    // Setelah memuat, kembali ke permainan: tutup ponsel & segarkan HUD
    private void FinishLoad(string message)
    {
        TutupPhone();
        if (HUDController.Instance != null)
        {
            HUDController.Instance.UpdateHUD();
            HUDController.Instance.ShowToastNotification(message);
        }
    }

    private void SetSaveStatus(string message)
    {
        if (txtSaveAppStatus != null) txtSaveAppStatus.text = message;
    }
}
