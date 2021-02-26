using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct NpcPatrolComponent : IComponentData
{
    public float patrolSpeed;

    public float minIdleTime;
    public float maxIdleTime;
    [HideInInspector] public float idleTimer;

    // move distance
    public float minMoveDistance;
    public float maxMoveDistance;

    public float maxDistanceFromStartPosition;

    public float keepMovementBiasFactor;
    public bool hasTargetKeep;
    public float3 targetKeepPosition;
    public float keepThreatBiasFactor;
}
