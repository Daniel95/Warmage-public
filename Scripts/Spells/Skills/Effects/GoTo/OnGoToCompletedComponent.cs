using System;
using Unity.Entities;
using Unity.Mathematics;

public class OnGoToCompletedComponent : IComponentData
{
    public Action<float3> onCompleted;
}
