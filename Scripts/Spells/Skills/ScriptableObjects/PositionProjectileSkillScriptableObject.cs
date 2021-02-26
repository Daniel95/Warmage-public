using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public abstract class PositionProjectileSkillScriptableObject : SkillScriptableObject
{
    protected private Entity casterEntity { get; private set; }
    protected Entity targetEntity { get; private set; }

    protected ulong casterNetId { get; private set; }
    protected FactionType casterFactionType { get; private set; }

    [SerializeField] private FXObject projectileFXObject = null;
    [SerializeField] private FXObject hitOneShotFXObject = null;
    [SerializeField] private float hitFXTimer = 0.5f;
    [SerializeField] private float projectileSpeed = 12;
    [SerializeField] private bool projectileAngled = false;
    [SerializeField] private float maxProjectileSideAngle = 20.0f;
    [SerializeField] private float maxProjectileUpAngle = 15.0f;


    public override void OnExecute(Entity casterEntity, ulong casterNetId, FactionType casterFactionType, Entity targetEntity, EntityCommandBuffer ecb)
    {
        this.casterEntity = casterEntity;
        this.targetEntity = targetEntity;
        this.casterNetId = casterNetId;
        this.casterFactionType = casterFactionType;
    }

    protected abstract void OnTargetPosition(float3 position);

    private void OnTargetPositionInternal(float3 position)
    {
        if (hitOneShotFXObject != null && SpawnEntity(FXLibrary.GetInstance().fxOneShotEntityPrefab, position, quaternion.identity, out Entity fxOneShotEntity))
        {
            DynamicBuffer<FXOneShotBufferElement> fXBufferElements = entityManager.GetBuffer<FXOneShotBufferElement>(fxOneShotEntity);
            fXBufferElements.Add(new FXOneShotBufferElement()
            {
                fxPoolIndex = hitOneShotFXObject.GetPoolIndex(),
                timer = hitFXTimer,
            });
        }

        OnTargetPosition(position);
    }

    protected bool SendProjectileToPosition(float3 targetPosition, EntityCommandBuffer ecb)
    {
        float3 ownerPosition = entityManager.GetComponentData<Translation>(casterEntity).Value;

        if (SpawnEntity(FXLibrary.GetInstance().fxProjectilePrefab, ownerPosition, quaternion.identity, ecb, out Entity projectileEntity))
        {
            if (projectileAngled)
            {
                ecb.AddComponent(projectileEntity, new GoToPositionAngledComponent
                {
                    speed = projectileSpeed,
                    targetPosition = targetPosition,
                    randomSideAngle = UnityEngine.Random.Range(-maxProjectileSideAngle, maxProjectileSideAngle),
                    randomUpAngle = UnityEngine.Random.Range(0.0f, maxProjectileUpAngle),
                });
            }
            else
            {
                ecb.AddComponent(projectileEntity, new GoToPositionComponent
                {
                    speed = projectileSpeed,
                    targetPosition = targetPosition
                });
            }

            ecb.AddComponent(projectileEntity, new OwnerShipComponent { ownerEntity = casterEntity });
            ecb.AddComponent(projectileEntity, new OnGoToCompletedComponent { onCompleted = OnTargetPositionInternal });

            if (projectileFXObject != null)
            {
                int fxPoolIndex = projectileFXObject.GetPoolIndex();

                ecb.AddComponent(projectileEntity, new FXEntityAddOnClientComponent { fxPoolIndex = fxPoolIndex });

                DynamicBuffer<FXBufferElement> fXBufferElements = ecb.AddBuffer<FXBufferElement>(projectileEntity);
                fXBufferElements.Add(new FXBufferElement() { fxPoolIndex = fxPoolIndex });
            }

            return true;
        }

        return false;
    }
}
