using DOTSNET;
using System;
using Unity.Entities;
using UnityEngine;

[DisallowMultipleComponent]
public class CirclePlacementSkillReceiverServerAuthoring : MonoBehaviour, SelectiveSystemAuthoring
{
    public Type GetSystemType() { return typeof(CirclePlacementSkillReceiverServer); }
}

[DisableAutoCreation]
public class CirclePlacementSkillReceiverServer : NetworkServerMessageSystem<CirclePlacementSkillMessage>
{
    [AutoAssign] private PrefabSystem prefabSystem = null;

    private EndServerSimulationEntityCommandBufferSystem entityCommandBufferSystem;

    protected override void OnCreate()
    {
        base.OnCreate();

        entityCommandBufferSystem = World.GetOrCreateSystem<EndServerSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate() { }
    protected override bool RequiresAuthentication() { return true; }
    protected override void OnMessage(int connectionId, CirclePlacementSkillMessage message)
    {
        if (!server.spawned.TryGetValue(message.netId, out Entity casterEntity)) { Debug.LogWarning("owner not found"); return; }
        if (!server.spawned.TryGetValue(message.targetNetId, out Entity targetEntity)) { Debug.LogWarning("target not found"); return; }

        ISkill skill = SkillLibrary.GetInstance().GetSkillTemplate(message.skillId);

        if (!(skill is ICirclePlacementSkill)) { Debug.LogError("Skill is not a ICirclePlacementSkill"); }

        ICirclePlacementSkill circlePlacementSkill = (ICirclePlacementSkill)skill;
        circlePlacementSkill.Init(message.placePosition);

        var ecb = entityCommandBufferSystem.CreateCommandBuffer();

        SpellEcsData spellEcsData = new SpellEcsData() 
        {
            prefabSystem = prefabSystem,
            server = server,
            entityManager = EntityManager
        };

        if (skill.GetCastTime() > 0)
        {
            ecb.AddComponent(casterEntity, new StartChannelingComponent
            {
                time = skill.GetCastTime(),
                onCompleteEvent = () =>
                {
                    var ecbTemp = entityCommandBufferSystem.CreateCommandBuffer();

                    skill.Execute(casterEntity, message.netId, message.factionType, targetEntity, ecbTemp, spellEcsData);

                    entityCommandBufferSystem.AddJobHandleForProducer(Dependency);
                }
            });
        }
        else
        {
            skill.Execute(casterEntity, message.netId, message.factionType, targetEntity, ecb, spellEcsData);
        }

        entityCommandBufferSystem.AddJobHandleForProducer(Dependency);
    }
}
