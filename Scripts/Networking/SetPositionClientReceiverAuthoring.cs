using DOTSNET;
using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[DisallowMultipleComponent]
public class SetPositionClientReceiverAuthoring : MonoBehaviour, SelectiveSystemAuthoring
{
    public Type GetSystemType() { return typeof(SetPositionClientReceiver); }
}

[DisableAutoCreation]
public class SetPositionClientReceiver : NetworkClientMessageSystem<SetTransformMessage>
{
    NativeHashMap<ulong, SetTransformMessage> messages;

    protected override void OnCreate()
    {
        base.OnCreate();

        messages = new NativeHashMap<ulong, SetTransformMessage>(50, Allocator.Persistent);
    }

    protected override void OnDestroy()
    {
        messages.Dispose(Dependency);

        base.OnDestroy();
    }

    protected override void OnMessage(SetTransformMessage message)
    {
        messages[message.netId] = message;
    }

    protected override void OnUpdate()
    {
        if(messages.IsEmpty) { return; }

        NativeHashMap<ulong, SetTransformMessage> _messages = messages;

        Entities.ForEach((ref Translation translation,
                            ref Rotation rotation,
                            in NetworkEntity networkEntity) =>
        {
            if (_messages.ContainsKey(networkEntity.netId))
            {
                SetTransformMessage message = _messages[networkEntity.netId];
                translation.Value = message.position;
                rotation.Value = message.rotation;
            }
        })
        .Run();

        messages.Clear();
    }
}
