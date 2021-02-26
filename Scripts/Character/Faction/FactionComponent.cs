using Unity.Entities;

[GenerateAuthoringComponent]
public struct FactionComponent : IComponentData
{
    public FactionType factionType;
}
