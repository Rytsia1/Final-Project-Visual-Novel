using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class ActivitySimulator : MonoBehaviour
{
    void Update()
    {
        // Tekan Angka 1: Pilih Aksi Belajar Mandiri (Studi Bahasa)
        if (IsKeyPressed(KeyCode.Alpha1))
        {
            Debug.Log("<color=white>[Input Aksi]</color> Kenzo memilih Belajar Kosakata Mandarin.");
            PlayerStats.Instance.ModifyStats(dLanguage: 15, dEtiquette: 0, dMental: -10, dPhysical: -5, dTheoretical: 0, dPractical: 0);
            GameManager.Instance.GeserWaktu();
        }

        // Tekan Angka 2: Mengajak Makan Siang Li Haoran (NPC ID: 102)
        if (IsKeyPressed(KeyCode.Alpha2))
        {
            Debug.Log("<color=white>[Input Aksi]</color> Kenzo mengajak Li Haoran Makan Siang.");
            PlayerStats.Instance.ModifyStats(dLanguage: 0, dEtiquette: 5, dMental: 10, dPhysical: -5, dTheoretical: 0, dPractical: 0);
            SocialManager.Instance.TambahGuanxi(npcId: 102, penambahanGuanxi: 10, reduksiLoneliness: 30);
            GameManager.Instance.GeserWaktu();
        }

        // Tekan Spasi: Langsung Geser Waktu (Skip Slot)
        if (IsKeyPressed(KeyCode.Space))
        {
            Debug.Log("<color=white>[Input Aksi]</color> Melewatkan waktu tanpa interaksi sosial.");
            GameManager.Instance.GeserWaktu();
        }

        // Tekan D: Mulai Dialog Laporan Progres ke Dosen Xiang Bai (Node 1001)
        if (IsKeyPressed(KeyCode.D))
        {
            DialogueManager.Instance.StartDialogue(1001);
        }

        // Tekan Enter: Pilih Opsi Respon Pertama (Evaluasi Stat)
        if (IsKeyPressed(KeyCode.Return))
        {
            DialogueManager.Instance.SelectOption(1);
        }

        // Shortcut Debug Status untuk Menguji 3 Rute:
        // Tekan F1: Set Status Awal (Bahasa 20, Etika 15) -> Menguji RUTE C
        if (IsKeyPressed(KeyCode.F1))
        {
            PlayerStats.Instance.physicalHealth = 100;
            PlayerStats.Instance.mentalHealth = 80;
            PlayerStats.Instance.languageProficiency = 20;
            PlayerStats.Instance.culturalEtiquette = 15;
            PlayerStats.Instance.SaveStatsToDatabase();
            Debug.Log("<color=magenta>[DEBUG STAT]</color> Diatur ke: PH 100, MH 80, Bahasa 20, Etika 15 (Target: Rute C)");
        }

        // Tekan F2: Set Status Bahasa Tinggi, Etika Rendah (Bahasa 40, Etika 20) -> Menguji RUTE B (Mianzi)
        if (IsKeyPressed(KeyCode.F2))
        {
            PlayerStats.Instance.physicalHealth = 100;
            PlayerStats.Instance.mentalHealth = 80;
            PlayerStats.Instance.languageProficiency = 40;
            PlayerStats.Instance.culturalEtiquette = 20;
            PlayerStats.Instance.SaveStatsToDatabase();
            Debug.Log("<color=magenta>[DEBUG STAT]</color> Diatur ke: PH 100, MH 80, Bahasa 40, Etika 20 (Target: Rute B / Mianzi)");
        }

        // Tekan F3: Set Status Memenuhi Syarat (Bahasa 40, Etika 60) -> Menguji RUTE A (Sukses)
        if (IsKeyPressed(KeyCode.F3))
        {
            PlayerStats.Instance.physicalHealth = 100;
            PlayerStats.Instance.mentalHealth = 80;
            PlayerStats.Instance.languageProficiency = 40;
            PlayerStats.Instance.culturalEtiquette = 60;
            PlayerStats.Instance.SaveStatsToDatabase();
            Debug.Log("<color=magenta>[DEBUG STAT]</color> Diatur ke: PH 100, MH 80, Bahasa 40, Etika 60 (Target: Rute A)");
        }
    }

    // Helper pembaca input kompatibel untuk New Input System dan Legacy Input
    private bool IsKeyPressed(KeyCode key)
    {
#if ENABLE_INPUT_SYSTEM
        var kb = Keyboard.current;
        if (kb != null)
        {
            switch (key)
            {
                case KeyCode.Alpha1: return kb.digit1Key.wasPressedThisFrame || kb.numpad1Key.wasPressedThisFrame;
                case KeyCode.Alpha2: return kb.digit2Key.wasPressedThisFrame || kb.numpad2Key.wasPressedThisFrame;
                case KeyCode.Space: return kb.spaceKey.wasPressedThisFrame;
                case KeyCode.D: return kb.dKey.wasPressedThisFrame;
                case KeyCode.Return: return kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame;
                case KeyCode.F1: return kb.f1Key.wasPressedThisFrame;
                case KeyCode.F2: return kb.f2Key.wasPressedThisFrame;
                case KeyCode.F3: return kb.f3Key.wasPressedThisFrame;
            }
        }
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetKeyDown(key);
#else
        return false;
#endif
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

        // Pulihkan status ke kondisi aman dan target stat Rute C
        PlayerStats.Instance.physicalHealth = 100;
        PlayerStats.Instance.mentalHealth = 80;
        PlayerStats.Instance.languageProficiency = 20;
        PlayerStats.Instance.culturalEtiquette = 15;
        PlayerStats.Instance.SaveStatsToDatabase();

        Debug.Log("<color=magenta>[DEBUG STAT]</color> Diatur ke: PH 100, MH 80, Bahasa 20, Etika 15 (Target: Rute C)");
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

        // Pulihkan status ke kondisi aman dan target stat Rute B (Mianzi)
        PlayerStats.Instance.physicalHealth = 100;
        PlayerStats.Instance.mentalHealth = 80;
        PlayerStats.Instance.languageProficiency = 40;
        PlayerStats.Instance.culturalEtiquette = 20;
        PlayerStats.Instance.SaveStatsToDatabase();

        Debug.Log("<color=magenta>[DEBUG STAT]</color> Diatur ke: PH 100, MH 80, Bahasa 40, Etika 20 (Target: Rute B / Mianzi)");
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

        // Pulihkan status ke kondisi sehat dan etika tinggi
        PlayerStats.Instance.physicalHealth = 100;
        PlayerStats.Instance.mentalHealth = 80;
        PlayerStats.Instance.languageProficiency = 40;
        PlayerStats.Instance.culturalEtiquette = 60;
        PlayerStats.Instance.SaveStatsToDatabase();

        Debug.Log("<color=magenta>[DEBUG STAT]</color> Diatur ke: PH 100, MH 80, Bahasa 40, Etika 60 (Target: Rute A)");
        DialogueManager.Instance.StartDialogue(1001);
        DialogueManager.Instance.SelectOption(1);
    }
#endif
}