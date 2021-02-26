using DOTSNET;
using System;
using Unity.Entities;
using Unity.Physics;
using UnityEngine;

[DisallowMultipleComponent]
public class RemovePhysicsServerSystemAuthoring : MonoBehaviour, SelectiveSystemAuthoring
{
    public Type GetSystemType() => typeof(RemovePhysicsServerSystem);
}

[ServerWorld]
[UpdateInGroup(typeof(ServerActiveSimulationSystemGroup))]
[DisableAutoCreation]
public class RemovePhysicsServerSystem : SystemBase
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


        // remove physics components from from spheres on the server,
        // so that we can apply NetworkTransform synchronization.
        Entities.ForEach((
            in Entity entity, 
            in PlayerMovementComponent playerMovementComponent,
            in PhysicsCollider physicsCollider,
            in PhysicsDamping physicsDamping,
            in PhysicsGravityFactor physicsGravityFactor, 
            in PhysicsMass physicsMass, 
            in PhysicsVelocity physicsVelocity) =>
        {
            ecb.RemoveComponent<PhysicsCollider>(entity);
            ecb.RemoveComponent<PhysicsDamping>(entity);
            ecb.RemoveComponent<PhysicsGravityFactor>(entity);
            ecb.RemoveComponent<PhysicsMass>(entity);
            ecb.RemoveComponent<PhysicsVelocity>(entity);
        })
        .Run();

        entityCommandBufferSystem.AddJobHandleForProducer(Dependency);
    }
}
