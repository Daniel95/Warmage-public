using DOTSNET;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[CreateAssetMenu(fileName = "TargetAoeProjectileSkill", menuName = "ScriptableObjects/Skills/TargetAoeProjectile", order = 1)]
public class TargetAoeProjectileScriptableObject : TargetProjectileSkillScriptableObject
{
    [SerializeField] private NetworkEntityAuthoring areaOfEffectPrefab = null;
    [SerializeField] private FXObject oneShotFXPrefab = null;
    [SerializeField] private float radius = 8;
    [SerializeField] private DamageOverTimeData damageOverTimeData = new DamageOverTimeData();
    [SerializeField] private float totalTime = 5;

    public float GetRadius() => radius;

    protected override void OnTargetReached(float3 position)
    {
        if (!SpawnEntity(areaOfEffectPrefab, position, quaternion.identity, out Entity entity)) { return; }


        if (SpawnEntity(FXLibrary.GetInstance().fxOneShotEntityPrefab, position, quaternion.identity, out Entity fxOneShotEntity))
        {
            DynamicBuffer<FXOneShotBufferElement> fxOneShotBufferElements = entityManager.GetBuffer<FXOneShotBufferElement>(fxOneShotEntity);

            fxOneShotBufferElements.Add(new FXOneShotBufferElement
            {
                fxPoolIndex = FXObjectPool.CirclePlacementIndex,
                timer = totalTime,
            });

            if(oneShotFXPrefab != null)
            {
                fxOneShotBufferElements.Add(new FXOneShotBufferElement
                {
                    fxPoolIndex = oneShotFXPrefab.GetPoolIndex(),
                    timer = totalTime,
                });
            }
        }

        damageOverTimeData.Init(totalTime);

        CircleAoeComponent circleAreaOfEffectComponent = new CircleAoeComponent(casterNetId,
            IsPlayerOwned(),
            casterFactionType, 
            radius, 
            damageOverTimeData, 
            totalTime);

        entityManager.SetComponentData(entity, circleAreaOfEffectComponent);

        entityManager.AddComponentData(entity, new OwnerShipComponent { });
    }
}
