using DOTSNET;
using System;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

[DisallowMultipleComponent]
public class UpdateStatusEffectServerDispatcherAuthoring : MonoBehaviour, SelectiveSystemAuthoring
{
    UpdateStatusEffectServerDispatcher system =>
        Bootstrap.ServerWorld.GetExistingSystem<UpdateStatusEffectServerDispatcher>();

    public Type GetSystemType() { return typeof(UpdateStatusEffectServerDispatcher); }

    public float interval = 0.1f;

    void Awake()
    {
        system.interval = interval;
    }
}

[UpdateAfter(typeof(StatusEffectServerSystem))]
[DisableAutoCreation]
public class UpdateStatusEffectServerDispatcher : NetworkBroadcastSystem
{
    private NativeMultiHashMap<int, StatusEffectMessage> messages;
    private NativeList<StatusEffectMessage> messagesList;

    /*
    public void DispatchExpiredStatusEffect(ulong netId, Guid statusEffectId)
    {
        NativeMultiHashMap<int, StatusEffectMessage> _messages = messages;
        Entities.ForEach((in DynamicBuffer<NetworkObserver> observers,
                            in DynamicBuffer<StatusEffectElement> statusEffectElements,
                            in NetworkEntity networkEntity) =>
        {
            if(networkEntity.netId == netId)
            {
                for (int i = 0; i < statusEffectElements.Length; i++)
                {
                    if(statusEffectElements[i].data.statusEffectId == statusEffectId)
                    {
                        StatusEffectElement statusEffectElement = statusEffectElements[i];

                        StatusEffectMessage message = new StatusEffectMessage(
                            networkEntity.netId,
                            statusEffectElement.data.statusEffectId,
                            statusEffectElement.data.casterNetId,
                            0
                        );

                        for (int o = 0; o < observers.Length; ++o)
                        {
                            int connectionId = observers[o];

                            bool owner = networkEntity.connectionId == connectionId;
                            _messages.Add(connectionId, message);
                        }
                    }
                }
            }
        })
        .Run();

        foreach (int connectionId in server.connections.Keys)
        {
            messagesList.Clear();
            NativeMultiHashMapIterator<int>? it = default;
            while (messages.TryIterate(connectionId, out StatusEffectMessage message, ref it))
            {
                messagesList.Add(message);
            }

            server.Send(connectionId, messagesList, Channel.Unreliable);
        }

        messages.Clear();
    }
     */

    protected override void OnCreate()
    {
        base.OnCreate();

        messages = new NativeMultiHashMap<int, StatusEffectMessage>(1000, Allocator.Persistent);
        messagesList = new NativeList<StatusEffectMessage>(1000, Allocator.Persistent);
    }

    protected override void OnDestroy()
    {
        messagesList.Dispose();
        messages.Dispose();

        base.OnDestroy();
    }

    protected override void Broadcast()
    {
        NativeMultiHashMap<int, StatusEffectMessage> _messages = messages;
        Entities.ForEach((DynamicBuffer<DispatchStatusEffectMessageElement> dispatchStatusEffectMessageBuffer,
                            in DynamicBuffer<NetworkObserver> observers,
                            in Entity entity,
                            in NetworkEntity networkEntity) =>
        {
            for (int i = 0; i < dispatchStatusEffectMessageBuffer.Length; i++)
            {
                DispatchStatusEffectMessageElement dispatchStatusEffectMessageElement = dispatchStatusEffectMessageBuffer[i];

                StatusEffectMessage message = new StatusEffectMessage(
                    networkEntity.netId,
                    dispatchStatusEffectMessageElement.statusEffectId,
                    dispatchStatusEffectMessageElement.casterNetId,
                    dispatchStatusEffectMessageElement.timeLeft,
                    dispatchStatusEffectMessageElement.count,
                    dispatchStatusEffectMessageElement.messageType
                );

                for (int o = 0; o < observers.Length; ++o)
                {
                    int connectionId = observers[o];

                    bool owner = networkEntity.connectionId == connectionId;
                    _messages.Add(connectionId, message);
                }
            }

            dispatchStatusEffectMessageBuffer.Clear();
        })
        .Run();

        foreach (int connectionId in server.connections.Keys)
        {
            messagesList.Clear();
            NativeMultiHashMapIterator<int>? it = default;
            while (messages.TryIterate(connectionId, out StatusEffectMessage message, ref it))
            {
                messagesList.Add(message);
            }

            server.Send(connectionId, messagesList, Channel.Reliable);
        }

        messages.Clear();
    }
}
