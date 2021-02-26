using UnityEngine;
using UnityEngine.EventSystems;

public class WindowMoveRaycastTarget : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public bool mouseOver { get; private set; } 

    public void OnPointerEnter(PointerEventData eventData)
    {
        mouseOver = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        mouseOver = false;
    }
}
