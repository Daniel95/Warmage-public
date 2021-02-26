using DOTSNET;
using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

[DisallowMultipleComponent]
public class FXOneShotServerDispatcherAuthoring : MonoBehaviour, SelectiveSystemAuthoring
{
    FXOneShotServerDispatcher system =>
        Bootstrap.ServerWorld.GetExistingSystem<FXOneShotServerDispatcher>();

    public Type GetSystemType() { return typeof(FXOneShotServerDispatcher); }

    public float interval = 0.1f;

    void Awake()
    {
        system.interval = interval;
    }
}

[DisableAutoCreation]
public class FXOneShotServerDispatcher : NetworkBroadcastSystem
{
    private NativeMultiHashMap<int, FXOneShotMessage> messages;
    private NativeList<FXOneShotMessage> messagesList;
    private NativeList<Entity> oneShotEntitiesToDespawn;
    private BeginServerSimulationEntityCommandBufferSystem beginServerSimulationEntityCommandBufferSystem;

    protected override void OnCreate()
    {
        base.OnCreate();

        messages = new NativeMultiHashMap<int, FXOneShotMessage>(20, Allocator.Persistent);
        messagesList = new NativeList<FXOneShotMessage>(20, Allocator.Persistent);
        oneShotEntitiesToDespawn = new NativeList<Entity>(20, Allocator.Persistent);

        beginServerSimulationEntityCommandBufferSystem = World.GetOrCreateSystem<BeginServerSimulationEntityCommandBufferSystem>();
    }

    protected override void OnDestroy()
    {
        oneShotEntitiesToDespawn.Dispose();
        messagesList.Dispose();
        messages.Dispose();

        base.OnDestroy();
    }

    protected override void Broadcast()
    {
        NativeMultiHashMap<int, FXOneShotMessage> _messages = messages;
        NativeList<Entity> _oneShotEntitiesToDespawn = oneShotEntitiesToDespawn;
        GameNetworkServerSystem gameServer = (GameNetworkServerSystem)server;

        float deltaTime = Time.DeltaTime;

        Entities.ForEach((DynamicBuffer<NetworkObserver> observers,
                            DynamicBuffer<FXOneShotBufferElement> fxOneShotBufferElements,
                            in NetworkEntity networkEntity,
                            in Translation translation,
                            in Rotation rotation,
                            in Entity entity) =>
        {
            if (observers.Length == 0) { return; }

            _oneShotEntitiesToDespawn.Add(entity);

            for (int i = 0; i < fxOneShotBufferElements.Length; i++)
            {
                FXOneShotBufferElement element = fxOneShotBufferElements[i];

                FXOneShotMessage message = new FXOneShotMessage(networkEntity.netId,
                    element.fxPoolIndex,
                    element.timer,
                    translation.Value,
                    rotation.Value);

                for (int o = 0; o < observers.Length; ++o)
                {
                    int connectionId = observers[o];

                    _messages.Add(connectionId, message);
                }
            }
        })
        .Run();

        foreach (int connectionId in server.connections.Keys)
        {
            messagesList.Clear();
            NativeMultiHashMapIterator<int>? it = default;
            while (messages.TryIterate(connectionId, out FXOneShotMessage message, ref it))
            {
                messagesList.Add(message);
            }

            server.Send(connectionId, messagesList, Channel.Reliable);
        }

        messages.Clear();

        var ecb = beginServerSimulationEntityCommandBufferSystem.CreateCommandBuffer();

        for (int i = 0; i < oneShotEntitiesToDespawn.Length; i++)
        {
            NetworkEntityHelper.Destroy(oneShotEntitiesToDespawn[i], ecb, server);
        }

        beginServerSimulationEntityCommandBufferSystem.AddJobHandleForProducer(Dependency);

        oneShotEntitiesToDespawn.Clear();
    }
}
