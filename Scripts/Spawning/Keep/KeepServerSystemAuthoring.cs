using DOTSNET;
using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.AI;

[DisallowMultipleComponent]
public class KeepServerSystemAuthoring : MonoBehaviour, SelectiveSystemAuthoring
{
    public Type GetSystemType() => typeof(KeepServerSystem);

    [Header("Npc Patrol Settings")]
    [SerializeField] private float patrolSpeed = 3.0f;
    [SerializeField] private float patrolMinIdleTime = 5.0f;
    [SerializeField] private float patrolMaxIdleTime = 10.0f;
    [SerializeField] private float patrolMinMoveDistance = 1.0f;
    [SerializeField] private float patrolMaxMoveDistance = 8.0f;
    [SerializeField] private float patrolMaxDistanceFromStartPosition = 15.0f;

    [Header("Npc Roam Freely Settings")]
    [SerializeField] private float roamFreelyMinIdleTime = 5.0f;
    [SerializeField] private float roamFreelyMaxIdleTime = 10.0f;
    [SerializeField] private float roamFreelyMinMoveDistance = 5.0f;
    [SerializeField] private float roamFreelyMaxMoveDistance = 50.0f;

    private void Awake()
    {
        KeepServerSystem keepServerSystem = Bootstrap.ServerWorld.GetExistingSystem<KeepServerSystem>();

        keepServerSystem.patrolSpeed = patrolSpeed;
        keepServerSystem.patrolMinIdleTime = patrolMinIdleTime;
        keepServerSystem.patrolMaxIdleTime = patrolMaxIdleTime;
        keepServerSystem.patrolMinMoveDistance = patrolMinMoveDistance;
        keepServerSystem.patrolMaxMoveDistance = patrolMaxMoveDistance;
        keepServerSystem.patrolMaxDistanceFromStartPosition = patrolMaxDistanceFromStartPosition;

        keepServerSystem.roamFreelyMinIdleTime = roamFreelyMinIdleTime;
        keepServerSystem.roamFreelyMaxIdleTime = roamFreelyMaxIdleTime;
        keepServerSystem.roamFreelyMinMoveDistance = roamFreelyMinMoveDistance;
        keepServerSystem.roamFreelyMaxMoveDistance = roamFreelyMaxMoveDistance;
    } 
}

[ServerWorld]
[UpdateInGroup(typeof(ServerActiveSimulationSystemGroup))]
[DisableAutoCreation]
public class KeepServerSystem : SystemBase
{
    private struct SpawnCommand
    {
        public int npcTypeStorage;
        public int npcTypesStorageLength;
        public float3 spawnPointOrigin;
        public Guid keepId;
        public int spawnPointIndex;
        public FactionType factionType;
        public NpcBehaviourType npcBehaviourType;
        public KeepSpawnPointComponent.KeepSpawnPointType spawnPointType;
        public float areaRadius;
        public float keepMovementBiasFactor;
    }

    public float patrolSpeed = 3.0f;
    public float patrolMinIdleTime = 5.0f;
    public float patrolMaxIdleTime = 10.0f;
    public float patrolMinMoveDistance = 1.0f;
    public float patrolMaxMoveDistance = 8.0f;
    public float patrolMaxDistanceFromStartPosition = 15.0f;

    public float roamFreelyMinIdleTime = 5.0f;
    public float roamFreelyMaxIdleTime = 10.0f;
    public float roamFreelyMinMoveDistance = 5.0f;
    public float roamFreelyMaxMoveDistance = 50.0f;

    [AutoAssign] protected NetworkServerSystem server;
    [AutoAssign] protected PrefabSystem prefabSystem;

    private NativeList<SpawnCommand> spawnCommands;
    private EndServerSimulationEntityCommandBufferSystem entityCommandBufferSystem;

