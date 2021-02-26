using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class IconSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public bool isReplaceable => replaceable; 

    protected bool occupied;

    [SerializeField] private Image icon = null;
    [SerializeField] private DraggingSlot draggingSlotPrefab = null;
    [SerializeField] private bool draggable = false;
    [SerializeField] private bool replaceable = false;

    private bool mouseOver;
    private Transform canvasTransform;

    protected abstract void OnPlace(IconSlot iconSlot);

    public void OnPointerEnter(PointerEventData eventData)
    {
        mouseOver = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        mouseOver = false;
    }

    public void SetIcon(Sprite sprite)
    {
        icon.sprite = sprite;

        if (icon.sprite == null)
        {
            icon.color = new Color(icon.color.r, icon.color.g, icon.color.b, 0);
        }
        else
        {
            icon.color = new Color(icon.color.r, icon.color.g, icon.color.b, 1);
        }
    }

    private void Update()
    {
        if(!Input.GetKeyDown(KeyCode.Mouse0)) { return; }
        if (!mouseOver) { return; }
        if (!occupied || !draggable) { return; }

        DraggingSlot draggingSlot = Instantiate(draggingSlotPrefab, transform.position, quaternion.identity, canvasTransform);

        draggingSlot.Init(icon.sprite, OnPlace);
    }

    protected virtual void Awake()
    {
        canvasTransform = UiGame.GetInstance().transform;
    }
}
