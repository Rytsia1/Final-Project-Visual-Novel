using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoadSlotItemUI : MonoBehaviour
{
    [Header("Komponen UI Slot")]
    public TextMeshProUGUI txtTitle;
    public TextMeshProUGUI txtDetails;
    public TextMeshProUGUI txtDate;
    public TextMeshProUGUI txtStats;
    public Button btnLoad;

    private int _slotId;
    private Action<int> _onLoadCallback;

    public void Setup(int slotId, string title, int day, string timeBlock, string dateStr, int ph, int mh, Action<int> onLoadCallback)
    {
        _slotId = slotId;
        _onLoadCallback = onLoadCallback;

        if (txtTitle != null)
        {
            string prefix = slotId == 0 ? "[QUICK SAVE] " : $"[SLOT {slotId}] ";
            txtTitle.text = prefix + (string.IsNullOrEmpty(title) ? "Sesi Tersimpan" : title);
        }

        if (txtDetails != null)
        {
            txtDetails.text = $"Hari {day} • Waktu: {timeBlock}";
        }

        if (txtDate != null)
        {
            txtDate.text = string.IsNullOrEmpty(dateStr) ? "" : $"Simpan: {dateStr}";
        }

        if (txtStats != null)
        {
            txtStats.text = $"PH: {ph} | MH: {mh}";
        }

        if (btnLoad != null)
        {
            btnLoad.onClick.RemoveAllListeners();
            btnLoad.onClick.AddListener(HandleLoadClicked);
        }
    }

    private void HandleLoadClicked()
    {
        _onLoadCallback?.Invoke(_slotId);
    }
}
