using System;
using UnityEngine;

public class UiSkillbar : MonoBehaviour
{
    private SkillIconSlot[] skillIconSlots;

    public void SaveSkillBar()
    {
        PlayerProfile playerProfile = PlayerProfileManager.GetInstance().playerProfile;

        playerProfile.skillBar = new SerializableGuid[skillIconSlots.Length];

        for (int i = 0; i < skillIconSlots.Length; i++)
        {
            playerProfile.skillBar[i] = skillIconSlots[i].skillId;
        }

        PlayerProfileManager.GetInstance().Save(playerProfile);
    }

    private void Start()
    {
        PlayerProfile playerProfile = PlayerProfileManager.GetInstance().playerProfile;

        skillIconSlots = GetComponentsInChildren<SkillIconSlot>();

        if (playerProfile.skillBar == null || playerProfile.skillBar.Length != skillIconSlots.Length) { return; }

        for (int i = 0; i < skillIconSlots.Length; i++)
        {
            if (playerProfile.skillBar[i] == null) { continue; }
            if (playerProfile.skillBar[i].Value == string.Empty) { continue; }
            if (!SkillLibrary.GetInstance().Exists(playerProfile.skillBar[i])) { continue; }

            skillIconSlots[i].SetSkill(playerProfile.skillBar[i]);
        }
    }
}
