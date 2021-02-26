using Unity.Entities;
using UnityEngine;

[DisallowMultipleComponent]
public class NewDamageBufferAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddBuffer<NewDamageElement>(entity);
    }
}

public struct NewDamageElement: IBufferElementData
{
    public int damage;
    public Entity damagerEntity;
    public ulong damagerNetId;
    public FactionType damagerFactionType;
    public bool damagerIsPlayer;
}