using Unity.Entities;
using UnityEngine;

[GenerateAuthoringComponent]
public struct ChannelingComponent : IComponentData
{
    [HideInInspector] public bool active;
    [HideInInspector] public float timeLeft;
    [HideInInspector] public float totalTime;
}
