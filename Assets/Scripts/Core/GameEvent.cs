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
        evt.eventTitle = row["event_title"] != DBNull.Value ? row["event_title"].ToString() : "";
        
        if (row.Table.Columns.Contains("npc_id") && row["npc_id"] != DBNull.Value && Convert.ToInt32(row["npc_id"]) > 0)
            evt.npcId = Convert.ToInt32(row["npc_id"]);
        else
            evt.npcId = null;

        evt.minDay = row.Table.Columns.Contains("min_day") && row["min_day"] != DBNull.Value ? Convert.ToInt32(row["min_day"]) : 1;
        evt.maxDay = row.Table.Columns.Contains("max_day") && row["max_day"] != DBNull.Value ? Convert.ToInt32(row["max_day"]) : 60;
        evt.timeBlock = row.Table.Columns.Contains("time_block") && row["time_block"] != DBNull.Value ? row["time_block"].ToString() : "Any";
        evt.dayType = row.Table.Columns.Contains("day_type") && row["day_type"] != DBNull.Value ? row["day_type"].ToString() : "Any";

        evt.minLang = row.Table.Columns.Contains("min_lang") && row["min_lang"] != DBNull.Value ? Convert.ToInt32(row["min_lang"]) : 0;
        evt.minEtiq = row.Table.Columns.Contains("min_etiq") && row["min_etiq"] != DBNull.Value ? Convert.ToInt32(row["min_etiq"]) : 0;
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

        evt.reqFlagName = (row.Table.Columns.Contains("req_flag_name") && row["req_flag_name"] != DBNull.Value && !string.IsNullOrEmpty(row["req_flag_name"].ToString())) 
            ? row["req_flag_name"].ToString() : null;
        evt.reqFlagVal = row.Table.Columns.Contains("req_flag_val") && row["req_flag_val"] != DBNull.Value ? Convert.ToInt32(row["req_flag_val"]) : 1;

        evt.priority = row.Table.Columns.Contains("priority") && row["priority"] != DBNull.Value ? Convert.ToInt32(row["priority"]) : 10;
        evt.startNodeId = row.Table.Columns.Contains("start_node_id") && row["start_node_id"] != DBNull.Value ? Convert.ToInt32(row["start_node_id"]) : 0;
        evt.isRepeatable = row.Table.Columns.Contains("is_repeatable") && row["is_repeatable"] != DBNull.Value && Convert.ToInt32(row["is_repeatable"]) == 1;
        evt.isCompleted = row.Table.Columns.Contains("is_completed") && row["is_completed"] != DBNull.Value && Convert.ToInt32(row["is_completed"]) == 1;

        return evt;
    }
}
