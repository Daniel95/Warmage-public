using DOTSNET;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[CreateAssetMenu(fileName = "SummonNpcSkill", menuName = "ScriptableObjects/Skills/SummonNpc", order = 1)]
public class SummonNpcScriptableObject : SkillScriptableObject
{
    [SerializeField] private NetworkEntityAuthoring spawnPrefab = null;

    public override void OnExecute(Entity casterEntity, ulong casterNetId, FactionType casterFactionType, Entity targetEntity, EntityCommandBuffer ecb)
    {
        float3 casterPosition = entityManager.GetComponentData<Translation>(casterEntity).Value;

        if (SpawnEntity(spawnPrefab, casterPosition, quaternion.identity, ecb, out Entity entity)) 
        {
            ecb.SetComponent(entity, new FactionComponent { factionType = casterFactionType });
        }
    }
}
