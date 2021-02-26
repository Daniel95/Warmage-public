using DOTSNET;
using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

[DisallowMultipleComponent]
public class ChannelBarServerSystemAuthoring : MonoBehaviour, SelectiveSystemAuthoring
{
    public Type GetSystemType() { return typeof(ChannelBarServerSystem); }

    [SerializeField] private float channelingSpeedFactorDecrease = 0.65f;

    private void Awake()
    {
        ChannelBarServerSystem system = Bootstrap.ServerWorld.GetExistingSystem<ChannelBarServerSystem>();

        system.channelingSpeedFactorDecrease = channelingSpeedFactorDecrease;
    }
}

[ServerWorld]
[UpdateInGroup(typeof(ServerActiveSimulationSystemGroup))]
[DisableAutoCreation]
public class ChannelBarServerSystem : SystemBase
{
    public float channelingSpeedFactorDecrease;

    private EndServerSimulationEntityCommandBufferSystem entityCommandBufferSystem;

    protected override void OnCreate()
    {
        base.OnCreate();

        entityCommandBufferSystem = World.GetOrCreateSystem<EndServerSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        var ecb = entityCommandBufferSystem.CreateCommandBuffer();

        //Start channeling when StartChannelingComponent is present.
        {
            Entities.ForEach((ChannelingEventComponent channelingEventComponent,
                ref ChannelingComponent channelingComponent,
                ref StatsComponent statsComponent,
                in Entity entity,
                in StartChannelingComponent startChannelingComponent) =>
            {
                if (!channelingComponent.active)
                {
                    channelingComponent.active = true;

                    statsComponent.speedFactor -= channelingSpeedFactorDecrease;

                    ecb.AddComponent<DispatchStatsMessageComponent>(entity);
                }

                channelingEventComponent.onCompleteEvent = startChannelingComponent.onCompleteEvent;
                channelingComponent.timeLeft = channelingComponent.totalTime = startChannelingComponent.time;

                ecb.AddComponent(entity, new DispatchChannelBarMessageComponent
                {
                    messageType = ChannelBarMessage.MessageType.Start,
                    time = startChannelingComponent.time
                });

                ecb.RemoveComponent<StartChannelingComponent>(entity);
            })
            .WithoutBurst()
            .Run();
        }

        //Handle Interruptions
        {
            {
                float _channelingSpeedFactorDecrease = channelingSpeedFactorDecrease;

                Entities.ForEach((ref ChannelingComponent channelingComponent,
                    ref StatsComponent statsComponent,
                    in Entity entity,
                    in InterruptChannelingComponent stopChannelingComponent) =>
                {
                    if (!channelingComponent.active) { return; }

                    statsComponent.speedFactor += _channelingSpeedFactorDecrease;
                    ecb.AddComponent<DispatchStatsMessageComponent>(entity);

                    channelingComponent.active = false;

                    ecb.AddComponent(entity, new DispatchChannelBarMessageComponent
                    {
                        messageType = ChannelBarMessage.MessageType.Interrupt,
                    });

                    ecb.RemoveComponent<InterruptChannelingComponent>(entity);
                })
                .Run();
            }
        }

        //Update channeling timer & execute the on complete event.
        {
            float deltaTime = Time.DeltaTime;

            Entities.ForEach((ChannelingEventComponent channelingEventComponent,
                ref ChannelingComponent channelingComponent,
                ref StatsComponent statsComponent,
                in Entity entity,
                in NetworkEntity networkEntity) =>
            {
                if (!channelingComponent.active) { return; }

                channelingComponent.timeLeft -= deltaTime;

                if (channelingComponent.timeLeft <= 0)
                {
                    statsComponent.speedFactor += channelingSpeedFactorDecrease;
                    ecb.AddComponent<DispatchStatsMessageComponent>(entity);

                    channelingComponent.active = false;
                    channelingEventComponent.onCompleteEvent();
                }
            })
            .WithoutBurst()
            .Run();
        }

        entityCommandBufferSystem.AddJobHandleForProducer(Dependency);
    }
}
