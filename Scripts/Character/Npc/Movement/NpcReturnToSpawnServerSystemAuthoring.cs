using System;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using DOTSNET;
using Reese.Nav;

[DisallowMultipleComponent]
public class NpcReturnToSpawnServerSystemAuthoring : MonoBehaviour, SelectiveSystemAuthoring
{
    // add system if Authoring is used
    public Type GetSystemType() => typeof(NpcReturnToSpawnServerSystem);
}

[ServerWorld]
[UpdateInGroup(typeof(ServerActiveSimulationSystemGroup))]
// use SelectiveSystemAuthoring to create it selectively
[DisableAutoCreation]
public class NpcReturnToSpawnServerSystem : SystemBase
{
    private EndServerSimulationEntityCommandBufferSystem entityCommandBufferSystem;

    protected override void OnCreate()
    {
        entityCommandBufferSystem = World.GetOrCreateSystem<EndServerSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        var ecb = entityCommandBufferSystem.CreateCommandBuffer();

        Entities
            .WithNone<NpcHasTargetComponent>()
            .WithAll<NpcStartReturnToSpawnComponent, NavAgent>()
            .ForEach((in Entity entity,
                in NpcSpawnPointOccupierComponent npcSpawnPointOccupierComponent) =>
        {
            ecb.AddComponent(entity, new NavNeedsDestination { Destination = npcSpawnPointOccupierComponent.spawnPosition });
            ecb.AddComponent<NpcReturningToSpawnComponent>(entity);

            ecb.RemoveComponent<NpcStartReturnToSpawnComponent>(entity);
        })
        .Schedule();

        Entities
            .WithAll<NpcStartReturnToSpawnComponent, NavAgent>()
            .ForEach((ref NpcHasTargetComponent npcHasTargetComponent,
                in Entity entity,
                in NpcSpawnPointOccupierComponent npcSpawnPointOccupierComponent) =>
        {
            ecb.AddComponent(entity, new NavNeedsDestination { Destination = npcSpawnPointOccupierComponent.spawnPosition });
            ecb.AddComponent<NpcReturningToSpawnComponent>(entity);

            ecb.RemoveComponent<NpcStartReturnToSpawnComponent>(entity);

            npcHasTargetComponent.targetEntity = Entity.Null;
            ecb.RemoveComponent<NpcHasTargetComponent>(entity);
        })
        .Schedule();

        Entities
            .WithAll<NpcReturningToSpawnComponent>()
            .ForEach((Entity entity,
                ref HealthComponent healthComponent,
                in NpcSpawnPointOccupierComponent npcSpawnPointOccupierComponent,
                in Translation translation) =>
        {
            if(healthComponent.currentHealth != healthComponent.maxHealth)
            {
                healthComponent.currentHealth = healthComponent.maxHealth;
            }

            if (NavUtil.ApproxEquals(translation.Value, npcSpawnPointOccupierComponent.spawnPosition, 3))
            {
                ecb.RemoveComponent<NpcReturningToSpawnComponent>(entity);
            }
        })
        .Schedule();

        entityCommandBufferSystem.AddJobHandleForProducer(Dependency);
    }
}
