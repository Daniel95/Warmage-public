using DOTSNET;
using System;
using Unity.Entities;
using UnityEngine;

[DisallowMultipleComponent]
public class DamageClientReceiverAuthoring : MonoBehaviour, SelectiveSystemAuthoring
{
    public Type GetSystemType() { return typeof(DamageClientReceiver); }
}

[DisableAutoCreation]
public class DamageClientReceiver : NetworkClientMessageSystem<DamageMessage>
{
    private UiFloatingTextManager uiFloatingTextManager;

    protected override void OnStartRunning()
    {
        base.OnStartRunning();

        uiFloatingTextManager = UiGame.GetInstance().floatingTextManager;
    }

    protected override void OnMessage(DamageMessage message)
    {
        if (PlayerLocalInfo.targetInfo.hasTarget && PlayerLocalInfo.targetInfo.netId == message.netId)
        {
            if (PlayerLocalInfo.netId == message.damagerNetId)
            {
                uiFloatingTextManager.SpawnDamageText(message.position, message.damage, FloatingTextType.DamageByThisPlayer);
            }
            else
            {
                uiFloatingTextManager.SpawnDamageText(message.position, message.damage, FloatingTextType.Damage);
            }
        }

        if(PlayerLocalInfo.netId == message.netId)
        {
            uiFloatingTextManager.SpawnDamageText(message.position, message.damage, FloatingTextType.Damage);
        }
    }

    protected override void OnUpdate() { }
}
