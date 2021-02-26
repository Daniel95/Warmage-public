using DOTSNET;
using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerDeathServerSystemAuthoring : MonoBehaviour, SelectiveSystemAuthoring
{
    public Type GetSystemType() { return typeof(PlayerDeathServerSystem); }

    [SerializeField] private int playerDeathXpDropAmount = 100;

    private void Awake()
    {
        PlayerDeathServerSystem system = Bootstrap.ServerWorld.GetExistingSystem<PlayerDeathServerSystem>();

        system.playerDeathXpDropAmount = playerDeathXpDropAmount;
    }
}

[ServerWorld]
[UpdateInGroup(typeof(ServerActiveSimulationSystemGroup))]
[UpdateBefore(typeof(NpcDetectionServerSystem))]
[DisableAutoCreation]
public class PlayerDeathServerSystem : SystemBase
{
    public int playerDeathXpDropAmount;

    [AutoAssign] private NetworkServerSystem networkServerSystem = null;

    private BeginServerSimulationEntityCommandBufferSystem beginServerSimulationEntityCommandBufferSystem;
    private EndServerSimulationEntityCommandBufferSystem endServerSimulationEntityCommandBufferSystem;
    private NativeList<Entity> dyingEntities;
    private NativeList<Entity> entitiesToRemove;

    protected override void OnCreate()
    {
        base.OnCreate();

        dyingEntities = new NativeList<Entity>(100, Allocator.Persistent);
        entitiesToRemove = new NativeList<Entity>(100, Allocator.Persistent);

        beginServerSimulationEntityCommandBufferSystem = World.GetOrCreateSystem<BeginServerSimulationEntityCommandBufferSystem>();
        endServerSimulationEntityCommandBufferSystem = World.GetOrCreateSystem<EndServerSimulationEntityCommandBufferSystem>();
    }

    protected override void OnDestroy()
    {
        dyingEntities.Dispose();
        entitiesToRemove.Dispose();

        base.OnDestroy();
    }

