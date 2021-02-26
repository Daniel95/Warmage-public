using DOTSNET;
using System;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

[DisallowMultipleComponent]
public class StatusEffectServerSystemAuthoring : MonoBehaviour, SelectiveSystemAuthoring
{
    public Type GetSystemType() => typeof(StatusEffectServerSystem);
}

[ServerWorld]
[UpdateInGroup(typeof(ServerActiveSimulationSystemGroup))]
[DisableAutoCreation]
public class StatusEffectServerSystem : SystemBase
{
    [AutoAssign] private PrefabSystem prefabSystem = null;
    [AutoAssign] private NetworkServerSystem networkServerSystem = null;
    [AutoAssign] private UpdateStatusEffectServerDispatcher updateStatusEffectServerDispatcher = null;

    private NativeList<int> indexesToRemove;
    private EndServerSimulationEntityCommandBufferSystem entityCommandBufferSystem;

    protected override void OnCreate()
    {
        base.OnCreate();

        indexesToRemove = new NativeList<int>(Allocator.Persistent);

        entityCommandBufferSystem = World.GetOrCreateSystem<EndServerSimulationEntityCommandBufferSystem>();
    }

    protected override void OnDestroy()
    {
        indexesToRemove.Dispose();
        base.OnDestroy();
    }

    protected override void OnUpdate()
    {
        var ecb = entityCommandBufferSystem.CreateCommandBuffer();

        StatusEffectLibrary statusLibrary = StatusEffectLibrary.GetInstance();

        float deltaTime = Time.DeltaTime;

        NativeList<int> _indexesToRemove = indexesToRemove;

        UpdateStatusEffectServerDispatcher _updateStatusEffectServerDispatcher = updateStatusEffectServerDispatcher;

        SpellEcsData spellEcsData = new SpellEcsData()
        {
            prefabSystem = prefabSystem,
            server = networkServerSystem,
            entityManager = EntityManager,
        };

        Entities.ForEach((DynamicBuffer <StatusEffectElement> statusEffectBuffer,
            DynamicBuffer<DispatchStatusEffectMessageElement> dispatchStatusEffectMessageBuffer,
            in NetworkEntity networkEntity,
            in Entity entity) => 
        {
            for (int i = 0; i < statusEffectBuffer.Length; i++)
            {
                StatusEffectElement statusEffectElement = statusEffectBuffer[i];

                if (!statusLibrary.InstanceExists(entity, statusEffectElement.statusEffectId))
                {
                    IStatusEffect statusEffect = statusLibrary.Instantiate(entity, statusEffectElement.statusEffectId);
                    statusEffect.Init(spellEcsData, statusEffectElement.caster, statusEffectElement.casterNetId, statusEffectElement.casterFactionType, entity);
                    statusEffect.StartEffect(ecb);

                    dispatchStatusEffectMessageBuffer.Add(new DispatchStatusEffectMessageElement
                    {
                        statusEffectId = statusEffectElement.statusEffectId,
                        casterNetId = statusEffectElement.casterNetId,
                        timeLeft = statusEffectElement.timeLeft,
                        count = statusEffectElement.count,
                        messageType = StatusEffectMessage.MessageType.Add
                    });
                }
                else
                {
                    IStatusEffect statusEffect = statusLibrary.GetInstance(entity, statusEffectElement.statusEffectId);
                    statusEffectElement.timeLeft -= deltaTime;

                    if (statusEffectElement.timeLeft > 0)
                    {
                        statusEffect.UpdateEffect(ecb, deltaTime);
                    }
                    else
                    {
                        statusEffect.EndEffect(ecb);
                        statusLibrary.DestroyInstance(entity, statusEffectElement.statusEffectId);

                        dispatchStatusEffectMessageBuffer.Add(new DispatchStatusEffectMessageElement
                        {
                            statusEffectId = statusEffectElement.statusEffectId,
                            casterNetId = statusEffectElement.casterNetId,
                            count = statusEffectElement.count,
                            messageType = StatusEffectMessage.MessageType.Remove
                        });

                        _indexesToRemove.Add(i);
                    }
                }

                statusEffectBuffer[i] = statusEffectElement;
            }

            for (int i = _indexesToRemove.Length - 1; i >= 0; i--)
            {
                statusEffectBuffer.RemoveAt(_indexesToRemove[i]);
            }
            _indexesToRemove.Clear();
        })
        .WithoutBurst()
        .Run();

        indexesToRemove.Clear();
        entityCommandBufferSystem.AddJobHandleForProducer(Dependency);
    }
}