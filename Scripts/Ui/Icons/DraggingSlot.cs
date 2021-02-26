using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggingSlot : MonoBehaviour
{
    [SerializeField] private Image icon = null;

    private Action<IconSlot> onPlace;
    private GraphicRaycaster raycaster;
    private EventSystem eventSystem;

    public void Init(Sprite sprite, Action<IconSlot> onPlace)
    {
        icon.sprite = sprite;
        this.onPlace = onPlace;

        eventSystem = UiGame.GetInstance().eventSystem;
        raycaster = UiGame.GetInstance().graphicsRaycaster;
    }

    private void Update()
    {
        transform.position = Input.mousePosition;

        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            PointerEventData pointerEventData = new PointerEventData(eventSystem);
            pointerEventData.position = Input.mousePosition;

            List<RaycastResult> results = new List<RaycastResult>();

            raycaster.Raycast(pointerEventData, results);

            IconSlot iconSlot = null;

            //For every result returned, output the name of the GameObject on the Canvas hit by the Ray
            foreach (RaycastResult result in results)
            {
                if(result.gameObject.transform.parent.TryGetComponent(out IconSlot hitIconSlot) && hitIconSlot.isReplaceable)
                {
                    iconSlot = hitIconSlot;
                    break;
                }
            }

            if (onPlace != null)
            {
                onPlace(iconSlot);
            }

            Destroy(gameObject);
        }
    }
}
