using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct GoToPositionComponent : IComponentData
{
    public float speed;
    public float3 targetPosition;
}
