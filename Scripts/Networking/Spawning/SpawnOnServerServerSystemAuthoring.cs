using DOTSNET;
using System;
using Unity.Entities;
using UnityEngine;

[DisallowMultipleComponent]
public class SpawnOnServerServerSystemAuthoring : MonoBehaviour, SelectiveSystemAuthoring
{
    public Type GetSystemType() => typeof(SpawnOnServerServerSystem);
}

[ServerWorld]
[UpdateInGroup(typeof(ServerActiveSimulationSystemGroup))]
[DisableAutoCreation]
public class SpawnOnServerServerSystem : SystemBase
{
    [AutoAssign] protected NetworkServerSystem server;

    private EndServerSimulationEntityCommandBufferSystem entityCommandBufferSystem;

    protected override void OnCreate()
    {
        base.OnCreate();

        entityCommandBufferSystem = World.GetOrCreateSystem<EndServerSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        var ecb = entityCommandBufferSystem.CreateCommandBuffer();

        Entities.ForEach((in Entity entity,
            in SpawnOnServerComponent networkSpawnComponent) =>
        {
            server.Spawn(entity, null);

            ecb.RemoveComponent<SpawnOnServerComponent>(entity);
        })
        .WithoutBurst()
        .Run();

        entityCommandBufferSystem.AddJobHandleForProducer(Dependency);
    }
}
