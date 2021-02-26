using DOTSNET;
using System;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

[DisallowMultipleComponent]
public class XpGainedServerDispatcherAuthoring : MonoBehaviour, SelectiveSystemAuthoring
{
    XpGainedServerDispatcher system =>
        Bootstrap.ServerWorld.GetExistingSystem<XpGainedServerDispatcher>();

    public Type GetSystemType() { return typeof(XpGainedServerDispatcher); }

    public float interval = 0.1f;

    void Awake()
    {
        system.interval = interval;
    }
}

[DisableAutoCreation]
public class XpGainedServerDispatcher : NetworkBroadcastSystem
{
    private NativeMultiHashMap<int, XpGainedMessage> messages;
    private NativeList<XpGainedMessage> messagesList;
    private EndServerSimulationEntityCommandBufferSystem entityCommandBufferSystem;

    protected override void OnCreate()
    {
        base.OnCreate();

        messages = new NativeMultiHashMap<int, XpGainedMessage>(20, Allocator.Persistent);
        messagesList = new NativeList<XpGainedMessage>(20, Allocator.Persistent);

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
        NativeMultiHashMap<int, XpGainedMessage> _messages = messages;

        var ecb = entityCommandBufferSystem.CreateCommandBuffer();

        Entities.ForEach((in DynamicBuffer<NetworkObserver> observers,
                            in DispatchXpGainedMessageComponent dispatchXpGainedMessageComponent,
                            in NetworkEntity networkEntity,
                            in Entity entity) =>
        {
            if (observers.Length == 0) { return; }
            ecb.RemoveComponent<DispatchXpGainedMessageComponent>(entity);

            XpGainedMessage message = new XpGainedMessage(networkEntity.netId, dispatchXpGainedMessageComponent.amount);

            for (int i = 0; i < observers.Length; ++i)
            {
                int connectionId = observers[i];

                _messages.Add(connectionId, message);
            }
        })
        .Run();

        entityCommandBufferSystem.AddJobHandleForProducer(Dependency);

        foreach (int connectionId in server.connections.Keys)
        {
            messagesList.Clear();
            NativeMultiHashMapIterator<int>? it = default;
            while (messages.TryIterate(connectionId, out XpGainedMessage message, ref it))
            {
                messagesList.Add(message);
            }

            server.Send(connectionId, messagesList, Channel.Reliable);
        }

        messages.Clear();
    }
}
