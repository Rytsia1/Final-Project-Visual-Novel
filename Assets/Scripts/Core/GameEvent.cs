using System;
using System.Data;
using UnityEngine;

[System.Serializable]
public class GameEvent
{
    public int eventId;
    public string eventTitle;
    public int? npcId;
    
    public int minDay, maxDay;
    public string timeBlock;
    public string dayType;
    
    public int minLang, minEtiq;
    public int minMh, maxMh, minPh;
    public int minTheory, minPractice;
    
    public int minGuanxi, minAffectionState;
    public int minRumorLevel;
    
    public int? prereqEventId;
    public string reqFlagName;
    public int reqFlagVal;
    
    public int priority;
    public int startNodeId;
    public int failLanguageNodeId;
    public int failMianziNodeId;
    public bool isRepeatable;
    public bool isCompleted;

    // Alias properti untuk menjaga kompatibilitas ke belakang
    public string eventName => eventTitle;
    public int dialogueNodeId => startNodeId;
    public string eventCode => $"EVT_{eventId}";

    public static GameEvent FromDataRow(DataRow row)
    {
        if (row == null) return null;

        GameEvent evt = new GameEvent();
        evt.eventId = Convert.ToInt32(row["event_id"]);
        
        // Title / Name
        if (row.Table.Columns.Contains("event_title") && row["event_title"] != DBNull.Value && !string.IsNullOrEmpty(row["event_title"].ToString()))
            evt.eventTitle = row["event_title"].ToString();
        else if (row.Table.Columns.Contains("event_name") && row["event_name"] != DBNull.Value)
            evt.eventTitle = row["event_name"].ToString();
        else
            evt.eventTitle = "";
        
        if (row.Table.Columns.Contains("npc_id") && row["npc_id"] != DBNull.Value && Convert.ToInt32(row["npc_id"]) > 0)
            evt.npcId = Convert.ToInt32(row["npc_id"]);
        else
            evt.npcId = null;

        // Days
        if (row.Table.Columns.Contains("min_day") && row["min_day"] != DBNull.Value)
            evt.minDay = Convert.ToInt32(row["min_day"]);
        else if (row.Table.Columns.Contains("day_trigger") && row["day_trigger"] != DBNull.Value)
            evt.minDay = Convert.ToInt32(row["day_trigger"]);
        else
            evt.minDay = 1;

        if (row.Table.Columns.Contains("max_day") && row["max_day"] != DBNull.Value)
            evt.maxDay = Convert.ToInt32(row["max_day"]);
        else if (row.Table.Columns.Contains("day_end") && row["day_end"] != DBNull.Value)
            evt.maxDay = Convert.ToInt32(row["day_end"]);
        else
            evt.maxDay = 60;

        // Time Block
        if (row.Table.Columns.Contains("time_block") && row["time_block"] != DBNull.Value)
            evt.timeBlock = row["time_block"].ToString();
        else if (row.Table.Columns.Contains("time_slot") && row["time_slot"] != DBNull.Value)
        {
            string ts = row["time_slot"].ToString();
            if (ts.Equals("Morning", StringComparison.OrdinalIgnoreCase)) evt.timeBlock = "Pagi";
            else if (ts.Equals("Afternoon", StringComparison.OrdinalIgnoreCase)) evt.timeBlock = "Siang";
            else if (ts.Equals("Night", StringComparison.OrdinalIgnoreCase)) evt.timeBlock = "Malam";
            else evt.timeBlock = ts;
        }
        else
            evt.timeBlock = "Any";

        // Day Type
        if (row.Table.Columns.Contains("day_type") && row["day_type"] != DBNull.Value)
            evt.dayType = row["day_type"].ToString();
        else if (row.Table.Columns.Contains("is_workday") && row["is_workday"] != DBNull.Value)
            evt.dayType = Convert.ToInt32(row["is_workday"]) == 1 ? "Workday" : "Weekend";
        else
            evt.dayType = "Any";

        // Stats
        if (row.Table.Columns.Contains("min_lang") && row["min_lang"] != DBNull.Value)
            evt.minLang = Convert.ToInt32(row["min_lang"]);
        else if (row.Table.Columns.Contains("required_language") && row["required_language"] != DBNull.Value)
            evt.minLang = Convert.ToInt32(row["required_language"]);
        else
            evt.minLang = 0;

        if (row.Table.Columns.Contains("min_etiq") && row["min_etiq"] != DBNull.Value)
            evt.minEtiq = Convert.ToInt32(row["min_etiq"]);
        else if (row.Table.Columns.Contains("required_etiquette") && row["required_etiquette"] != DBNull.Value)
            evt.minEtiq = Convert.ToInt32(row["required_etiquette"]);
        else
            evt.minEtiq = 0;

        evt.minMh = row.Table.Columns.Contains("min_mh") && row["min_mh"] != DBNull.Value ? Convert.ToInt32(row["min_mh"]) : 0;
        evt.maxMh = row.Table.Columns.Contains("max_mh") && row["max_mh"] != DBNull.Value ? Convert.ToInt32(row["max_mh"]) : 100;
        evt.minPh = row.Table.Columns.Contains("min_ph") && row["min_ph"] != DBNull.Value ? Convert.ToInt32(row["min_ph"]) : 0;
        evt.minTheory = row.Table.Columns.Contains("min_theory") && row["min_theory"] != DBNull.Value ? Convert.ToInt32(row["min_theory"]) : 0;
        evt.minPractice = row.Table.Columns.Contains("min_practice") && row["min_practice"] != DBNull.Value ? Convert.ToInt32(row["min_practice"]) : 0;

        evt.minGuanxi = row.Table.Columns.Contains("min_guanxi") && row["min_guanxi"] != DBNull.Value ? Convert.ToInt32(row["min_guanxi"]) : 0;
        evt.minAffectionState = row.Table.Columns.Contains("min_affection_state") && row["min_affection_state"] != DBNull.Value ? Convert.ToInt32(row["min_affection_state"]) : 0;
        evt.minRumorLevel = row.Table.Columns.Contains("min_rumor_level") && row["min_rumor_level"] != DBNull.Value ? Convert.ToInt32(row["min_rumor_level"]) : 0;

        if (row.Table.Columns.Contains("prereq_event_id") && row["prereq_event_id"] != DBNull.Value && Convert.ToInt32(row["prereq_event_id"]) > 0)
            evt.prereqEventId = Convert.ToInt32(row["prereq_event_id"]);
        else
            evt.prereqEventId = null;

        if (row.Table.Columns.Contains("req_flag_name") && row["req_flag_name"] != DBNull.Value && !string.IsNullOrEmpty(row["req_flag_name"].ToString()))
            evt.reqFlagName = row["req_flag_name"].ToString();
        else if (row.Table.Columns.Contains("required_flag_key") && row["required_flag_key"] != DBNull.Value && !string.IsNullOrEmpty(row["required_flag_key"].ToString()))
            evt.reqFlagName = row["required_flag_key"].ToString();
        else
            evt.reqFlagName = null;

        if (row.Table.Columns.Contains("req_flag_val") && row["req_flag_val"] != DBNull.Value)
            evt.reqFlagVal = Convert.ToInt32(row["req_flag_val"]);
        else if (row.Table.Columns.Contains("required_flag_value") && row["required_flag_value"] != DBNull.Value)
            evt.reqFlagVal = Convert.ToInt32(row["required_flag_value"]);
        else
            evt.reqFlagVal = 1;

        evt.priority = row.Table.Columns.Contains("priority") && row["priority"] != DBNull.Value ? Convert.ToInt32(row["priority"]) : 10;
        
        // Start node ID
        if (row.Table.Columns.Contains("start_node_id") && row["start_node_id"] != DBNull.Value)
            evt.startNodeId = Convert.ToInt32(row["start_node_id"]);
        else if (row.Table.Columns.Contains("start_dialogue_id") && row["start_dialogue_id"] != DBNull.Value)
            evt.startNodeId = Convert.ToInt32(row["start_dialogue_id"]);
        else
            evt.startNodeId = 0;

        // Fail node IDs
        evt.failLanguageNodeId = row.Table.Columns.Contains("fail_language_node_id") && row["fail_language_node_id"] != DBNull.Value ? Convert.ToInt32(row["fail_language_node_id"]) : 0;
        evt.failMianziNodeId = row.Table.Columns.Contains("fail_mianzi_node_id") && row["fail_mianzi_node_id"] != DBNull.Value ? Convert.ToInt32(row["fail_mianzi_node_id"]) : 0;

        evt.isRepeatable = row.Table.Columns.Contains("is_repeatable") && row["is_repeatable"] != DBNull.Value && Convert.ToInt32(row["is_repeatable"]) == 1;
        evt.isCompleted = row.Table.Columns.Contains("is_completed") && row["is_completed"] != DBNull.Value && Convert.ToInt32(row["is_completed"]) == 1;

        return evt;
    }
}
