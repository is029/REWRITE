using UnityEngine;
using UnityEngine.EventSystems;

public class ItemButtonHover : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    [SerializeField] private BattleItemUI itemUI;

    [SerializeField] private int itemIndex;

    public void OnPointerEnter(
        PointerEventData eventData)
    {
        if (itemUI != null)
        {
            itemUI.ShowDescription(itemIndex);
        }
    }

    public void OnPointerExit(
        PointerEventData eventData)
    {
        if (itemUI != null)
        {
            itemUI.HideDescription();
        }
    }
}