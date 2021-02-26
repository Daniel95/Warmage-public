using System;
using Unity.Entities;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(ObjectId))]
public class KeepComponentAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public enum KeepFactionType
    {
        Yellow,
        Blue,
        Red,
        Random
    }

    [SerializeField] private KeepFactionType factionType = KeepFactionType.Yellow;
    [SerializeField] private float respawnTime = 160;
    [Range(0, 1)] [SerializeField] private float spawnedNpcKeepMovementBiasFactor = 0.8f;
    [Range(0, 1)] [SerializeField] private float npcThreatFactor = 1.0f;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        Guid id = GetComponent<ObjectId>().Id;

        FactionType faction;

        if(factionType == KeepFactionType.Random)
        {
            float random = UnityEngine.Random.Range(0, 0.99f);

            if(random < 0.33f)
            {
                faction = FactionType.Yellow;
            }
            else if(random < 0.66f)
            {
                faction = FactionType.Blue;
            }
            else
            {
                faction = FactionType.Red;
            }
        }  
        else if(factionType == KeepFactionType.Yellow)
        {
            faction = FactionType.Yellow;
        }
        else if (factionType == KeepFactionType.Blue)
        {
            faction = FactionType.Blue;
        }
        else
        {
            faction = FactionType.Red;
        }

        dstManager.AddComponentData(entity, new KeepComponent 
        {
            id = id, 
            factionType = faction, 
            respawnTime = respawnTime,
            spawnedNpcKeepMovementBiasFactor = spawnedNpcKeepMovementBiasFactor,
            threatFactor = npcThreatFactor
        });
    }

    private void OnValidate()
    {
        if(TryGetComponent(out ObjectId objectId))
        {
            KeepSpawnPointComponentAuthoring[] keepSpawnPointComponentAuthorings = GetComponentsInChildren<KeepSpawnPointComponentAuthoring>();
            for (int i = 0; i < keepSpawnPointComponentAuthorings.Length; i++)
            {
                keepSpawnPointComponentAuthorings[i].keepId = objectId.Id;
                keepSpawnPointComponentAuthorings[i].spawnPointIndex = i;
            }
        }
    }
}

public struct KeepComponent : IComponentData
{
    public FactionType factionType;
    public Guid id;
    public float respawnTime;
    public float spawnedNpcKeepMovementBiasFactor;
    public float threatFactor;
}
