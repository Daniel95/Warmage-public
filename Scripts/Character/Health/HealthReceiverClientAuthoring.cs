using DOTSNET;
using System;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

[DisallowMultipleComponent]
public class HealthReceiverClientAuthoring : MonoBehaviour, SelectiveSystemAuthoring
{
    public Type GetSystemType() { return typeof(HealthReceiverClient); }
}

[DisableAutoCreation]
public class HealthReceiverClient : NetworkClientMessageSystem<HealthMessage>
{
    NativeHashMap<ulong, HealthMessage> messages;

    protected override void OnCreate()
    {
        base.OnCreate();

        messages = new NativeHashMap<ulong, HealthMessage>(1000, Allocator.Persistent);
    }

    protected override void OnDestroy()
    {
        messages.Dispose(Dependency);

        base.OnDestroy();
    }

    protected override void OnMessage(HealthMessage message)
    {
        messages[message.netId] = message;
    }

    protected override void OnUpdate()
    {
        NativeHashMap<ulong, HealthMessage> _messages = messages;

        Entities.WithNone<LocalPlayerComponent>().ForEach((ref HealthComponent healthComponent,
                            in NetworkEntity networkEntity) =>
        {
            if (_messages.ContainsKey(networkEntity.netId))
            {
                HealthMessage message = _messages[networkEntity.netId];
                healthComponent.currentHealth = message.currentHealth;
                healthComponent.maxHealth = message.maxHealth;
            }
        })
        .Run();

        int playerHealthChange = 0; 

        Entities.WithAll<LocalPlayerComponent>().ForEach((ref HealthComponent healthComponent,
                    in NetworkEntity networkEntity) =>
        {
            if (_messages.ContainsKey(networkEntity.netId))
            {
                HealthMessage message = _messages[networkEntity.netId];

                playerHealthChange = message.currentHealth - healthComponent.currentHealth;

                healthComponent.currentHealth = message.currentHealth;
                healthComponent.maxHealth = message.maxHealth;
            }
        })
        .Run();

        if(playerHealthChange != 0)
        {
            UiGame.GetInstance().healthEffect.Play(playerHealthChange);
        }

        messages.Clear();
    }
}
