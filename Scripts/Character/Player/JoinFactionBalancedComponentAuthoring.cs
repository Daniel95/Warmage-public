using Unity.Entities;
using UnityEngine;

public class JoinFactionBalancedComponentAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        FactionType factionType = FactionManager.GetSmallestFaction();
        FactionManager.RegisterAtFaction(factionType);

        FactionComponent factionComponent = new FactionComponent()
        {
            factionType = factionType
        };
        dstManager.AddComponentData(entity, factionComponent);
    }
}