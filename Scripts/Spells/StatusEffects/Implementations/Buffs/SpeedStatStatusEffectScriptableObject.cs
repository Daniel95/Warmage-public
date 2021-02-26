using Unity.Entities;
using UnityEngine;

[CreateAssetMenu(fileName = "SpeedStatStatusEffect", menuName = "ScriptableObjects/StatusEffects/SpeedStat", order = 1)]
public class SpeedStatStatusEffectScriptableObject : StatusEffectScriptableObject
{
    [SerializeField] private float speedFactorChange = 0;

    public override void StartEffect(EntityCommandBuffer ecb)
    {
        StatsComponent statsComponent = entityManager.GetComponentData<StatsComponent>(targetEntity);
        statsComponent.speedFactor += speedFactorChange;
        ecb.SetComponent(targetEntity, statsComponent);
        ecb.AddComponent<DispatchStatsMessageComponent>(targetEntity);
    }

    public override void UpdateEffect(EntityCommandBuffer ecb, float deltaTime) { }

    public override void EndEffect(EntityCommandBuffer ecb)
    {
        StatsComponent statsComponent = entityManager.GetComponentData<StatsComponent>(targetEntity);
        statsComponent.speedFactor -= speedFactorChange;
        ecb.SetComponent(targetEntity, statsComponent);
        ecb.AddComponent<DispatchStatsMessageComponent>(targetEntity);
    }
}