using Unity.Entities;

[GenerateAuthoringComponent]
public struct GoToTargetAngledComponent : IComponentData
{
    public float speed;
    public float randomSideAngle;
    public float randomUpAngle;
    public Entity targetEntity;
}
