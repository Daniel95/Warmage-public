using Unity.Entities;
using UnityEngine;

[DisallowMultipleComponent]
public class AccumulatedDamageBufferAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddBuffer<AccumulatedDamageElement>(entity);
    }
}

public struct AccumulatedDamageElement : IBufferElementData
{
    public int damage;
    public Entity damagerEntity;
    public bool isPlayer;
}