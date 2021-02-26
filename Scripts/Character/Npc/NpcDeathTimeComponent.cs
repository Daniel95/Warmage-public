using Unity.Entities;

[GenerateAuthoringComponent]
public struct NpcDeathTimeComponent : IComponentData
{
    public float deathTime;
}
