using UnityEngine;
using UnityEngine.EventSystems;

public class ShowSkillTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private TooltipManager.SkillInfo skillInfo;

    public void OnPointerEnter(PointerEventData eventData)
    {
        TooltipManager.GetInstance().Show(skillInfo);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipManager.GetInstance().Hide();
    }

    public void SetInfo(TooltipManager.SkillInfo skillInfo)
    {
        this.skillInfo = skillInfo;
    }
}
