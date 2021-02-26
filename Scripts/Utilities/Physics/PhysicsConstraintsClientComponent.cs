using Unity.Entities;

[GenerateAuthoringComponent]
public struct PhysicsConstraintsClientComponent : IComponentData
{
    public bool LockX;
    public bool LockY;
    public bool LockZ;
}
