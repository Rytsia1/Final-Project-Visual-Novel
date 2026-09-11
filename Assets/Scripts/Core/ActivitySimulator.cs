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
    }
}