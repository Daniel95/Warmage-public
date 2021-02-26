using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct GoToPositionAngledComponent : IComponentData
{
    public float speed;
    public float randomSideAngle;
    public float randomUpAngle;
    public float3 targetPosition;
}
