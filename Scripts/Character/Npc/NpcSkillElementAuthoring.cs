using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class NpcSkillElementAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    [SerializeField] private List<SkillScriptableObject> skills = null;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        DynamicBuffer<NpcSkillElement> npcSkillElements = dstManager.AddBuffer<NpcSkillElement>(entity);

        foreach (SkillScriptableObject skill in skills)
        {
            npcSkillElements.Add(new NpcSkillElement { skillId = skill.GetId(), cooldown = skill.GetCooldown() });
        }
    }
}

public struct NpcSkillElement : IBufferElementData
{
    public Guid skillId;
    public float cooldown;
    public float cooldownTimer;
}
