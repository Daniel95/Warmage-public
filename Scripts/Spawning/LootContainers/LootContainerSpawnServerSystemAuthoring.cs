using DOTSNET;
using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[DisallowMultipleComponent]
public class LootContainerSpawnServerSystemAuthoring : MonoBehaviour, SelectiveSystemAuthoring
{
    private LootContainerSpawnServerSystem system => 
        Bootstrap.ServerWorld.GetExistingSystem<LootContainerSpawnServerSystem>();

    [SerializeField] private float respawnTime = 60;
    [SerializeField] private NetworkEntityAuthoring spawnPrefab = null;

    public Type GetSystemType() => typeof(LootContainerSpawnServerSystem);


    private void Awake()
    {
        system.respawnTime = respawnTime;
        system.spawnPrefabId = Conversion.GuidToBytes16(spawnPrefab.prefabId);
    }
}

[ServerWorld]
[UpdateInGroup(typeof(ServerActiveSimulationSystemGroup))]
[DisableAutoCreation]
public class LootContainerSpawnServerSystem : SystemBase
{
    [AutoAssign] protected NetworkServerSystem server;
    [AutoAssign] protected PrefabSystem prefabSystem;

    public Bytes16 spawnPrefabId;
    public float respawnTime;

    private EndServerSimulationEntityCommandBufferSystem entityCommandBufferSystem;
    private NativeList<SpawnCommand> spawnCommands;

    public void OnPickup(int spawnPointIndex)
    {
        Entities.ForEach((ref SpawnPointComponent spawnPointComponent,
            in LootContainerSpawnPointComponent lootContainerSpawnPointComponent) =>
        {
            if(lootContainerSpawnPointComponent.spawnPointIndex == spawnPointIndex)
            {
                spawnPointComponent.isFull = false;
            }
        }).Run();
    }

    protected override void OnCreate()
    {
        base.OnCreate();

        entityCommandBufferSystem = World.GetOrCreateSystem<EndServerSimulationEntityCommandBufferSystem>();
        spawnCommands = new NativeList<SpawnCommand>(Allocator.Persistent);
    }

    protected override void OnDestroy()
    {
        spawnCommands.Dispose();

        base.OnDestroy();
    }

    protected override void OnUpdate()
    {
        if (!prefabSystem.Get(spawnPrefabId, out Entity prefab)) { Debug.Assert(true, "Prefab does not exists!"); }

        var ecb = entityCommandBufferSystem.CreateCommandBuffer();
        NativeList<SpawnCommand> _spawnCommands = spawnCommands;
        float _respawnTime = respawnTime;

        //Check if units need to spawn
        Entities.ForEach((ref SpawnPointComponent spawnPointComponent,
            in LootContainerSpawnPointComponent lootContainerSpawnPointComponent,
            in Entity entity,
            in LocalToWorld localToWorld) =>
        {
            if (spawnPointComponent.readyToRespawn)
            {
                _spawnCommands.Add(new SpawnCommand 
                {
                    position = localToWorld.Position,
                    spawnPointIndex = lootContainerSpawnPointComponent.spawnPointIndex
                });

                spawnPointComponent.ResetTimer(_respawnTime);
                spawnPointComponent.isFull = true;
            }
        })
        .Run();

        foreach (SpawnCommand spawnCommand in spawnCommands)
        {
            Entity spawnedEntity = EntityManager.Instantiate(prefab);

            EntityManager.SetComponentData(spawnedEntity, new Translation { Value = spawnCommand.position });

            LootContainerComponent lootContainerComponent = EntityManager.GetComponentData<LootContainerComponent>(spawnedEntity);
            lootContainerComponent.spawnPointIndex = spawnCommand.spawnPointIndex;
            ecb.SetComponent(spawnedEntity, lootContainerComponent);

            ecb.AddComponent(spawnedEntity, new Translation { Value = spawnCommand.position });

            server.Spawn(spawnedEntity, null);
        }

        spawnCommands.Clear();

        entityCommandBufferSystem.AddJobHandleForProducer(Dependency);
    }

    private struct SpawnCommand
    {
        public float3 position;
        public int spawnPointIndex;
    }
}
