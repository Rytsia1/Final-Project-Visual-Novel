using UnityEngine;
using UnityEngine.EventSystems;

public class ActivityTooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [TextArea]
    public string activityDescription;
    public string costGainPreview;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (HUDController.Instance != null)
        {
            HUDController.Instance.ShowPredictiveTooltip(activityDescription, costGainPreview);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (HUDController.Instance != null)
        {
            HUDController.Instance.HidePredictiveTooltip();
        }
    }
}
