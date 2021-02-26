using DOTSNET;
using System;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

[DisallowMultipleComponent]
public class ChannelBarDispatcherServerAuthoring : MonoBehaviour, SelectiveSystemAuthoring
{
    ChannelBarDispatcherServer system =>
        Bootstrap.ServerWorld.GetExistingSystem<ChannelBarDispatcherServer>();

    public Type GetSystemType() { return typeof(ChannelBarDispatcherServer); }

    public float interval = 0.1f;

    void Awake()
    {
        system.interval = interval;
    }
}

[DisableAutoCreation]
public class ChannelBarDispatcherServer : NetworkBroadcastSystem
{
    private NativeMultiHashMap<int, ChannelBarMessage> messages;
    private NativeList<ChannelBarMessage> messagesList;

    private EndServerSimulationEntityCommandBufferSystem entityCommandBufferSystem;

    protected override void OnCreate()
    {
        base.OnCreate();

        messages = new NativeMultiHashMap<int, ChannelBarMessage>(1000, Allocator.Persistent);
        messagesList = new NativeList<ChannelBarMessage>(1000, Allocator.Persistent);

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

        NativeMultiHashMap<int, ChannelBarMessage> _messages = messages;
        Entities.ForEach((in DynamicBuffer<NetworkObserver> observers,
                            in DispatchChannelBarMessageComponent dispatchChannelBarMessageComponent,
                            in Entity entity,
                            in NetworkEntity networkEntity) =>
        {
            ChannelBarMessage message = new ChannelBarMessage(
                networkEntity.netId,
                dispatchChannelBarMessageComponent.messageType,
                dispatchChannelBarMessageComponent.time
            );

            for (int i = 0; i < observers.Length; ++i)
            {
                int connectionId = observers[i];

                _messages.Add(connectionId, message);
            }

            ecb.RemoveComponent<DispatchChannelBarMessageComponent>(entity);
        })
        .Run();

        foreach (int connectionId in server.connections.Keys)
        {
            messagesList.Clear();
            NativeMultiHashMapIterator<int>? it = default;
            while (messages.TryIterate(connectionId, out ChannelBarMessage message, ref it))
            {
                messagesList.Add(message);
            }

            server.Send(connectionId, messagesList, Channel.Reliable);
        }

        messages.Clear();

        entityCommandBufferSystem.AddJobHandleForProducer(Dependency);
    }
}
