using Unity.Entities;
using Unity.Mathematics;

public struct NpcTargetKeepComponent : IComponentData
{
    public bool hasTarget;
    public float3 targetKeepPosition;
}
