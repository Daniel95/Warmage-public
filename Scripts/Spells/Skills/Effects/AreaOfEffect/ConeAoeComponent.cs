using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[GenerateAuthoringComponent]
public struct ConeAoeComponent : IComponentData
{
    [HideInInspector] public ulong casterNetId;
    [HideInInspector] public FactionType casterFactionType;
    [HideInInspector] public float coneMinDotProduct;
    [HideInInspector] public float3 casterPosition;
    [HideInInspector] public float maxDistance;
    [HideInInspector] public float intervalTime;
    [HideInInspector] public int intervalDamage;
    [HideInInspector] public float intervalTimer;
    [HideInInspector] public float totalTime;
    [HideInInspector] public float3 direction;
    [HideInInspector] public bool casterIsPlayer;

    public ConeAoeComponent(ulong casterNetId,
        bool casterIsPlayer,
        FactionType casterFactionType,
        float3 direction,
        float3 casterPosition,
        float coneMinDotProduct,
        float maxDistance,
        DamageOverTimeData damageOverTimeData,
        float totalTime)
    {
        this.casterIsPlayer = casterIsPlayer;
        this.casterFactionType = casterFactionType;
        this.coneMinDotProduct = coneMinDotProduct;
        this.direction = direction;
        this.maxDistance = maxDistance;
        this.intervalDamage = damageOverTimeData.intervalDamage;
        this.intervalTime = damageOverTimeData.intervalTime;
        this.totalTime = totalTime;
        this.casterNetId = casterNetId;
        this.casterPosition = casterPosition;

        intervalTimer = 0;
    }
}
