using Unity.Collections;
using Unity.Entities;

[GenerateAuthoringComponent]
public struct PlayerInfoComponent : IComponentData
{
    public FixedString32 name;
}
