using DOTSNET;
using System;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

[DisallowMultipleComponent]
public class SetFactionDispatcherServerAuthoring : MonoBehaviour, SelectiveSystemAuthoring
{
    SetFactionDispatcherServer system =>
        Bootstrap.ServerWorld.GetExistingSystem<SetFactionDispatcherServer>();

    public Type GetSystemType() { return typeof(SetFactionDispatcherServer); }

    public float interval = 0.1f;

    void Awake()
    {
        system.interval = interval;
    }
}

[DisableAutoCreation]
public class SetFactionDispatcherServer : NetworkBroadcastSystem
{
    private NativeMultiHashMap<int, SetFactionMessage> messages;
    private NativeList<SetFactionMessage> messagesList;

    private EndServerSimulationEntityCommandBufferSystem entityCommandBufferSystem;

    protected override void OnCreate()
    {
        base.OnCreate();

        messages = new NativeMultiHashMap<int, SetFactionMessage>(20, Allocator.Persistent);
        messagesList = new NativeList<SetFactionMessage>(20, Allocator.Persistent);

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

        NativeMultiHashMap<int, SetFactionMessage> _messages = messages;

        Entities.WithAll<DispatchSetFactionMessageComponent>().ForEach((in DynamicBuffer<NetworkObserver> observers,
                            in FactionComponent factionComponent,
                            in NetworkEntity networkEntity,
                            in Entity entity) =>
        {
            SetFactionMessage message = new SetFactionMessage(networkEntity.netId, factionComponent.factionType);

            for (int i = 0; i < observers.Length; ++i)
            {
                int connectionId = observers[i];

                _messages.Add(connectionId, message);
            }

            ecb.RemoveComponent<DispatchSetFactionMessageComponent>(entity);
        })
        .Run();

        foreach (int connectionId in server.connections.Keys)
        {
            messagesList.Clear();
            NativeMultiHashMapIterator<int>? it = default;
            while (messages.TryIterate(connectionId, out SetFactionMessage message, ref it))
            {
                messagesList.Add(message);
            }

            server.Send(connectionId, messagesList, Channel.Reliable);
        }

        messages.Clear();

        entityCommandBufferSystem.AddJobHandleForProducer(Dependency);
    }
}