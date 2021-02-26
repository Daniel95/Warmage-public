using DOTSNET;
using System;
using Unity.Entities;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerDeathClientReceiverAuthoring : MonoBehaviour, SelectiveSystemAuthoring
{
    public Type GetSystemType() { return typeof(PlayerDeathClientReceiver); }
}

[DisableAutoCreation]
public class PlayerDeathClientReceiver : NetworkClientMessageSystem<PlayerDeathMessage>
{
    protected override void OnMessage(PlayerDeathMessage message)
    {
        if (message.netId == PlayerLocalInfo.netId)
        {
            ControlModeManager.GetInstance().SetControlMode(ControlMode.MouseControl);

            UiGame.GetInstance().respawn.Show(true);

            PlayerLocalInfo.isAlive = false;

            UiStatusBar playerStatusBar = UiGame.GetInstance().playerStatusBar;

            if (playerStatusBar.channelBar.channeling)
            {
                playerStatusBar.channelBar.StopChanneling();
            }

            SkillPlacementManager.GetInstance().StopPlacement();

            playerStatusBar.statusEffect.Clear();

            Debug.LogWarning("clear status effects");
        }
    }

    protected override void OnUpdate() { }
}
