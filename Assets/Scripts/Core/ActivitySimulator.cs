using UnityEngine;

public class ActivitySimulator : MonoBehaviour
{
    void Update()
    {
        // Tekan Angka 1: Pilih Aksi Belajar Mandiri (Studi Bahasa)
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Debug.Log("<color=white>[Input Aksi]</color> Kenzo memilih Belajar Kosakata Mandarin.");
            PlayerStats.Instance.ModifyStats(dLanguage: 15, dEtiquette: 0, dMental: -10, dPhysical: -5, dTheoretical: 0, dPractical: 0);
            GameManager.Instance.GeserWaktu();
        }

        // Tekan Angka 2: Mengajak Makan Siang Li Haoran (NPC ID: 102)
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Debug.Log("<color=white>[Input Aksi]</color> Kenzo mengajak Li Haoran Makan Siang.");
            PlayerStats.Instance.ModifyStats(dLanguage: 0, dEtiquette: 5, dMental: 10, dPhysical: -5, dTheoretical: 0, dPractical: 0);
            SocialManager.Instance.TambahGuanxi(npcId: 102, penambahanGuanxi: 10, reduksiLoneliness: 30);
            GameManager.Instance.GeserWaktu();
        }

        // Tekan Spasi: Langsung Geser Waktu (Skip Slot)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("<color=white>[Input Aksi]</color> Melewatkan waktu tanpa interaksi sosial.");
            GameManager.Instance.GeserWaktu();
        }

        // Tekan D: Mulai Dialog Laporan Progres ke Dosen Xiang Bai (Node 1001)
        if (Input.GetKeyDown(KeyCode.D))
        {
            DialogueManager.Instance.StartDialogue(1001);
        }

        // Tekan Enter: Pilih Opsi Respon Pertama (Evaluasi Stat)
        if (Input.GetKeyDown(KeyCode.Return))
        {
            DialogueManager.Instance.SelectOption(1);
        }

        // Shortcut Debug Status untuk Menguji 3 Rute:
        // Tekan F1: Set Status Awal (Bahasa 20, Etika 15) -> Menguji RUTE C
        if (Input.GetKeyDown(KeyCode.F1))
        {
            PlayerStats.Instance.languageProficiency = 20;
            PlayerStats.Instance.culturalEtiquette = 15;
            PlayerStats.Instance.SaveStatsToDatabase();
            Debug.Log("<color=magenta>[DEBUG STAT]</color> Diatur ke: Bahasa 20, Etika 15 (Target: Rute C)");
        }

        // Tekan F2: Set Status Bahasa Tinggi, Etika Rendah (Bahasa 40, Etika 20) -> Menguji RUTE B (Mianzi)
        if (Input.GetKeyDown(KeyCode.F2))
        {
            PlayerStats.Instance.languageProficiency = 40;
            PlayerStats.Instance.culturalEtiquette = 20;
            PlayerStats.Instance.SaveStatsToDatabase();
            Debug.Log("<color=magenta>[DEBUG STAT]</color> Diatur ke: Bahasa 40, Etika 20 (Target: Rute B / Mianzi)");
        }

        // Tekan F3: Set Status Memenuhi Syarat (Bahasa 40, Etika 60) -> Menguji RUTE A (Sukses)
        if (Input.GetKeyDown(KeyCode.F3))
        {
            PlayerStats.Instance.languageProficiency = 40;
            PlayerStats.Instance.culturalEtiquette = 60;
            PlayerStats.Instance.SaveStatsToDatabase();
            Debug.Log("<color=magenta>[DEBUG STAT]</color> Diatur ke: Bahasa 40, Etika 60 (Target: Rute A)");
        }
    }

#if UNITY_EDITOR
    [UnityEditor.MenuItem("Game Debug/Test Route C (Bahasa 20, Etika 15)")]
    public static void TestRouteC()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("[Test] Jalankan Play Mode terlebih dahulu sebelum menjalankan pengujian rute.");
            return;
        }
        PlayerStats.Instance.languageProficiency = 20;
        PlayerStats.Instance.culturalEtiquette = 15;
        PlayerStats.Instance.SaveStatsToDatabase();
        Debug.Log("<color=magenta>[DEBUG STAT]</color> Diatur ke: Bahasa 20, Etika 15 (Target: Rute C)");
        DialogueManager.Instance.StartDialogue(1001);
        DialogueManager.Instance.SelectOption(1);
    }

    [UnityEditor.MenuItem("Game Debug/Test Route B (Bahasa 40, Etika 20)")]
    public static void TestRouteB()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("[Test] Jalankan Play Mode terlebih dahulu sebelum menjalankan pengujian rute.");
            return;
        }
        PlayerStats.Instance.languageProficiency = 40;
        PlayerStats.Instance.culturalEtiquette = 20;
        PlayerStats.Instance.SaveStatsToDatabase();
        Debug.Log("<color=magenta>[DEBUG STAT]</color> Diatur ke: Bahasa 40, Etika 20 (Target: Rute B / Mianzi)");
        DialogueManager.Instance.StartDialogue(1001);
        DialogueManager.Instance.SelectOption(1);
    }

    [UnityEditor.MenuItem("Game Debug/Test Route A (Bahasa 40, Etika 60)")]
    public static void TestRouteA()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("[Test] Jalankan Play Mode terlebih dahulu sebelum menjalankan pengujian rute.");
            return;
        }
        PlayerStats.Instance.languageProficiency = 40;
        PlayerStats.Instance.culturalEtiquette = 60;
        PlayerStats.Instance.SaveStatsToDatabase();
        Debug.Log("<color=magenta>[DEBUG STAT]</color> Diatur ke: Bahasa 40, Etika 60 (Target: Rute A)");
        DialogueManager.Instance.StartDialogue(1001);
        DialogueManager.Instance.SelectOption(1);
    }
#endif
}