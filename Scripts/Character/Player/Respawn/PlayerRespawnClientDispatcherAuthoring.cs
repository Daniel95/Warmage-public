using DOTSNET;
using System;
using Unity.Entities;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerRespawnClientDispatcherAuthoring : MonoBehaviour, SelectiveSystemAuthoring
{
    // add system if Authoring is used
    public Type GetSystemType() => typeof(PlayerRespawnClientDispatcher);
}

[ClientWorld]
[UpdateInGroup(typeof(ClientConnectedSimulationSystemGroup))]
// use SelectiveSystemAuthoring to create it selectively
[DisableAutoCreation]
public class PlayerRespawnClientDispatcher : SystemBase
{
    [AutoAssign] protected NetworkClientSystem client;

    protected override void OnStartRunning()
    {
        base.OnStartRunning();

        UiRespawn.respawnButtonClickedEvent += OnRespawnButtonClicked;
    }

    protected override void OnStopRunning()
    {
        UiRespawn.respawnButtonClickedEvent -= OnRespawnButtonClicked;

        base.OnStopRunning();
    }

    protected override void OnUpdate() { }

    private void OnRespawnButtonClicked()
    {
        Debug.Assert(!PlayerLocalInfo.isAlive, "Player is not death!");

        ControlModeManager.GetInstance().SetControlMode(ControlMode.CharacterControl);

        PlayerLocalInfo.isAlive = true;

        UiGame.GetInstance().respawn.Show(false);

        client.Send(new PlayerRespawnMessage(PlayerLocalInfo.netId));
    }
}
