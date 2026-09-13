using UnityEngine;
using TMPro;

public class BacklogItemUI : MonoBehaviour
{
    [Header("Komponen Teks Backlog")]
    public TextMeshProUGUI txtSpeaker;
    public TextMeshProUGUI txtDialogue;

    public void Setup(string speaker, string text, Color speakerColor)
    {
        if (txtSpeaker != null)
        {
            txtSpeaker.text = speaker;
            txtSpeaker.color = speakerColor;
        }

        if (txtDialogue != null)
        {
            txtDialogue.text = text;
        }
    }
}
