using UnityEngine;

/// <summary>
/// Menghitung & menerapkan hasil Evaluasi Tengah Semester (Hari ke-30): membandingkan
/// parameter akademik Devano terhadap ambang kelulusan (GameManager.PASS_THEORETICAL /
/// PASS_PRACTICAL), lalu menerapkan reward/penalti terkait (stat, guanxi, rumor, telemetry).
/// Diekstraksi dari GameManager.EksekusiEvaluasiTengahSemester(): GameManager tetap yang
/// memutuskan alur dialog berikutnya (node 5001/5002/5003); kelas ini hanya menjawab
/// "bagaimana hasil evaluasi akademik dihitung".
/// </summary>
public static class AcademicEvaluationSystem
{
    /// <summary>
    /// Mengevaluasi & menerapkan hasil UTS. Mengembalikan null jika evaluasi tidak dapat
    /// dijalankan (PlayerStats.Instance tidak tersedia) — caller harus membatalkan alur
    /// dialog/DB berikutnya pada kasus ini, persis seperti perilaku sebelum refaktor.
    /// </summary>
    public static bool? EvaluateMidterm()
    {
        if (PlayerStats.Instance == null)
        {
            Debug.LogError("[Midterm] PlayerStats.Instance null — evaluasi dibatalkan.");
            return null;
        }

        int teori = PlayerStats.Instance.academicTheoretical;
        int praktis = PlayerStats.Instance.academicPractical;
        bool lulus = (teori >= GameManager.PASS_THEORETICAL) && (praktis >= GameManager.PASS_PRACTICAL);

        Debug.Log($"<color=cyan>[MIDTERM EVAL]</color> Hari 30 — Teori: {teori}/{GameManager.PASS_THEORETICAL}, Praktis: {praktis}/{GameManager.PASS_PRACTICAL} → {(lulus ? "<color=green>LULUS</color>" : "<color=red>PROBATION</color>")}");

        if (TelemetryLogger.Instance != null)
        {
            string detail = lulus
                ? $"MIDTERM LULUS — Teori:{teori}, Praktis:{praktis}"
                : $"MIDTERM PROBATION — Teori:{teori} (min {GameManager.PASS_THEORETICAL}), Praktis:{praktis} (min {GameManager.PASS_PRACTICAL})";
            TelemetryLogger.Instance.RecordEvent("MIDTERM_EVALUATION", detail);
        }

        // Terapkan reward / penalti langsung sebelum dialog
        if (lulus)
        {
            // Reward: Guanxi naik (diwakili MH+15 karena rasa percaya diri) + kepercayaan dosen
            PlayerStats.Instance.ModifyStats(
                dLanguage: 0, dEtiquette: 0,
                dMental: 15, dPhysical: 0,
                dTheoretical: 0, dPractical: 0
            );
            if (SocialManager.Instance != null)
                SocialManager.Instance.TambahGuanxi(npcId: 101, penambahanGuanxi: 20, reduksiLoneliness: 10);
        }
        else
        {
            // Penalti: Academic Probation — MH turun, rumor bertambah, guanxi dosen turun
            PlayerStats.Instance.ModifyStats(
                dLanguage: 0, dEtiquette: 0,
                dMental: -25, dPhysical: 0,
                dTheoretical: 0, dPractical: 0
            );
            if (SocialManager.Instance != null)
            {
                SocialManager.Instance.TambahGuanxi(npcId: 101, penambahanGuanxi: -20, reduksiLoneliness: 0);
                SocialManager.Instance.TambahRumor(20);
            }
        }

        return lulus;
    }
}
