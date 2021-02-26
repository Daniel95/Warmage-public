using DOTSNET;
using System;
using Unity.Entities;
using UnityEngine;

[DisallowMultipleComponent]
public class LootContainerOpenServerReceiverAuthoring : MonoBehaviour, SelectiveSystemAuthoring
{
    public Type GetSystemType() { return typeof(LootContainerOpenServerReceiver); }

    [SerializeField] private float openChannelTime = 1f;

    private void Awake()
    {
        LootContainerOpenServerReceiver system = Bootstrap.ServerWorld.GetExistingSystem<LootContainerOpenServerReceiver>();
        system.openChannelTime = openChannelTime;
    }
}

[DisableAutoCreation]
public class LootContainerOpenServerReceiver : NetworkServerMessageSystem<LootContainerOpenMessage>
{
    public float openChannelTime;

    [AutoAssign] private LootContainerSpawnServerSystem lootContainerSpawnServerSystem = null;

    private EndServerSimulationEntityCommandBufferSystem entityCommandBufferSystem;

    protected override void OnCreate()
    {
        base.OnCreate();
        entityCommandBufferSystem = World.GetExistingSystem<EndServerSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate() { }
    protected override bool RequiresAuthentication() { return true; }
    protected override void OnMessage(int connectionId, LootContainerOpenMessage message)
    {
        if (!server.spawned.TryGetValue(message.netId, out Entity entity)) { return; }
        if (!server.spawned.TryGetValue(message.lootContainerNetId, out Entity temp)) { return; }

        var ecb = entityCommandBufferSystem.CreateCommandBuffer();

        ecb.AddComponent(entity, new StartChannelingComponent
        {
            time = openChannelTime,
            onCompleteEvent = () =>
            {
                if (server.spawned.TryGetValue(message.lootContainerNetId, out Entity lootContainerEntity))
                {
                    var endServerEcb = World.GetExistingSystem<EndServerSimulationEntityCommandBufferSystem>().CreateCommandBuffer();

                    LootContainerComponent lootContainerComponent = EntityManager.GetComponentData<LootContainerComponent>(lootContainerEntity);

                    SpellHelper.AddStatusEffect(lootContainerComponent.statusEffectId, entity, entity, message.netId, FactionType.Yellow, EntityManager);

                    lootContainerSpawnServerSystem.OnPickup(lootContainerComponent.spawnPointIndex);

                    NetworkEntityHelper.Destroy(lootContainerEntity, endServerEcb, server);
                }
            }
        });

        entityCommandBufferSystem.AddJobHandleForProducer(Dependency);
    }
}