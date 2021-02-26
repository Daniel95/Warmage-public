using DOTSNET;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[CreateAssetMenu(fileName = "TargetAoeSkill", menuName = "ScriptableObjects/Skills/TargetAoe", order = 1)]
public class TargetAoeScriptableObject : SkillScriptableObject
{
    [SerializeField] private NetworkEntityAuthoring areaOfEffectPrefab = null;
    [SerializeField] private FXObject fxObject = null;
    [SerializeField] private float radius = 8;
    [SerializeField] private DamageOverTimeData damageOverTimeData = new DamageOverTimeData();
    [SerializeField] private float totalTime = 0;

    public override void OnExecute(Entity casterEntity, ulong casterNetId, FactionType casterFactionType, Entity targetEntity, EntityCommandBuffer ecb)
    {
        float3 targetPosition = entityManager.GetComponentData<Translation>(targetEntity).Value;

        if (!SpawnEntity(areaOfEffectPrefab, targetPosition, quaternion.identity, ecb, out Entity entity)) { return; }

        if(fxObject != null)
        {
            int fxPoolIndex = fxObject.GetPoolIndex();
            ecb.AddComponent(entity, new FXEntityAddOnClientComponent { fxPoolIndex = fxPoolIndex });

            DynamicBuffer<FXBufferElement> fXBufferElements = ecb.AddBuffer<FXBufferElement>(entity);
            fXBufferElements.Add(new FXBufferElement() { fxPoolIndex = fxPoolIndex });
        }

        damageOverTimeData.Init(totalTime);

        CircleAoeComponent circleAreaOfEffectComponent = new CircleAoeComponent(casterNetId,
            IsPlayerOwned(), 
            casterFactionType, 
            radius, 
            damageOverTimeData, 
            totalTime);

        ecb.SetComponent(entity, circleAreaOfEffectComponent);

        ecb.AddComponent(entity, new OwnerShipComponent { ownerEntity = entity });
    }
}