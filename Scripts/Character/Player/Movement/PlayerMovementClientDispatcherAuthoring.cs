using DOTSNET;
using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerMovementClientDispatcherAuthoring : MonoBehaviour, SelectiveSystemAuthoring
{
    // add system if Authoring is used
    public Type GetSystemType() => typeof(PlayerMovementDispatcherClient);
}

[ClientWorld]
[UpdateInGroup(typeof(ClientConnectedSimulationSystemGroup))]
// use SelectiveSystemAuthoring to create it selectively
[DisableAutoCreation]
public class PlayerMovementDispatcherClient : SystemBase
{
    [AutoAssign] protected NetworkClientSystem client;

    NativeList<PlayerMovementMessage> messages;

    protected override void OnCreate()
    {
        base.OnCreate();
        messages = new NativeList<PlayerMovementMessage>(10, Allocator.Persistent);
    }

    protected override void OnDestroy()
    {
        messages.Dispose();
        base.OnDestroy();
    }

    protected override void OnUpdate()
    {
        NativeList<PlayerMovementMessage> _messages = messages;

        // get input
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // get delta time
        float deltaTime = Time.DeltaTime;

        // foreach
        Entities.ForEach((ref PhysicsVelocity velocity,
                          in NetworkEntity networkEntity,
                          in LocalPlayerComponent localPlayerComponent,
                          in PlayerMovementComponent playerMovementComponent) =>
        {
            float3 direction = new float3(horizontal, 0, vertical);

            if (math.length(direction) > 1)
            {
                direction = math.normalize(direction);
            }

            float3 movement = direction * playerMovementComponent.speed;

            velocity.Linear = movement;

            PlayerMovementMessage message = new PlayerMovementMessage(
                networkEntity.netId,
                new float2(movement.x, movement.z)
            );

            _messages.Add(message);
        })
        .Run();

        client.Send(_messages);
        messages.Clear();
    }
}
