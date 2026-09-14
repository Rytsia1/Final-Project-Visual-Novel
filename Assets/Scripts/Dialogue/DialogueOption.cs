using System;
using System.Data;

[System.Serializable]
public class DialogueOption
{
    public int optionId;
    public int nodeId;
    public string optionText;
    public int nextNodeId;

    // Prasyarat
    public int minLang, minEtiq;
    public int failMianziNodeId, failLanguageNodeId;

    // Consequence Payload
    public int deltaPh, deltaMh, deltaTheory, deltaPractice, deltaLang, deltaEtiq;
    public int? targetNpcId;
    public int deltaGuanxi, deltaLoneliness, deltaRumor;
    public string setFlagName;
    public int setFlagVal;

    public static DialogueOption FromDataRow(DataRow row)
    {
        if (row == null) return null;

        var opt = new DialogueOption();
        opt.optionId = Convert.ToInt32(row["option_id"]);
        opt.nodeId = Convert.ToInt32(row["node_id"]);
        opt.optionText = row["option_text"] != DBNull.Value ? row["option_text"].ToString() : "";
        opt.nextNodeId = row["next_node_id"] != DBNull.Value ? Convert.ToInt32(row["next_node_id"]) : 0;

        // Prasyarat
        opt.minLang = row.Table.Columns.Contains("min_lang") && row["min_lang"] != DBNull.Value ? Convert.ToInt32(row["min_lang"]) : 0;
        opt.minEtiq = row.Table.Columns.Contains("min_etiq") && row["min_etiq"] != DBNull.Value ? Convert.ToInt32(row["min_etiq"]) : 0;
        opt.failMianziNodeId = row.Table.Columns.Contains("fail_mianzi_node_id") && row["fail_mianzi_node_id"] != DBNull.Value ? Convert.ToInt32(row["fail_mianzi_node_id"]) : 0;
        opt.failLanguageNodeId = row.Table.Columns.Contains("fail_language_node_id") && row["fail_language_node_id"] != DBNull.Value ? Convert.ToInt32(row["fail_language_node_id"]) : 0;

        // Consequence Payload (dengan sinkronisasi fallback ke kolom effect_)
        opt.deltaPh = row.Table.Columns.Contains("delta_ph") && row["delta_ph"] != DBNull.Value ? Convert.ToInt32(row["delta_ph"]) :
                     (row.Table.Columns.Contains("effect_physical") && row["effect_physical"] != DBNull.Value ? Convert.ToInt32(row["effect_physical"]) : 0);

        opt.deltaMh = row.Table.Columns.Contains("delta_mh") && row["delta_mh"] != DBNull.Value ? Convert.ToInt32(row["delta_mh"]) :
                     (row.Table.Columns.Contains("effect_mental") && row["effect_mental"] != DBNull.Value ? Convert.ToInt32(row["effect_mental"]) : 0);

        opt.deltaTheory = row.Table.Columns.Contains("delta_theory") && row["delta_theory"] != DBNull.Value ? Convert.ToInt32(row["delta_theory"]) :
                         (row.Table.Columns.Contains("effect_theoretical") && row["effect_theoretical"] != DBNull.Value ? Convert.ToInt32(row["effect_theoretical"]) : 0);

        opt.deltaPractice = row.Table.Columns.Contains("delta_practice") && row["delta_practice"] != DBNull.Value ? Convert.ToInt32(row["delta_practice"]) :
                           (row.Table.Columns.Contains("effect_practical") && row["effect_practical"] != DBNull.Value ? Convert.ToInt32(row["effect_practical"]) : 0);

        opt.deltaLang = row.Table.Columns.Contains("delta_lang") && row["delta_lang"] != DBNull.Value ? Convert.ToInt32(row["delta_lang"]) :
                       (row.Table.Columns.Contains("effect_language") && row["effect_language"] != DBNull.Value ? Convert.ToInt32(row["effect_language"]) : 0);

        opt.deltaEtiq = row.Table.Columns.Contains("delta_etiq") && row["delta_etiq"] != DBNull.Value ? Convert.ToInt32(row["delta_etiq"]) :
                       (row.Table.Columns.Contains("effect_etiquette") && row["effect_etiquette"] != DBNull.Value ? Convert.ToInt32(row["effect_etiquette"]) : 0);

        if (row.Table.Columns.Contains("target_npc_id") && row["target_npc_id"] != DBNull.Value && Convert.ToInt32(row["target_npc_id"]) > 0)
        {
            opt.targetNpcId = Convert.ToInt32(row["target_npc_id"]);
        }
        else
        {
            opt.targetNpcId = null;
        }

        opt.deltaGuanxi = row.Table.Columns.Contains("delta_guanxi") && row["delta_guanxi"] != DBNull.Value ? Convert.ToInt32(row["delta_guanxi"]) :
                         (row.Table.Columns.Contains("effect_guanxi") && row["effect_guanxi"] != DBNull.Value ? Convert.ToInt32(row["effect_guanxi"]) : 0);

        opt.deltaLoneliness = row.Table.Columns.Contains("delta_loneliness") && row["delta_loneliness"] != DBNull.Value ? Convert.ToInt32(row["delta_loneliness"]) :
                             (row.Table.Columns.Contains("effect_loneliness") && row["effect_loneliness"] != DBNull.Value ? Convert.ToInt32(row["effect_loneliness"]) : 0);

        opt.deltaRumor = row.Table.Columns.Contains("delta_rumor") && row["delta_rumor"] != DBNull.Value ? Convert.ToInt32(row["delta_rumor"]) :
                        (row.Table.Columns.Contains("effect_rumor") && row["effect_rumor"] != DBNull.Value ? Convert.ToInt32(row["effect_rumor"]) : 0);

        opt.setFlagName = (row.Table.Columns.Contains("set_flag_name") && row["set_flag_name"] != DBNull.Value && !string.IsNullOrEmpty(row["set_flag_name"].ToString()))
                         ? row["set_flag_name"].ToString() : null;

        opt.setFlagVal = row.Table.Columns.Contains("set_flag_val") && row["set_flag_val"] != DBNull.Value ? Convert.ToInt32(row["set_flag_val"]) : 1;

        return opt;
    }
}
