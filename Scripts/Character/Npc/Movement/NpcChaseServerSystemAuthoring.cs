using DOTSNET;
using Reese.Nav;
using System;
using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Physics.Authoring;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;

[DisallowMultipleComponent]
public class NpcChaseServerSystemAuthoring : MonoBehaviour, SelectiveSystemAuthoring
{
    [SerializeField] private PhysicsCategoryTags physicsCategoryTags = PhysicsCategoryTags.Everything;

    public Type GetSystemType() => typeof(NpcChaseServerSystem);

    private void Start()
    {
        NpcChaseServerSystem system = Bootstrap.ServerWorld.GetExistingSystem<NpcChaseServerSystem>();

        system.physicsCategoryTags = physicsCategoryTags;
    }
}

[ServerWorld]
[UpdateInGroup(typeof(ServerActiveSimulationSystemGroup))]
[UpdateAfter(typeof(PlayerDeathServerSystem))]
[DisableAutoCreation]
public class NpcChaseServerSystem : SystemBase
{
    public PhysicsCategoryTags physicsCategoryTags;

    private EndServerSimulationEntityCommandBufferSystem entityCommandBufferSystem;

    protected override void OnCreate()
    {
        entityCommandBufferSystem = World.GetOrCreateSystem<EndServerSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        var ecb = entityCommandBufferSystem.CreateCommandBuffer();

        PhysicsWorld physicsWorld = Bootstrap.ClientWorld.GetExistingSystem<BuildPhysicsWorld>().PhysicsWorld;

        CollisionFilter collisionFilter = new CollisionFilter()
        {
            BelongsTo = unchecked((uint)~0),
            CollidesWith = physicsCategoryTags.Value
        };

        float deltaTime = Time.DeltaTime;

        Entities
            .WithNone<NpcReturningToSpawnComponent, NpcStartReturnToSpawnComponent, NpcTargetKeepComponent>()
            .ForEach((ref NpcCombatComponent npcCombatComponent,
                ref NavAgent navAgent,
                in Entity entity,
                in Translation translation,
                in NpcSpawnPointOccupierComponent npcSpawnPointOccupierComponent,
                in NpcHasTargetComponent npcHasTargetComponent) =>
        {
            if (!npcHasTargetComponent.targetIsNotNull) { return; }

            npcCombatComponent.pathfindTimer -= deltaTime;

            if (npcCombatComponent.pathfindTimer > 0) { return; }

            npcCombatComponent.pathfindTimer += npcCombatComponent.pathfindInterval;

            if (npcCombatComponent.inAttackRange)
            {
                if (npcCombatComponent.isChasing)
                {
                    npcCombatComponent.isChasing = false;

                    ecb.AddComponent(entity, new NavNeedsDestination
                    {
                        Destination = translation.Value
                    });
                }
            }
            else
            {
                if (NavUtil.ApproxEquals(npcSpawnPointOccupierComponent.spawnPosition, npcHasTargetComponent.targetPosition, npcCombatComponent.maxDistanceFromSpawn))
                {
                    npcCombatComponent.isChasing = true;

                    navAgent.TranslationSpeed = npcCombatComponent.combatSpeed;

                    ecb.AddComponent(entity, new NavNeedsDestination
                    {
                        Destination = npcHasTargetComponent.targetPosition
                    });
                } 
                else
                {
                    npcCombatComponent.isChasing = false;

                    navAgent.TranslationSpeed = npcCombatComponent.returnSpeed;

                    ecb.AddComponent<NpcStartReturnToSpawnComponent>(entity);
                    ecb.RemoveComponent<NpcHasTargetComponent>(entity);
                }
            }
        })
        .Schedule();

        Entities
            .WithNone<NpcReturningToSpawnComponent, NpcStartReturnToSpawnComponent>()
            .ForEach((ref NpcCombatComponent npcCombatComponent,
                ref NavAgent navAgent,
                in Entity entity,
                in Translation translation,
                in NpcHasTargetComponent npcHasTargetComponent) =>
        {
            if (!npcHasTargetComponent.targetIsNotNull) { return; }

            npcCombatComponent.pathfindTimer -= deltaTime;

            if (npcCombatComponent.pathfindTimer > 0) { return; }

            npcCombatComponent.pathfindTimer += npcCombatComponent.pathfindInterval;

            if (npcCombatComponent.inAttackRange)
            {
                if (npcCombatComponent.isChasing)
                {
                    npcCombatComponent.isChasing = false;

                    ecb.AddComponent(entity, new NavNeedsDestination
                    {
                        Destination = translation.Value
                    });
                }
            }
            else
            {
                npcCombatComponent.isChasing = true;

                navAgent.TranslationSpeed = npcCombatComponent.combatSpeed;

                ecb.AddComponent(entity, new NavNeedsDestination
                {
                    Destination = npcHasTargetComponent.targetPosition
                });
            }
        })
        .Schedule();

        entityCommandBufferSystem.AddJobHandleForProducer(Dependency);
    }
}
