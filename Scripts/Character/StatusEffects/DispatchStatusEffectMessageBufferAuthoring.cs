using System;
using Unity.Entities;
using UnityEngine;

[DisallowMultipleComponent]
public class DispatchStatusEffectMessageBufferAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddBuffer<DispatchStatusEffectMessageElement>(entity);
    }
}

public struct DispatchStatusEffectMessageElement : IBufferElementData
{
    public Guid statusEffectId;
    public ulong casterNetId;
    public StatusEffectMessage.MessageType messageType;
    public float timeLeft;
    public int count;
}
