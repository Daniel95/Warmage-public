using DOTSNET;
using System;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

[DisallowMultipleComponent]
public class NpcDeathServerSystemAuthoring : MonoBehaviour, SelectiveSystemAuthoring
{
    public Type GetSystemType() { return typeof(NpcDeathServerSystem); }
}

[ServerWorld]
[UpdateInGroup(typeof(ServerActiveSimulationSystemGroup))]
[UpdateBefore(typeof(NpcDetectionServerSystem))]
[DisableAutoCreation]
public class NpcDeathServerSystem : SystemBase
{
    private struct DeathInfo
    {
        public Entity deathEntity;
        public FactionType killerFactionType;
    }

    [AutoAssign] private NetworkServerSystem server = null;

    private NativeList<DeathInfo> dyingEntities;
    private BeginServerSimulationEntityCommandBufferSystem beginServerSimulationEntityCommandBufferSystem;
    private EndServerSimulationEntityCommandBufferSystem endServerSimulationEntityCommandBufferSystem;
    private NativeList<Entity> entitiesToRemove;

    protected override void OnCreate()
    {
        base.OnCreate();

        dyingEntities = new NativeList<DeathInfo>(100, Allocator.Persistent);
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
        NativeList<DeathInfo> _dyingEntities = dyingEntities;

        Entities.WithAll<NpcComponent>().ForEach((
            in Entity entity,
            in HealthComponent healthComponent) =>
        {
            if (healthComponent.currentHealth <= 0)
            {
                _dyingEntities.Add(new DeathInfo { deathEntity = entity, killerFactionType = healthComponent.lastDamagerFaction });
            }
        }).Run();

        if (dyingEntities.IsEmpty) { return; }

        //Entities.ForEach((ref NpcDetectionComponent npcDetectionComponent, in Entity entity) =>
        //{
        //    if (npcDetectionComponent.hasTarget) 
        //    {
        //        for (int i = 0; i < _dyingEntities.Length; i++)
        //        {
        //            if (npcDetectionComponent.targetEntity == _dyingEntities[i].deathEntity)
        //            {
        //                //Debug.LogWarning("Reset target of: " + entity + ", target was: " + npcDetectionComponent.targetEntity);

        //                npcDetectionComponent.targetEntity = Entity.Null;
        //            }
        //        }
        //    }
        //})
        //.WithReadOnly(_dyingEntities)
        //.Run();

        var beginServerEcb = beginServerSimulationEntityCommandBufferSystem.CreateCommandBuffer();
        var endServerEcb = endServerSimulationEntityCommandBufferSystem.CreateCommandBuffer();

        /*
        Entities.ForEach((in Translation translation,
            in Entity entity,
            in GoToTargetComponent goToTargetComponent) =>
        {
            for (int i = 0; i < _dyingEntities.Length; i++)
            {
                if (goToTargetComponent.targetEntity == _dyingEntities[i].deathEntity)
                {
                    NetworkEntityHelper.Destroy(entity, beginServerSimEcb, server);

                    //_projectilesToRemove.Add(entity);
                }
            }
        }).WithoutBurst().Run();

        Entities.ForEach((in Translation translation,
            in Entity entity,
            in GoToTargetAngledComponent goToTargetComponent) =>
        {
            for (int i = 0; i < _dyingEntities.Length; i++)
            {
                if (goToTargetComponent.targetEntity == _dyingEntities[i].deathEntity)
                {
                    NetworkEntityHelper.Destroy(entity, beginServerSimEcb, server);
                }
            }
        }).WithoutBurst().Run();
         */

        //Destroy any projectiles that are following this player, or are cast by this player
        {
            NativeList<Entity> _entitiesToRemove = entitiesToRemove;

            Entities.ForEach((in Entity entity,
                in GoToTargetComponent goToTargetComponent) =>
            {
                for (int i = 0; i < _dyingEntities.Length; i++)
                {
                    Entity dyingEntity = _dyingEntities[i].deathEntity;

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
                    Entity dyingEntity = _dyingEntities[i].deathEntity;

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
                    if (ownerShipComponent.ownerEntity == _dyingEntities[i].deathEntity)
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
        }

        Entities.ForEach((Entity entity, ref NpcHasTargetComponent npcHasTargetComponent) =>
        {
            for (int i = 0; i < _dyingEntities.Length; i++)
            {
                if (npcHasTargetComponent.targetEntity == _dyingEntities[i].deathEntity)
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
                        if (newDamageBuffer[i].damagerEntity == _dyingEntities[j].deathEntity)
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
                        if (accumulatedDamageElement[i].damagerEntity == _dyingEntities[j].deathEntity)
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
                        if (statusEffectBuffer[i].caster == _dyingEntities[j].deathEntity)
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
        .WithName("NpcRemoveDamageRecordsAndResetTarget")
        .Run();

        indexesToRemove.Dispose();

        for (int i = 0; i < _dyingEntities.Length; i++)
        {
            DeathInfo deathInfo = _dyingEntities[i];

            if (EntityManager.HasComponent<NpcSpawnPointOccupierComponent>(deathInfo.deathEntity))
            {
                NpcSpawnPointOccupierComponent npcSpawnPointOccupierComponent = EntityManager.GetComponentData<NpcSpawnPointOccupierComponent>(deathInfo.deathEntity);

                Bootstrap.ServerWorld.GetExistingSystem<KeepServerSystem>().OnDeath(npcSpawnPointOccupierComponent, deathInfo.killerFactionType);
            }

            NetworkEntityHelper.Destroy(deathInfo.deathEntity, beginServerEcb, server);
        }

        beginServerSimulationEntityCommandBufferSystem.AddJobHandleForProducer(Dependency);
        endServerSimulationEntityCommandBufferSystem.AddJobHandleForProducer(Dependency);

        dyingEntities.Clear();
    }
}
