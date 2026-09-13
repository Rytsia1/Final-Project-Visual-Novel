using System;

/// <summary>
/// Konvensi Penomoran ID Dialog (Naming & ID Convention) untuk SQLite dan Branching Dialogue Engine.
/// Menetapkan rentang node_id dan option_id per kategori karakter/event agar relasi percabangan
/// tidak saling tumpang tindih (clashing) selama penulisan naskah interaksi naratif.
/// </summary>
public static class DialogueIdConstants
{
    // =========================================================
    // 1000 - 1999: Interaksi Dosen Xiang Bai
    // =========================================================
    public const int XIANG_BAI_START = 1000;
    public const int XIANG_BAI_END   = 1999;

    // =========================================================
    // 2000 - 2999: Interaksi Edelweiss (Info Broker & Peringatan)
    // =========================================================
    public const int EDELWEISS_START = 2000;
    public const int EDELWEISS_END   = 2999;

    // =========================================================
    // 3000 - 3999: Event Krisis / Ledakan Rumor (Critical Events)
    // =========================================================
    public const int CRISIS_RUMOR_START = 3000;
    public const int CRISIS_RUMOR_END   = 3999;

    // =========================================================
    // 4000 - 4999: Interaksi Li Haoran
    // =========================================================
    public const int LI_HAORAN_START = 4000;
    public const int LI_HAORAN_END   = 4999;

    // =========================================================
    // 5000 - 5999: Evaluasi Akademik (UTS Hari 30 / UAS Hari 60)
    // =========================================================
    public const int ACADEMIC_EVALUATION_START = 5000;
    public const int ACADEMIC_EVALUATION_END   = 5999;

    // =========================================================
    // 6000 - 6999: Skenario Weekend Hangout (Dilema Budaya 3-Tier)
    // =========================================================
    public const int WEEKEND_HANGOUT_START = 6000;
    public const int WEEKEND_HANGOUT_END   = 6999;

    // =========================================================
    // 7000 - 7999: Interaksi Yang Mei
    // =========================================================
    public const int YANG_MEI_START = 7000;
    public const int YANG_MEI_END   = 7999;

    /// <summary>
    /// Mengembalikan nama kategori/konteks berdasarkan rentang ID dialog.
    /// </summary>
    public static string GetCategoryName(int dialogueId)
    {
        if (dialogueId >= XIANG_BAI_START && dialogueId <= XIANG_BAI_END)
            return "Interaksi Dosen Xiang Bai";
        if (dialogueId >= EDELWEISS_START && dialogueId <= EDELWEISS_END)
            return "Interaksi Edelweiss (Info Broker & Peringatan)";
        if (dialogueId >= CRISIS_RUMOR_START && dialogueId <= CRISIS_RUMOR_END)
            return "Event Krisis / Ledakan Rumor";
        if (dialogueId >= LI_HAORAN_START && dialogueId <= LI_HAORAN_END)
            return "Interaksi Li Haoran";
        if (dialogueId >= ACADEMIC_EVALUATION_START && dialogueId <= ACADEMIC_EVALUATION_END)
            return "Evaluasi Akademik (UTS/UAS)";
        if (dialogueId >= WEEKEND_HANGOUT_START && dialogueId <= WEEKEND_HANGOUT_END)
            return "Skenario Weekend Hangout (Dilema Budaya 3-Tier)";
        if (dialogueId >= YANG_MEI_START && dialogueId <= YANG_MEI_END)
            return "Interaksi Yang Mei";

        return "Kategori Tidak Dikenal";
    }

    /// <summary>
    /// Memeriksa apakah ID dialog berada dalam rentang kategori tertentu.
    /// </summary>
    public static bool IsInRange(int dialogueId, int rangeStart, int rangeEnd)
    {
        return dialogueId >= rangeStart && dialogueId <= rangeEnd;
    }
}
