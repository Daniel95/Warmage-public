using System;
using Unity.Entities;

[GenerateAuthoringComponent]
public struct GoToTargetComponent : IComponentData
{
    public float speed;
    public Entity targetEntity;
}
