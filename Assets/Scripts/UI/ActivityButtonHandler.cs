using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ActivityButtonHandler : MonoBehaviour
{
    void Start()
    {
        BindButtonListeners();
    }

    // Menghubungkan seluruh listener tombol aktivitas saat runtime
    public void BindButtonListeners()
    {
        if (HUDController.Instance == null) return;

        Button btnStudy = HUDController.Instance.btnStudyLanguage;
        Button btnLunch = HUDController.Instance.btnLunchWithNPC;
        Button btnLecturer = HUDController.Instance.btnMeetLecturer;
        Button btnSleep = HUDController.Instance.btnSleep;

        if (btnStudy != null)
        {
            btnStudy.onClick.RemoveListener(OnClick_StudyLanguage);
            btnStudy.onClick.AddListener(OnClick_StudyLanguage);
            AddHoverTrigger(btnStudy.gameObject, OnHover_StudyLanguage, OnPointerExit);
        }

        if (btnLunch != null)
        {
            btnLunch.onClick.RemoveListener(OnClick_LunchWithLiHaoran);
            btnLunch.onClick.AddListener(OnClick_LunchWithLiHaoran);
            AddHoverTrigger(btnLunch.gameObject, OnHover_LunchWithLiHaoran, OnPointerExit);
        }

        if (btnLecturer != null)
        {
            btnLecturer.onClick.RemoveListener(OnClick_ReportToLecturer);
            btnLecturer.onClick.AddListener(OnClick_ReportToLecturer);
            AddHoverTrigger(btnLecturer.gameObject, OnHover_ReportToLecturer, OnPointerExit);
        }

        if (btnSleep != null)
        {
            btnSleep.onClick.RemoveListener(OnClick_Sleep);
            btnSleep.onClick.AddListener(OnClick_Sleep);
            AddHoverTrigger(btnSleep.gameObject, OnHover_Sleep, OnPointerExit);
        }

        Debug.Log("<color=green>[ActivityButtonHandler]</color> Listener klik dan hover tombol UI berhasil dihubungkan.");
    }

    // Aksi 1: Belajar Kosakata Bahasa Mandarin
    public void OnClick_StudyLanguage()
    {
        Debug.Log("<color=cyan>[Aksi UI]</color> Tombol 'Belajar Kosakata' diklik.");
        PlayerStats.Instance.ModifyStats(dLanguage: 15, dEtiquette: 0, dMental: -10, dPhysical: -5, dTheoretical: 0, dPractical: 0);
        GameManager.Instance.GeserWaktu();
        if (HUDController.Instance != null) HUDController.Instance.HidePredictiveTooltip();
    }

    // Aksi 2: Mengajak Makan Siang Li Haoran (NPC ID: 102)
    public void OnClick_LunchWithLiHaoran()
    {
        Debug.Log("<color=cyan>[Aksi UI]</color> Tombol 'Makan Siang Li Haoran' diklik.");
        PlayerStats.Instance.ModifyStats(dLanguage: 0, dEtiquette: 5, dMental: 10, dPhysical: -5, dTheoretical: 0, dPractical: 0);
        SocialManager.Instance.TambahGuanxi(npcId: 102, penambahanGuanxi: 10, reduksiLoneliness: 30);

        // Picu interaksi narasi dinamis (Milestone Event pending atau Casual Dialogue Pool sesuai Affection State)
        if (RelationshipProgressionManager.Instance != null)
        {
            RelationshipProgressionManager.Instance.StartCasualInteraction(102);
        }

        GameManager.Instance.GeserWaktu();
        if (HUDController.Instance != null) HUDController.Instance.HidePredictiveTooltip();
    }

    // Aksi 3: Laporan Progres ke Dosen Xiang Bai (Pemicu Dialog Node 1001 / Milestone Event)
    public void OnClick_ReportToLecturer()
    {
        Debug.Log("<color=cyan>[Aksi UI]</color> Tombol 'Laporan Progres Dosen' diklik.");
        if (RelationshipProgressionManager.Instance != null && RelationshipProgressionManager.Instance.TryStartPendingMilestoneEvent(101))
        {
            // Event milestone Xiang Bai dipicu
        }
        else
        {
            DialogueManager.Instance.StartDialogue(1001);
        }
        if (HUDController.Instance != null) HUDController.Instance.HidePredictiveTooltip();
    }

    // Aksi 4: Istirahat / Tidur Lebih Cepat
    public void OnClick_Sleep()
    {
        Debug.Log("<color=cyan>[Aksi UI]</color> Tombol 'Istirahat / Tidur' diklik.");
        GameManager.Instance.EvaluasiAkhirHari();
        if (HUDController.Instance != null) HUDController.Instance.HidePredictiveTooltip();
    }

    // Hover Tooltip Handlers (Predictive Tooltip)
    public void OnHover_StudyLanguage()
    {
        if (HUDController.Instance != null)
            HUDController.Instance.ShowPredictiveTooltip("Belajar Mandiri: Mengulang kosakata HSK dan tata bahasa Mandarin.", "Biaya: PH -5, MH -10 | Efek: Bahasa +15");
    }

    public void OnHover_LunchWithLiHaoran()
    {
        if (HUDController.Instance != null)
            HUDController.Instance.ShowPredictiveTooltip("Makan Siang bersama Li Haoran di kantin kampus.", "Biaya: PH -5 | Efek: Etika +5, MH +10, Guanxi +10");
    }

    public void OnHover_ReportToLecturer()
    {
        if (HUDController.Instance != null)
            HUDController.Instance.ShowPredictiveTooltip("Menemui Dosen Xiang Bai untuk asistensi progres analisis data.", "Syarat: Bahasa >= 30, Etika >= 50 | Risiko: Penalti Mianzi");
    }

    public void OnHover_Sleep()
    {
        if (HUDController.Instance != null)
            HUDController.Instance.ShowPredictiveTooltip("Mengakhiri hari lebih awal untuk istirahat penuh.", "Efek: PH +40, MH +40, Hari Berlanjut");
    }

    public void OnPointerExit()
    {
        if (HUDController.Instance != null)
            HUDController.Instance.HidePredictiveTooltip();
    }

    private void AddHoverTrigger(GameObject target, UnityEngine.Events.UnityAction onEnter, UnityEngine.Events.UnityAction onExit)
    {
        EventTrigger trigger = target.GetComponent<EventTrigger>();
        if (trigger == null) trigger = target.AddComponent<EventTrigger>();
        trigger.triggers.Clear();

        var entryEnter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        entryEnter.callback.AddListener((d) => onEnter?.Invoke());
        trigger.triggers.Add(entryEnter);

        var entryExit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        entryExit.callback.AddListener((d) => onExit?.Invoke());
        trigger.triggers.Add(entryExit);
    }
}
