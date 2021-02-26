using DOTSNET;
using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

public class PhysicsConstraintsClientSystemAuthoring : MonoBehaviour, SelectiveSystemAuthoring
{
    public Type GetSystemType() => typeof(PhysicsConstraintsClientSystem);
}

[ClientWorld]
[UpdateInGroup(typeof(ClientConnectedSimulationSystemGroup))]
[DisableAutoCreation]
public class PhysicsConstraintsClientSystem : SystemBase
{
    //private EndSimulationEntityCommandBufferSystem entityCommandBufferSystem;

    protected override void OnCreate()
    {
        base.OnCreate();

        //entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        //var ecb = entityCommandBufferSystem.CreateCommandBuffer();

        //Entities.ForEach((
        //    ref PhysicsMass physicsMass,
        //    ref Rotation rotation,
        //    in Entity entity,
        //    in PhysicsConstraintsClientComponent physicsConstraint
        //    ) =>
        //{
        //    //float3 rotation = Vector3
            //rotation.Value = quaternion.Euler(0, math.eu) quaternion.identity;

            //physicsMass.InverseInertia[0] = 0;
            //physicsMass.InverseInertia[2] = 0;

            //physicsMass.InverseInertia[0] = physicsConstraint.LockX ? 0 : physicsMass.InverseInertia[0];
            //physicsMass.InverseInertia[1] = physicsConstraint.LockY ? 0 : physicsMass.InverseInertia[1];
            //physicsMass.InverseInertia[2] = physicsConstraint.LockZ ? 0 : physicsMass.InverseInertia[2];

            //ecb.RemoveComponent<PhysicsConstraintsClientComponent>(entity);
        //})
        //.Run();

        //entityCommandBufferSystem.AddJobHandleForProducer(Dependency);
    }
}
