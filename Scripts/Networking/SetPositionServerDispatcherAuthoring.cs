using DOTSNET;
using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[DisallowMultipleComponent]
public class SetPositionServerDispatcherAuthoring : MonoBehaviour, SelectiveSystemAuthoring
{
    SetPositionServerDispatcher system =>
        Bootstrap.ServerWorld.GetExistingSystem<SetPositionServerDispatcher>();

    public Type GetSystemType() { return typeof(SetPositionServerDispatcher); }

    public float interval = 0.1f;

    void Awake()
    {
        system.interval = interval;
    }
}

[DisableAutoCreation]
public class SetPositionServerDispatcher : NetworkBroadcastSystem
{
    private NativeMultiHashMap<int, SetTransformMessage> messages;
    private NativeList<SetTransformMessage> messagesList;

    private EndServerSimulationEntityCommandBufferSystem entityCommandBufferSystem;

    protected override void OnCreate()
    {
        base.OnCreate();

        messages = new NativeMultiHashMap<int, SetTransformMessage>(1000, Allocator.Persistent);
        messagesList = new NativeList<SetTransformMessage>(1000, Allocator.Persistent);

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

        NativeMultiHashMap<int, SetTransformMessage> _messages = messages;
        Entities.ForEach((ref Translation translation,
                            in DynamicBuffer<NetworkObserver> observers,
                            in Rotation rotation,
                            in Entity entity,
                            in NetworkEntity networkEntity,
                            in DispatchSetPositionMessageComponent dispatchSetPositionMessageComponent) =>
        {
            translation.Value = dispatchSetPositionMessageComponent.position;

            SetTransformMessage message = new SetTransformMessage(
                networkEntity.netId,
                dispatchSetPositionMessageComponent.position,
                rotation.Value
            );

            for (int i = 0; i < observers.Length; ++i)
            {
                int connectionId = observers[i];

                _messages.Add(connectionId, message);
            }

            ecb.RemoveComponent<DispatchSetPositionMessageComponent>(entity);
        })
        .Run();

        Entities.ForEach((ref Translation translation,
                    ref Rotation rotation,
                    in DynamicBuffer<NetworkObserver> observers,
                    in Entity entity,
                    in NetworkEntity networkEntity,
                    in DispatchSetTransformMessageComponent dispatchSetTransformMessageComponent) =>
        {
            translation.Value = dispatchSetTransformMessageComponent.position;
            rotation.Value = dispatchSetTransformMessageComponent.rotation;

            SetTransformMessage message = new SetTransformMessage(
                networkEntity.netId,
                dispatchSetTransformMessageComponent.position,
                dispatchSetTransformMessageComponent.rotation
            );

            for (int i = 0; i < observers.Length; ++i)
            {
                int connectionId = observers[i];

                _messages.Add(connectionId, message);
            }

            ecb.RemoveComponent<DispatchSetTransformMessageComponent>(entity);
        })
        .Run();

        entityCommandBufferSystem.AddJobHandleForProducer(Dependency);

        foreach (int connectionId in server.connections.Keys)
        {
            messagesList.Clear();
            NativeMultiHashMapIterator<int>? it = default;
            while (messages.TryIterate(connectionId, out SetTransformMessage message, ref it))
            {
                messagesList.Add(message);
            }

            server.Send(connectionId, messagesList, Channel.Reliable);
        }

        messages.Clear();
    }
}
