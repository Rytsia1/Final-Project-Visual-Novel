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
            txtSlotTitle.text = hasData ? $"Slot {slotId}  ·  Hari {day}, {timeBlock}" : $"Slot {slotId}  ·  Kosong";
        }

        if (txtSlotDetails != null)
        {
            txtSlotDetails.text = hasData ? $"Fisik {ph}  ·  Mental {mh}" : "Belum ada cerita tersimpan.";
        }

        if (txtSlotDate != null)
        {
            txtSlotDate.text = hasData ? dateStr : "";
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
