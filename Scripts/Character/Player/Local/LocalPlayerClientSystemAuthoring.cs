using DOTSNET;
using System;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[DisallowMultipleComponent]
public class LocalPlayerClientSystemAuthoring : MonoBehaviour, SelectiveSystemAuthoring
{
    public Type GetSystemType() => typeof(LocalPlayerClientSystem);
}

[ClientWorld]
[UpdateInGroup(typeof(ClientConnectedSimulationSystemGroup))]
[DisableAutoCreation]
public class LocalPlayerClientSystem : SystemBase
{
    private bool attachedLocalPlayerComponent = false;

    private EndSimulationEntityCommandBufferSystem entityCommandBufferSystem;

    protected override void OnCreate()
    {
        base.OnCreate();

        entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        PlayerLocalInfo.isAlive = true;
    }

    protected override void OnUpdate()
    {
        if(!attachedLocalPlayerComponent)
        {
            var ecb = entityCommandBufferSystem.CreateCommandBuffer();

            ulong localPlayerNetId = 0;
            Entity localPlayerEntity = new Entity();
            bool _attachedLocalPlayerComponent = false;

            Entities.ForEach((in Entity entity,
                in FactionComponent factionComponent,
                in NetworkEntity networkEntity) =>
            {
                if (networkEntity.owned)
                {
                    localPlayerNetId = networkEntity.netId;
                    localPlayerEntity = entity;

                    ecb.AddComponent<LocalPlayerComponent>(entity);
                    _attachedLocalPlayerComponent = true;
                }
            })
            .Run();

            entityCommandBufferSystem.AddJobHandleForProducer(Dependency);

            if (_attachedLocalPlayerComponent)
            {
                attachedLocalPlayerComponent = true;
                PlayerLocalInfo.entity = localPlayerEntity;
                PlayerLocalInfo.netId = localPlayerNetId;

                ControlModeManager.GetInstance().SetControlMode(ControlMode.CharacterControl);
            }
        }

        float3 localPlayerPosition = new float3();
        float3 forward = new float3();

        Entities.ForEach((in Translation translation,
            in LocalToWorld localToWorld,
            in LocalPlayerComponent localPlayerComponent) =>
        {
            localPlayerPosition = translation.Value;
            forward = localToWorld.Forward;
        })
        .Run();

        PlayerLocalInfo.forward = forward;
        PlayerLocalInfo.position = localPlayerPosition;
    }
}
