using DOTSNET;
using System;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

[DisallowMultipleComponent]
public class StatsDispatcherServerAuthoring : MonoBehaviour, SelectiveSystemAuthoring
{
    StatDispatcherServer system =>
        Bootstrap.ServerWorld.GetExistingSystem<StatDispatcherServer>();

    public Type GetSystemType() { return typeof(StatDispatcherServer); }

    public float interval = 0.1f;

    void Awake()
    {
        system.interval = interval;
    }
}

[DisableAutoCreation]
public class StatDispatcherServer : NetworkBroadcastSystem
{
    private NativeMultiHashMap<int, StatsMessage> messages;
    private NativeList<StatsMessage> messagesList;

    private EndServerSimulationEntityCommandBufferSystem entityCommandBufferSystem;

    protected override void OnCreate()
    {
        base.OnCreate();

        messages = new NativeMultiHashMap<int, StatsMessage>(1000, Allocator.Persistent);
        messagesList = new NativeList<StatsMessage>(1000, Allocator.Persistent);

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

        NativeMultiHashMap<int, StatsMessage> _messages = messages;
        Entities.WithAll<DispatchStatsMessageComponent>().ForEach((in DynamicBuffer<NetworkObserver> observers,
                            in Entity entity,
                            in StatsComponent statsComponent,
                            in NetworkEntity networkEntity) =>
        {
            StatsMessage message = new StatsMessage(
                networkEntity.netId,
                statsComponent.speedFactor
            );

            for (int i = 0; i < observers.Length; ++i)
            {
                int connectionId = observers[i];

                _messages.Add(connectionId, message);
            }

            ecb.RemoveComponent<DispatchStatsMessageComponent>(entity);
        })
        .Run();

        foreach (int connectionId in server.connections.Keys)
        {
            messagesList.Clear();
            NativeMultiHashMapIterator<int>? it = default;
            while (messages.TryIterate(connectionId, out StatsMessage message, ref it))
            {
                messagesList.Add(message);
            }

            server.Send(connectionId, messagesList, Channel.Reliable);
        }

        messages.Clear();

        entityCommandBufferSystem.AddJobHandleForProducer(Dependency);
    }
}
