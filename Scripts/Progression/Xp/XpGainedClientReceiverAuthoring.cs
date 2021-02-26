using DOTSNET;
using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

[DisallowMultipleComponent]
public class XpGainedClientReceiverAuthoring : MonoBehaviour, SelectiveSystemAuthoring
{
    public Type GetSystemType() { return typeof(XpGainedClientReceiver); }
}

[DisableAutoCreation]
public class XpGainedClientReceiver : NetworkClientMessageSystem<XpGainedMessage>
{
    public void GainXp(int amount)
    {
        Debug.LogWarning("xp gained: " + amount);

        PlayerProfileManager playerProfileManager = PlayerProfileManager.GetInstance();
        PlayerProfile playerProfile = playerProfileManager.playerProfile;
        LevelXpRequirementLibrary levelXpRequirementLibrary = LevelXpRequirementLibrary.GetInstance();

        int previousLevelIndex = levelXpRequirementLibrary.GetLevelIndex(playerProfile.globalXp);
        playerProfile.globalXp += amount;
        int newLevelIndex = levelXpRequirementLibrary.GetLevelIndex(playerProfile.globalXp);

        if (previousLevelIndex != newLevelIndex)
        {
            Debug.LogWarning("Level up! New level index: " + newLevelIndex);
            Debug.LogWarning("previous level index: " + previousLevelIndex);

            List<Guid> allSkills = SkillLibrary.GetInstance().GetAllPlayerSkillIds();

            allSkills.RemoveAll(x => playerProfile.skills.Contains(x));

            if(allSkills.Count > 0)
            {
                Guid randomSkillId = allSkills.GetRandom();

                playerProfile.skills.Add(randomSkillId);
            }
        }

        playerProfileManager.Save(playerProfile);

        UiGame.GetInstance().floatingTextManager.SpawnText(PlayerLocalInfo.position, amount + " Experience", FloatingTextType.Experience);
        UiGame.GetInstance().XpBar.UpdateXp();
    }

    protected override void OnMessage(XpGainedMessage message)
    {
        if(PlayerLocalInfo.netId != message.netId) { return; }

        GainXp(message.amount);
    }

    protected override void OnUpdate() 
    {
#if UNITY_EDITOR
        if(Input.GetKeyDown(KeyCode.P))
        {
            GainXp(100);
            UiGame.GetInstance().XpBar.UpdateXp();
        }
#endif
    }
}

