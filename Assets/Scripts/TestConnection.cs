using System.Data;
using UnityEngine;

public class TestConnection : MonoBehaviour
{
    void Start()
    {
        // Uji baca data status Kenzo
        string qPlayer = "SELECT p.player_name, s.language_proficiency, s.cultural_etiquette " +
                         "FROM tbl_player_profile p JOIN tbl_player_stats s ON p.player_id = s.player_id;";
        DataTable dtPlayer = DatabaseManager.Instance.ExecuteQuery(qPlayer);

        if (dtPlayer.Rows.Count > 0)
        {
            DataRow row = dtPlayer.Rows[0];
            Debug.Log($"<color=green>[KONEKSI SUKSES]</color> Karakter: {row["player_name"]} | Bahasa: {row["language_proficiency"]} | Etika: {row["cultural_etiquette"]}");
        }

        // Uji baca roster NPC
        DataTable dtNpc = DatabaseManager.Instance.ExecuteQuery("SELECT npc_name, npc_role FROM tbl_npc_list;");
        foreach (DataRow row in dtNpc.Rows)
        {
            Debug.Log($"<color=yellow>[DATA NPC]</color> {row["npc_name"]} - {row["npc_role"]}");
        }
    }
}