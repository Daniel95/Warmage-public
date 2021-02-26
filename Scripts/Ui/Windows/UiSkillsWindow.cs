using System;
using UnityEngine;
using UnityEngine.UI;

public class UiSkillsWindow : UiWindowBase
{
    [SerializeField] private GridLayoutGroup gridLayoutGroup = null;
    [SerializeField] private SkillIconSlot skillIconPrefab = null;

    protected override void OnWindowOpen()
    {
        PlayerProfile playerProfile = PlayerProfileManager.GetInstance().playerProfile;

        foreach (Guid skillId in playerProfile.skills)
        {
            SkillIconSlot skillSlotIcon = Instantiate(skillIconPrefab, gridLayoutGroup.transform);
            skillSlotIcon.SetSkill(skillId);
        }
    }

    protected override void OnWindowClose()
    {
        foreach (Transform child in gridLayoutGroup.transform)
        {
            Destroy(child.gameObject);
        }
    }
}
