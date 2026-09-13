using System;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum PhoneScreenState
{
    Home,
    WeTalk_List,
    WeTalk_Room,
    OutingApp
}

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

    [Header("Status Navigasi")]
    public PhoneScreenState currentScreenState = PhoneScreenState.Home;

    [Header("Top Status Bar")]
    public TextMeshProUGUI txtStatusBarTime;
    public TextMeshProUGUI txtStatusBarDay;
    public TextMeshProUGUI txtStatusBarBattery;

    [Header("Layar Utama (Screens)")]
    public GameObject screenHome;
    public GameObject screenWeTalkApp;
    public GameObject screenOutingApp;

    [Header("Sub-Panel WeTalk")]
    public GameObject panelChatList;
    public GameObject panelChatRoom;
    public TextMeshProUGUI txtChatRoomContactName;
    public TextMeshProUGUI txtChatRoomStatus;
    public ScrollRect messageScrollRect;
    public Transform messageContainer;
    public GameObject bubblePlayerPrefab;
    public GameObject bubbleNpcPrefab;
    public GameObject chatInputArea;

    [Header("Sub-Panel Campus Outing")]
    public GameObject panelOutingSelectContact;
    public GameObject panelOutingSelectVenue;
    public TextMeshProUGUI txtWeekendWarning;
    public Transform venueCardsContainer;

    [Header("Bottom Nav Bar")]
    public Button btnNavBack;
    public Button btnNavHome;
    public Button btnNavClose;

    [Header("State Runtime")]
    public int selectedNpcId = 0;
    private bool hasGreetedEdelweiss = false;

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
        TutupPhone();
    }

    // =========================================================
    // KONTROL DASAR PONSEL
    // =========================================================
    public void BukaPhone()
    {
        gameObject.SetActive(true);
        UpdateStatusBar();
        SetScreenState(PhoneScreenState.Home);
    }

    public void TutupPhone()
    {
        gameObject.SetActive(false);
    }

    public void UpdateStatusBar()
    {
        if (GameManager.Instance != null)
        {
            if (txtStatusBarDay != null)
            {
                string workdayStr = GameManager.Instance.IsWorkday() ? "Hari Kerja" : "Akhir Pekan";
                txtStatusBarDay.text = $"Hari {GameManager.Instance.currentDay} ({workdayStr})";
            }
            if (txtStatusBarTime != null)
            {
                txtStatusBarTime.text = $"{GameManager.Instance.currentTimeBlock}";
            }
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
        if (txtChatRoomContactName != null) txtChatRoomContactName.text = "Edelweiss Mayori";
        if (txtChatRoomStatus != null) txtChatRoomStatus.text = "Online • Info Broker";

        if (!hasGreetedEdelweiss)
        {
            hasGreetedEdelweiss = true;
            AddEdelweissMessage("Hai Devano! Selamat datang di WeTalk. Ada kabar teman atau dosen yang ingin kamu tanyakan hari ini? 😉");
        }
    }

    /// <summary>
    /// Mengirimkan pertanyaan teks dari Devano ke Edelweiss untuk mendapatkan bocoran intel sosial.
    /// </summary>
    public void SendEdelweissInquiry(int targetNpcId)
    {
        string targetName = "Kampus";
        if (targetNpcId == 101) targetName = "Dosen Xiang Bai";
        else if (targetNpcId == 102) targetName = "Li Haoran";
        else if (targetNpcId == 103) targetName = "Yang Mei";
        else if (targetNpcId == 104) targetName = "Edelweiss";

        // 1. Tambahkan bubble chat dari Devano (Kanan)
        if (targetNpcId == 0)
        {
            AddPlayerMessage("Edel, bagaimana kondisi kampus belakangan ini? Ada gosip atau rumor aneh nggak?");
        }
        else
        {
            AddPlayerMessage($"Edel, kamu tahu kabar soal {targetName} belakangan ini?");
        }

        // 2. Beri balasan alami dari Edelweiss
        if (Application.isPlaying)
        {
            StartCoroutine(CoroutineEdelweissReply(targetNpcId, targetName));
        }
        else
        {
            ProcessEdelweissReply(targetNpcId, targetName);
        }
    }

    private IEnumerator CoroutineEdelweissReply(int targetNpcId, string targetName)
    {
        yield return new WaitForSeconds(0.2f);
        ProcessEdelweissReply(targetNpcId, targetName);
    }

    private void ProcessEdelweissReply(int targetNpcId, string targetName)
    {
        if (targetNpcId == 0)
        {
            // Intel rumor kampus global
            int rumorLevel = (SocialManager.Instance != null) ? SocialManager.Instance.globalRumorLevel : 0;
            string rumorReply = "";
            if (rumorLevel == 0)
            {
                rumorReply = "Kampus lagi tenang banget kok, Devano! Nama kamu bersih dan nggak ada desas-desus aneh. Lanjutkan fokus belajarmu ya!";
            }
            else if (rumorLevel == 1)
            {
                rumorReply = "Hmm, aku dengar bisik-bisik kecil tentangmu di sekitar kantin. Tapi masih taraf wajar kok. Jangan lupa rajin sapa teman-teman lokal biar suasana tetap cair!";
            }
            else if (rumorLevel == 2)
            {
                rumorReply = "Devano, hati-hati ya! Rumor tentangmu mulai menyebar di fakultas. Beberapa mahasiswa mulai membicarakanmu di lorong. Coba perbaiki hubungan sosialmu sebelum Dosen turun tangan!";
            }
            else
            {
                rumorReply = "GAWAT DEVANO! Terjadi ledakan rumor di seluruh kampus! Namamu sedang jadi sorotan negatif dan Dosen Xiang Bai mungkin akan segera memanggilmu ke ruangannya!";
            }
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
        if (dt != null && dt.Rows.Count > 0)
        {
            DataRow row = dt.Rows[0];
            string npcName = row["npc_name"].ToString();
            int guanxi = Convert.ToInt32(row["guanxi_score"]);
            int loneliness = Convert.ToInt32(row["loneliness_meter"]);
            int affState = row["affection_state"] != DBNull.Value ? Convert.ToInt32(row["affection_state"]) : 0;
            string favVenue = row["fav_venue"] != DBNull.Value ? row["fav_venue"].ToString() : "Tempat santai";
            string sensitiveTopic = row["sensitive_topic"] != DBNull.Value ? row["sensitive_topic"].ToString() : "Topik sensitif";
            string intelHint = row["intel_hint"] != DBNull.Value ? row["intel_hint"].ToString() : "";

            // 1. Indikator Terselubung Loneliness (Bakudan Radar)
            string lonelinessText = "";
            if (loneliness >= 80)
            {
                lonelinessText = $"[PERINGATAN KRITIS] <b>Duh Devano, gawat banget...</b> {npcName} lagi sensi parah dan merasa kamu cuekin belakangan ini. Kalau diabaikan terus, bisa memicu ledakan rumor kampus! Mending buruan sapa atau ajak ngobrol!";
            }
            else if (loneliness >= 50)
            {
                lonelinessText = $"[WASPADA] Kayaknya {npcName} mulai merasa kamu jarang nyapa dia deh. Jangan kelamaan dicuekin ya, nanti hubungannya makin renggang.";
            }
            else
            {
                lonelinessText = $"[STATUS AMAN] Hubunganmu dengan {npcName} aman banget kok, santai aja! Dia senang temenan sama kamu.";
            }

            // 2. Status Kedekatan Emosional (Affection & Guanxi)
            string affLabel = "Formal / Kenal Biasa";
            if (affState == 3) affLabel = "Lingkaran Inti / Tokimeki";
            else if (affState == 2) affLabel = "Sahabat Akrab";
            else if (affState == 1) affLabel = "Teman Bicara";

            string statusText = $"Status Guanxi: <b>{guanxi}/100</b> ({affLabel}).";

            // 3. Tips Sosio-Kultural & Preferensi
            string tipsText = $"<b>Tips Hangout:</b> Dia suka banget kalau diajak ke <i>{favVenue}</i>! Tapi ingat, jangan pernah singgung soal <i>{sensitiveTopic}</i> ya.\n\n<i>Catatan Edel: \"{intelHint}\"</i>";

            string fullMessage = $"{lonelinessText}\n\n{statusText}\n\n{tipsText}";
            AddEdelweissMessage(fullMessage);

            if (TelemetryLogger.Instance != null)
            {
                TelemetryLogger.Instance.RecordCriticalEvent("PHONE_INTEL", $"WeTalk intel Edelweiss: {npcName} (Guanxi: {guanxi}, Loneliness: {loneliness})");
            }
        }
        else
        {
            AddEdelweissMessage($"Hmm, aneh... aku belum punya banyak catatan tentang {targetName}. Coba kenali dia lebih dekat di kampus dulu ya!");
        }
    }

    public void AddPlayerMessage(string messageText)
    {
        CreateBubbleObject("Bubble_Player", TextAnchor.MiddleRight, new Color(0.05f, 0.45f, 0.38f, 0.95f), "Devano", new Color(0.4f, 0.95f, 0.7f), messageText);
        ScrollToBottom();
    }

    public void AddEdelweissMessage(string messageText)
    {
        CreateBubbleObject("Bubble_Edelweiss", TextAnchor.MiddleLeft, new Color(0.16f, 0.20f, 0.26f, 0.95f), "Edelweiss (Info Broker)", new Color(1f, 0.55f, 0.8f), messageText);
        ScrollToBottom();
    }

    private GameObject CreateBubbleObject(string bubbleName, TextAnchor alignment, Color bgColor, string senderName, Color senderColor, string bodyText)
    {
        if (messageContainer == null) return null;

        // Container pembungkus untuk penataan rata kiri/kanan
        GameObject rowGO = new GameObject(bubbleName, typeof(RectTransform));
        rowGO.transform.SetParent(messageContainer, false);
        RectTransform rtRow = rowGO.GetComponent<RectTransform>();
        rtRow.sizeDelta = new Vector2(0, 0);

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

        Image img = bubbleGO.GetComponent<Image>();
        img.color = bgColor;

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

        LayoutElement le = bubbleGO.AddComponent<LayoutElement>();
        le.preferredWidth = 320f;

        // Teks Pengirim
        GameObject txtSenderGO = new GameObject("Txt_Sender", typeof(RectTransform), typeof(TextMeshProUGUI));
        txtSenderGO.transform.SetParent(bubbleGO.transform, false);
        TextMeshProUGUI tmpSender = txtSenderGO.GetComponent<TextMeshProUGUI>();
        tmpSender.text = senderName;
        tmpSender.fontSize = 11;
        tmpSender.fontStyle = FontStyles.Bold;
        tmpSender.color = senderColor;

        // Teks Pesan
        GameObject txtBodyGO = new GameObject("Txt_Body", typeof(RectTransform), typeof(TextMeshProUGUI));
        txtBodyGO.transform.SetParent(bubbleGO.transform, false);
        TextMeshProUGUI tmpBody = txtBodyGO.GetComponent<TextMeshProUGUI>();
        tmpBody.text = bodyText;
        tmpBody.fontSize = 13;
        tmpBody.color = Color.white;
        tmpBody.textWrappingMode = TextWrappingModes.Normal;

        return rowGO;
    }

    private void ScrollToBottom()
    {
        if (messageScrollRect != null)
        {
            Canvas.ForceUpdateCanvases();
            messageScrollRect.verticalNormalizedPosition = 0f;
        }
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
                txtWeekendWarning.text = "⚠️ <b>Aplikasi Weekend Outing Sedang Terkunci</b>\nJanjian hangout hanya dapat dilakukan di akhir pekan (Sabtu & Minggu). Hari kerja fokus kuliah dulu ya!";
            }
        }

        if (panelOutingSelectContact != null)
        {
            panelOutingSelectContact.SetActive(!isWorkday);
        }
        if (panelOutingSelectVenue != null)
        {
            panelOutingSelectVenue.SetActive(false);
        }
    }

    public void OnSelectNpcForHangout(int npcId)
    {
        selectedNpcId = npcId;
        if (panelOutingSelectContact != null) panelOutingSelectContact.SetActive(false);
        if (panelOutingSelectVenue != null) panelOutingSelectVenue.SetActive(true);
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

    // =========================================================
    // BOTTOM NAV BAR HANDLERS
    // =========================================================
    public void OnClick_NavBack()
    {
        if (currentScreenState == PhoneScreenState.WeTalk_Room)
        {
            SetScreenState(PhoneScreenState.WeTalk_List);
        }
        else if (currentScreenState == PhoneScreenState.WeTalk_List)
        {
            SetScreenState(PhoneScreenState.Home);
        }
        else if (currentScreenState == PhoneScreenState.OutingApp)
        {
            if (panelOutingSelectVenue != null && panelOutingSelectVenue.activeSelf)
            {
                panelOutingSelectVenue.SetActive(false);
                if (panelOutingSelectContact != null) panelOutingSelectContact.SetActive(true);
            }
            else
            {
                SetScreenState(PhoneScreenState.Home);
            }
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

    // Kompatibilitas mundur untuk fungsi lama
    public void OnClick_TanyaEdelweiss(int targetNpcId)
    {
        SetScreenState(PhoneScreenState.WeTalk_Room);
        SendEdelweissInquiry(targetNpcId);
    }

    public void OnClick_BukaMenuHangout()
    {
        SetScreenState(PhoneScreenState.OutingApp);
    }

    public void OnClick_KembaliKeMenuUtama()
    {
        SetScreenState(PhoneScreenState.Home);
    }

    public void OnClick_KembaliKePilihKontak()
    {
        if (panelOutingSelectVenue != null) panelOutingSelectVenue.SetActive(false);
        if (panelOutingSelectContact != null) panelOutingSelectContact.SetActive(true);
    }
}
