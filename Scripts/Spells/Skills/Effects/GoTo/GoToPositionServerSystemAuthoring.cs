using DOTSNET;
using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[DisallowMultipleComponent]
public class GoToPositionServerSystemAuthoring : MonoBehaviour, SelectiveSystemAuthoring
{
    public Type GetSystemType() => typeof(GoToPositionServerSystem);
}

[ServerWorld]
[UpdateInGroup(typeof(ServerActiveSimulationSystemGroup))]
[DisableAutoCreation]
public class GoToPositionServerSystem : SystemBase
{
    [AutoAssign] private NetworkServerSystem networkServerSystem = null;

    private NativeList<Entity> entitiesThatReachedPosition;
    private BeginServerSimulationEntityCommandBufferSystem beginServerSimulationEntityCommandBufferSystem;

    protected override void OnCreate()
    {
        base.OnCreate();
        entitiesThatReachedPosition = new NativeList<Entity>(100, Allocator.Persistent);
        beginServerSimulationEntityCommandBufferSystem = World.GetOrCreateSystem<BeginServerSimulationEntityCommandBufferSystem>();
    }

    protected override void OnDestroy()
    {
        entitiesThatReachedPosition.Dispose();
        base.OnDestroy();
    }

    protected override void OnUpdate()
    {
        NativeList<Entity> _entitiesThatReachedTarget = entitiesThatReachedPosition;

        float deltaTime = Time.DeltaTime;

        Entities.ForEach((
            ref Translation translation, 
            in Entity entity,
            in GoToPositionComponent goToPositionComponent) => 
        {
            float3 offset = goToPositionComponent.targetPosition - translation.Value;

            if (math.length(offset) < 1)
            {
                _entitiesThatReachedTarget.Add(entity);
            }
            else
            {
                float3 direction = math.normalize(offset);
                float3 movement = direction * (goToPositionComponent.speed * deltaTime);

                translation.Value += movement;
            }
        }).Run();

        var ecb = beginServerSimulationEntityCommandBufferSystem.CreateCommandBuffer();

        for (int i = 0; i < entitiesThatReachedPosition.Length; ++i)
        {
            Entity entityThatReachedPosition = entitiesThatReachedPosition[i];
            OnGoToCompletedComponent onGoToCompletedomponent = EntityManager.GetComponentData<OnGoToCompletedComponent>(entityThatReachedPosition);

            float3 position = EntityManager.GetComponentData<Translation>(entityThatReachedPosition).Value;

            if (onGoToCompletedomponent.onCompleted != null)
            {
                onGoToCompletedomponent.onCompleted(position);
            }

            NetworkEntityHelper.Destroy(entityThatReachedPosition, ecb, networkServerSystem);
            //networkServerSystem.Destroy(entityThatReachedPosition);
        }

        beginServerSimulationEntityCommandBufferSystem.AddJobHandleForProducer(Dependency);

        entitiesThatReachedPosition.Clear();
    }
}
