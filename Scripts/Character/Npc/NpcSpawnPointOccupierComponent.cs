using System;
using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct NpcSpawnPointOccupierComponent : IComponentData
{
    public Guid keepId;
    public int spawnPointIndex;
    public float3 spawnPosition;
}

// start position