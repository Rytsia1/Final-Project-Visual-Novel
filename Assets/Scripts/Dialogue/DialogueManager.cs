using System;
using System.Data;
using UnityEngine;

/// <summary>
/// Mengorkestrasi seluruh siklus dialog: state percakapan aktif, membaca node/opsi dari SQLite,
/// gerbang kelayakan bahasa/etika (Mianzi), menerapkan konsekuensi satu opsi (stat, guanxi,
/// rumor, story flag, telemetry), lalu memutuskan node berikutnya atau menutup dialog — termasuk
/// alur khusus hangout dan interupsi pagi/rumor. Data access (query SQLite) ditulis inline per
/// method di sini secara sengaja, mengikuti pola yang sama dengan EventManager/GameManager di
/// proyek ini (tidak ada layer repository terpisah di manapun) — lihat audit refactor DialogueManager
/// untuk alasan mengapa memisahkannya menjadi kelas baru tidak dianggap perlu untuk saat ini.
/// </summary>
public class DialogueManager : MonoBehaviour
{
    private static DialogueManager _instance;
    public static DialogueManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<DialogueManager>();
            }
            return _instance;
        }
        private set { _instance = value; }
    }

    [Header("State Dialog Aktif")]
    public int currentNodeId;
    public int currentNpcId;
    public bool isDialogueActive = false;

    [Header("State Sesi Hangout")]
    public bool isHangoutDialogueActive = false;
    public int currentHangoutNpcId = 0;
    public int currentHangoutVenueId = 0;

    [Header("Callback Penutupan Dialog")]
    public System.Action onDialogueEndCallback = null;

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    // Memulai interaksi dialog dari node pembuka mana pun
    public void StartDialogue(int startingNodeId, System.Action onEndCallback = null)
    {
        if (startingNodeId <= 0)
        {
            EndDialogue();
            return;
        }

        isDialogueActive = true;
        currentNodeId = startingNodeId;
        if (onEndCallback != null)
        {
            onDialogueEndCallback = onEndCallback;
        }
        RenderCurrentNode();
    }

    public void StartHangoutDialogue(int startingNodeId, int npcId, int venueId)
    {
        isHangoutDialogueActive = true;
        currentHangoutNpcId = npcId;
        currentHangoutVenueId = venueId;
        StartDialogue(startingNodeId);
    }

    // Menarik teks dan nama pembicara dari tbl_dialogue_nodes
    public void RenderCurrentNode()
    {
        string query = $"SELECT npc_id, speaker_name, dialogue_text FROM tbl_dialogue_nodes WHERE node_id = {currentNodeId};";
        DataTable dt = DatabaseManager.Instance.ExecuteQuery(query);

        if (dt != null && dt.Rows.Count > 0)
        {
            currentNpcId = Convert.ToInt32(dt.Rows[0]["npc_id"]);
            string speaker = dt.Rows[0]["speaker_name"].ToString();
            string text = dt.Rows[0]["dialogue_text"].ToString();

            Debug.Log($"<color=cyan>[DIALOG]</color> <b>{speaker}</b>: \"{text}\"");

            if (DialogueUIController.Instance != null)
            {
                DialogueUIController.Instance.DisplayDialogue(speaker, text);
            }

            LoadDialogueOptions(currentNodeId);
        }
        else
        {
            EndDialogue();
        }
    }

    // Memuat daftar opsi respon dari tbl_dialogue_options
    private void LoadDialogueOptions(int nodeId)
    {
        string query = $"SELECT option_id, option_text FROM tbl_dialogue_options WHERE node_id = {nodeId};";
        DataTable dt = DatabaseManager.Instance.ExecuteQuery(query);

        if (dt != null && dt.Rows.Count > 0)
        {
            foreach (DataRow row in dt.Rows)
            {
                int optId = Convert.ToInt32(row["option_id"]);
                string optText = row["option_text"].ToString();

                if (DialogueUIController.Instance != null)
                {
                    DialogueUIController.Instance.CreateOptionButton(optId, optText);
                }
            }
        }
        else
        {
            // Jika node tidak memiliki opsi lanjutan, tampilkan tombol tutup
            if (DialogueUIController.Instance != null)
            {
                DialogueUIController.Instance.ShowCloseButton();
            }
        }
    }

    // =========================================================================
    // TIER 3: MULTI-PAYLOAD ATOMIC CONSEQUENCE EXECUTION
    // =========================================================================

    /// <summary>
    /// Mengeksekusi paket konsekuensi multi-variabel secara atomik dan terarah:
    /// Stat Devano, Relasi NPC, Rumor Global, Story Flag, Telemetri, dan Navigasi Dialog.
    /// </summary>
    public void SelectOption(DialogueOption opt)
    {
        if (opt == null)
        {
            EndDialogue();
            return;
        }

        // 1. Evaluasi Kelayakan Bahasa & Mianzi (Bila ada syarat)
        if (PlayerStats.Instance != null && PlayerStats.Instance.languageProficiency < opt.minLang && opt.failLanguageNodeId > 0)
        {
            StartDialogue(opt.failLanguageNodeId);
            return;
        }
        if (PlayerStats.Instance != null && PlayerStats.Instance.culturalEtiquette < opt.minEtiq && opt.failMianziNodeId > 0)
        {
            StartDialogue(opt.failMianziNodeId);
            return;
        }

        // 2. Eksekusi Efek Parameter Pemain (Stats Effect)
        if (opt.deltaPh != 0 || opt.deltaMh != 0 || opt.deltaTheory != 0 || 
            opt.deltaPractice != 0 || opt.deltaLang != 0 || opt.deltaEtiq != 0)
        {
            if (PlayerStats.Instance != null)
            {
                PlayerStats.Instance.ModifyStats(
                    dLanguage: opt.deltaLang, 
                    dEtiquette: opt.deltaEtiq, 
                    dMental: opt.deltaMh, 
                    dPhysical: opt.deltaPh, 
                    dTheoretical: opt.deltaTheory, 
                    dPractical: opt.deltaPractice
                );
            }
        }

        // 3. Eksekusi Efek Relasi Sosial (Relationship Effect)
        if (opt.targetNpcId.HasValue && SocialManager.Instance != null)
        {
            if (opt.deltaGuanxi != 0)
            {
                SocialManager.Instance.ModifyGuanxi(opt.targetNpcId.Value, opt.deltaGuanxi);
            }
            if (opt.deltaLoneliness != 0)
            {
                SocialManager.Instance.ModifyLoneliness(opt.targetNpcId.Value, opt.deltaLoneliness);
            }
        }

        // 4. Eksekusi Efek Rumor (Rumor Effect)
        if (opt.deltaRumor != 0 && GameManager.Instance != null)
        {
            GameManager.Instance.ModifyGlobalRumor(opt.deltaRumor);
        }

        // 5. Eksekusi Pencatatan Story Flag (Story Flag Effect)
        if (!string.IsNullOrEmpty(opt.setFlagName) && FlagManager.Instance != null)
        {
            FlagManager.Instance.SetFlag(opt.setFlagName, opt.setFlagVal, $"Dihasilkan dari pilihan ID {opt.optionId}");
        }

        // 6. Catat Telemetri Analitik
        if (TelemetryLogger.Instance != null)
        {
            TelemetryLogger.Instance.RecordCriticalEvent("CHOICE_CONSEQUENCE", 
                $"Option {opt.optionId} dipilih. Flag: {opt.setFlagName}={opt.setFlagVal}, Guanxi: {opt.deltaGuanxi}");
        }

        // 7. Pindah ke Next Node
        if (opt.nextNodeId > 0)
        {
            StartDialogue(opt.nextNodeId);
        }
        else
        {
            EndDialogue();
        }
    }

    // Titik Masuk Generik: Mengevaluasi opsi berdasarkan ID yang diklik pemain dari UI
    public void SelectOption(int optionId)
    {
        string query = $"SELECT * FROM tbl_dialogue_options WHERE option_id = {optionId};";
        DataTable dt = DatabaseManager.Instance.ExecuteQuery(query);

        if (dt == null || dt.Rows.Count == 0)
        {
            EndDialogue();
            return;
        }

        DialogueOption opt = DialogueOption.FromDataRow(dt.Rows[0]);
        if (opt == null)
        {
            EndDialogue();
            return;
        }

        // Fallback targetNpcId dari sesi dialog aktif jika belum diset di opsi
        if (!opt.targetNpcId.HasValue && currentNpcId > 0)
        {
            opt.targetNpcId = currentNpcId;
        }

        // Fallback prasyarat etika & bahasa dari node tujuan jika opsi belum mendefinisikannya
        if (opt.minLang == 0 && opt.minEtiq == 0 && opt.nextNodeId > 0)
        {
            string nodeQuery = $"SELECT req_language, req_etiquette, npc_id FROM tbl_dialogue_nodes WHERE node_id = {opt.nextNodeId};";
            DataTable dtNode = DatabaseManager.Instance.ExecuteQuery(nodeQuery);
            if (dtNode != null && dtNode.Rows.Count > 0)
            {
                opt.minLang = Convert.ToInt32(dtNode.Rows[0]["req_language"]);
                opt.minEtiq = Convert.ToInt32(dtNode.Rows[0]["req_etiquette"]);
                if (!opt.targetNpcId.HasValue && dtNode.Rows[0]["npc_id"] != DBNull.Value)
                {
                    opt.targetNpcId = Convert.ToInt32(dtNode.Rows[0]["npc_id"]);
                }
            }
        }

        SelectOption(opt);
    }

    public void EndDialogue()
    {
        isDialogueActive = false;
        int finishedNodeId = currentNodeId;

        if (DialogueUIController.Instance != null)
        {
            DialogueUIController.Instance.CloseDialoguePanel();
        }

        // 1. Eksekusi Custom Callback jika didaftarkan oleh pemanggil dialog
        if (onDialogueEndCallback != null)
        {
            var callback = onDialogueEndCallback;
            onDialogueEndCallback = null;
            callback.Invoke();
            return;
        }

        // 2. Penanganan paska sesi hangout akhir pekan (Aktivitas Berbiaya Waktu)
        if (isHangoutDialogueActive)
        {
            isHangoutDialogueActive = false;
            currentHangoutNpcId = 0;
            currentHangoutVenueId = 0;
            if (GameManager.Instance != null)
            {
                GameManager.Instance.GeserWaktu();
                Debug.Log("<color=green>[Hangout Selesai]</color> Waktu bergeser 1 blok paska interaksi hangout.");
            }
            return;
        }

        // 3. Penanganan paska interupsi paksa (Ledakan Rumor 3001)
        if (finishedNodeId == 3001 && SocialManager.Instance != null && SocialManager.Instance.globalRumorLevel >= 3)
        {
            SocialManager.Instance.globalRumorLevel = 2;
        }

        // 4. Penanganan interupsi pagi tanpa biaya waktu (Peringatan Edelweiss 2001 / Sapaan / Ledakan Rumor 3001)
        // Melanjutkan jadwal normal hari itu (pagi) tanpa memotong blok waktu
        if (GameManager.Instance != null && GameManager.Instance.currentTimeBlock == TimeBlock.Pagi && (finishedNodeId == 2001 || finishedNodeId == 3001))
        {
            Debug.Log("<color=cyan>[Interupsi Selesai]</color> Melanjutkan rutinitas pagi normal tanpa memotong blok waktu.");
            GameManager.Instance.LanjutRutinitasPagi();
        }

        Debug.Log("<color=grey>[Dialog]</color> Interaksi dialog selesai.");
    }
}
