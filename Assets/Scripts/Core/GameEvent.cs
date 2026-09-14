using System;
using System.Data;
using UnityEngine;

[System.Serializable]
public class GameEvent
{
    public int eventId;
    public string eventCode;
    public string eventName;
    public int priority;
    public int startDay;
    public int endDay;
    public string timeBlock; // "Pagi", "Sore", "Malam", or null
    public int dayType;      // 0: Any, 1: Workday only, 2: Weekend only
    public int reqNpcId;
    public int reqMinAffectionState;
    public int reqMinGuanxi;
    public int reqMinPh;
    public int reqMaxPh;
    public int reqMinMh;
    public int reqMaxMh;
    public int reqMinAcadTheory;
    public int reqMinAcadPractice;
    public int reqMinLang;
    public int reqMinEtiquette;
    public int reqRumorLevel; // -1 = ignore
    public string reqFlags;   // comma-separated
    public string setFlags;   // comma-separated
    public int dialogueNodeId;
    public int prereqEventId;
    public bool isRepeatable;
    public bool isCompleted;
    public int costTimeBlock; // 0 = free interrupt, 1 = consumes time block

    public static GameEvent FromDataRow(DataRow row)
    {
        if (row == null) return null;

        GameEvent evt = new GameEvent();
        evt.eventId = Convert.ToInt32(row["event_id"]);
        evt.eventCode = row["event_code"] != DBNull.Value ? row["event_code"].ToString() : "";
        evt.eventName = row["event_name"] != DBNull.Value ? row["event_name"].ToString() : "";
        evt.priority = row["priority"] != DBNull.Value ? Convert.ToInt32(row["priority"]) : 10;
        evt.startDay = row["start_day"] != DBNull.Value ? Convert.ToInt32(row["start_day"]) : 1;
        evt.endDay = row["end_day"] != DBNull.Value ? Convert.ToInt32(row["end_day"]) : 60;
        evt.timeBlock = row["time_block"] != DBNull.Value ? row["time_block"].ToString() : null;
        evt.dayType = row["day_type"] != DBNull.Value ? Convert.ToInt32(row["day_type"]) : 0;

        evt.reqNpcId = row["req_npc_id"] != DBNull.Value ? Convert.ToInt32(row["req_npc_id"]) : 0;
        evt.reqMinAffectionState = row["req_min_affection_state"] != DBNull.Value ? Convert.ToInt32(row["req_min_affection_state"]) : 0;
        evt.reqMinGuanxi = row["req_min_guanxi"] != DBNull.Value ? Convert.ToInt32(row["req_min_guanxi"]) : 0;

        evt.reqMinPh = row["req_min_ph"] != DBNull.Value ? Convert.ToInt32(row["req_min_ph"]) : 0;
        evt.reqMaxPh = row["req_max_ph"] != DBNull.Value ? Convert.ToInt32(row["req_max_ph"]) : 100;
        evt.reqMinMh = row["req_min_mh"] != DBNull.Value ? Convert.ToInt32(row["req_min_mh"]) : 0;
        evt.reqMaxMh = row["req_max_mh"] != DBNull.Value ? Convert.ToInt32(row["req_max_mh"]) : 100;

        evt.reqMinAcadTheory = row["req_min_acad_theory"] != DBNull.Value ? Convert.ToInt32(row["req_min_acad_theory"]) : 0;
        evt.reqMinAcadPractice = row["req_min_acad_practice"] != DBNull.Value ? Convert.ToInt32(row["req_min_acad_practice"]) : 0;
        evt.reqMinLang = row["req_min_lang"] != DBNull.Value ? Convert.ToInt32(row["req_min_lang"]) : 0;
        evt.reqMinEtiquette = row["req_min_etiquette"] != DBNull.Value ? Convert.ToInt32(row["req_min_etiquette"]) : 0;

        evt.reqRumorLevel = row["req_rumor_level"] != DBNull.Value ? Convert.ToInt32(row["req_rumor_level"]) : -1;
        evt.reqFlags = row["req_flags"] != DBNull.Value ? row["req_flags"].ToString() : null;
        evt.setFlags = row["set_flags"] != DBNull.Value ? row["set_flags"].ToString() : null;

        evt.dialogueNodeId = row["dialogue_node_id"] != DBNull.Value ? Convert.ToInt32(row["dialogue_node_id"]) : 0;
        evt.prereqEventId = row["prereq_event_id"] != DBNull.Value ? Convert.ToInt32(row["prereq_event_id"]) : 0;
        evt.isRepeatable = row["is_repeatable"] != DBNull.Value && Convert.ToInt32(row["is_repeatable"]) == 1;
        evt.isCompleted = row["is_completed"] != DBNull.Value && Convert.ToInt32(row["is_completed"]) == 1;
        evt.costTimeBlock = row["cost_time_block"] != DBNull.Value ? Convert.ToInt32(row["cost_time_block"]) : 0;

        return evt;
    }
}
