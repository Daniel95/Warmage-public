using DOTSNET;
using System;
using Unity.Entities;
using UnityEngine;

[DisallowMultipleComponent]
public class StatusEffectClientSystemAuthoring : MonoBehaviour, SelectiveSystemAuthoring
{
    public Type GetSystemType() => typeof(StatusEffectClientSystem);
}

[ClientWorld]
[DisableAutoCreation]
[UpdateInGroup(typeof(ClientConnectedSimulationSystemGroup))]
public class StatusEffectClientSystem : SystemBase
{
    protected override void OnUpdate()
    {
        float deltaTime = Time.DeltaTime;

        Entities.ForEach((DynamicBuffer<StatusEffectElement> statusEffectBuffer) => 
        {
            for (int i = 0; i < statusEffectBuffer.Length; i++)
            {
                StatusEffectElement statusEffectElement = statusEffectBuffer[i];
                statusEffectElement.timeLeft -= deltaTime;

                statusEffectBuffer[i] = statusEffectElement;
            }
        })
        .Run();
    }
}
