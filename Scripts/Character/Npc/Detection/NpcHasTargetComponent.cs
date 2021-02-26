using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct NpcHasTargetComponent : IComponentData
{
    public bool targetIsNotNull => targetEntity != Entity.Null;

    [HideInInspector] public Entity targetEntity;
    [HideInInspector] public float distanceToTarget;
    [HideInInspector] public float3 targetPosition;
    [HideInInspector] public bool inView;
}
