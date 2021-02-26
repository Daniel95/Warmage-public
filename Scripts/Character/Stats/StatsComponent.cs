using Unity.Entities;

[GenerateAuthoringComponent]
public struct StatsComponent : IComponentData
{
    public float Speed => speedBase * speedFactor;

    public float speedBase;
    public float speedFactor;

    public void ResetAllFactors()
    {
        speedFactor = 1;
    }
}
