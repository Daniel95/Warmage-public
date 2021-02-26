using UnityEngine;
using UnityEngine.UI;

public class SkillTooltip : TooltipBase
{
    [SerializeField] private new Text name = null;
    [SerializeField] private Text range = null;
    [SerializeField] private Text cooldown = null;
    [SerializeField] private Text castTime = null;
    [SerializeField] private Text mana = null;
    [SerializeField] private Text description = null;

    public void SetInfo(TooltipManager.SkillInfo skillInfo)
    {
        description.text = skillInfo.description;
        name.text = skillInfo.name;
        range.text = skillInfo.range.ToString();
        cooldown.text = skillInfo.cooldown.ToString();
        castTime.text = skillInfo.castTime.ToString();
        mana.text = skillInfo.mana.ToString();
    }
}
