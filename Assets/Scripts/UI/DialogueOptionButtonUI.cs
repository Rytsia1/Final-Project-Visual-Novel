using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class DialogueOptionButtonUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    [Header("UI References")]
    public Image imgBackground;
    public TextMeshProUGUI txtOption;
    public GameObject pointerHand;

    [Header("Sprites")]
    public Sprite spriteNormal;
    public Sprite spriteSelected;

    [Header("Colors")]
    public Color colorNormalText = new Color(0.965f, 0.973f, 1f, 1f); // #F6F8FF
    public Color colorSelectedText = new Color(0.118f, 0.141f, 0.251f, 1f); // #1E2440

    [Header("State")]
    public bool isSelected = false;

    private void Awake()
    {
        if (imgBackground == null) imgBackground = GetComponent<Image>();
        if (txtOption == null) txtOption = GetComponentInChildren<TextMeshProUGUI>();
        ApplyVisualState(isSelected);
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
        ApplyVisualState(selected);
    }

    private void ApplyVisualState(bool selected)
    {
        if (imgBackground != null)
        {
            if (selected && spriteSelected != null)
            {
                imgBackground.sprite = spriteSelected;
                imgBackground.color = Color.white;
            }
            else if (spriteNormal != null)
            {
                imgBackground.sprite = spriteNormal;
                imgBackground.color = Color.white;
            }
        }

        if (txtOption != null)
        {
            txtOption.color = selected ? colorSelectedText : colorNormalText;
        }

        if (pointerHand != null)
        {
            pointerHand.SetActive(selected);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        SetSelected(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SetSelected(false);
    }

    public void OnSelect(BaseEventData eventData)
    {
        SetSelected(true);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        SetSelected(false);
    }
}
