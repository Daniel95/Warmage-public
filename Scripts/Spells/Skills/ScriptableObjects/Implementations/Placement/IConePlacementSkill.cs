using Unity.Mathematics;

public interface IConePlacementSkill
{
    float GetConeMinDotProduct();
    float GetMaxDistance();

    void Init(float3 placePosition, quaternion rotation, float3 playerPosition);
}