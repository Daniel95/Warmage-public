using Unity.Entities;
using UnityEngine;

public struct HasTargetComponent : IComponentData
{
    public bool targetIsNotNull => targetEntity != Entity.Null;

    [HideInInspector] public Entity targetEntity;
}
