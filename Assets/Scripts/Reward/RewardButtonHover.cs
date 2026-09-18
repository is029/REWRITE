using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class RewardButtonHover : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    [Header("ï\é¶Ç∑ÇÈèÍèä")]
    [SerializeField] private TMP_Text descriptionText;

    [Header("ïÒèVì‡óe")]
    [TextArea]
    [SerializeField] private string description;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (descriptionText == null)
            return;

        descriptionText.text = description;
        descriptionText.gameObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (descriptionText == null)
            return;

        descriptionText.gameObject.SetActive(false);
    }
}