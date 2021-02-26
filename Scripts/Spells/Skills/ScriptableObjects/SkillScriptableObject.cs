using DOTSNET;
using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEditor;
using UnityEngine;

public abstract class SkillScriptableObject : ScriptableObject, ISkill
{
    public int GetManaCost() => actionPointsCost;
    public float GetRange() => range;
    public string GetName() => skillName;
    public string GetDescription() => description;
    public Guid GetId() => id;
    public float GetCooldown() => cooldown;
    public bool HasGlobalCooldown() => hasGlobalCooldown;
    public float GetCastTime() => castTime;
    public int GetMaxCharges() => maxCharges;
    public int GetActionPointCost() => actionPointsCost;
    public bool ActivateOnFriendly() => activateOnFriendly;
    public SkillPlacementType GetPlacementType() => placementType;
    public bool IsManuallyPlaced() => placementType != SkillPlacementType.None;
    public Sprite GetIcon() => icon;
    public bool IsPlayerOwned() => playerOwned;

    protected EntityManager entityManager;
    protected NetworkServerSystem server;
    protected PrefabSystem prefabSystem;

    [Header("Visual info")]
    [SerializeField] private string skillName = string.Empty;
    [SerializeField] private string description = string.Empty;
    [SerializeField] private Sprite icon = null;
    [Header("Functional settings")]
    [SerializeField] [Range(0, 40)] private float range = 0;
    [SerializeField] private bool hasGlobalCooldown = true;
    [SerializeField] [Range(0, 120)] private float cooldown = 0;
    [SerializeField] [Range(0, 10)] private float castTime = 0;
    [SerializeField] [Range(0, 6)] private int actionPointsCost = 0;
    [SerializeField] [Range(1, 15)] private int maxCharges = 1;
    [SerializeField] private bool activateOnFriendly = false;
    [Header("Extra settings")]
    [SerializeField] private SerializableGuid id = new SerializableGuid();

    [HideInInspector] [SerializeField] private bool playerOwned = false;
    [HideInInspector] [SerializeField] private SkillPlacementType placementType = SkillPlacementType.None;

    public abstract void OnExecute(Entity casterEntity, ulong casterNetId, FactionType casterFactionType, Entity targetEntity, EntityCommandBuffer ecb);

    public void Execute(Entity casterEntity, ulong casterNetId, FactionType casterFactionType, Entity targetEntity, EntityCommandBuffer ecb, SpellEcsData spellEcsData)
    {
        entityManager = spellEcsData.entityManager;
        server = spellEcsData.server;
        prefabSystem = spellEcsData.prefabSystem;

        OnExecute(casterEntity, casterNetId, casterFactionType, targetEntity, ecb);
    }

    public void GenerateId()
    {
        id = Guid.NewGuid();
    }

    public ISkill ShallowCopy()
    {
        return (ISkill)MemberwiseClone();
    }

    protected bool SpawnEntity(NetworkEntityAuthoring entityPrefab, float3 position, quaternion rotation, out Entity entity)
    {
        Debug.Assert(entityPrefab != null, "NetworkEntityAuthoring is null!");

        Bytes16 spawnPrefabId = Conversion.GuidToBytes16(entityPrefab.prefabId);

        if (prefabSystem.Get(spawnPrefabId, out Entity prefab))
        {
            entity = entityManager.Instantiate(prefab);

            entityManager.AddComponentData(entity, new Translation { Value = position });
            entityManager.AddComponentData(entity, new Rotation { Value = rotation });

            server.Spawn(entity, null);

            return true;
        }

        Debug.LogWarning("Entity that you are trying to spawn is not a registered prefab!");

        entity = Entity.Null;
        return false;
    }

    //Make sure to use the ecb for any modifications on the returned entity, since the entity is not yet created!
    protected bool SpawnEntity(NetworkEntityAuthoring entityPrefab, float3 position, quaternion rotation, EntityCommandBuffer ecb, out Entity entity)
    {
        Debug.Assert(entityPrefab != null, "NetworkEntityAuthoring is null!");

        Bytes16 spawnPrefabId = Conversion.GuidToBytes16(entityPrefab.prefabId);

        if (prefabSystem.Get(spawnPrefabId, out Entity prefab))
        {
            entity = ecb.Instantiate(prefab);

            ecb.AddComponent(entity, new Translation { Value = position });
            ecb.AddComponent(entity, new Rotation { Value = rotation });

            ecb.AddComponent(entity, new SpawnOnServerComponent { });

            return true;
        }

        Debug.LogWarning("Entity that you are trying to spawn is not a registered prefab!");

        entity = Entity.Null;
        return false;
    }

#if UNITY_EDITOR
    protected virtual void OnValidate()
    {
        //Placement type will be assigned when the scriptable object is created.
        {
            placementType = SkillPlacementType.None;

            if (this is ICirclePlacementSkill)
            {
                placementType = SkillPlacementType.Circle;
            }
            else if (this is IConePlacementSkill)
            {
                placementType = SkillPlacementType.Cone;
            }
        }

        if (id.Value == string.Empty)
        {
            GenerateId();
        }
        var path = AssetDatabase.GetAssetPath(this);

        if(path.Contains("Player"))
        {
            playerOwned = true;
        }
    }
#endif
}
