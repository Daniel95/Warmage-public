using DOTSNET;
using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[DisallowMultipleComponent]
public class ConeAoeServerSystemAuthoring : MonoBehaviour, SelectiveSystemAuthoring
{
    public Type GetSystemType() => typeof(ConeAoeServerSystem);
}

[ServerWorld]
[UpdateInGroup(typeof(ServerActiveSimulationSystemGroup))]
[DisableAutoCreation]
public class ConeAoeServerSystem : SystemBase
{
    private struct AreaOfEffectDamageData
    {
        public FactionType casterFactionType;
        public int damage;
        public float3 position;
        public float3 direction;
        public float minConeDotProduct;
        public float maxDistance;
        public float3 casterPosition;
        public Entity casterEntity;
        public bool casterIsPlayer;
        public ulong casterNetId;
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
        Entities.ForEach((ref ConeAoeComponent coneAoeComponent,
            in DynamicBuffer<FXBufferElement> fxBufferElements,
            in OwnerShipComponent ownerShipComponent,
            in Entity entity,
            in NetworkEntity networkEntity,
            in Translation translation) =>
        {
            coneAoeComponent.totalTime -= deltaTime;

            if (coneAoeComponent.totalTime < 0)
            {
                _areaOfEffectsToRemove.Add(entity);
                return;
            }

            coneAoeComponent.intervalTimer -= deltaTime;

            if (coneAoeComponent.intervalTimer < 0)
            {
                coneAoeComponent.intervalTimer += coneAoeComponent.intervalTime;

                areaOfEffectsDamageData.Add(new AreaOfEffectDamageData
                {
                    casterEntity = ownerShipComponent.ownerEntity,
                    casterNetId = coneAoeComponent.casterNetId,
                    casterIsPlayer = coneAoeComponent.casterIsPlayer,
                    casterFactionType = coneAoeComponent.casterFactionType,
                    damage = coneAoeComponent.intervalDamage,
                    direction = coneAoeComponent.direction,
                    minConeDotProduct = coneAoeComponent.coneMinDotProduct,
                    maxDistance = coneAoeComponent.maxDistance,
                    casterPosition = coneAoeComponent.casterPosition,
                    position = translation.Value,
                });
            }
        })
        .Run();

        var ecb = beginServerSimulationEntityCommandBufferSystem.CreateCommandBuffer();

        //Destroy all expired area of effects
        {
            for (int i = 0; i < _areaOfEffectsToRemove.Length; i++)
            {
                NetworkEntityHelper.Destroy(_areaOfEffectsToRemove[i], ecb, server);
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

                if (factionComponent.factionType != aoeDamageData.casterFactionType &&
                    math.distance(translation.Value, aoeDamageData.position) < aoeDamageData.maxDistance)
                {
                    float dot = math.dot(math.normalize(aoeDamageData.direction), math.normalize(translation.Value - aoeDamageData.casterPosition));

                    if (dot > aoeDamageData.minConeDotProduct)
                    {
                        damageBufferElements.Add(new NewDamageElement
                        {
                            damage = aoeDamageData.damage,
                            damagerEntity = aoeDamageData.casterEntity,
                            damagerFactionType = aoeDamageData.casterFactionType,
                            damagerIsPlayer = aoeDamageData.casterIsPlayer,
                            damagerNetId = aoeDamageData.casterNetId
                        });
                    }
                }
            }
        })
        .WithReadOnly(areaOfEffectsDamageData)
        .WithDisposeOnCompletion(areaOfEffectsDamageData)
        .ScheduleParallel();
    }
}
