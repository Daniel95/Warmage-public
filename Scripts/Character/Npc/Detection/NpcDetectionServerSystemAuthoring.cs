using DOTSNET;
using Reese.Nav;
using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Authoring;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;
using RaycastHit = Unity.Physics.RaycastHit;

[DisallowMultipleComponent]
public class NpcDetectionServerSystemAuthoring : MonoBehaviour, SelectiveSystemAuthoring
{
    public Type GetSystemType() => typeof(NpcDetectionServerSystem);

    [SerializeField] private float detectionCheckInterval = 0.5f;
    [SerializeField] private PhysicsCategoryTags detectionPhysicsCategoryTags = PhysicsCategoryTags.Nothing;

    private void Start()
    {
        NpcDetectionServerSystem system = Bootstrap.ServerWorld.GetExistingSystem<NpcDetectionServerSystem>();

        system.detectionCheckInterval = detectionCheckInterval;
        system.detectionPhysicsCategoryTags = detectionPhysicsCategoryTags;
    }
}

[ServerWorld]
[UpdateInGroup(typeof(ServerActiveSimulationSystemGroup))]
[UpdateAfter(typeof(NpcDeathServerSystem))]
[UpdateAfter(typeof(PlayerDeathServerSystem))]
[DisableAutoCreation]
public class NpcDetectionServerSystem : SystemBase
{
    public PhysicsCategoryTags detectionPhysicsCategoryTags;
    public float detectionCheckInterval;

    private float detectionCheckTimer = 0;

    private EndServerSimulationEntityCommandBufferSystem endServerSimulationEntityCommandBufferSystem;

    protected override void OnCreate()
    {
        base.OnCreate();

        endServerSimulationEntityCommandBufferSystem = World.GetExistingSystem<EndServerSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        detectionCheckTimer += Time.DeltaTime;

        if (detectionCheckTimer > detectionCheckInterval)
        {
            detectionCheckTimer -= detectionCheckInterval;
            CheckDetection();
        }
    }

