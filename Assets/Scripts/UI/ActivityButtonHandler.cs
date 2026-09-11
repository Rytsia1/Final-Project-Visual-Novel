using UnityEngine;

public class ActivityButtonHandler : MonoBehaviour
{
    // Aksi 1: Belajar Kosakata Bahasa Mandarin
    public void OnClick_StudyLanguage()
    {
        PlayerStats.Instance.ModifyStats(dLanguage: 15, dEtiquette: 0, dMental: -10, dPhysical: -5, dTheoretical: 0, dPractical: 0);
        GameManager.Instance.GeserWaktu();
        if (HUDController.Instance != null) HUDController.Instance.HidePredictiveTooltip();
    }

    // Aksi 2: Mengajak Makan Siang Li Haoran (NPC ID: 102)
    public void OnClick_LunchWithLiHaoran()
    {
        PlayerStats.Instance.ModifyStats(dLanguage: 0, dEtiquette: 5, dMental: 10, dPhysical: -5, dTheoretical: 0, dPractical: 0);
        SocialManager.Instance.TambahGuanxi(npcId: 102, penambahanGuanxi: 10, reduksiLoneliness: 30);
        GameManager.Instance.GeserWaktu();
        if (HUDController.Instance != null) HUDController.Instance.HidePredictiveTooltip();
    }

    // Aksi 3: Laporan Progres ke Dosen Xiang Bai (Pemicu Dialog Node 1001)
    public void OnClick_ReportToLecturer()
    {
        DialogueManager.Instance.StartDialogue(1001);
        if (HUDController.Instance != null) HUDController.Instance.HidePredictiveTooltip();
    }

    // Aksi 4: Istirahat / Tidur Lebih Cepat
    public void OnClick_Sleep()
    {
        GameManager.Instance.EvaluasiAkhirHari();
        if (HUDController.Instance != null) HUDController.Instance.HidePredictiveTooltip();
    }

    // Hover Tooltip Handlers (Persiapan Predictive Tooltip)
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
}
