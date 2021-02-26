using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct CameraFollowTargetComponent : IComponentData 
{
    public float3 offset;
}
