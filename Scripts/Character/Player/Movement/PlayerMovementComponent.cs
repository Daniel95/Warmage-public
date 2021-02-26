using Unity.Entities;

[GenerateAuthoringComponent]
public struct PlayerMovementComponent : IComponentData
{
    public float speed;
}
