using DOTSNET;
using System;
using Unity.Entities;
using UnityEngine;

[DisallowMultipleComponent]
public class DamageBufferServerSystemAuthoring : MonoBehaviour, SelectiveSystemAuthoring
{
    public Type GetSystemType() { return typeof(DamageBufferServerSystem); }

    [SerializeField] private float damageChannelingFactorDecrease = 0.33f;

    private void Awake()
    {
        DamageBufferServerSystem system = Bootstrap.ServerWorld.GetExistingSystem<DamageBufferServerSystem>();

        system.damageChannelingFactorDecrease = damageChannelingFactorDecrease;
    }
}

[ServerWorld]
[UpdateInGroup(typeof(ServerActiveSimulationSystemGroup))]
[DisableAutoCreation]
public class DamageBufferServerSystem : SystemBase
{
    [SerializeField] public float damageChannelingFactorDecrease;

    private EndServerSimulationEntityCommandBufferSystem entityCommandBufferSystem;

    protected override void OnCreate()
    {
        base.OnCreate();

        entityCommandBufferSystem = World.GetOrCreateSystem<EndServerSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        Entities.ForEach((ref NpcHasTargetComponent npcHasTargetComponent,
            in DynamicBuffer<NewDamageElement> newDamageBuffer,
            in DynamicBuffer<AccumulatedDamageElement> accumulatedDamageBuffer) =>
        {
            if(npcHasTargetComponent.targetIsNotNull && newDamageBuffer.IsEmpty) { return; }

            Entity highestDamagerEntity = Entity.Null;
            int highestDamage = -1;

            for (int i = 0; i < accumulatedDamageBuffer.Length; i++)
            {
                if(accumulatedDamageBuffer[i].damage > highestDamage)
                {
                    highestDamagerEntity = accumulatedDamageBuffer[i].damagerEntity;
                    highestDamage = accumulatedDamageBuffer[i].damage;
                }
            }
        
            if (highestDamagerEntity != Entity.Null)
            {
                npcHasTargetComponent.targetEntity = highestDamagerEntity;
            } 
        })
        .WithName("NpcUpdateTarget")
        .Run();

        /*
        Entities.ForEach((ChannelingComponent channelingComponent,
            in DynamicBuffer<NewDamageBufferElement> newDamageBuffer,
            in Entity entity) =>
        {
            if (newDamageBuffer.IsEmpty || !channelingComponent.active) { return; }

            float timeIncrease = channelingComponent.totalTime * damageChannelingFactorDecrease;

            channelingComponent.timeLeft += timeIncrease;

            if (channelingComponent.timeLeft > channelingComponent.totalTime)
            {
                ecb.AddComponent<InterruptChannelingComponent> (entity);

                channelingComponent.timeLeft = channelingComponent.totalTime;
            }
            else
            {
                ecb.AddComponent(entity, new DispatchChannelBarMessageComponent
                {
                    messageType = ChannelBarMessage.MessageType.UpdateTime,
                    time = channelingComponent.timeLeft
                });
            }
        })
        .WithName("InterruptChanneling")
        .WithoutBurst()
        .Run();
         */

        var ecb = entityCommandBufferSystem.CreateCommandBuffer();

        Entities.ForEach((
            DynamicBuffer<NewDamageElement> newDamageBuffer,
            DynamicBuffer<AccumulatedDamageElement> accumulatedDamageBuffer,
            DynamicBuffer<DispatchDamageMessageElement> dispatchDamageMessageBuffer,
            ref HealthComponent healthComponent,
            in Entity entity) =>
        {
            if (newDamageBuffer.IsEmpty) { return; }

            for (int i = 0; i < newDamageBuffer.Length; i++)
            {
                NewDamageElement newDamageElement = newDamageBuffer[i];
                healthComponent.currentHealth -= newDamageElement.damage;
                healthComponent.lastDamagerFaction = newDamageElement.damagerFactionType;

                bool foundReceivedDamageElement = false;

                for (int j = 0; j < accumulatedDamageBuffer.Length; j++)
                {
                    AccumulatedDamageElement damagedBufferElement = accumulatedDamageBuffer[j];
                    if (accumulatedDamageBuffer[j].damagerEntity == newDamageElement.damagerEntity)
                    {
                        damagedBufferElement.damage += newDamageElement.damage;

                        foundReceivedDamageElement = true;

                        accumulatedDamageBuffer[j] = damagedBufferElement;

                        break;
                    }
                }

                if (!foundReceivedDamageElement)
                {
                    accumulatedDamageBuffer.Add(new AccumulatedDamageElement
                    {
                        damage = newDamageElement.damage,
                        damagerEntity = newDamageElement.damagerEntity,
                        isPlayer = newDamageElement.damagerIsPlayer
                    });
                }

                dispatchDamageMessageBuffer.Add(new DispatchDamageMessageElement
                { 
                    damage = newDamageElement.damage, 
                    damagerNetId = newDamageElement.damagerNetId 
                });
            }

            ecb.AddComponent<DispatchHealthMessageComponent>(entity);

            newDamageBuffer.Clear();
        })
        .WithName("ApplyDamage")
        .Run();

        entityCommandBufferSystem.AddJobHandleForProducer(Dependency);
    }
}
