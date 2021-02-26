using DOTSNET;
using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[DisallowMultipleComponent]
public class FXOneShotClientReceiverAuthoring : MonoBehaviour, SelectiveSystemAuthoring
{
    public Type GetSystemType() { return typeof(FXOneShotClientReceiver); }
}

[DisableAutoCreation]
public class FXOneShotClientReceiver : NetworkClientMessageSystem<FXOneShotMessage>
{
    protected override void OnMessage(FXOneShotMessage message)
    {
        FXObject fxObject = FXLibrary.GetInstance().fxPool.GetPooledObject(message.fxPoolIndex);

        fxObject.transform.position = message.position;
        fxObject.transform.rotation = message.rotation;

        fxObject.Play(message.timer);
    }

    protected override void OnUpdate() { }
}
