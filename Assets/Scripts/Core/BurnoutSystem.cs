using UnityEngine;

/// <summary>
/// Menghitung & menerapkan pemulihan malam hari Devano: pemulihan darurat jika sedang
/// Burnout (dengan penalti etika/akademik), atau pemulihan tidur normal jika tidak.
/// Diekstraksi dari GameManager.EvaluasiAkhirHari(). Deteksi TRIGGER burnout (PH/MH menyentuh
/// batas minimum) tetap berada di PlayerStats.TriggerBurnoutState(); kelas ini hanya menangani
/// formula pemulihannya di keesokan paginya.
/// </summary>
public static class BurnoutSystem
{
    public static void ApplyOvernightRecovery()
    {
        if (PlayerStats.Instance == null) return;

        if (PlayerStats.Instance.isBurnedOut)
        {
            PlayerStats.Instance.isBurnedOut = false;

            // Pemulihan stamina darurat (PH +50, MH +50) dengan penalti etika/akademik karena bolos
            PlayerStats.Instance.ModifyStats(
                dLanguage: 0,
                dEtiquette: -5,
                dMental: 50,
                dPhysical: 50,
                dTheoretical: -5,
                dPractical: 0
            );

            Debug.Log("<color=green>[Recovery Burnout]</color> Devano pulih dari kondisi sakit. Hari baru dimulai.");
        }
        else
        {
            // Pemulihan tidur normal harian (Persamaan 3.1: PH +40, MH +40)
            PlayerStats.Instance.ModifyStats(
                dLanguage: 0,
                dEtiquette: 0,
                dMental: 40,
                dPhysical: 40,
                dTheoretical: 0,
                dPractical: 0
            );
        }
    }
}
