using UnityEngine;
using UnityEngine.EventSystems;

public class ShowStatusEffectTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private TooltipManager.StatusEffectInfo statusEffectInfo;

    private TooltipManager tooltipManager;

    private void Awake()
    {
        tooltipManager = TooltipManager.GetInstance();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        tooltipManager.Show(statusEffectInfo);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        tooltipManager.Hide();
    }

    public void OnDestroy()
    {
        if(tooltipManager.tooltipMode == TooltipManager.TooltipMode.StatusEffect)
        {
            TooltipManager.GetInstance().Hide();
        }
    }

    public void SetInfo(TooltipManager.StatusEffectInfo statusEffectInfo)
    {
        this.statusEffectInfo = statusEffectInfo;
    }
}
