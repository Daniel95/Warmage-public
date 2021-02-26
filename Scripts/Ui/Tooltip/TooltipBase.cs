using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public abstract class TooltipBase : MonoBehaviour
{
    public RectTransform TooltipRectTransform { get; private set; }
    public RectTransform BackgroundRectTransform => backgroundRectTransform;

    [SerializeField] private RectTransform backgroundRectTransform = null;

    private void Awake()
    {
        TooltipRectTransform = GetComponent<RectTransform>();
        gameObject.SetActive(false);
    }
}