    protected override void OnUpdate()
    {
        NativeList<Entity> _dyingEntities = dyingEntities;

        NetworkServerSystem server = networkServerSystem;
        var endServerEcb = endServerSimulationEntityCommandBufferSystem.CreateCommandBuffer();

        //Check for any dying players
        {
            Entities.WithNone<DeathComponent>().WithAll<PlayerComponent>().ForEach((
                in HealthComponent healthComponent,
                in Entity entity) => 
            {
                if(healthComponent.currentHealth <= 0)
                {
                    _dyingEntities.Add(entity);

                    endServerEcb.AddComponent<DeathComponent>(entity);
                    endServerEcb.AddComponent<DispatchPlayerDeathMessageComponent>(entity);
                }   
            }).Run();
        }

        if (dyingEntities.IsEmpty) { return; }

        //Destroy any projectiles that are following this player, or are cast by this player
        {
            var beginServerEcb = beginServerSimulationEntityCommandBufferSystem.CreateCommandBuffer();

            NativeList<Entity> _entitiesToRemove = entitiesToRemove;

            Entities.ForEach((in Entity entity,
                in GoToTargetComponent goToTargetComponent) =>
            {
                for (int i = 0; i < _dyingEntities.Length; i++)
                {
                    Entity dyingEntity = _dyingEntities[i];

                    if (goToTargetComponent.targetEntity == dyingEntity)
                    {
                        _entitiesToRemove.Add(entity);
                    }
                }
            }).Run();

            Entities.ForEach((in Entity entity,
                in GoToTargetAngledComponent goToTargetAngledComponent) =>
            {
                for (int i = 0; i < _dyingEntities.Length; i++)
                {
                    Entity dyingEntity = _dyingEntities[i];

                    if (goToTargetAngledComponent.targetEntity == dyingEntity)
                    {
                        _entitiesToRemove.Add(entity);
                    } 
                }
            }).Run();

            Entities.ForEach((in Entity entity,
                in OwnerShipComponent ownerShipComponent) =>
            {
                for (int i = 0; i < _dyingEntities.Length; i++)
                {
                    if (ownerShipComponent.ownerEntity == _dyingEntities[i])
                    {
                        _entitiesToRemove.Add(entity);
                    }
                }
            }).Run();

            for (int i = 0; i < entitiesToRemove.Length; i++)
            {
                NetworkEntityHelper.Destroy(entitiesToRemove[i], beginServerEcb, server);
            }
            entitiesToRemove.Clear();

            beginServerSimulationEntityCommandBufferSystem.AddJobHandleForProducer(Dependency);
        }


        Entities.ForEach((Entity entity, ref NpcHasTargetComponent npcHasTargetComponent) =>
        {
            for (int i = 0; i < _dyingEntities.Length; i++)
            {
                if (npcHasTargetComponent.targetEntity == _dyingEntities[i])
                {
                    npcHasTargetComponent.targetEntity = Entity.Null;
                    endServerEcb.RemoveComponent<NpcHasTargetComponent>(entity);
                }
            }
        }).Run();

        NativeList<int> indexesToRemove = new NativeList<int>(Allocator.Temp);

        Entities.ForEach((DynamicBuffer<NewDamageElement> newDamageBuffer,
            DynamicBuffer<AccumulatedDamageElement> accumulatedDamageElement,
            DynamicBuffer<StatusEffectElement> statusEffectBuffer) =>
        {
            //Remove new damage done by this entity
            {
                for (int i = 0; i < newDamageBuffer.Length; i++)
                {
                    for (int j = 0; j < _dyingEntities.Length; j++)
                    {
                        if (newDamageBuffer[i].damagerEntity == _dyingEntities[j])
                        {
                            indexesToRemove.Add(i);
                        }
                    }
                }

                for (int i = indexesToRemove.Length - 1; i >= 0; i--)
                {
                    newDamageBuffer.RemoveAt(indexesToRemove[i]);
                }
                indexesToRemove.Clear();
            }

            //Remove accumulated damage done by this entity
            {
                for (int i = 0; i < accumulatedDamageElement.Length; i++)
                {
                    for (int j = 0; j < _dyingEntities.Length; j++)
                    {
                        if (accumulatedDamageElement[i].damagerEntity == _dyingEntities[j])
                        {
                            indexesToRemove.Add(i);
                        }
                    }
                }

                for (int i = indexesToRemove.Length - 1; i >= 0; i--)
                {
                    accumulatedDamageElement.RemoveAt(indexesToRemove[i]);
                }
                indexesToRemove.Clear();
            }

            //Remove status effects applied by this entity
            {
                for (int i = 0; i < statusEffectBuffer.Length; i++)
                {
                    for (int j = 0; j < _dyingEntities.Length; j++)
                    {
                        if (statusEffectBuffer[i].caster == _dyingEntities[j])
                        {
                            StatusEffectElement statusEffectElement = statusEffectBuffer[i];
                            statusEffectElement.timeLeft = 0;
                            statusEffectBuffer[i] = statusEffectElement;
                        }
                    }
                }
            }
        })
        .WithReadOnly(_dyingEntities)
        .WithName("PlayerRemoveDamageRecordsAndResetTarget")
        .Run();

        indexesToRemove.Dispose();


        StatusEffectLibrary statusLibrary = StatusEffectLibrary.GetInstance();

        //Handle player death
        {
            for (int i = 0; i < dyingEntities.Length; i++)
            {
                Entity dyingEntity = dyingEntities[i];

                //Give XP to all players who participated in killing this player
                {
                    DynamicBuffer<AccumulatedDamageElement> receivedDamageBuffer = EntityManager.GetBuffer<AccumulatedDamageElement>(dyingEntity);

                    int totalDamage = 0;

                    for (int j = 0; j < receivedDamageBuffer.Length; j++)
                    {
                        if(receivedDamageBuffer[i].isPlayer)
                        {
                            totalDamage += receivedDamageBuffer[j].damage;
                        }
                    }

                    for (int j = 0; j < receivedDamageBuffer.Length; j++)
                    {
                        AccumulatedDamageElement receivedDamageElement = receivedDamageBuffer[i];

                        if (receivedDamageElement.isPlayer && EntityManager.Exists(receivedDamageElement.damagerEntity))
                        {
                            float damageFactor = totalDamage / receivedDamageElement.damage;

                            int xpGained = (int)math.ceil(damageFactor * playerDeathXpDropAmount);

                            endServerEcb.AddComponent(receivedDamageElement.damagerEntity, new DispatchXpGainedMessageComponent 
                            {
                                amount = xpGained
                            });
                        }
                    }
                }

                //Remove all status effects, reset stat factors.
                {
                    DynamicBuffer<StatusEffectElement> statusEffectElements = EntityManager.GetBuffer<StatusEffectElement>(dyingEntity);

                    ulong netId = EntityManager.GetComponentData<NetworkEntity>(dyingEntity).netId;

                    for (int s = 0; s < statusEffectElements.Length; s++)
                    {
                        Guid statusEffectId = statusEffectElements[s].statusEffectId;

                        //It is possible that the effect is not registered in the status effect library yet if the player dies the same frame the status effect is applied.
                        if(statusLibrary.InstanceExists(dyingEntity, statusEffectId))
                        {
                            statusLibrary.DestroyInstance(dyingEntity, statusEffectId);
                        }
                    }
                    
                    statusEffectElements.Clear();

                    StatsComponent statsComponent = EntityManager.GetComponentData<StatsComponent>(dyingEntity);
                    statsComponent.ResetAllFactors();
                    EntityManager.SetComponentData(dyingEntity, statsComponent);

                }
            }
        }

        endServerSimulationEntityCommandBufferSystem.AddJobHandleForProducer(Dependency);

        dyingEntities.Clear();
    }
}
