using System;
using Unity.Entities;

public struct StatusEffectComponent : IComponentData
{
    public Guid statusId;
    public float timeLeft;
    public Entity owner;
}
