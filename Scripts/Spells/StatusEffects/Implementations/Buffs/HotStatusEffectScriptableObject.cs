using Unity.Entities;
using UnityEngine;

[CreateAssetMenu(fileName = "HotStatusEffect", menuName = "ScriptableObjects/StatusEffects/Hot", order = 1)]
public class HotStatusEffectScriptableObject : StatusEffectScriptableObject
{
    [SerializeField] private int tickHeal = 0;
    [SerializeField] private int tickAmount = 0;

    private float interval;
    private float timer = 0;

    public override void StartEffect(EntityCommandBuffer ecb)
    {
        interval = time / tickAmount;
        timer = interval;
    }

    public override void UpdateEffect(EntityCommandBuffer ecb, float deltaTime)
    {
        timer -= deltaTime;

        if (timer < 0)
        {
            timer += interval;

            SpellHelper.HealEntity(tickHeal, targetEntity, ecb, entityManager);
        }
    }

    public override void EndEffect(EntityCommandBuffer ecb)
    {
        SpellHelper.HealEntity(tickHeal, targetEntity, ecb, entityManager);
    }
}