    private void CheckDetection()
    {
        PhysicsWorld physicsWorld = Bootstrap.ServerWorld.GetExistingSystem<BuildPhysicsWorld>().PhysicsWorld;

        var ecb = endServerSimulationEntityCommandBufferSystem.CreateCommandBuffer();

        Dependency = JobHandle.CombineDependencies(Dependency, World.GetExistingSystem<BuildPhysicsWorld>().GetOutputDependency());
        PhysicsCategoryTags _detectionPhysicsCategoryTags = detectionPhysicsCategoryTags;

        var targetInRangeTranslationComponents = GetComponentDataFromEntity<Translation>(true);

        Entities.ForEach((Entity entity, ref NpcHasTargetComponent npcHasTargetComponent) =>
        {
            if (!HasComponent<Translation>(npcHasTargetComponent.targetEntity) || HasComponent<DeathComponent>(npcHasTargetComponent.targetEntity))
            {
                //    //Debug.LogError("Target does not exists, target: " + npcDetectionComponent.targetEntity + ", this entity: " + entity);
                ecb.RemoveComponent<NpcHasTargetComponent>(entity);

                Debug.LogWarning("remove has target error");

                npcHasTargetComponent.targetEntity = Entity.Null;
                return;
            }
        }).Run(); 

        Entities
            .WithName("Check_target_in_range_and_view")
            .WithReadOnly(physicsWorld)
            .WithReadOnly(targetInRangeTranslationComponents)
            .WithDisposeOnCompletion(targetInRangeTranslationComponents)
            .ForEach((ref NpcHasTargetComponent npcHasTargetComponent,
                in NpcDetectionComponent npcDetectionComponent,
                in Translation translation,
                in Entity entity) =>
        {
            if(!npcHasTargetComponent.targetIsNotNull) { return; }

            npcHasTargetComponent.targetPosition = targetInRangeTranslationComponents[npcHasTargetComponent.targetEntity].Value;

            npcHasTargetComponent.distanceToTarget = math.distance(translation.Value, npcHasTargetComponent.targetPosition);

            if (npcHasTargetComponent.distanceToTarget > npcDetectionComponent.detectionRange) 
            {
                npcHasTargetComponent.inView = false;

                return;
            }

            CollisionFilter collisionFilter = new CollisionFilter()
            {
                BelongsTo = unchecked((uint)~0),
                CollidesWith = _detectionPhysicsCategoryTags.Value
            };

            RaycastInput canSeeTargetInput = new RaycastInput
            {
                Start = translation.Value,
                End = npcHasTargetComponent.targetPosition,
                Filter = collisionFilter
            };

            if (physicsWorld.CastRay(canSeeTargetInput))
            {
                npcHasTargetComponent.inView = false;
            } 
            else
            {
                npcHasTargetComponent.inView = true;
            }
        })
        .Run();

        var queryDesc = new EntityQueryDesc
        {
            None = new ComponentType[] { typeof(DeathComponent) },
            All = new ComponentType[] { ComponentType.ReadOnly<FactionComponent>(), ComponentType.ReadOnly<Translation>() }
        };
        var entityArray = GetEntityQuery(queryDesc).ToEntityArray(Allocator.TempJob);

        //var entityArray = GetEntityQuery(ComponentType.ReadOnly<FactionComponent>(), ComponentType.ReadOnly<Translation>(), ComponentType.rea).ToEntityArray(Allocator.TempJob);
        var factionComponents = GetComponentDataFromEntity<FactionComponent>(true);
        var findTargetTranslationsComponents = GetComponentDataFromEntity<Translation>(true);

        var ecbParallel = endServerSimulationEntityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter();

        //Check BruteForceInterest how to find closest in a more optimized way
        //Loop through all npcs
        Entities
            .WithName("Find_target")
            .WithReadOnly(physicsWorld)
            .WithReadOnly(entityArray)
            .WithReadOnly(factionComponents)
            .WithReadOnly(findTargetTranslationsComponents)
            .WithDisposeOnCompletion(entityArray)
            .WithNone<NpcHasTargetComponent>()
            .ForEach((Entity entity, int entityInQueryIndex, ref NpcDetectionComponent npcDetectionComponent,
                in Entity npcEntity,
                in FactionComponent npcFactionComponent,
                in Translation ncpTranslation) =>
        {
            Entity target = Entity.Null;
            float3 npcPosition = ncpTranslation.Value;
            FactionType npcFaction = npcFactionComponent.factionType;
            float npcRange = npcDetectionComponent.detectionRange;

            for (int i = 0; i < entityArray.Length; i++)
            {
                FactionType factionType = factionComponents[entityArray[i]].factionType;
                float3 targetPosition = findTargetTranslationsComponents[entityArray[i]].Value;

                if (npcFaction != factionType &&
                    NavUtil.ApproxEquals(npcPosition, targetPosition, npcRange))
                {
                    CollisionFilter collisionFilter = new CollisionFilter()
                    {
                        BelongsTo = unchecked((uint)~0),
                        CollidesWith = _detectionPhysicsCategoryTags.Value
                    };

                    RaycastInput raycastInput = new RaycastInput
                    {
                        Start = npcPosition,
                        End = targetPosition,
                        Filter = collisionFilter
                    };

                    bool hitSomething = physicsWorld.CastRay(raycastInput, out RaycastHit hit);

                    if (!hitSomething)
                    {
                        target = entityArray[i];
                        break;
                    }
                }
            }

            if (target != Entity.Null)
            {
                ecbParallel.AddComponent(entityInQueryIndex, entity, new NpcHasTargetComponent { targetEntity = target });
            }
        })
        .ScheduleParallel();

        endServerSimulationEntityCommandBufferSystem.AddJobHandleForProducer(Dependency);
    }
}
