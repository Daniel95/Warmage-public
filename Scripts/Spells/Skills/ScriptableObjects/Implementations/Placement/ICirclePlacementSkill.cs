using Unity.Mathematics;

public interface ICirclePlacementSkill
{
    float GetRadius();

    void Init(float3 placePosition);
}