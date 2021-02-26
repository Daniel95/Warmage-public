using Unity.Entities;
using UnityEngine;

[GenerateAuthoringComponent]
public struct CircleAoeComponent : IComponentData
{
    [HideInInspector] public ulong casterNetId;
    [HideInInspector] public FactionType casterFactionType;
    [HideInInspector] public float radius;
    [HideInInspector] public float intervalTime;
    [HideInInspector] public float damageIntervalTimer;
    [HideInInspector] public int intervalDamage;
    [HideInInspector] public float totalTime;
    [HideInInspector] public bool casterIsPlayer;

    public CircleAoeComponent(ulong casterNetId,
        bool casterIsPlayer,
        FactionType casterFactionType,
        float radius,
        DamageOverTimeData damageOverTimeData,
        float totalTime)
    {
        this.casterNetId = casterNetId;
        this.casterIsPlayer = casterIsPlayer;
        this.casterFactionType = casterFactionType;
        this.radius = radius;
        this.intervalDamage = damageOverTimeData.intervalDamage;
        this.intervalTime = damageOverTimeData.intervalTime;
        this.totalTime = totalTime;

        damageIntervalTimer = 0;
    }

    //public void Init(Entity casterEntity, 
    //    ulong casterNetId, 
    //    bool casterIsPlayer, 
    //    FactionType casterFactionType, 
    //    float radius, 
    //    DamageOverTimeData damageOverTimeData, 
    //    float totalTime)
    //{
    //    this.casterEntity = casterEntity;
    //    this.casterNetId = casterNetId;
    //    this.casterIsPlayer = casterIsPlayer;
    //    this.casterFactionType = casterFactionType;
    //    this.radius = radius;
    //    this.intervalDamage = damageOverTimeData.intervalDamage;
    //    this.intervalTime = damageOverTimeData.intervalTime;
    //    this.totalTime = totalTime;
    //}
}
