﻿using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct MonsterMovementData : IComponentData
{
    // movement speed in m/s
    public float speed;

    // movement probability in % [0, 1]
    public float moveProbability;

    // are we currently moving somewhere?
    public bool isMoving;

    // move distance
    public float moveDistance;

    // start position so we don't run too far off
    public float3 startPosition;

    // current destination
    public float3 destination;
}
