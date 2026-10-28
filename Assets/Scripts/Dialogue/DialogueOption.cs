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
    public string nextDialogueCode;

    public static DialogueOption FromDataRow(DataRow row)
    {
        if (row == null) return null;

        var opt = new DialogueOption();
        opt.optionId = Convert.ToInt32(row["option_id"]);

        if (row.Table.Columns.Contains("node_id") && row["node_id"] != DBNull.Value)
        {
            int.TryParse(row["node_id"].ToString(), out opt.nodeId);
        }
        else if (row.Table.Columns.Contains("dialogue_id") && row["dialogue_id"] != DBNull.Value)
        {
            int.TryParse(row["dialogue_id"].ToString(), out opt.nodeId);
        }
        else
        {
            opt.nodeId = 0;
        }

        opt.optionText = row["option_text"] != DBNull.Value ? row["option_text"].ToString() : "";

        if (row.Table.Columns.Contains("next_node_id") && row["next_node_id"] != DBNull.Value)
        {
            string val = row["next_node_id"].ToString();
            if (int.TryParse(val, out int nId)) opt.nextNodeId = nId;
            else opt.nextDialogueCode = val;
        }
        else if (row.Table.Columns.Contains("next_dialogue_id") && row["next_dialogue_id"] != DBNull.Value)
        {
            string val = row["next_dialogue_id"].ToString();
            if (int.TryParse(val, out int dId)) opt.nextNodeId = dId;
            else opt.nextDialogueCode = val;
        }
        else
        {
            opt.nextNodeId = 0;
        }

        // Prasyarat
        opt.minLang = row.Table.Columns.Contains("min_lang") && row["min_lang"] != DBNull.Value ? Convert.ToInt32(row["min_lang"]) : 0;
        opt.minEtiq = row.Table.Columns.Contains("min_etiq") && row["min_etiq"] != DBNull.Value ? Convert.ToInt32(row["min_etiq"]) : 0;
        opt.failMianziNodeId = row.Table.Columns.Contains("fail_mianzi_node_id") && row["fail_mianzi_node_id"] != DBNull.Value ? Convert.ToInt32(row["fail_mianzi_node_id"]) : 0;
        opt.failLanguageNodeId = row.Table.Columns.Contains("fail_language_node_id") && row["fail_language_node_id"] != DBNull.Value ? Convert.ToInt32(row["fail_language_node_id"]) : 0;

        // Consequence Payload (dengan sinkronisasi fallback ke kolom effect_ dan stat_)
        if (row.Table.Columns.Contains("delta_ph") && row["delta_ph"] != DBNull.Value)
            opt.deltaPh = Convert.ToInt32(row["delta_ph"]);
        else if (row.Table.Columns.Contains("effect_physical") && row["effect_physical"] != DBNull.Value)
            opt.deltaPh = Convert.ToInt32(row["effect_physical"]);
        else if (row.Table.Columns.Contains("stat_cost_ph") && row["stat_cost_ph"] != DBNull.Value)
            opt.deltaPh = -Convert.ToInt32(row["stat_cost_ph"]);
        else
            opt.deltaPh = 0;

        if (row.Table.Columns.Contains("delta_mh") && row["delta_mh"] != DBNull.Value)
            opt.deltaMh = Convert.ToInt32(row["delta_mh"]);
        else if (row.Table.Columns.Contains("effect_mental") && row["effect_mental"] != DBNull.Value)
            opt.deltaMh = Convert.ToInt32(row["effect_mental"]);
        else if (row.Table.Columns.Contains("stat_cost_mh") && row["stat_cost_mh"] != DBNull.Value)
            opt.deltaMh = -Convert.ToInt32(row["stat_cost_mh"]);
        else
            opt.deltaMh = 0;

        if (row.Table.Columns.Contains("delta_theory") && row["delta_theory"] != DBNull.Value)
            opt.deltaTheory = Convert.ToInt32(row["delta_theory"]);
        else if (row.Table.Columns.Contains("effect_theoretical") && row["effect_theoretical"] != DBNull.Value)
            opt.deltaTheory = Convert.ToInt32(row["effect_theoretical"]);
        else if (row.Table.Columns.Contains("stat_gain_theory") && row["stat_gain_theory"] != DBNull.Value)
            opt.deltaTheory = Convert.ToInt32(row["stat_gain_theory"]);
        else
            opt.deltaTheory = 0;

        if (row.Table.Columns.Contains("delta_practice") && row["delta_practice"] != DBNull.Value)
            opt.deltaPractice = Convert.ToInt32(row["delta_practice"]);
        else if (row.Table.Columns.Contains("effect_practical") && row["effect_practical"] != DBNull.Value)
            opt.deltaPractice = Convert.ToInt32(row["effect_practical"]);
        else if (row.Table.Columns.Contains("stat_gain_practice") && row["stat_gain_practice"] != DBNull.Value)
            opt.deltaPractice = Convert.ToInt32(row["stat_gain_practice"]);
        else
            opt.deltaPractice = 0;

        if (row.Table.Columns.Contains("delta_lang") && row["delta_lang"] != DBNull.Value)
            opt.deltaLang = Convert.ToInt32(row["delta_lang"]);
        else if (row.Table.Columns.Contains("effect_language") && row["effect_language"] != DBNull.Value)
            opt.deltaLang = Convert.ToInt32(row["effect_language"]);
        else if (row.Table.Columns.Contains("stat_gain_language") && row["stat_gain_language"] != DBNull.Value)
            opt.deltaLang = Convert.ToInt32(row["stat_gain_language"]);
        else
            opt.deltaLang = 0;

        if (row.Table.Columns.Contains("delta_etiq") && row["delta_etiq"] != DBNull.Value)
            opt.deltaEtiq = Convert.ToInt32(row["delta_etiq"]);
        else if (row.Table.Columns.Contains("effect_etiquette") && row["effect_etiquette"] != DBNull.Value)
            opt.deltaEtiq = Convert.ToInt32(row["effect_etiquette"]);
        else if (row.Table.Columns.Contains("stat_gain_etiquette") && row["stat_gain_etiquette"] != DBNull.Value)
            opt.deltaEtiq = Convert.ToInt32(row["stat_gain_etiquette"]);
        else
            opt.deltaEtiq = 0;

        if (row.Table.Columns.Contains("target_npc_id") && row["target_npc_id"] != DBNull.Value && Convert.ToInt32(row["target_npc_id"]) > 0)
            opt.targetNpcId = Convert.ToInt32(row["target_npc_id"]);
        else if (row.Table.Columns.Contains("stat_gain_guanxi_npc_id") && row["stat_gain_guanxi_npc_id"] != DBNull.Value && Convert.ToInt32(row["stat_gain_guanxi_npc_id"]) > 0)
            opt.targetNpcId = Convert.ToInt32(row["stat_gain_guanxi_npc_id"]);
        else
            opt.targetNpcId = null;

        if (row.Table.Columns.Contains("delta_guanxi") && row["delta_guanxi"] != DBNull.Value)
            opt.deltaGuanxi = Convert.ToInt32(row["delta_guanxi"]);
        else if (row.Table.Columns.Contains("effect_guanxi") && row["effect_guanxi"] != DBNull.Value)
            opt.deltaGuanxi = Convert.ToInt32(row["effect_guanxi"]);
        else if (row.Table.Columns.Contains("stat_gain_guanxi_amount") && row["stat_gain_guanxi_amount"] != DBNull.Value)
            opt.deltaGuanxi = Convert.ToInt32(row["stat_gain_guanxi_amount"]);
        else
            opt.deltaGuanxi = 0;

        opt.deltaLoneliness = row.Table.Columns.Contains("delta_loneliness") && row["delta_loneliness"] != DBNull.Value ? Convert.ToInt32(row["delta_loneliness"]) :
                             (row.Table.Columns.Contains("effect_loneliness") && row["effect_loneliness"] != DBNull.Value ? Convert.ToInt32(row["effect_loneliness"]) : 0);

        opt.deltaRumor = row.Table.Columns.Contains("delta_rumor") && row["delta_rumor"] != DBNull.Value ? Convert.ToInt32(row["delta_rumor"]) :
                        (row.Table.Columns.Contains("effect_rumor") && row["effect_rumor"] != DBNull.Value ? Convert.ToInt32(row["effect_rumor"]) : 0);

        if (row.Table.Columns.Contains("set_flag_name") && row["set_flag_name"] != DBNull.Value && !string.IsNullOrEmpty(row["set_flag_name"].ToString()))
            opt.setFlagName = row["set_flag_name"].ToString();
        else if (row.Table.Columns.Contains("set_flag_key") && row["set_flag_key"] != DBNull.Value && !string.IsNullOrEmpty(row["set_flag_key"].ToString()))
            opt.setFlagName = row["set_flag_key"].ToString();
        else
            opt.setFlagName = null;

        if (row.Table.Columns.Contains("set_flag_val") && row["set_flag_val"] != DBNull.Value)
            opt.setFlagVal = Convert.ToInt32(row["set_flag_val"]);
        else if (row.Table.Columns.Contains("set_flag_value") && row["set_flag_value"] != DBNull.Value)
            opt.setFlagVal = Convert.ToInt32(row["set_flag_value"]);
        else
            opt.setFlagVal = 1;

        return opt;
    }
}
