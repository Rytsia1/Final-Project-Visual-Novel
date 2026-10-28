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

    /// <summary>
    /// Evaluasi Kuis/Ujian Mingguan Hari 7 (Vertical Slice).
    /// Memeriksa akumulasi stat academic_theoretical dan academic_practical.
    /// Jika lulus: Mental Health +15.
    /// Jika gagal: pemicu status Academic Warning.
    /// </summary>
    public static bool EvaluateWeeklyExam(int minTheory = 35, int minPractice = 45)
    {
        if (PlayerStats.Instance == null)
        {
            Debug.LogWarning("[WeeklyExam] PlayerStats.Instance null — fallback default lulus.");
            return true;
        }

        int teori = PlayerStats.Instance.academicTheoretical;
        int praktis = PlayerStats.Instance.academicPractical;
        bool lulus = (teori >= minTheory) && (praktis >= minPractice);

        Debug.Log($"<color=cyan>[WEEKLY EXAM EVAL]</color> Hari 7 — Teori: {teori}/{minTheory}, Praktis: {praktis}/{minPractice} → {(lulus ? "<color=green>LULUS</color>" : "<color=red>ACADEMIC WARNING</color>")}");

        if (TelemetryLogger.Instance != null)
        {
            TelemetryLogger.Instance.RecordCriticalEvent("WEEKLY_EXAM", 
                $"Review Mingguan Hari 7: {(lulus ? "LULUS" : "GAGAL (WARNING)")} — Teori:{teori}/{minTheory}, Praktik:{praktis}/{minPractice}");
        }

        if (lulus)
        {
            PlayerStats.Instance.ModifyStats(
                dLanguage: 0, dEtiquette: 0,
                dMental: 15, dPhysical: 0,
                dTheoretical: 0, dPractical: 0
            );
            if (FlagManager.Instance != null)
            {
                FlagManager.Instance.SetFlag("academic_warning", 0, "Lulus evaluasi mingguan");
                FlagManager.Instance.SetFlag("weekly_exam_passed", 1, "Hasil evaluasi mingguan");
            }
        }
        else
        {
            PlayerStats.Instance.ModifyStats(
                dLanguage: 0, dEtiquette: 0,
                dMental: -15, dPhysical: 0,
                dTheoretical: 0, dPractical: 0
            );
            if (FlagManager.Instance != null)
            {
                FlagManager.Instance.SetFlag("academic_warning", 1, "Peringatan akademik minggu 1");
                FlagManager.Instance.SetFlag("weekly_exam_passed", 0, "Hasil evaluasi mingguan");
            }
            if (SocialManager.Instance != null)
            {
                SocialManager.Instance.ModifyGuanxi(101, -5);
                SocialManager.Instance.ModifyGuanxi(3, -5);
            }
        }

        return lulus;
    }
}
