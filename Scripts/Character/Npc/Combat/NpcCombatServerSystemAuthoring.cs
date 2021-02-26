using DOTSNET;
using Reese.Nav;
using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[DisallowMultipleComponent]
public class NpcCombatServerSystemAuthoring : MonoBehaviour, SelectiveSystemAuthoring
{
    public Type GetSystemType() => typeof(NpcCombatServerSystem);
}

[ServerWorld]
[UpdateInGroup(typeof(ServerActiveSimulationSystemGroup))]
[DisableAutoCreation]
public class NpcCombatServerSystem : SystemBase
{
    private struct NpcSkillExecuteData
    {
        public Guid skillId;
        public Entity casterEntity;
        public ulong casterNetId;
        public FactionType casterFactionType;
        public Entity targetEntity;
    }

    [AutoAssign] private PrefabSystem prefabSystem = null;
    [AutoAssign] private NetworkServerSystem server = null;

    private EndServerSimulationEntityCommandBufferSystem entityCommandBufferSystem;

    protected override void OnCreate()
    {
        entityCommandBufferSystem = World.GetOrCreateSystem<EndServerSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        //Stores all the skills that need to be executed. Filled in the first loop, used in the second.
        NativeList<NpcSkillExecuteData> npcSkillExecuteDatas = new NativeList<NpcSkillExecuteData>(Allocator.TempJob);

        //Check if any npc can execute a skill
        {
            var random = new Unity.Mathematics.Random((uint)UnityEngine.Random.Range(int.MinValue, int.MaxValue));

            float deltaTime = Time.DeltaTime;

            NativeList<int> availableSkills = new NativeList<int>(Allocator.Temp);

            Entities.WithNone<NpcReturningToSpawnComponent, NpcStartReturnToSpawnComponent>().ForEach((
                ref NpcCombatComponent npcCombatComponent,
                ref DynamicBuffer<NpcSkillElement> skills,
                in Entity entity,
                in FactionComponent factionComponent,
                in NpcHasTargetComponent npcHasTargetComponent,
                in ChannelingComponent channelingComponent,
                in NetworkEntity networkEntity) =>
            {
                if (!npcHasTargetComponent.targetIsNotNull) { return; }

                npcCombatComponent.inAttackRange = npcHasTargetComponent.distanceToTarget <= npcCombatComponent.attackRange;

                if (!npcCombatComponent.inAttackRange) { return; }
                if (channelingComponent.active) { return; }

                int count = 0;

                for (int i = 0; i < skills.Length; i++)
                {
                    NpcSkillElement npcSkillElement = skills[i];
                    npcSkillElement.cooldownTimer -= deltaTime;

                    if (npcSkillElement.cooldownTimer < 0)
                    {
                        npcSkillElement.cooldownTimer += npcSkillElement.cooldown;
                        availableSkills.Add(i);
                        count++;
                    }

                    skills[i] = npcSkillElement;
                }

                npcCombatComponent.globalCooldownTimer -= deltaTime;

                if (npcCombatComponent.globalCooldownTimer < 0)
                {
                    npcCombatComponent.globalCooldownTimer += npcCombatComponent.globalCooldown;

                    if (count != 0)
                    {
                        npcCombatComponent.activeSkillIndex = availableSkills[random.NextInt(0, count)];

                        npcSkillExecuteDatas.Add(new NpcSkillExecuteData
                        {
                            skillId = skills[npcCombatComponent.activeSkillIndex].skillId,
                            casterEntity = entity,
                            casterNetId = networkEntity.netId,
                            casterFactionType = factionComponent.factionType,
                            targetEntity = npcHasTargetComponent.targetEntity
                        });
                    }
                }

                availableSkills.Clear();
            })
            .Run();

            entityCommandBufferSystem.AddJobHandleForProducer(Dependency);

            availableSkills.Dispose();
        }

        //Execute the skills.
        {
            SkillLibrary skillLibrary = SkillLibrary.GetInstance();

            SpellEcsData spellEcsData = new SpellEcsData()
            {
                prefabSystem = prefabSystem,
                server = server,
                entityManager = EntityManager
            };

            var ecb = entityCommandBufferSystem.CreateCommandBuffer();

            for (int i = 0; i < npcSkillExecuteDatas.Length; i++)
            {
                NpcSkillExecuteData npcSkillExecuteData = npcSkillExecuteDatas[i];
                ISkill skill = skillLibrary.GetSkillTemplate(npcSkillExecuteData.skillId);

                if (skill.GetCastTime() > 0)
                {
                    ecb.AddComponent(npcSkillExecuteData.casterEntity, new StartChannelingComponent
                    {
                        time = skill.GetCastTime(),
                        onCompleteEvent = () =>
                        {
                            var ecbTemp = World.GetExistingSystem<EndServerSimulationEntityCommandBufferSystem>().CreateCommandBuffer();

                            skill.Execute(npcSkillExecuteData.casterEntity,
                                npcSkillExecuteData.casterNetId,
                                npcSkillExecuteData.casterFactionType,
                                npcSkillExecuteData.targetEntity,
                                ecbTemp,
                                spellEcsData);
                        }
                    });
                }
                else
                {
                    skill.Execute(npcSkillExecuteData.casterEntity,
                        npcSkillExecuteData.casterNetId,
                        npcSkillExecuteData.casterFactionType,
                        npcSkillExecuteData.targetEntity,
                        ecb,
                        spellEcsData);
                }
            }

            entityCommandBufferSystem.AddJobHandleForProducer(Dependency);
        }

        npcSkillExecuteDatas.Dispose();
    }
}