    public void OnDeath(NpcSpawnPointOccupierComponent npcSpawnPointOccupierComponent, FactionType killerFaction)
    {
        bool wasCommander = false;
        Guid keepId;

        Entities.ForEach((ref KeepSpawnPointComponent keepSpawnPointComponent,
            ref SpawnPointComponent spawnPointComponent
            ) => 
        {
            if (keepSpawnPointComponent.spawnPointIndex == npcSpawnPointOccupierComponent.spawnPointIndex 
                && keepSpawnPointComponent.keepId == npcSpawnPointOccupierComponent.keepId)
            {
                keepSpawnPointComponent.occupierCount--;
                spawnPointComponent.isFull = false;

                keepId = keepSpawnPointComponent.keepId;
                wasCommander = keepSpawnPointComponent.npcBehaviourType == NpcBehaviourType.Commander;

                return;
            }
        })
        .Run();

        if(wasCommander)
        {
            Entities.ForEach((ref KeepComponent keepComponent) =>
            {
                if (keepComponent.id == keepId)
                {
                    keepComponent.factionType = killerFaction;

                    return;
                }
            })
            .Run();

            Entities.ForEach((ref SpawnPointComponent spawnPointComponent,
                in KeepSpawnPointComponent keepSpawnPointComponent) =>
                {
                    if (keepSpawnPointComponent.keepId == npcSpawnPointOccupierComponent.keepId)
                    {
                        spawnPointComponent.respawnTimer = 0;
                        spawnPointComponent.readyToRespawn = true;
                    }
                })
            .Run();
        }
    }

    protected override void OnCreate()
    {
        base.OnCreate();

        spawnCommands = new NativeList<SpawnCommand>(Allocator.Persistent);
        entityCommandBufferSystem = World.GetOrCreateSystem<EndServerSimulationEntityCommandBufferSystem>();
    }

    protected override void OnDestroy()
    {
        spawnCommands.Dispose();

        base.OnDestroy();
    }

