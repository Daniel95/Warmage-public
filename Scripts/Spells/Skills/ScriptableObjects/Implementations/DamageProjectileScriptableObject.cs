using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(fileName = "DamageProjectile", menuName = "ScriptableObjects/Skills/DamageProjectile", order = 1)]
public class DamageProjectileScriptableObject : TargetProjectileSkillScriptableObject
{
    [SerializeField] private int damage = 0;
    [SerializeField] private StatusEffectScriptableObject statusEffectScriptableObject = null;

    protected override void OnTargetReached(float3 position)
    {
        SpellHelper.DamageEntity(damage, 
            targetEntity, 
            casterEntity, 
            casterNetId, 
            casterFactionType, 
            IsPlayerOwned(), 
            entityManager);

        if(statusEffectScriptableObject != null)
        {
            SpellHelper.AddStatusEffect(statusEffectScriptableObject.GetId(),
                targetEntity, 
                casterEntity, 
                casterNetId, 
                casterFactionType,
                entityManager);
        } 
    }
}