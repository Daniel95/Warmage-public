using System;
using Unity.Entities;
using UnityEngine;

public class LootContainerComponentAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    [SerializeField] private StatusEffectScriptableObject statusEffect = null;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        Debug.Assert(statusEffect != null, "No status effect assigned on " + gameObject.name, gameObject); 

        dstManager.AddComponentData(entity, new LootContainerComponent { statusEffectId = statusEffect.GetId() });
    }
}

public struct LootContainerComponent : IComponentData
{
    public Guid statusEffectId;
    public int spawnPointIndex;
}
