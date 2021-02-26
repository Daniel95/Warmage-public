using Unity.Entities;
using UnityEngine;

[CreateAssetMenu(fileName = "Dot", menuName = "ScriptableObjects/StatusEffects/Dot", order = 1)]
public class DotStatusEffectScriptableObject : StatusEffectScriptableObject
{
    [SerializeField] private DamageOverTimeData damageOverTimeData;

    private float timer;

    public override void StartEffect(EntityCommandBuffer ecb)
    {
        damageOverTimeData.Init(time);
    }

    public override void UpdateEffect(EntityCommandBuffer ecb, float deltaTime)
    {
        timer -= deltaTime;

        if (timer < 0)
        {
            timer += damageOverTimeData.intervalTime;
            SpellHelper.DamageEntity(damageOverTimeData.intervalDamage, targetEntity, casterEntity, casterNetId, casterFactionType, playerOwned, entityManager);
        }
    }

    public override void EndEffect(EntityCommandBuffer ecb) { }
}
