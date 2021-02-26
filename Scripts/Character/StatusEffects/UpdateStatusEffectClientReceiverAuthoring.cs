using DOTSNET;
using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

[DisallowMultipleComponent]
public class UpdateStatusEffectClientReceiverAuthoring : MonoBehaviour, SelectiveSystemAuthoring
{
    public Type GetSystemType() { return typeof(UpdateStatusEffectClientReceiver); }
}

[DisableAutoCreation]
public class UpdateStatusEffectClientReceiver : NetworkClientMessageSystem<StatusEffectMessage>
{
    private ulong targetNetId;

    private NativeList<StatusEffectMessage> messages;

    protected override void OnCreate()
    {
        base.OnCreate();

        messages = new NativeList<StatusEffectMessage>(200, Allocator.Persistent);
    }

    protected override void OnDestroy()
    {
        messages.Dispose(Dependency);

        base.OnDestroy();
    }

    protected override void OnMessage(StatusEffectMessage message)
    {
        messages.Add(message);
    }

    protected override void OnUpdate() 
    {
        //If no updates from server, only check if we need to update the player target
        if (messages.IsEmpty) 
        {
            CheckPlayerTargetSwitch();

            return;
        }

        NativeList<StatusEffectMessage> _messages = messages;

        //Update status effect buffers
        Entities.ForEach((DynamicBuffer<StatusEffectElement> statusEffectBuffer,
            in NetworkEntity networkEntity) =>
        {
            for (int i = 0; i < _messages.Length; i++)
            {
                StatusEffectMessage message = _messages[i];

                if (message.netId != networkEntity.netId) { continue; }

                if (message.messageType == StatusEffectMessage.MessageType.Add)
                {
                    statusEffectBuffer.Add(new StatusEffectElement
                    {
                        statusEffectId = message.statusEffectId,
                        casterNetId = message.casterNetId,
                        timeLeft = message.timeLeft,
                        count = message.count
                    });
                }
                else if (message.messageType == StatusEffectMessage.MessageType.Update)
                {
                    for (int j = 0; j < statusEffectBuffer.Length; j++)
                    {
                        StatusEffectElement statusEffectElement = statusEffectBuffer[j];

                        if (statusEffectElement.statusEffectId == message.statusEffectId && statusEffectElement.casterNetId == message.casterNetId)
                        {
                            statusEffectElement.timeLeft = message.timeLeft;
                            statusEffectElement.count = message.count;

                            statusEffectBuffer[j] = statusEffectElement;
                            break;
                        }
                    }
                }
                else if (message.messageType == StatusEffectMessage.MessageType.Remove)
                {
                    for (int j = 0; j < statusEffectBuffer.Length; j++)
                    {
                        StatusEffectElement statusEffectElement = statusEffectBuffer[j];

                        if (statusEffectElement.statusEffectId == message.statusEffectId && statusEffectElement.casterNetId == message.casterNetId)
                        {
                            statusEffectBuffer.RemoveAt(j);
                            break;
                        }
                    }
                }
            }
        })
        .Run();

        //Update player target status effect Ui
        if (PlayerLocalInfo.targetInfo.hasTarget)
        {
            CheckPlayerTargetSwitch();
            UiStatusEffect targetStatusEffect = UiWorldTargetStatusBar.GetInstance().statusEffect;

            for (int i = 0; i < messages.Length; i++)
            {
                StatusEffectMessage message = messages[i];

                if (message.netId == PlayerLocalInfo.targetInfo.netId)
                {
                    if (message.messageType == StatusEffectMessage.MessageType.Add)
                    {
                        targetStatusEffect.AddStatusEffect(message.casterNetId, message.statusEffectId, message.timeLeft, message.count);
                    }
                    else if (message.messageType == StatusEffectMessage.MessageType.Update)
                    {
                        targetStatusEffect.UpdateStatusEffect(message.casterNetId, message.statusEffectId, message.timeLeft, message.count);
                    }
                    else if (message.messageType == StatusEffectMessage.MessageType.Remove)
                    {
                        targetStatusEffect.RemoveStatusEffect(message.casterNetId, message.statusEffectId);
                    }
                }
            }
        }

        //Update player status effect Ui
        {
            UiStatusEffect statusEffectUI = UiGame.GetInstance().playerStatusBar.statusEffect;

            for (int i = 0; i < messages.Length; i++)
            {
                StatusEffectMessage message = messages[i];

                if (message.netId == PlayerLocalInfo.netId)
                {
                    if (message.messageType == StatusEffectMessage.MessageType.Add)
                    {
                        statusEffectUI.AddStatusEffect(message.casterNetId, message.statusEffectId, message.timeLeft, message.count);
                    }
                    else if (message.messageType == StatusEffectMessage.MessageType.Update)
                    {
                        statusEffectUI.UpdateStatusEffect(message.casterNetId, message.statusEffectId, message.timeLeft, message.count);
                    }
                    else if (message.messageType == StatusEffectMessage.MessageType.Remove)
                    {
                        statusEffectUI.RemoveStatusEffect(message.casterNetId, message.statusEffectId);
                    }
                }
            }
        }

        messages.Clear();
    }

    private void CheckPlayerTargetSwitch()
    {
        UiStatusEffect targetStatusEffect = UiWorldTargetStatusBar.GetInstance().statusEffect;

        //If player switched from target.
        if (PlayerLocalInfo.targetInfo.hasTarget && targetNetId != PlayerLocalInfo.targetInfo.netId)
        {
            //if changed target, update statues effect ui
            if (targetNetId != PlayerLocalInfo.targetInfo.netId)
            {
                targetNetId = PlayerLocalInfo.targetInfo.netId;

                targetStatusEffect.Clear();

                DynamicBuffer<StatusEffectElement> statusEffectBuffer = EntityManager.GetBuffer<StatusEffectElement>(PlayerLocalInfo.targetInfo.entity);

                for (int i = 0; i < statusEffectBuffer.Length; i++)
                {
                    StatusEffectElement statusEffectElement = statusEffectBuffer[i];
                    targetStatusEffect.AddStatusEffect(statusEffectElement.casterNetId,
                        statusEffectElement.statusEffectId,
                        statusEffectElement.timeLeft,
                        statusEffectElement.count);
                }
            }
        }
    }
}
