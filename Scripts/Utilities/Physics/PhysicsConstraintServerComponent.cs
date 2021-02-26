using Unity.Entities;

[GenerateAuthoringComponent]
public struct PhysicsConstraintsServerComponent : IComponentData
{
    public bool LockX;
    public bool LockY;
    public bool LockZ;
}
