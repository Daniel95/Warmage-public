using DOTSNET;
using System;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

[DisallowMultipleComponent]
public class StatsReceiverClientAuthoring : MonoBehaviour, SelectiveSystemAuthoring
{
    public Type GetSystemType() { return typeof(StatsReceiverClient); }
}

[DisableAutoCreation]
public class StatsReceiverClient : NetworkClientMessageSystem<StatsMessage>
{
    NativeHashMap<ulong, StatsMessage> messages;

    protected override void OnCreate()
    {
        base.OnCreate();

        messages = new NativeHashMap<ulong, StatsMessage>(1000, Allocator.Persistent);
    }

    protected override void OnDestroy()
    {
        messages.Dispose(Dependency);

        base.OnDestroy();
    }

    protected override void OnMessage(StatsMessage message)
    {
        messages[message.netId] = message;
    }

    protected override void OnUpdate()
    {
        NativeHashMap<ulong, StatsMessage> _messages = messages;

        Entities.ForEach((ref StatsComponent statsComponent,
                            in NetworkEntity networkEntity) =>
        {
            if (_messages.ContainsKey(networkEntity.netId))
            {
                StatsMessage message = _messages[networkEntity.netId];
                statsComponent.speedFactor = message.speedFactor;
            }
        })
        .Run();

        messages.Clear();
    }
}
