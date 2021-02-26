using DOTSNET;
using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[DisallowMultipleComponent]
public class GoToTargetServerSystemAuthoring : MonoBehaviour, SelectiveSystemAuthoring
{
    public Type GetSystemType() => typeof(GoToTargetServerSystem);
}

[ServerWorld]
[UpdateInGroup(typeof(ServerActiveSimulationSystemGroup))]
[DisableAutoCreation]
public class GoToTargetServerSystem : SystemBase
{
    [AutoAssign] private NetworkServerSystem server = null;

    private NativeList<Entity> entitiesThatReachedTarget;
    private NativeList<Entity> entitiesThatHaveNoTarget;
    private BeginServerSimulationEntityCommandBufferSystem beginServerSimulationEntityCommandBufferSystem;

    protected override void OnCreate()
    {
        base.OnCreate();

        entitiesThatReachedTarget = new NativeList<Entity>(100, Allocator.Persistent);
        entitiesThatHaveNoTarget = new NativeList<Entity>(100, Allocator.Persistent);

        beginServerSimulationEntityCommandBufferSystem = World.GetOrCreateSystem<BeginServerSimulationEntityCommandBufferSystem>();
    }

    protected override void OnDestroy()
    {
        entitiesThatReachedTarget.Dispose();
        entitiesThatHaveNoTarget.Dispose();
        base.OnDestroy();
    }

    protected override void OnUpdate()
    {
        NativeList<Entity> _entitiesThatReachedTarget = entitiesThatReachedTarget;
        NativeList<Entity> _entitiesThatHaveNoTarget = entitiesThatHaveNoTarget;

        float deltaTime = Time.DeltaTime;
        EntityManager entityManager = Bootstrap.ServerWorld.EntityManager;

        Entities.WithNone<SpawnOnServerComponent>().ForEach((
            ref Translation translation, 
            in Entity entity,
            in GoToTargetComponent goToTargetComponent) => 
        {
            if(entityManager.HasComponent<Translation>(goToTargetComponent.targetEntity))
            {
                float3 targetPosition = entityManager.GetComponentData<Translation>(goToTargetComponent.targetEntity).Value;

                float3 offset = targetPosition - translation.Value;

                if (math.length(offset) < 1)
                {
                    _entitiesThatReachedTarget.Add(entity);
                }
                else
                {
                    float3 direction = math.normalize(offset);
                    float3 movement = direction * (goToTargetComponent.speed * deltaTime);

                    translation.Value += movement;
                }
            } 
            else
            {
                _entitiesThatHaveNoTarget.Add(entity);
            }
        }).Run();

        var beginServerEcb = beginServerSimulationEntityCommandBufferSystem.CreateCommandBuffer();

        for (int i = 0; i < entitiesThatReachedTarget.Length; ++i)
        {
            Entity entityThatReachedTarget = entitiesThatReachedTarget[i];
            OnGoToCompletedComponent onTargetReachedComponent = entityManager.GetComponentData<OnGoToCompletedComponent>(entityThatReachedTarget);
            float3 position = entityManager.GetComponentData<Translation>(entityThatReachedTarget).Value;

            if(onTargetReachedComponent.onCompleted != null)
            {
                onTargetReachedComponent.onCompleted(position);
            }

            NetworkEntityHelper.Destroy(entityThatReachedTarget, beginServerEcb, server);
        }

        for (int i = 0; i < entitiesThatHaveNoTarget.Length; i++)
        {
            Entity entityThatHasNoTarget = entitiesThatHaveNoTarget[i];

            NetworkEntityHelper.Destroy(entityThatHasNoTarget, beginServerEcb, server);
        }

        beginServerSimulationEntityCommandBufferSystem.AddJobHandleForProducer(Dependency);

        entitiesThatReachedTarget.Clear();
        entitiesThatHaveNoTarget.Clear();
    }
}
