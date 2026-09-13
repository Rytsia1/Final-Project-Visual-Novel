using System;
using System.Data;
using UnityEngine;

public class PhoneOutingManager : MonoBehaviour
{
    private static PhoneOutingManager _instance;
    public static PhoneOutingManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<PhoneOutingManager>();
                if (_instance == null)
                {
                    GameObject core = GameObject.Find("GAME_CORE");
                    if (core != null)
                    {
                        _instance = core.AddComponent<PhoneOutingManager>();
                    }
                    else
                    {
                        GameObject go = new GameObject("[PhoneOutingManager]");
                        _instance = go.AddComponent<PhoneOutingManager>();
                        DontDestroyOnLoad(go);
                    }
                }
            }
            return _instance;
        }
        private set => _instance = value;
    }

    void Awake()
    {
        if (_instance == null) _instance = this;
        else if (_instance != this) Destroy(this);
    }

    // 1. FITUR YOSHIO: Telepon Edelweiss untuk Cek Radar & Bocoran Intel NPC
    public void CallEdelweissIntel(int targetNpcId)
    {
        int pId = GameManager.Instance != null ? GameManager.Instance.playerId : 1;

        // Tarik data rumor & preferensi dari database
        string query = $"SELECT p.intel_hint, p.favorite_topic, p.sensitive_topic, " +
                       $"r.loneliness_meter, r.guanxi_score, r.affection_state, n.npc_name " +
                       $"FROM tbl_npc_preferences p " +
                       $"JOIN tbl_npc_relations r ON p.npc_id = r.npc_id " +
                       $"JOIN tbl_npc_list n ON p.npc_id = n.npc_id " +
                       $"WHERE p.npc_id = {targetNpcId} AND r.player_id = {pId};";

        DataTable dt = DatabaseManager.Instance.ExecuteQuery(query);

        if (dt != null && dt.Rows.Count > 0)
        {
            DataRow row = dt.Rows[0];
            string npcName = row["npc_name"].ToString();
            int loneliness = Convert.ToInt32(row["loneliness_meter"]);
            int guanxi = Convert.ToInt32(row["guanxi_score"]);
            string hint = row["intel_hint"].ToString();

            // Status afeksi (Affinity State)
            string[] affLabels = { "Dingin/Formal", "Kenal Dekat", "Sahabat Karib", "Tokimeki (Lingkaran Inti)" };
            int affState = row["affection_state"] != DBNull.Value ? Convert.ToInt32(row["affection_state"]) : 0;
            string affName = (affState >= 0 && affState < affLabels.Length) ? affLabels[affState] : "Normal";

            // Format status bom ala Tokimeki
            string bombStatus = loneliness >= 80 
                ? "<color=red>[BAHAYA: BOM MELEDAK]</color> Hati-hati, dia merasa sangat kamu abaikan!"
                : (loneliness >= 50 ? "<color=yellow>[WASPADA: BOM AKTIF]</color> Dia mulai membicarakanmu di belakang." : "<color=green>[AMAN]</color> Hubungan kalian stabil.");

            string dialogText = $"Halo Devano! Soal {npcName} ya?\n" +
                                $"Status Guanxi: {guanxi} ({affName}) | Tingkat Loneliness: {loneliness}/100\n" +
                                $"{bombStatus}\n\n" +
                                $"<b>Tips dari Edel:</b> \"{hint}\"";

            if (DialogueUIController.Instance != null)
            {
                DialogueUIController.Instance.DisplayDialogue("Edelweiss (Telepon)", dialogText);
                DialogueUIController.Instance.ShowCloseButton();
            }

            if (TelemetryLogger.Instance != null)
            {
                TelemetryLogger.Instance.RecordCriticalEvent("PHONE_INTEL", $"Akses intel Edelweiss tentang NPC {npcName} (Guanxi: {guanxi}, Loneliness: {loneliness})");
            }

            Debug.Log($"<color=cyan>[Phone Broker]</color> Intel mengenai {npcName} berhasil diakses. (Guanxi: {guanxi}, Loneliness: {loneliness}, Bom: {bombStatus})");
        }
        else
        {
            Debug.LogWarning($"[PhoneOutingManager] Data preferensi untuk NPC ID {targetNpcId} tidak ditemukan.");
        }
    }

    // 2. FITUR WEEKEND OUTING: Mengajak NPC Hangout ke Lokasi Tertentu
    public void AjakHangout(int npcId, int venueId)
    {
        // Validasi kalender: Hanya bisa di akhir pekan (Sabtu-Minggu)
        if (GameManager.Instance != null && GameManager.Instance.IsWorkday())
        {
            if (DialogueUIController.Instance != null)
            {
                DialogueUIController.Instance.DisplayDialogue("Sistem", "Devano hanya bisa mengajak jalan-jalan di luar kampus pada akhir pekan (Sabtu/Minggu). Hari kerja prioritaskan perkuliahan!");
                DialogueUIController.Instance.ShowCloseButton();
            }
            Debug.LogWarning("[PhoneOutingManager] Gagal mengajak hangout: Hanya bisa dilakukan di akhir pekan.");
            return;
        }

        // Cek data preferensi NPC
        string query = $"SELECT favorite_venue_id, hated_venue_id FROM tbl_npc_preferences WHERE npc_id = {npcId};";
        DataTable dt = DatabaseManager.Instance.ExecuteQuery(query);

        string qVenue = $"SELECT venue_name, cost_ph, cost_mh, min_etiquette_req FROM tbl_venues WHERE venue_id = {venueId};";
        DataTable dtVenue = DatabaseManager.Instance.ExecuteQuery(qVenue);

        if (dt != null && dt.Rows.Count > 0 && dtVenue != null && dtVenue.Rows.Count > 0)
        {
            int favVenue = Convert.ToInt32(dt.Rows[0]["favorite_venue_id"]);
            int hatedVenue = Convert.ToInt32(dt.Rows[0]["hated_venue_id"]);
            string venueName = dtVenue.Rows[0]["venue_name"].ToString();
            int minEtiq = Convert.ToInt32(dtVenue.Rows[0]["min_etiquette_req"]);
            int costPh = Convert.ToInt32(dtVenue.Rows[0]["cost_ph"]);
            int costMh = Convert.ToInt32(dtVenue.Rows[0]["cost_mh"]);

            // Ambil nama target NPC untuk dialog
            string targetName = "Target";
            NPCRelationData rel = SocialManager.Instance != null ? SocialManager.Instance.relations.Find(x => x.npcId == npcId) : null;
            if (rel != null) targetName = rel.npcName;

            // Validasi Etika Tempat
            if (PlayerStats.Instance != null && PlayerStats.Instance.culturalEtiquette < minEtiq)
            {
                string etiqWarn = $"Aku belum cukup paham etika ({PlayerStats.Instance.culturalEtiquette}/{minEtiq}) untuk pergi ke {venueName}. Bisa-bisa aku mempermalukan diri sendiri dan melanggar Mianzi.";
                if (DialogueUIController.Instance != null)
                {
                    DialogueUIController.Instance.DisplayDialogue("Devano", etiqWarn);
                    DialogueUIController.Instance.ShowCloseButton();
                }
                Debug.LogWarning($"[PhoneOutingManager] Etika tidak mencukupi untuk {venueName}.");
                return;
            }

            // Evaluasi Tempat yang Dibenci (NPC Menolak Pergi)
            if (venueId == hatedVenue)
            {
                if (DialogueUIController.Instance != null)
                {
                    DialogueUIController.Instance.DisplayDialogue(targetName, $"Duh Devano, sejujurnya aku kurang nyaman pergi ke {venueName}... Lain kali saja ya.");
                    DialogueUIController.Instance.ShowCloseButton();
                }

                if (TelemetryLogger.Instance != null)
                {
                    TelemetryLogger.Instance.RecordCriticalEvent("OUTING_REJECTED_HATED", $"{targetName} menolak ajakan hangout ke tempat yang dibenci ({venueName}). Acara dibatalkan.");
                }

                Debug.Log($"<color=orange>[Weekend Outing]</color> {targetName} menolak pergi ke {venueName} (Tempat yang dibenci). Acara dibatalkan tanpa penalti waktu.");
                return;
            }

            // Eksekusi Perjalanan: Kurangi stamina sesuai biaya venue
            if (PlayerStats.Instance != null)
            {
                PlayerStats.Instance.ModifyStats(0, 0, -costMh, -costPh, 0, 0);
            }

            // Tandai target telah berinteraksi hari ini agar terhindar dari Guanxi Decay & Bakudan malam ini
            if (SocialManager.Instance != null && SocialManager.Instance.relations != null)
            {
                var relTarget = SocialManager.Instance.relations.Find(x => x.npcId == npcId);
                if (relTarget != null) relTarget.interactedToday = true;
            }

            // Cari Skenario Dilema Interaktif dari tbl_hangout_events
            string qEvent = $"SELECT start_node_id FROM tbl_hangout_events WHERE venue_id = {venueId} AND npc_id = {npcId};";
            DataTable dtEvent = DatabaseManager.Instance.ExecuteQuery(qEvent);

            if (dtEvent != null && dtEvent.Rows.Count > 0)
            {
                int startNodeId = Convert.ToInt32(dtEvent.Rows[0]["start_node_id"]);
                Debug.Log($"<color=green>[PhoneOutingManager]</color> Memulai skenario hangout interaktif (Node {startNodeId}) bersama {targetName} di {venueName}.");

                if (TelemetryLogger.Instance != null)
                {
                    TelemetryLogger.Instance.RecordCriticalEvent("OUTING_START", $"Memulai sesi skenario hangout dengan {targetName} di {venueName} (Node {startNodeId})");
                }

                if (DialogueManager.Instance != null)
                {
                    DialogueManager.Instance.StartHangoutDialogue(startNodeId, npcId, venueId);
                }
            }
            else
            {
                // Fallback Skenario Generik jika pasangan venue-NPC belum memiliki naskah khusus
                ExecuteGenericOuting(npcId, venueId, targetName, venueName, favVenue);
            }
        }
        else
        {
            Debug.LogWarning($"[PhoneOutingManager] Data venue atau NPC tidak lengkap untuk Outing.");
        }
    }

    private void ExecuteGenericOuting(int npcId, int venueId, string targetName, string venueName, int favVenue)
    {
        if (SocialManager.Instance != null && SocialManager.Instance.relations != null)
        {
            var rel = SocialManager.Instance.relations.Find(x => x.npcId == npcId);
            if (rel != null) rel.interactedToday = true;
        }

        if (venueId == favVenue)
        {
            if (SocialManager.Instance != null)
                SocialManager.Instance.TambahGuanxi(npcId, penambahanGuanxi: 25, reduksiLoneliness: 60);

            if (PlayerStats.Instance != null)
                PlayerStats.Instance.ModifyStats(0, 5, 20, 0, 0, 0);

            if (DialogueUIController.Instance != null)
                DialogueUIController.Instance.DisplayDialogue(targetName, $"Wah, kamu tahu saja tempat kesukaanku! Ayo kita berangkat ke {venueName} sekarang!");

            if (TelemetryLogger.Instance != null)
                TelemetryLogger.Instance.RecordCriticalEvent("OUTING_FAVORITE", $"Hangout sukses besar dengan {targetName} di {venueName} (Tempat Favorit)");

            Debug.Log($"<color=green>[Weekend Outing - Favorite]</color> {targetName} sangat senang di {venueName}! Guanxi +25, Loneliness -60.");
        }
        else
        {
            if (SocialManager.Instance != null)
                SocialManager.Instance.TambahGuanxi(npcId, penambahanGuanxi: 12, reduksiLoneliness: 35);

            if (PlayerStats.Instance != null)
                PlayerStats.Instance.ModifyStats(0, 2, 10, 0, 0, 0);

            if (DialogueUIController.Instance != null)
                DialogueUIController.Instance.DisplayDialogue(targetName, $"Boleh juga, aku sedang senggang. Ayo kita ke {venueName}.");

            if (TelemetryLogger.Instance != null)
                TelemetryLogger.Instance.RecordCriticalEvent("OUTING_NORMAL", $"Hangout biasa dengan {targetName} di {venueName}");

            Debug.Log($"<color=green>[Weekend Outing - Normal]</color> Hangout bersama {targetName} di {venueName} selesai. Guanxi +12, Loneliness -35.");
        }

        if (DialogueUIController.Instance != null)
        {
            // Panggil GeserWaktu() saat tombol [Lanjut / Selesai] ditekan oleh pemain
            DialogueUIController.Instance.ShowCloseButton(() =>
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.GeserWaktu();
                    Debug.Log("<color=green>[Weekend Outing]</color> Waktu bergeser 1 blok setelah dialog hangout ditutup.");
                }
            });
        }
    }
}
