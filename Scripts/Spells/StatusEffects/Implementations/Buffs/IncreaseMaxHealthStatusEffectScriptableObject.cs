using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(fileName = "IncreaseMaxHealthStatusEffect", menuName = "ScriptableObjects/StatusEffects/IncreaseMaxHealth", order = 1)]
public class IncreaseMaxHealthStatusEffectScriptableObject : StatusEffectScriptableObject
{
    [SerializeField] private int maxHealthIncrease = 0;

    public override void StartEffect(EntityCommandBuffer ecb)
    {
        HealthComponent health = entityManager.GetComponentData<HealthComponent>(targetEntity);
        health.maxHealth += maxHealthIncrease;
        health.currentHealth += maxHealthIncrease;
        ecb.SetComponent(targetEntity, health);
    }

    public override void UpdateEffect(EntityCommandBuffer ecb, float deltaTime) { }

    public override void EndEffect(EntityCommandBuffer ecb)
    {
        HealthComponent health = entityManager.GetComponentData<HealthComponent>(targetEntity);
        health.maxHealth -= maxHealthIncrease;
        health.currentHealth = math.clamp(health.currentHealth, 0, health.maxHealth);
        ecb.SetComponent(targetEntity, health);
    }
}