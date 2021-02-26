using DOTSNET;
using System;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

[DisallowMultipleComponent]
public class FXServerDispatcherAuthoring : MonoBehaviour, SelectiveSystemAuthoring
{
    FXServerDispatcher system =>
        Bootstrap.ServerWorld.GetExistingSystem<FXServerDispatcher>();

    public Type GetSystemType() { return typeof(FXServerDispatcher); }

    public float interval = 0.1f;

    void Awake()
    {
        system.interval = interval;
    }
}

[DisableAutoCreation]
public class FXServerDispatcher : NetworkBroadcastSystem
{
    private NativeMultiHashMap<int, FXMessage> messages;
    private NativeList<FXMessage> messagesList;
    private EndServerSimulationEntityCommandBufferSystem entityCommandBufferSystem;

    protected override void OnCreate()
    {
        base.OnCreate();

        messages = new NativeMultiHashMap<int, FXMessage>(20, Allocator.Persistent);
        messagesList = new NativeList<FXMessage>(20, Allocator.Persistent);

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
        NativeMultiHashMap<int, FXMessage> _messages = messages;
        GameNetworkServerSystem gameServer = (GameNetworkServerSystem)server;

        var ecb = entityCommandBufferSystem.CreateCommandBuffer();

        Entities.ForEach((in DynamicBuffer<NetworkObserver> observers,
                            in FXEntityAddOnClientComponent fxAddOnClientComponent,
                            in NetworkEntity networkEntity,
                            in Entity entity) =>
        {
            if(observers.Length == 0) { return; }
            ecb.RemoveComponent<FXEntityAddOnClientComponent>(entity);

            FXMessage message = new FXMessage(networkEntity.netId, FXMessage.AddOrRemoveType.Add, fxAddOnClientComponent.fxPoolIndex);

            for (int i = 0; i < observers.Length; ++i)
            {
                int connectionId = observers[i];

                _messages.Add(connectionId, message);
            }
        })
        .Run();

        Entities.ForEach((in DynamicBuffer<NetworkObserver> observers,
                    in FXEntityRemoveOnClientComponent fxRemoveOnClientComponent,
                    in NetworkEntity networkEntity,
                    in Entity entity) =>
        {
            if (observers.Length == 0) { return; }
            ecb.RemoveComponent<FXEntityRemoveOnClientComponent>(entity);

            FXMessage message = new FXMessage(networkEntity.netId, FXMessage.AddOrRemoveType.Remove, fxRemoveOnClientComponent.fxPoolIndex);

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
            while (messages.TryIterate(connectionId, out FXMessage message, ref it))
            {
                messagesList.Add(message);
            }

            server.Send(connectionId, messagesList, Channel.Reliable);
        }

        messages.Clear();
    }
}
