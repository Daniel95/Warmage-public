using DOTSNET;
using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Rendering;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerInfoReceiverClientAuthoring : MonoBehaviour, SelectiveSystemAuthoring
{
    public Type GetSystemType() { return typeof(PlayerInfoReceiverClient); }
}

[DisableAutoCreation]
public class PlayerInfoReceiverClient : NetworkClientMessageSystem<PlayerInfoMessage>
{
    NativeHashMap<ulong, PlayerInfoMessage> messages;

    protected override void OnCreate()
    {
        base.OnCreate();

        messages = new NativeHashMap<ulong, PlayerInfoMessage>(200, Allocator.Persistent);
    }

    protected override void OnDestroy()
    {
        messages.Dispose(Dependency);

        base.OnDestroy();
    }

    protected override void OnMessage(PlayerInfoMessage message)
    {
        messages[message.netId] = message;
    }

    protected override void OnUpdate()
    {
        NativeHashMap<ulong, PlayerInfoMessage> _messages = messages;

        Entities.ForEach((ref PlayerInfoComponent factionComponent,
                            in NetworkEntity networkEntity) =>
        {
            if (_messages.ContainsKey(networkEntity.netId))
            {
                factionComponent.name = _messages[networkEntity.netId].name;
            }
        })
        .Run();

        messages.Clear();
    }
}
