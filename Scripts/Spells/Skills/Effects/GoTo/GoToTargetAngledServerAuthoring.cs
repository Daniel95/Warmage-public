using DOTSNET;
using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[DisallowMultipleComponent]
public class GoToTargetAngledServerAuthoring : MonoBehaviour, SelectiveSystemAuthoring
{
    public Type GetSystemType() => typeof(GoToTargetAngledServer);
}

[ServerWorld]
[UpdateInGroup(typeof(ServerActiveSimulationSystemGroup))]
[DisableAutoCreation]
public class GoToTargetAngledServer : SystemBase
{
    [AutoAssign] private NetworkServerSystem networkServerSystem = null;

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

        var transformComponents = GetComponentDataFromEntity<Translation>(true);

        Entities.WithNone<SpawnOnServerComponent>().ForEach((
            ref Translation translation,
            ref Rotation rotation,
            in Entity entity,
            in GoToTargetAngledComponent goToTargetAngleComponent) =>
        {
            if (transformComponents.HasComponent(goToTargetAngleComponent.targetEntity))
            {
                float3 targetPosition = transformComponents[goToTargetAngleComponent.targetEntity].Value;

                float3 offset = targetPosition - translation.Value;

                if (math.length(offset) < 1)
                {
                    _entitiesThatReachedTarget.Add(entity);
                }
                else
                {
                    float3 forward = offset;
                    float3 crossDirection = math.cross(forward, Vector3.up);
                    Quaternion randomDeltaRotation = Quaternion.Euler(0, goToTargetAngleComponent.randomSideAngle, 0) *
                        Quaternion.AngleAxis(goToTargetAngleComponent.randomUpAngle, crossDirection);
                    float3 direction = math.mul(randomDeltaRotation, offset);

                    float3 moveDistance = goToTargetAngleComponent.speed * deltaTime;

                    float3 movement = math.normalize(direction) * moveDistance;

                    translation.Value += math.normalize(direction) * (goToTargetAngleComponent.speed * deltaTime);
                    rotation.Value = quaternion.LookRotation(direction, Vector3.up);
                }
            }
            else
            {
                _entitiesThatHaveNoTarget.Add(entity);
            }
        })
        .Run();

        var ecb = beginServerSimulationEntityCommandBufferSystem.CreateCommandBuffer();

        for (int i = 0; i < entitiesThatHaveNoTarget.Length; i++)
        {
            Entity entityThatHasNoTarget = entitiesThatHaveNoTarget[i];

            NetworkEntityHelper.Destroy(entityThatHasNoTarget, ecb, networkServerSystem);
        }

        for (int i = 0; i < entitiesThatReachedTarget.Length; ++i)
        {
            Entity entityThatReachedTarget = entitiesThatReachedTarget[i];
            OnGoToCompletedComponent onTargetReachedComponent = EntityManager.GetComponentData<OnGoToCompletedComponent>(entityThatReachedTarget);
            float3 position = EntityManager.GetComponentData<Translation>(entityThatReachedTarget).Value;

            NetworkEntityHelper.Destroy(entityThatReachedTarget, ecb, networkServerSystem);

            onTargetReachedComponent.onCompleted?.Invoke(position);
        }

        beginServerSimulationEntityCommandBufferSystem.AddJobHandleForProducer(Dependency);

        entitiesThatReachedTarget.Clear();
        entitiesThatHaveNoTarget.Clear();
    }
}
