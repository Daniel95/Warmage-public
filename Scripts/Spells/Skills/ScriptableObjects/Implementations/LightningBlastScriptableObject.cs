using DOTSNET;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[CreateAssetMenu(fileName = "LightningBlastSkill", menuName = "ScriptableObjects/Skills/LightningBlastSkill", order = 1)]
public class LightningBlastScriptableObject : SkillScriptableObject
{
    [SerializeField] private FXObject chargedHitOneShotFXObject = null;
    [SerializeField] private FXObject unchargedHitOneShotFXObject = null;
    [SerializeField] private float hitFXTimer = 0.5f;
    [SerializeField] private int baseDamage = 100;
    [SerializeField] private int damagePerCharge = 100;
    [SerializeField] [Range(1, 9)] private int maxChargesToConsume = 3;
    [SerializeField] private ChargeStatusEffectScriptableObject chargeStatusEffect;

    public override void OnExecute(Entity casterEntity, ulong casterNetId, FactionType casterFactionType, Entity targetEntity, EntityCommandBuffer ecb)
    {
        DynamicBuffer<StatusEffectElement> statusEffectBuffer = entityManager.GetBuffer<StatusEffectElement>(targetEntity);

        int count = 0;

        for (int i = 0; i < statusEffectBuffer.Length; i++)
        {
            StatusEffectElement statusEffectElement = statusEffectBuffer[i];

            if (statusEffectElement.statusEffectId == chargeStatusEffect.GetId())
            {
                if (statusEffectElement.count > maxChargesToConsume)
                {
                    count = maxChargesToConsume;
                    statusEffectElement.count -= maxChargesToConsume;

                    DynamicBuffer<DispatchStatusEffectMessageElement> dispatchStatusEffectMessageBuffer = entityManager.GetBuffer<DispatchStatusEffectMessageElement>(targetEntity);
                    dispatchStatusEffectMessageBuffer.Add(new DispatchStatusEffectMessageElement
                    {
                        statusEffectId = statusEffectElement.statusEffectId,
                        casterNetId = casterNetId,
                        messageType = StatusEffectMessage.MessageType.Update,
                        timeLeft = statusEffectElement.timeLeft,
                        count = statusEffectElement.count
                    });
                }
                else
                {
                    //Set time left to zero to remove the status effect.
                    statusEffectElement.timeLeft = 0;

                    count = statusEffectBuffer[i].count;
                }

                statusEffectBuffer[i] = statusEffectElement;
                break;
            }
        }

        int damage = baseDamage + count * damagePerCharge;
        SpellHelper.DamageEntity(damage, targetEntity, casterEntity, casterNetId, casterFactionType, IsPlayerOwned(), entityManager);

        float3 targetPosition = entityManager.GetComponentData<Translation>(targetEntity).Value;

        if (count > 0)
        {
            if (SpawnEntity(FXLibrary.GetInstance().fxOneShotEntityPrefab, targetPosition, quaternion.identity, ecb, out Entity fxOneShotEntity))
            {
                DynamicBuffer<FXOneShotBufferElement> fXBufferElements = ecb.AddBuffer<FXOneShotBufferElement>(fxOneShotEntity);
                fXBufferElements.Add(new FXOneShotBufferElement()
                {
                    fxPoolIndex = chargedHitOneShotFXObject.GetPoolIndex(),
                    timer = hitFXTimer,
                });
            }
        }
        else
        {
            if (SpawnEntity(FXLibrary.GetInstance().fxOneShotEntityPrefab, targetPosition, quaternion.identity, ecb, out Entity fxOneShotEntity))
            {
                DynamicBuffer<FXOneShotBufferElement> fXBufferElements = ecb.AddBuffer<FXOneShotBufferElement>(fxOneShotEntity);
                fXBufferElements.Add(new FXOneShotBufferElement()
                {
                    fxPoolIndex = unchargedHitOneShotFXObject.GetPoolIndex(),
                    timer = hitFXTimer,
                });
            }
        }
    }
}