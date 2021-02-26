using Unity.Entities;
using UnityEngine;

[DisallowMultipleComponent]
public class DispatchDamageMessageBufferAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddBuffer<DispatchDamageMessageElement>(entity);
    }
}

public struct DispatchDamageMessageElement : IBufferElementData
{
    public int damage;
    public ulong damagerNetId;
}