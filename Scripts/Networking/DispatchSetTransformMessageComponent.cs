using Unity.Entities;
using Unity.Mathematics;

public struct DispatchSetTransformMessageComponent : IComponentData
{
    public float3 position;
    public quaternion rotation;
}
