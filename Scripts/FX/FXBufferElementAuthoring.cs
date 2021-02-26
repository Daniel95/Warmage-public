using Unity.Entities;
using UnityEngine;

public class FXBufferElementAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddBuffer<FXBufferElement>(entity);
    }
}

public struct FXBufferElement : IBufferElementData
{
    public int fxPoolIndex;
    public float timer;
    public bool timerActive;
}