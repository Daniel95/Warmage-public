using DOTSNET;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[CreateAssetMenu(fileName = "CirclePlacementSkill", menuName = "ScriptableObjects/Skills/CirclePlacementSkill", order = 1)]
public class CirclePlacementSkillScriptableObject : PositionProjectileSkillScriptableObject, ICirclePlacementSkill
{
    [SerializeField] private NetworkEntityAuthoring areaOfEffectPrefab = null;
    [SerializeField] private FXObject oneShotFXObject = null;
    [SerializeField] private float radius = 8;
    [SerializeField] private DamageOverTimeData damageOverTimeData;
    [SerializeField] private float totalTime = 0;

    private float3 placePosition;

    public float GetRadius() => radius;

    public void Init(float3 placePosition)
    {
        damageOverTimeData.Init(totalTime);
        this.placePosition = placePosition;
    }

    public override void OnExecute(Entity casterEntity, ulong casterNetId, FactionType casterFactionType, Entity targetEntity, EntityCommandBuffer ecb)
    {
        base.OnExecute(casterEntity, casterNetId, casterFactionType, targetEntity, ecb);

        SendProjectileToPosition(placePosition, ecb);
    }

    protected override void OnTargetPosition(float3 position)
    {
        if(!SpawnEntity(areaOfEffectPrefab, placePosition, quaternion.identity, out Entity entity)) { return; }

        if (SpawnEntity(FXLibrary.GetInstance().fxOneShotEntityPrefab, position, quaternion.identity, out Entity fxOneShotEntity))
        {
            DynamicBuffer<FXOneShotBufferElement> fxOneShotBufferElements = entityManager.GetBuffer<FXOneShotBufferElement>(fxOneShotEntity);

            fxOneShotBufferElements.Add(new FXOneShotBufferElement
            {
                    fxPoolIndex = FXObjectPool.CirclePlacementIndex,
                    timer = totalTime,
            });

            if (oneShotFXObject != null)
            {
                fxOneShotBufferElements.Add(new FXOneShotBufferElement
                {
                    fxPoolIndex = oneShotFXObject.GetPoolIndex(),
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