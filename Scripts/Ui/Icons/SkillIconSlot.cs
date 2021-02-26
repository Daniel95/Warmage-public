using System;
using UnityEngine;

[RequireComponent(typeof(ShowSkillTooltip))]
public class SkillIconSlot : IconSlot
{
    public ISkill skill { get; private set; }
    public Guid skillId { get; private set; }

    private ShowSkillTooltip showSkillTooltip;

    public void SetSkill(Guid skillId)
    {
        this.skillId = skillId;

        occupied = true;

        skill = (SkillScriptableObject)SkillLibrary.GetInstance().GetSkillTemplate(skillId);
        SetIcon(skill.GetIcon());

        TooltipManager.SkillInfo skillInfo = new TooltipManager.SkillInfo
        {
            name = skill.GetName(),
            range = skill.GetRange(),
            cooldown = skill.GetCooldown(),
            mana = skill.GetManaCost(),
            castTime = skill.GetCastTime(),
            description = skill.GetDescription()
        };

        showSkillTooltip.enabled = true;
        showSkillTooltip.SetInfo(skillInfo);
    }

    public void Clear()
    {
        SetIcon(null);
        occupied = false;
        skill = null;
        showSkillTooltip.enabled = false;
    }

    protected override void OnPlace(IconSlot iconSlot)
    {
        if (iconSlot is SkillIconSlot)
        {
            SkillIconSlot skillIconSlot = (SkillIconSlot)iconSlot;

            if(iconSlot != this)
            {
                skillIconSlot.SetSkill(skillId);

                if(isReplaceable)
                {
                    Clear();
                }

                UiGame.GetInstance().skillBar.SaveSkillBar();
            }
        }
        else if(isReplaceable)
        {
            Clear();
        }
    }

    protected override void Awake()
    {
        base.Awake();

        showSkillTooltip = GetComponent<ShowSkillTooltip>();
    }
}
