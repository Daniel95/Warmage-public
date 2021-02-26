using DOTSNET;
using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics.GraphicsIntegration;
using Unity.Transforms;
using UnityEngine;

[DisallowMultipleComponent]
public class CircleAreaOfEffectServerSystemAuthoring : MonoBehaviour, SelectiveSystemAuthoring
{
    public Type GetSystemType() => typeof(CircleAreaOfEffectServerSystem);
}

[ServerWorld]
[UpdateInGroup(typeof(ServerActiveSimulationSystemGroup))]
[DisableAutoCreation]
public class CircleAreaOfEffectServerSystem : SystemBase
{
    private struct AreaOfEffectDamageData
    {
        public FactionType casterFactionType;
        public int damage;
        public float areaSize;
        public float3 position;
        public Entity casterEntity;
        public ulong casterNetId;
        public bool casterIsPlayer;
    }

    [AutoAssign] private NetworkServerSystem server = null;

    private BeginServerSimulationEntityCommandBufferSystem beginServerSimulationEntityCommandBufferSystem;

    private NativeList<Entity> areaOfEffectsToRemove;

    protected override void OnCreate()
    {
        base.OnCreate();

        areaOfEffectsToRemove = new NativeList<Entity>(40, Allocator.Persistent);

        beginServerSimulationEntityCommandBufferSystem = World.GetOrCreateSystem<BeginServerSimulationEntityCommandBufferSystem>();
    }

    protected override void OnDestroy()
    {
        areaOfEffectsToRemove.Dispose();

        base.OnDestroy();
    }

    protected override void OnUpdate()
    {
        NativeList<AreaOfEffectDamageData> areaOfEffectsDamageData = new NativeList<AreaOfEffectDamageData>(40, Allocator.TempJob);
        NativeList<Entity> _areaOfEffectsToRemove = areaOfEffectsToRemove;

        float deltaTime = Time.DeltaTime;

        //Check which area of effects will do damage
        Entities.ForEach((ref CircleAoeComponent circleAoeComponent,
            in DynamicBuffer<FXBufferElement> fxBufferElements,
            in OwnerShipComponent ownerShipComponent,
            in Entity entity,
            in Translation translation) =>
        {
            circleAoeComponent.totalTime -= deltaTime;

            if(circleAoeComponent.totalTime < 0)
            {
                _areaOfEffectsToRemove.Add(entity);
                return;
            }

            circleAoeComponent.damageIntervalTimer -= deltaTime;

            if (circleAoeComponent.damageIntervalTimer < 0)
            {
                circleAoeComponent.damageIntervalTimer += circleAoeComponent.intervalTime;

                areaOfEffectsDamageData.Add(new AreaOfEffectDamageData 
                {
                    casterEntity = ownerShipComponent.ownerEntity,
                    casterNetId = circleAoeComponent.casterNetId,
                    casterFactionType = circleAoeComponent.casterFactionType,
                    casterIsPlayer = circleAoeComponent.casterIsPlayer,
                    damage = circleAoeComponent.intervalDamage,
                    areaSize = circleAoeComponent.radius,
                    position = translation.Value
                });
            }
        })
        .Run();

        var beginServerEcb = beginServerSimulationEntityCommandBufferSystem.CreateCommandBuffer();

        //Destroy all expired area of effects
        {
            for (int i = 0; i < _areaOfEffectsToRemove.Length; i++)
            {
                NetworkEntityHelper.Destroy(_areaOfEffectsToRemove[i], beginServerEcb, server);
            }

            areaOfEffectsToRemove.Clear();
        }

        beginServerSimulationEntityCommandBufferSystem.AddJobHandleForProducer(Dependency);

        Entities.ForEach((DynamicBuffer<NewDamageElement> damageBufferElements,
            in Translation translation,
            in FactionComponent factionComponent) =>
        {
            for (int i = 0; i < areaOfEffectsDamageData.Length; i++)
            {
                AreaOfEffectDamageData aoeDamageData = areaOfEffectsDamageData[i];

                if (factionComponent.factionType != aoeDamageData.casterFactionType
                    && math.distance(translation.Value, aoeDamageData.position) < aoeDamageData.areaSize)
                {
                    damageBufferElements.Add(new NewDamageElement
                    {
                        damage = aoeDamageData.damage,
                        damagerEntity = aoeDamageData.casterEntity,
                        damagerNetId = aoeDamageData.casterNetId,
                        damagerFactionType = aoeDamageData.casterFactionType,
                        damagerIsPlayer = aoeDamageData.casterIsPlayer,
                    });
                }
            }
        })
        .WithReadOnly(areaOfEffectsDamageData)
        .WithDisposeOnCompletion(areaOfEffectsDamageData)
        .ScheduleParallel();
    }
}
