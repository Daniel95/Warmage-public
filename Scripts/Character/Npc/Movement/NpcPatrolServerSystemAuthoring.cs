using DOTSNET;
using Reese.Nav;
using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.AI;

[DisallowMultipleComponent]
public class NpcPatrolServerSystemAuthoring : MonoBehaviour, SelectiveSystemAuthoring
{
    public Type GetSystemType() => typeof(NpcPatrolServerSystem);
}

[ServerWorld]
[UpdateInGroup(typeof(ServerActiveSimulationSystemGroup))]
[DisableAutoCreation]
public class NpcPatrolServerSystem : SystemBase
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

        // new random for each update
        // (time+1 because seed must be non-zero to avoid exceptions)
        //uint seed = 1 + (uint)Time.ElapsedTime;
        uint seed = 1 + (uint)UnityEngine.Random.Range(int.MinValue, int.MaxValue - 1);
        Unity.Mathematics.Random random = new Unity.Mathematics.Random(seed);

        float deltaTime = Time.DeltaTime;

        /*
        float3 randomPositionWithinBounds = WorldBounds.GetRandomPositionWithinBounds(min, max, random);
        randomPositionWithinBounds.y = worldPosition.y;

        float3 randomDirection = math.normalize(randomPositionWithinBounds - worldPosition);
            */
        //float3 min = WorldBounds.GetInstance().min;
        //float3 max = WorldBounds.GetInstance().max;

        Entities
            .WithNone<NavNeedsDestination, NavNeedsSurface, NavLerping>()
            .WithNone<NpcTargetKeepComponent, NpcHasTargetComponent>()
            .ForEach((
                Entity entity,
                ref NpcPatrolComponent npcPatrolComponent,
                ref NavAgent navAgent,
                in NpcSpawnPointOccupierComponent npcSpawnPointOccupierComponent,
                in Translation translation) =>
        {
            npcPatrolComponent.idleTimer -= deltaTime;

            if (npcPatrolComponent.idleTimer < 0)
            {
                float2 circle2D = random.NextFloat2Direction();
                float3 randomDirection = new float3(circle2D.x, 0, circle2D.y);

                float distance = random.NextFloat(npcPatrolComponent.minMoveDistance, npcPatrolComponent.maxMoveDistance);
                float3 destination = translation.Value + randomDirection * random.NextFloat(npcPatrolComponent.minMoveDistance, npcPatrolComponent.maxMoveDistance);

                // only go there if it's within a circle around start
                // so we don't wander off into nirvana
                if (math.distance(npcSpawnPointOccupierComponent.spawnPosition, destination) > npcPatrolComponent.maxDistanceFromStartPosition)
                {
                    destination = npcSpawnPointOccupierComponent.spawnPosition + randomDirection * npcPatrolComponent.maxDistanceFromStartPosition;
                }

                if(NavMesh.SamplePosition(destination, out NavMeshHit navHit, int.MaxValue, -1))
                {
                    npcPatrolComponent.idleTimer = random.NextFloat(npcPatrolComponent.minIdleTime, npcPatrolComponent.maxIdleTime);

                    navAgent.TranslationSpeed = npcPatrolComponent.patrolSpeed;

                    ecb.AddComponent(entity, new NavNeedsDestination
                    {
                        Destination = navHit.position
                    });
                }
            }
        })
        .Run();

        Entities
            .WithNone<NavNeedsDestination, NavNeedsSurface, NavLerping>()
            .WithNone<NpcHasTargetComponent>()
            .ForEach((
                ref NpcPatrolComponent npcPatrolComponent,
                ref NavAgent navAgent,
                in NpcTargetKeepComponent npcTargetKeepComponent,
                in NpcCombatComponent npcCombatComponent,
                in Entity entity,
                in Translation translation) =>
        {
            npcPatrolComponent.idleTimer -= deltaTime;

            if (npcPatrolComponent.idleTimer < 0 && npcTargetKeepComponent.hasTarget)
            {
                float3 worldPosition = translation.Value;

                float2 circle2D = random.NextFloat2Direction();
                float3 randomDirectionTest = new float3(circle2D.x, 0, circle2D.y);

                float3 directionToClosestKeep = math.normalize(npcTargetKeepComponent.targetKeepPosition - worldPosition);

                float3 direction = math.lerp(randomDirectionTest, directionToClosestKeep, npcPatrolComponent.keepMovementBiasFactor);

                float distance = random.NextFloat(npcPatrolComponent.minMoveDistance, npcPatrolComponent.maxMoveDistance);
                float3 destination = worldPosition + direction * random.NextFloat(npcPatrolComponent.minMoveDistance, npcPatrolComponent.maxMoveDistance);

                if (NavMesh.SamplePosition(destination, out NavMeshHit navHit, int.MaxValue, -1))
                {
                    npcPatrolComponent.idleTimer = random.NextFloat(npcPatrolComponent.minIdleTime, npcPatrolComponent.maxIdleTime);
                    navAgent.TranslationSpeed = npcCombatComponent.outOfCombatSpeed;

                    ecb.AddComponent(entity, new NavNeedsDestination
                    {
                        Destination = navHit.position
                    });
                }
            }
        })
        .Run();

        entityCommandBufferSystem.AddJobHandleForProducer(Dependency);
    }
}
