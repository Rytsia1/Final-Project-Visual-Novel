using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PhoneSaveSlotItemUI : MonoBehaviour
{
    [Header("Slot Components")]
    public int slotId;
    public TextMeshProUGUI txtSlotTitle;
    public TextMeshProUGUI txtSlotDetails;
    public TextMeshProUGUI txtSlotDate;
    public Button btnSave;
    public Button btnLoad;

    public void Setup(int id, string title, int day, string timeBlock, string dateStr, int ph, int mh, bool hasData, Action<int> onSave, Action<int> onLoad)
    {
        slotId = id;

        if (txtSlotTitle != null)
        {
            txtSlotTitle.text = $"SLOT {slotId}" + (hasData ? $" - {title}" : " (Kosong / 空)");
        }

        if (txtSlotDetails != null)
        {
            if (hasData)
                txtSlotDetails.text = $"Hari {day} • {timeBlock} | PH: {ph}  MH: {mh}";
            else
                txtSlotDetails.text = "Belum ada catatan permainan tersimpan.";
        }

        if (txtSlotDate != null)
        {
            txtSlotDate.text = hasData ? $"Waktu: {dateStr}" : "--/--/----";
        }

        if (btnSave != null)
        {
            btnSave.onClick.RemoveAllListeners();
            btnSave.onClick.AddListener(() => onSave?.Invoke(slotId));
        }

        if (btnLoad != null)
        {
            btnLoad.interactable = hasData;
            btnLoad.onClick.RemoveAllListeners();
            btnLoad.onClick.AddListener(() => onLoad?.Invoke(slotId));
        }
    }
}
