using Unity.Entities;
using UnityEngine;

public class FXOneShotBufferElementAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddBuffer<FXOneShotBufferElement>(entity);
    }
}

public struct FXOneShotBufferElement : IBufferElementData
{
    public int fxPoolIndex;
    public float timer;
}