using DOTSNET;
using System;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

[DisallowMultipleComponent]
public class HealthDispatcherServerAuthoring : MonoBehaviour, SelectiveSystemAuthoring
{
    HealthDispatcherServer system =>
        Bootstrap.ServerWorld.GetExistingSystem<HealthDispatcherServer>();

    public Type GetSystemType() { return typeof(HealthDispatcherServer); }

    public float interval = 0.1f;

    void Awake()
    {
        system.interval = interval;
    }
}

[DisableAutoCreation]
public class HealthDispatcherServer : NetworkBroadcastSystem
{
    private NativeMultiHashMap<int, HealthMessage> messages;
    private NativeList<HealthMessage> messagesList;

    private EndServerSimulationEntityCommandBufferSystem entityCommandBufferSystem;

    protected override void OnCreate()
    {
        base.OnCreate();

        messages = new NativeMultiHashMap<int, HealthMessage>(1000, Allocator.Persistent);
        messagesList = new NativeList<HealthMessage>(1000, Allocator.Persistent);

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

        NativeMultiHashMap<int, HealthMessage> _messages = messages;
        Entities.WithAll<DispatchHealthMessageComponent>().ForEach((in DynamicBuffer<NetworkObserver> observers,
                            in Entity entity,
                            in HealthComponent healthComponent,
                            in NetworkEntity networkEntity) =>
        {
            HealthMessage message = new HealthMessage(
                networkEntity.netId,
                healthComponent.currentHealth,
                healthComponent.maxHealth
            );

            for (int i = 0; i < observers.Length; ++i)
            {
                int connectionId = observers[i];

                _messages.Add(connectionId, message);
            }

            ecb.RemoveComponent<DispatchHealthMessageComponent>(entity);
        })
        .Run();

        foreach (int connectionId in server.connections.Keys)
        {
            messagesList.Clear();
            NativeMultiHashMapIterator<int>? it = default;
            while (messages.TryIterate(connectionId, out HealthMessage message, ref it))
            {
                messagesList.Add(message);
            }

            server.Send(connectionId, messagesList, Channel.Reliable);
        }

        messages.Clear();

        entityCommandBufferSystem.AddJobHandleForProducer(Dependency);
    }
}
