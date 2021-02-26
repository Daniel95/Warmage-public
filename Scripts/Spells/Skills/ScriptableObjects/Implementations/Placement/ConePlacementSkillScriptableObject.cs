using DOTSNET;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[CreateAssetMenu(fileName = "ConePlacementSkill", menuName = "ScriptableObjects/Skills/ConePlacementSkill", order = 1)]
public class ConePlacementSkillScriptableObject : SkillScriptableObject, IConePlacementSkill
{
    public float GetConeMinDotProduct() => coneMinDotProduct;
    public float GetMaxDistance() => maxDistance;

    [SerializeField] private NetworkEntityAuthoring areaOfEffectPrefab = null;
    [SerializeField] private FXObject oneShotFXObject = null;
    [SerializeField] private float oneShotFXTime = 2.0f;
    [SerializeField] private DamageOverTimeData damageOverTimeData = new DamageOverTimeData();
    [SerializeField] private float totalTime = 0;
    [SerializeField] private float coneMinDotProduct = 0;
    [SerializeField] private float maxDistance = 0;

    private float3 placePosition;
    private quaternion rotation;
    private float3 casterPosition;

    public void Init(float3 placePosition, quaternion rotation, float3 casterPosition)
    {
        damageOverTimeData.Init(totalTime);

        this.placePosition = placePosition;
        this.casterPosition = casterPosition;
        this.rotation = rotation;
    }

    public override void OnExecute(Entity casterEntity, ulong casterNetId, FactionType casterFactionType, Entity targetEntity, EntityCommandBuffer ecb)
    {
        if (!SpawnEntity(areaOfEffectPrefab, placePosition, rotation, ecb, out Entity entity)) { return; }

        if (SpawnEntity(FXLibrary.GetInstance().fxOneShotEntityPrefab, placePosition, rotation, ecb, out Entity fxOneShotEntity))
        {
            DynamicBuffer<FXOneShotBufferElement> fXBufferElements = ecb.AddBuffer<FXOneShotBufferElement>(fxOneShotEntity);

            fXBufferElements.Add(new FXOneShotBufferElement()
            {
                fxPoolIndex = FXObjectPool.ConePlacementIndex,
                timer = totalTime,
            });

            if (oneShotFXObject != null)
            {
                fXBufferElements.Add(new FXOneShotBufferElement()
                {
                    fxPoolIndex = oneShotFXObject.GetPoolIndex(),
                    timer = oneShotFXTime,
                });
            }
        }

        var direction = math.mul(rotation, new float3(0, 0, 1));

        ConeAoeComponent circleAoeComponent = new ConeAoeComponent(casterNetId,
            IsPlayerOwned(),
            casterFactionType,
            direction,
            casterPosition,
            coneMinDotProduct,
            maxDistance,
            damageOverTimeData,
            totalTime);

        ecb.SetComponent(entity, circleAoeComponent);

        ecb.AddComponent(entity, new OwnerShipComponent { ownerEntity = entity });
    }
}
