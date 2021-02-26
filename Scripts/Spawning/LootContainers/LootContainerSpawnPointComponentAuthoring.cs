using Unity.Entities;
using UnityEngine;

[DisallowMultipleComponent]
public class LootContainerSpawnPointComponentAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public int spawnPointIndex;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new LootContainerSpawnPointComponent { spawnPointIndex = spawnPointIndex });
    }
}

public struct LootContainerSpawnPointComponent : IComponentData
{
    public int spawnPointIndex;
}
