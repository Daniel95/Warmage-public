using System;
using Unity.Entities;
using UnityEngine;

[DisallowMultipleComponent]
public class StatusEffectElementAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddBuffer<StatusEffectElement>(entity);
    }
}

public struct StatusEffectElement : IBufferElementData
{
    public StatusEffectElement(StatusEffectScriptableObject statusEffectScriptableObject, Entity caster, ulong casterId, FactionType casterFactionType, float timeLeft)
    {
        this.statusEffectId = statusEffectScriptableObject.GetId();
        this.caster = caster;
        this.casterNetId = casterId;
        this.casterFactionType = casterFactionType;
        this.timeLeft = timeLeft;
        count = 1;
    }

    public StatusEffectElement(Guid statusEffectId, Entity caster, ulong casterNetId, FactionType casterFactionType, float timeLeft)
    {
        this.statusEffectId = statusEffectId;
        this.caster = caster;
        this.casterNetId = casterNetId;
        this.timeLeft = timeLeft;
        this.casterFactionType = casterFactionType;
        count = 1;
    }

    public Guid statusEffectId;
    public Entity caster;
    public ulong casterNetId;
    public float timeLeft;
    public int count;
    public FactionType casterFactionType;
}