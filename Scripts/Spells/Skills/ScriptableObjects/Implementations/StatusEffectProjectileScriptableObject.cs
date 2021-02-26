using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(fileName = "StatusEffectProjectile", menuName = "ScriptableObjects/Skills/StatusEffectProjectileSkill", order = 1)]
public class StatusEffectProjectileScriptableObject : TargetProjectileSkillScriptableObject
{
    [SerializeField] private StatusEffectScriptableObject statusEffectScriptableObject = null;

    protected override void OnTargetReached(float3 position)
    {
        SpellHelper.AddStatusEffect(statusEffectScriptableObject.GetId(), targetEntity, casterEntity, casterNetId, casterFactionType, entityManager);
    }
}