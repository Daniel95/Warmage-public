using DOTSNET;
using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Rendering;
using UnityEngine;

[DisallowMultipleComponent]
public class SetFactionReceiverClientAuthoring : MonoBehaviour, SelectiveSystemAuthoring
{
    public Type GetSystemType() { return typeof(SetFactionReceiverClient); }
}

[DisableAutoCreation]
public class SetFactionReceiverClient : NetworkClientMessageSystem<SetFactionMessage>
{
    private NativeHashMap<ulong, SetFactionMessage> messages;

    protected override void OnCreate()
    {
        base.OnCreate();

        messages = new NativeHashMap<ulong, SetFactionMessage>(200, Allocator.Persistent);
    }

    protected override void OnDestroy()
    {
        messages.Dispose(Dependency);

        base.OnDestroy();
    }

    protected override void OnMessage(SetFactionMessage message)
    {
        messages[message.netId] = message;
    }

    protected override void OnUpdate()
    {
        if (messages.IsEmpty) { return; }

        NativeHashMap<ulong, SetFactionMessage> _messages = messages;

        Entities.ForEach((ref FactionComponent factionComponent,
                            ref MaterialColor materialColor,
                            in NetworkEntity networkEntity) =>
        {
            if (_messages.ContainsKey(networkEntity.netId))
            {
                SetFactionMessage message = _messages[networkEntity.netId];

                factionComponent.factionType = message.factionType;

                materialColor.Value = FactionManager.GetFactionColor(message.factionType);
            }
        })
        .Run();

        FactionType factionType = FactionType.Blue;
        bool setFactionType = false;


        Entities.ForEach((ref FactionComponent factionComponent,
                    in NetworkEntity networkEntity,
                    in LocalPlayerComponent localPlayerComponent) =>
        {
            if (_messages.ContainsKey(networkEntity.netId))
            {
                factionType = factionComponent.factionType;
                setFactionType = true;
            }
        })
        .Run();

        if (setFactionType)
        {
            PlayerLocalInfo.factionType = factionType;
        }

        messages.Clear();
    }
}