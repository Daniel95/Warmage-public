using DOTSNET;
using System;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerDeathServerDispatcherAuthoring : MonoBehaviour, SelectiveSystemAuthoring
{
    SetPositionServerDispatcher system =>
        Bootstrap.ServerWorld.GetExistingSystem<SetPositionServerDispatcher>();

    public Type GetSystemType() { return typeof(PlayerDeathServerDispatcher); }

    public float interval = 0.1f;

    void Awake()
    {
        system.interval = interval;
    }
}

[DisableAutoCreation]
public class PlayerDeathServerDispatcher : NetworkBroadcastSystem
{
    NativeMultiHashMap<int, PlayerDeathMessage> messages;
    NativeList<PlayerDeathMessage> messagesList;

    private EndServerSimulationEntityCommandBufferSystem entityCommandBufferSystem;

    protected override void OnCreate()
    {
        base.OnCreate();

        messages = new NativeMultiHashMap<int, PlayerDeathMessage>(10, Allocator.Persistent);
        messagesList = new NativeList<PlayerDeathMessage>(10, Allocator.Persistent);
        entityCommandBufferSystem = World.GetOrCreateSystem<EndServerSimulationEntityCommandBufferSystem>();
    }

    protected override void OnDestroy()
    {
        messagesList.Dispose();
        messages.Dispose();

        base.OnDestroy();
    }

    protected override void Broadcast()
    {
        var ecb = entityCommandBufferSystem.CreateCommandBuffer();

        NativeMultiHashMap<int, PlayerDeathMessage> _messages = messages;
        Entities.WithAll<DispatchPlayerDeathMessageComponent>().ForEach((in DynamicBuffer<NetworkObserver> observers,
                            in Entity entity,
                            in NetworkEntity networkEntity) =>
        {
            PlayerDeathMessage message = new PlayerDeathMessage(
                networkEntity.netId
            );

            for (int i = 0; i < observers.Length; ++i)
            {
                int connectionId = observers[i];

                _messages.Add(connectionId, message);
            }

            ecb.RemoveComponent<DispatchPlayerDeathMessageComponent>(entity);

            ecb.AddComponent<DispatchStatsMessageComponent>(entity);
            ecb.AddComponent<DispatchHealthMessageComponent>(entity);
        })
        .Run();

        entityCommandBufferSystem.AddJobHandleForProducer(Dependency);

        foreach (int connectionId in server.connections.Keys)
        {
            messagesList.Clear();
            NativeMultiHashMapIterator<int>? it = default;
            while (messages.TryIterate(connectionId, out PlayerDeathMessage message, ref it))
            {
                messagesList.Add(message);
            }

            server.Send(connectionId, messagesList, Channel.Reliable);
        }

        messages.Clear();
    }
}
