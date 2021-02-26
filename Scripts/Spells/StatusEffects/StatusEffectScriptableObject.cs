using DOTSNET;
using System;
using Unity.Entities;
using UnityEditor;
using UnityEngine;

public abstract class StatusEffectScriptableObject : ScriptableObject, IStatusEffect
{
    public string GetName() => skillName;
    public string GetDescription() => description;
    public float GetTime() => time;
    public void SetTime(float newTime) { time = newTime; }
    public bool GetIsStackable() => maxStacks > 1;
    public int GetMaxStacks() => maxStacks;
    public bool IsInfinite() => isInfinite;
    public Sprite GetIcon() => icon;
    public Guid GetId() => id;

    protected Entity casterEntity;
    protected Entity targetEntity;
    protected ulong casterNetId;
    protected FactionType casterFactionType;
    protected PrefabSystem prefabSystem;
    protected NetworkServerSystem server;
    protected EntityManager entityManager;

    [HideInInspector] [SerializeField] protected bool playerOwned;

    [Header("Visual info")]
    [SerializeField] private Sprite icon = null;
    [SerializeField] private string skillName = string.Empty;
    [SerializeField] private string description = string.Empty;

    [Header("Functional settings")]
    [SerializeField] [Range(0, 60)] protected float time = 10;
    [SerializeField] private bool isInfinite = false;
    [SerializeField] [Range(1, 9)] private int maxStacks = 1;

    [Header("Extra settings")]
    [SerializeField] private SerializableGuid id = new SerializableGuid();

    public void Init(SpellEcsData spellEcsData, Entity casterEntity, ulong casterNetId, FactionType casterFactionType, Entity targetEntity)
    {
        prefabSystem = spellEcsData.prefabSystem;
        server = spellEcsData.server;
        entityManager = spellEcsData.entityManager;

        this.casterEntity = casterEntity;
        this.targetEntity = targetEntity;
        this.casterNetId = casterNetId;
        this.casterFactionType = casterFactionType;
    }

    public abstract void StartEffect(EntityCommandBuffer ecb);
    public abstract void UpdateEffect(EntityCommandBuffer ecb, float deltaTime);
    public abstract void EndEffect(EntityCommandBuffer ecb);

    public IStatusEffect ShallowCopy()
    {
        return (IStatusEffect)MemberwiseClone();
    }

    public void GenerateId()
    {
        id = Guid.NewGuid();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (id.Value == string.Empty)
        {
            GenerateId();
        }

        if(isInfinite)
        {
            time = float.PositiveInfinity;
        }

        var path = AssetDatabase.GetAssetPath(this);

        if (path.Contains("Player"))
        {
            playerOwned = true;
        }
    }
#endif
}
