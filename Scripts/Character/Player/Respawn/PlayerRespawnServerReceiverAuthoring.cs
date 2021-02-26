using DOTSNET;
using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerRespawnServerReceiverAuthoring : MonoBehaviour, SelectiveSystemAuthoring
{
    public Type GetSystemType() { return typeof(PlayerRespawnServerReceiver); }
}

[DisableAutoCreation]
public class PlayerRespawnServerReceiver : NetworkServerMessageSystem<PlayerRespawnMessage>
{
    private EndServerSimulationEntityCommandBufferSystem endServerSimulationEntityCommandBufferSystem;

    protected override void OnCreate()
    {
        base.OnCreate();

        endServerSimulationEntityCommandBufferSystem = World.GetOrCreateSystem<EndServerSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate() { }
    protected override bool RequiresAuthentication() { return true; }
    protected override void OnMessage(int connectionId, PlayerRespawnMessage message)
    {
        if (server.spawned.TryGetValue(message.netId, out Entity playerEntity))
        {
            var ecb = endServerSimulationEntityCommandBufferSystem.CreateCommandBuffer();

            //Remove all status effects, reset stat factors.
            {
                StatusEffectLibrary statusLibrary = StatusEffectLibrary.GetInstance();

                DynamicBuffer<StatusEffectElement> statusEffectElements = EntityManager.GetBuffer<StatusEffectElement>(playerEntity);

                for (int s = 0; s < statusEffectElements.Length; s++)
                {
                    Guid statusEffectId = statusEffectElements[s].statusEffectId;

                    //It is possible that the effect is not registered in the status effect library yet if the player dies the same frame the status effect is applied.
                    if (statusLibrary.InstanceExists(playerEntity, statusEffectId))
                    {
                        statusLibrary.DestroyInstance(playerEntity, statusEffectId);
                    }
                }

                statusEffectElements.Clear();

                StatsComponent statsComponent = EntityManager.GetComponentData<StatsComponent>(playerEntity);
                statsComponent.ResetAllFactors();
                ecb.SetComponent(playerEntity, statsComponent);
            }

            Entities.ForEach((Entity entity, ref NpcHasTargetComponent npcHasTargetComponent) =>
            {
                if (npcHasTargetComponent.targetEntity == entity)
                {
                    ecb.RemoveComponent<NpcHasTargetComponent>(entity);
                    npcHasTargetComponent.targetEntity = Entity.Null;
                }
            }).Run();

            //Teleport to respawn position
            {
                FactionType factionType = EntityManager.GetComponentData<FactionComponent>(playerEntity).factionType;
                float3 position = EntityManager.GetComponentData<Translation>(playerEntity).Value;

                Vector3 respawnPoint = PlayerRespawnPoint.GetNearest(factionType, position);

                //Immediately set the position, otherwise npc's will detect the player at the old position.
                EntityManager.SetComponentData(playerEntity, new Translation { Value = respawnPoint });

                ecb.AddComponent(playerEntity, new DispatchSetPositionMessageComponent { position = respawnPoint });
            }

            //Remove death component
            {
                ecb.RemoveComponent<DeathComponent>(playerEntity);
            }

            //Reset health
            {
                HealthComponent healthComponent = EntityManager.GetComponentData<HealthComponent>(playerEntity);

                healthComponent.currentHealth = healthComponent.maxHealth;

                ecb.SetComponent(playerEntity, healthComponent);

                ecb.AddComponent<DispatchHealthMessageComponent>(playerEntity);
            }

            endServerSimulationEntityCommandBufferSystem.AddJobHandleForProducer(Dependency);
        }
    }
}
