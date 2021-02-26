using DOTSNET;
using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[DisallowMultipleComponent]
public class GoToPositionAngledServerSystemAuthoring : MonoBehaviour, SelectiveSystemAuthoring
{
    public Type GetSystemType() => typeof(GoToPositionAngledServerSystem);
}

[ServerWorld]
[UpdateInGroup(typeof(ServerActiveSimulationSystemGroup))]
[DisableAutoCreation]
public class GoToPositionAngledServerSystem : SystemBase
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
            ref Rotation rotation,
            in Entity entity,
            in GoToPositionAngledComponent goToPositionAngledComponent) =>
        {
            float3 offset = goToPositionAngledComponent.targetPosition - translation.Value;

            if (math.length(offset) < 1)
            {
                _entitiesThatReachedTarget.Add(entity);
            }
            else
            {
                float3 forward = offset;
                float3 crossDirection = math.cross(forward, Vector3.up);
                Quaternion randomDeltaRotation = Quaternion.Euler(0, goToPositionAngledComponent.randomSideAngle, 0) *
                    Quaternion.AngleAxis(goToPositionAngledComponent.randomUpAngle, crossDirection);
                float3 direction = math.mul(randomDeltaRotation, offset);

                float3 moveDistance = goToPositionAngledComponent.speed * deltaTime;

                float3 movement = math.normalize(direction) * moveDistance;

                translation.Value += math.normalize(direction) * (goToPositionAngledComponent.speed * deltaTime);
                rotation.Value = quaternion.LookRotation(direction, Vector3.up);
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
        }

        beginServerSimulationEntityCommandBufferSystem.AddJobHandleForProducer(Dependency);

        entitiesThatReachedPosition.Clear();
    }
}
