using DOTSNET;
using System;
using Unity.Entities;
using UnityEngine;

[DisallowMultipleComponent]
public class EnterObserversServerSystemAuthoring : MonoBehaviour, SelectiveSystemAuthoring
{
    public Type GetSystemType() => typeof(EnterObserversServerSystem);
}

[ServerWorld]
[UpdateInGroup(typeof(ServerActiveSimulationSystemGroup))]
[DisableAutoCreation]
public class EnterObserversServerSystem : SystemBase
{
    private EndServerSimulationEntityCommandBufferSystem entityCommandBufferSystem;

    protected override void OnCreate()
    {
        base.OnCreate();

        entityCommandBufferSystem = World.GetOrCreateSystem<EndServerSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        var ecb = entityCommandBufferSystem.CreateCommandBuffer();

        Entities.ForEach((DynamicBuffer<DispatchStatusEffectMessageElement> dispatchStatusEffectMessageBuffer,
            in DynamicBuffer<StatusEffectElement> statusEffectBuffer,
            in Entity entity,
            in EnterObserversComponent enterObserversComponent) =>
        {
            for (int i = 0; i < statusEffectBuffer.Length; i++)
            {
                StatusEffectElement statusEffectElement = statusEffectBuffer[i];

                dispatchStatusEffectMessageBuffer.Add(new DispatchStatusEffectMessageElement 
                {
                    statusEffectId = statusEffectElement.statusEffectId,
                    casterNetId = statusEffectElement.casterNetId,
                    timeLeft = statusEffectElement.timeLeft,
                    count = statusEffectElement.count,
                    messageType = StatusEffectMessage.MessageType.Add
                });
            }

            ecb.AddComponent<DispatchStatsMessageComponent>(entity);
            ecb.AddComponent<DispatchSetFactionMessageComponent>(entity);
            ecb.AddComponent<DispatchHealthMessageComponent>(entity);

            ecb.RemoveComponent<EnterObserversComponent>(entity);
        })
        .Run();

        entityCommandBufferSystem.AddJobHandleForProducer(Dependency);
    }
}
