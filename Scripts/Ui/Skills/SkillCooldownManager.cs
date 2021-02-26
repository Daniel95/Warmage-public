using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SkillCooldownManager : MonoBehaviour
{
    private class SkillCooldownData
    {
        public ISkill skill;
        public int charges;
        public float fillAmount;
        public float remainingCooldown;
    }

    public float globalCooldown => _globalCooldown;

    [SerializeField] private float _globalCooldown = 0.75f;

    private Dictionary<Guid, SkillCooldownData> skillCooldowns = new Dictionary<Guid, SkillCooldownData>();

    private float remainingGlobalCooldown;
    private ActionPointManager actionPointManager;

    public bool CanUse(Guid skillId)
    {
        ISkill skill = skillCooldowns[skillId].skill;

        if (skill.HasGlobalCooldown() && remainingGlobalCooldown > 0)
        {
            return false;
        }

        if (skill.GetActionPointCost() > actionPointManager.availableCount)
        {
            return false;
        }

        return GetSkillCharges(skillId) > 0;
    }

    public void RegisterSkillUsed(Guid skillId)
    {
        UpdateLastActivateTime(skillId);

        ISkill skill = skillCooldowns[skillId].skill;

        actionPointManager.Spend(skill.GetActionPointCost());
    }

    public float GetCooldownFillAmount(Guid skillId)
    {
        Debug.Assert(skillCooldowns.ContainsKey(skillId), "Skill does not exist in skill cooldown manager! (GetCooldownFillAmount) " + skillId);

        return skillCooldowns[skillId].fillAmount;
    }

    public int GetSkillCharges(Guid skillId)
    {
        Debug.Assert(skillCooldowns.ContainsKey(skillId), "Skill does not exist in skill cooldown manager! (GetSkillCharges)" + skillId);

        return skillCooldowns[skillId].charges;
    }

    private void UpdateLastActivateTime(Guid skillId)
    {
        Debug.Assert(skillCooldowns.ContainsKey(skillId), "Skill does not exist in skill cooldown manager! (UpdateLastActivateTime)" + skillId);

        SkillCooldownData skillCooldownData = skillCooldowns[skillId];
        ISkill skill = skillCooldownData.skill;

        if (skill.HasGlobalCooldown())
        {
            remainingGlobalCooldown = globalCooldown;
        }

        skillCooldownData.remainingCooldown = skill.GetCooldown();
        skillCooldownData.charges--;
    }

    private void Update()
    {
        remainingGlobalCooldown -= Time.deltaTime;

        foreach (var keyValuePair in skillCooldowns)
        {
            SkillCooldownData skillCooldownData = keyValuePair.Value;
            ISkill skill = skillCooldownData.skill;

            skillCooldownData.remainingCooldown -= Time.deltaTime;
            float remainingCooldown = 0;

            float _remainingGlobalCooldown = skill.HasGlobalCooldown() ? remainingGlobalCooldown : 0;

            float remainingActionPointsCooldown = 0;

            UiActionPoints actionPoints = UiGame.GetInstance().playerStatusBar.actionPoints;

            if (skill.GetActionPointCost() > 0)
            {
                remainingActionPointsCooldown = actionPointManager.GetRemainingCooldown(skill.GetActionPointCost());
            }

            if (skillCooldownData.charges == 0 && skillCooldownData.remainingCooldown > _remainingGlobalCooldown && skillCooldownData.remainingCooldown > remainingActionPointsCooldown)
            {
                skillCooldownData.fillAmount = skillCooldownData.remainingCooldown / skill.GetCooldown();
            }
            else if ((_remainingGlobalCooldown > skillCooldownData.remainingCooldown || skillCooldownData.charges > 0) && _remainingGlobalCooldown > remainingActionPointsCooldown)
            {
                skillCooldownData.fillAmount = _remainingGlobalCooldown / globalCooldown;
            }
            else if ((remainingActionPointsCooldown > skillCooldownData.remainingCooldown || skillCooldownData.charges > 0) && remainingActionPointsCooldown > _remainingGlobalCooldown)
            {
                skillCooldownData.fillAmount = remainingActionPointsCooldown / actionPointManager.GetCooldown();
            }
            else
            {
                skillCooldownData.fillAmount = 0;
            }

            if (skillCooldownData.remainingCooldown > _remainingGlobalCooldown && skillCooldownData.remainingCooldown > remainingActionPointsCooldown)
            {
                remainingCooldown = skillCooldownData.remainingCooldown;
            }
            else if (_remainingGlobalCooldown > skillCooldownData.remainingCooldown && _remainingGlobalCooldown > remainingActionPointsCooldown)
            {
                remainingCooldown = _remainingGlobalCooldown;
            }
            else if (remainingActionPointsCooldown > skillCooldownData.remainingCooldown && remainingActionPointsCooldown > _remainingGlobalCooldown)
            {
                remainingCooldown = remainingActionPointsCooldown;
            } 
            else
            {
                remainingCooldown = 0;
            }

            if (remainingCooldown <= 0 && skillCooldownData.charges != skill.GetMaxCharges())
            {
                if (skillCooldownData.charges == skill.GetMaxCharges() - 1)
                {
                    skillCooldownData.charges = skill.GetMaxCharges();
                }
                else
                {
                    skillCooldownData.charges++;
                    skillCooldownData.remainingCooldown = skill.GetCooldown();
                }
            }
        }
    }

    private void Start()
    {
        actionPointManager = ActionPointManager.GetInstance();

        List<Guid> skillIds = SkillLibrary.GetInstance().GetAllPlayerSkillIds();

        for (int i = 0; i < skillIds.Count; i++)
        {
            ISkill skill = SkillLibrary.GetInstance().GetSkillTemplate(skillIds[i]);

            skillCooldowns.Add(skill.GetId(), new SkillCooldownData
            {
                skill = skill,
                remainingCooldown = skill.GetCooldown(),
                charges = skill.GetMaxCharges() - 1
            });
        }
    }
}
