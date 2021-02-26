using DOTSNET;
using System;
using Unity.Entities;
using Unity.Physics;
using UnityEngine;

public class PhysicsConstraintServerSystemAuthoring : MonoBehaviour, SelectiveSystemAuthoring
{
    public Type GetSystemType() => typeof(PhysicsConstraintServerSystem);
}

[ServerWorld]
[UpdateInGroup(typeof(ServerActiveSimulationSystemGroup))]
[DisableAutoCreation]
public class PhysicsConstraintServerSystem : SystemBase
{
    private EndServerSimulationEntityCommandBufferSystem entityCommandBufferSystem;

    protected override void OnCreate()
    {
        base.OnCreate();

        entityCommandBufferSystem = World.GetOrCreateSystem<EndServerSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        var ecb = entityCommandBufferSystem.CreateCommandBuffer();

        Entities.ForEach((
            ref PhysicsMass physicsMass,
            in Entity entity,
            in PhysicsConstraintsServerComponent physicsConstraint
            ) =>
        {
            physicsMass.InverseInertia[0] = physicsConstraint.LockX ? 0 : physicsMass.InverseInertia[0];
            physicsMass.InverseInertia[1] = physicsConstraint.LockY ? 0 : physicsMass.InverseInertia[1];
            physicsMass.InverseInertia[2] = physicsConstraint.LockZ ? 0 : physicsMass.InverseInertia[2];

            ecb.RemoveComponent<PhysicsConstraintsServerComponent>(entity);
        })
        .Run();

        entityCommandBufferSystem.AddJobHandleForProducer(Dependency);
    }
}
