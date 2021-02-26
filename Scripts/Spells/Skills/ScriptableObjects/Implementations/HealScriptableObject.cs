using Unity.Entities;
using UnityEngine;

[CreateAssetMenu(fileName = "SelfHeal", menuName = "ScriptableObjects/Skills/SelfHealSkill", order = 1)]
public class HealScriptableObject : SkillScriptableObject
{
    [SerializeField] private int heal = 0;

    public override void OnExecute(Entity casterEntity, ulong casterNetId, FactionType factionType, Entity targetEntity, EntityCommandBuffer ecb)
    {
        SpellHelper.HealEntity(heal, casterEntity, ecb, entityManager);
    }
}