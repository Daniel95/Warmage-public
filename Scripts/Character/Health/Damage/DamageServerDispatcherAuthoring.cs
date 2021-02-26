using DOTSNET;
using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

[DisallowMultipleComponent]
public class DamageServerDispatcherAuthoring : MonoBehaviour, SelectiveSystemAuthoring
{
    DamageServerDispatcher system =>
        Bootstrap.ServerWorld.GetExistingSystem<DamageServerDispatcher>();

    public Type GetSystemType() { return typeof(DamageServerDispatcher); }

    public float interval = 0.1f;

    void Awake()
    {
        system.interval = interval;
    }
}

[DisableAutoCreation]
public class DamageServerDispatcher : NetworkBroadcastSystem
{
    private NativeMultiHashMap<int, DamageMessage> messages;
    private NativeList<DamageMessage> messagesList;

    private EndServerSimulationEntityCommandBufferSystem entityCommandBufferSystem;

    protected override void OnCreate()
    {
        base.OnCreate();

        messages = new NativeMultiHashMap<int, DamageMessage>(1000, Allocator.Persistent);
        messagesList = new NativeList<DamageMessage>(1000, Allocator.Persistent);

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
        NativeMultiHashMap<int, DamageMessage> _messages = messages;
        Entities.ForEach((DynamicBuffer<DispatchDamageMessageElement> dispatchDamageMessageBuffer,
                            in DynamicBuffer<NetworkObserver> observers,
                            in Entity entity,
                            in Translation translation,
                            in NetworkEntity networkEntity) =>
        {
            for (int i = 0; i < dispatchDamageMessageBuffer.Length; i++)
            {
                DispatchDamageMessageElement dispatchDamageMessageElement = dispatchDamageMessageBuffer[i];

                DamageMessage message = new DamageMessage(
                    networkEntity.netId,
                    (short)dispatchDamageMessageElement.damage,
                    dispatchDamageMessageElement.damagerNetId,
                    translation.Value
                );

                for (int j = 0; j < observers.Length; ++j)
                {
                    int connectionId = observers[j];

                    _messages.Add(connectionId, message);
                }
            }

            dispatchDamageMessageBuffer.Clear();
        })
        .Run();

        foreach (int connectionId in server.connections.Keys)
        {
            messagesList.Clear();
            NativeMultiHashMapIterator<int>? it = default;
            while (messages.TryIterate(connectionId, out DamageMessage message, ref it))
            {
                messagesList.Add(message);
            }

            server.Send(connectionId, messagesList, Channel.Reliable);
        }

        messages.Clear();
    }
}