    protected override void OnUpdate()
    {
        float deltaTime = Time.DeltaTime;

        PrefabSystem _prefabSystem = prefabSystem;
        var entityArray = GetEntityQuery(ComponentType.ReadOnly<KeepComponent>()).ToEntityArray(Allocator.TempJob);
        var keepComponents = GetComponentDataFromEntity<KeepComponent>(true);

        NativeList<SpawnCommand> _spawnCommands = spawnCommands;

        //Check if units need to spawn
        Entities.ForEach((ref KeepSpawnPointComponent keepSpawnPointComponent,
            ref SpawnPointComponent spawnPointComponent,
            in Entity entity,
            in LocalToWorld localToWorld) =>
        {
            if (!spawnPointComponent.readyToRespawn ||
                keepSpawnPointComponent.occupierCount >= keepSpawnPointComponent.maxOccupiers) { return; }

            for (int i = 0; i < entityArray.Length; i++)
            {
                KeepComponent keepComponent = keepComponents[entityArray[i]];

                if (keepComponent.id == keepSpawnPointComponent.keepId)
                {
                    _spawnCommands.Add(new SpawnCommand
                    {
                        npcTypeStorage = keepSpawnPointComponent.npcTypesStorage,
                        npcTypesStorageLength = keepSpawnPointComponent.npcTypesStorageLength,
                        spawnPointOrigin = localToWorld.Position,
                        spawnPointIndex = keepSpawnPointComponent.spawnPointIndex,
                        keepId = keepComponent.id,
                        factionType = keepComponent.factionType,
                        npcBehaviourType = keepSpawnPointComponent.npcBehaviourType,
                        spawnPointType = keepSpawnPointComponent.spawnPointType,
                        areaRadius = keepSpawnPointComponent.areaSize,
                        keepMovementBiasFactor = keepComponent.spawnedNpcKeepMovementBiasFactor
                    });

                    keepSpawnPointComponent.occupierCount++;

                    spawnPointComponent.isFull = keepSpawnPointComponent.occupierCount >= keepSpawnPointComponent.maxOccupiers;

                    spawnPointComponent.ResetTimer(keepComponent.respawnTime / keepSpawnPointComponent.maxOccupiers);
                }
            }
        })
        .WithReadOnly(entityArray)
        .WithReadOnly(keepComponents)
        .Run();

        entityArray.Dispose();

        var ecb = entityCommandBufferSystem.CreateCommandBuffer();

        //Spawn units
        foreach (SpawnCommand spawnCommand in spawnCommands)
        {
            int randomIndex = UnityEngine.Random.Range(0, spawnCommand.npcTypesStorageLength - 1);

            NpcType npcType = (NpcType)BitHelper.Get(spawnCommand.npcTypeStorage, randomIndex);
            NetworkEntityAuthoring npcNetworkEntityPrefab = NpcLibrary.GetInstance().GetNpcPrefab(npcType);

            Bytes16 prefabId = Conversion.GuidToBytes16(npcNetworkEntityPrefab.prefabId);

            if (_prefabSystem.Get(prefabId, out Entity prefab))
            {
                Entity newEntity = EntityManager.Instantiate(prefab);

                server.Spawn(newEntity, null);

                float3 spawnPosition = new float3();

                if (spawnCommand.spawnPointType == KeepSpawnPointComponent.KeepSpawnPointType.Spot)
                {
                    spawnPosition = spawnCommand.spawnPointOrigin;
                }
                else
                {
                    float3 randomPositionInArea = (float3)UnityEngine.Random.insideUnitSphere * spawnCommand.areaRadius + spawnCommand.spawnPointOrigin;

                    if (NavMesh.SamplePosition(randomPositionInArea, out NavMeshHit navHit, int.MaxValue, -1))
                    {
                        spawnPosition = navHit.position + new Vector3(0, 1, 0);
                    }
                    else
                    {
                        spawnPosition = spawnCommand.spawnPointOrigin;
                    }
                }

                EntityManager.SetComponentData(newEntity, new Translation { Value = spawnPosition });
                EntityManager.SetComponentData(newEntity, new FactionComponent { factionType = spawnCommand.factionType });

                ecb.AddComponent(newEntity, new NpcSpawnPointOccupierComponent
                {
                    keepId = spawnCommand.keepId,
                    spawnPointIndex = spawnCommand.spawnPointIndex,
                    spawnPosition = spawnPosition
                });

                if (spawnCommand.npcBehaviourType == NpcBehaviourType.Patrol)
                {
                    ecb.AddComponent(newEntity, new NpcPatrolComponent
                    {
                        patrolSpeed = patrolSpeed,
                        minIdleTime = patrolMinIdleTime,
                        maxIdleTime = patrolMaxIdleTime,
                        minMoveDistance = patrolMinMoveDistance,
                        maxMoveDistance = patrolMaxMoveDistance,
                        maxDistanceFromStartPosition = patrolMaxDistanceFromStartPosition,
                        keepMovementBiasFactor = spawnCommand.keepMovementBiasFactor
                    });

                } 
                else if(spawnCommand.npcBehaviourType == NpcBehaviourType.RoamFreely)
                {
                    ecb.AddComponent(newEntity, new NpcPatrolComponent
                    {
                        patrolSpeed = patrolSpeed,
                        minIdleTime = roamFreelyMinIdleTime,
                        maxIdleTime = roamFreelyMaxIdleTime,
                        minMoveDistance = roamFreelyMinMoveDistance,
                        maxMoveDistance = roamFreelyMaxMoveDistance,
                        keepMovementBiasFactor = spawnCommand.keepMovementBiasFactor
                    });

                    ecb.AddComponent<NpcTargetKeepComponent>(newEntity);
                }
            }
        }
        spawnCommands.Clear();

        entityCommandBufferSystem.AddJobHandleForProducer(Dependency);
    }
}
