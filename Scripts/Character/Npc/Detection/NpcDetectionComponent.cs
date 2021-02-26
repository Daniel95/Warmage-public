using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[GenerateAuthoringComponent]
public struct NpcDetectionComponent : IComponentData
{
    [HideInInspector] public float detectionRange;
}
