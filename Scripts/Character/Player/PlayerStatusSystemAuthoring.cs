using DOTSNET;
using System;
using Unity.Entities;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerStatusSystemAuthoring : MonoBehaviour, SelectiveSystemAuthoring
{
    public Type GetSystemType() { return typeof(PlayerStatusSystem); }
}

[ClientWorld]
[UpdateInGroup(typeof(ClientConnectedSimulationSystemGroup))]
[DisableAutoCreation]
public class PlayerStatusSystem : SystemBase
{
    protected override void OnUpdate()
    {
        int playerCurrentHealth = 0;
        int playerMaxHealth = 0;

        Entities.ForEach((
            in LocalPlayerComponent localPlayerComponent,
            in HealthComponent healthComponent) =>
        {
            playerCurrentHealth = healthComponent.currentHealth;
            playerMaxHealth = healthComponent.maxHealth;
        }).Run();

        PlayerLocalInfo.currentHealth = playerCurrentHealth;
        PlayerLocalInfo.maxHealth = playerMaxHealth;
    }
}
