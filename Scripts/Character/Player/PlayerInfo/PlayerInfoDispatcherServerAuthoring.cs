using DOTSNET;
using System;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerInfoDispatcherServerAuthoring : MonoBehaviour, SelectiveSystemAuthoring
{
    PlayerInfoDispatcherServer system =>
        Bootstrap.ServerWorld.GetExistingSystem<PlayerInfoDispatcherServer>();

    public Type GetSystemType() { return typeof(PlayerInfoDispatcherServer); }

    public float interval = 0.1f;

    void Awake()
    {
        system.interval = interval;
    }
}

[DisableAutoCreation]
public class PlayerInfoDispatcherServer : NetworkBroadcastSystem
{
    private NativeMultiHashMap<int, PlayerInfoMessage> messages;
    private NativeList<PlayerInfoMessage> messagesList;

    protected override void OnCreate()
    {
        base.OnCreate();

        messages = new NativeMultiHashMap<int, PlayerInfoMessage>(20, Allocator.Persistent);
        messagesList = new NativeList<PlayerInfoMessage>(20, Allocator.Persistent);
    }

    protected override void OnDestroy()
    {
        messagesList.Dispose();
        messages.Dispose();

        base.OnDestroy();
    }

    protected override void Broadcast()
    {
        NativeMultiHashMap<int, PlayerInfoMessage> _messages = messages;
        GameNetworkServerSystem gameServer = (GameNetworkServerSystem)server;
        NativeHashMap<int, FixedString32> names = gameServer.names;

        Entities.ForEach((in DynamicBuffer<NetworkObserver> observers,
                            in FactionComponent factionComponent,
                            in NetworkEntity networkEntity,
                            in Entity entity) =>
        {
            if(!networkEntity.connectionId.HasValue) { return; }
            int networkEntityConnectionId = networkEntity.connectionId.Value;

            if (names.TryGetValue(networkEntityConnectionId, out FixedString32 name))
            {
                PlayerInfoMessage message = new PlayerInfoMessage(networkEntity.netId, name, factionComponent.factionType);

                for (int i = 0; i < observers.Length; ++i)
                {
                    int connectionId = observers[i];

                    _messages.Add(connectionId, message);
                }
            }
        })
        .WithoutBurst()
        .Run();

        foreach (int connectionId in server.connections.Keys)
        {
            messagesList.Clear();
            NativeMultiHashMapIterator<int>? it = default;
            while (messages.TryIterate(connectionId, out PlayerInfoMessage message, ref it))
            {
                messagesList.Add(message);
            }

            server.Send(connectionId, messagesList, Channel.Unreliable);
        }

        messages.Clear();
    }
}
