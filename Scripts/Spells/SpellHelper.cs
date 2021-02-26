using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public static class SpellHelper
{
    public static void DamageEntity(int damage, Entity targetEntity, Entity casterEntity, ulong casterNetId, FactionType casterFactionType, bool casterIsPlayer, EntityManager entityManager)
    {
        //TODO: Properly fix this error if the entity does not exists
        if (!entityManager.Exists(targetEntity)) { Debug.LogError("DamageEntity 1"); return; }
        if (!entityManager.Exists(casterEntity)) { Debug.LogError("DamageEntity 2"); return; }
        if (!entityManager.HasComponent<NewDamageElement>(targetEntity)) { Debug.LogError("DamageEntity 3"); return; }

        DynamicBuffer<NewDamageElement> newDamageBuffer = entityManager.GetBuffer<NewDamageElement>(targetEntity);

        newDamageBuffer.Add(new NewDamageElement
        {
            damage = damage,
            damagerEntity = casterEntity,
            damagerNetId = casterNetId,
            damagerFactionType = casterFactionType,
            damagerIsPlayer = casterIsPlayer
        });
    }

    public static void HealEntity(int heal, Entity targetEntity, EntityCommandBuffer ecb, EntityManager entityManager)
    {
        if (!entityManager.Exists(targetEntity)) { Debug.LogError("HealEntity 1"); return; }

        HealthComponent health = entityManager.GetComponentData<HealthComponent>(targetEntity);
        health.currentHealth += heal;
        health.currentHealth = math.clamp(health.currentHealth, 0, health.maxHealth);
        ecb.SetComponent(targetEntity, health);
    }

    public static void AddStatusEffect(Guid statusEffectId, Entity targetEntity, Entity casterEntity, ulong casterNetId, FactionType casterFactionType, EntityManager entityManager)
    {
        //TODO: Properly fix this error if the entity does not exists
        if (!entityManager.Exists(targetEntity)) { Debug.LogError("AddStatusEffect 1"); return; }
        if (!entityManager.Exists(casterEntity)) { Debug.LogError("AddStatusEffect 2"); return; }
        if (!entityManager.HasComponent<NewDamageElement>(targetEntity)) { Debug.LogError("AddStatusEffect 3"); return; }

        DynamicBuffer<StatusEffectElement> statusEffectElements = entityManager.GetBuffer<StatusEffectElement>(targetEntity);

        int index = 0;
        bool isActive = false;
        for (int i = 0; i < statusEffectElements.Length; i++)
        {
            if (statusEffectElements[i].statusEffectId == statusEffectId &&
                statusEffectElements[i].caster == casterEntity)
            {
                isActive = true;
                index = i;
                break;
            }
        }

        DynamicBuffer<DispatchStatusEffectMessageElement> dispatchStatusEffectMessageBuffer = entityManager.GetBuffer<DispatchStatusEffectMessageElement>(targetEntity);

        IStatusEffect statusEffect = StatusEffectLibrary.GetInstance().GetStatusEffectTemplate(statusEffectId);

        if (!isActive)
        {
            StatusEffectElement statusEffectElement = new StatusEffectElement(statusEffectId, casterEntity, casterNetId, casterFactionType, statusEffect.GetTime());

            statusEffectElements.Add(statusEffectElement);
        }
        else
        {
            StatusEffectElement statusEffectElement = statusEffectElements[index];

            statusEffectElement.timeLeft = statusEffect.GetTime();

            if(statusEffect.GetIsStackable() && statusEffectElement.count < statusEffect.GetMaxStacks())
            {
                statusEffectElement.count++;
            }

            statusEffectElements[index] = statusEffectElement;

            dispatchStatusEffectMessageBuffer.Add(new DispatchStatusEffectMessageElement
            {
                statusEffectId = statusEffectId,
                casterNetId = casterNetId,
                messageType = StatusEffectMessage.MessageType.Update,
                timeLeft = statusEffectElement.timeLeft,
                count = statusEffectElement.count
            });
        }
    }
}
