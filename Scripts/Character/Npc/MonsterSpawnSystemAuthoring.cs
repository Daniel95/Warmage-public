using DOTSNET;
using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[DisallowMultipleComponent]
public class MonsterSpawnSystemAuthoring : MonoBehaviour, SelectiveSystemAuthoring
{
    // spawn prefab
    public NetworkEntityAuthoring spawnPrefab;

    [SerializeField] private Transform spawnPosition = null;

    // the system
    MonsterSpawnSystem system =>
        Bootstrap.ServerWorld.GetExistingSystem<MonsterSpawnSystem>();

    // add system if Authoring is used
    public Type GetSystemType() => typeof(MonsterSpawnSystem);

    // configuration
    public int spawnAmount = 10000;
    public float interleave = 1;

    // apply configuration
    void Awake()
    {
        system.spawnPrefabId = Conversion.GuidToBytes16(spawnPrefab.prefabId);
        system.spawnAmount = spawnAmount;
        system.interleave = interleave;
        system.spawnPosition = spawnPosition.position;
    }
}

// use SelectiveSystemAuthoring to create it selectively
[DisableAutoCreation]
[ServerWorld]
[UpdateInGroup(typeof(ServerActiveSimulationSystemGroup))]
public class MonsterSpawnSystem : SystemBase
{
    // dependencies
    [AutoAssign] protected NetworkServerSystem server = null;
    [AutoAssign] protected PrefabSystem prefabs = null;

    public Bytes16 spawnPrefabId;
    public int spawnAmount;
    public float interleave;
    public float3 spawnPosition;

    public void SpawnAll()
    {
        // get the ECS prefab
        if (prefabs.Get(spawnPrefabId, out Entity prefab))
        {
            // calculate sqrt so we can spawn N * N = Amount
            float sqrt = math.sqrt(spawnAmount);

            // calculate spawn xz start positions
            // based on spawnAmount * distance
            float offset = -sqrt / 2 * interleave;

            // spawn exactly the amount, not one more.
            int spawned = 0;
            for (int spawnX = 0; spawnX < sqrt; ++spawnX)
            {
                for (int spawnZ = 0; spawnZ < sqrt; ++spawnZ)
                {
                    // spawn exactly the amount, not any more
                    // (our sqrt method isn't 100% precise)
                    if (spawned < spawnAmount)
                    {
                        Entity entity = EntityManager.Instantiate(prefab);
                        float x = offset + spawnX * interleave;
                        float z = offset + spawnZ * interleave;
                        float3 position = new float3(x, 0, z) + spawnPosition;
                        SetComponent(entity, new Translation{Value = position});

                        // spawn it on all clients, owned by no one
                        server.Spawn(entity, null);

                        ++spawned;
                    }
                }
            }
        }
        else Debug.LogError("Failed to find Spawn prefab. Was it added to the PrefabSystem's spawnable prefabs list?");
    }

    protected override void OnStartRunning()
    {
        // spawn when the server starts
        SpawnAll();
    }

    protected override void OnUpdate() {}
}
