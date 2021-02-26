using Unity.Entities;
using UnityEngine;

[CreateAssetMenu(fileName = "StatusEffectOnSelf", menuName = "ScriptableObjects/Skills/StatusEffectOnSelf", order = 1)]
public class StatusEffectOnSelfScriptableObject : SkillScriptableObject
{
    [SerializeField] private StatusEffectScriptableObject statusEffectScriptableObject = null;

    public override void OnExecute(Entity casterEntity, ulong casterNetId, FactionType casterFactionType, Entity targetEntity, EntityCommandBuffer ecb)
    {
        StatusEffectElement statusEffectElement = new StatusEffectElement(statusEffectScriptableObject, casterEntity, casterNetId, casterFactionType, statusEffectScriptableObject.GetTime());

        DynamicBuffer<StatusEffectElement> statusEffectElements = entityManager.GetBuffer<StatusEffectElement>(casterEntity);

        int index = 0;
        bool isActive = false;
        for (int i = 0; i < statusEffectElements.Length; i++)
        {
            if (statusEffectElements[i].statusEffectId == statusEffectScriptableObject.GetId() &&
                statusEffectElements[i].caster == casterEntity)
            {
                isActive = true;
                index = i;
                break;
            }
        }

        if (!isActive)
        {
            statusEffectElements.Add(statusEffectElement);
        }
        else
        {
            statusEffectElements[index] = statusEffectElement;
        }
    }
}