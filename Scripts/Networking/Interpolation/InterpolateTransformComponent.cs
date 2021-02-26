using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[GenerateAuthoringComponent]
public struct InterpolateTransformComponent : IComponentData
{
    [HideInInspector] public float3 position;
    [HideInInspector] public quaternion rotation;
}
