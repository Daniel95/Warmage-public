using DOTSNET;
using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerMovementServerReceiverAuthoring : MonoBehaviour, SelectiveSystemAuthoring
{
    public Type GetSystemType() { return typeof(PlayerMovementServerReceiver); }
}

[DisableAutoCreation]
public class PlayerMovementServerReceiver : NetworkServerMessageSystem<PlayerMovementMessage>
{
    protected override void OnUpdate() { }
    protected override bool RequiresAuthentication() { return true; }
    protected override void OnMessage(int connectionId, PlayerMovementMessage message)
    {
        if (server.spawned.TryGetValue(message.netId, out Entity entity))
        {
            PhysicsVelocity velocity = GetComponent<PhysicsVelocity>(entity);
            velocity.Linear = new float3(message.movement.x, 0, message.movement.y);
            SetComponent(entity, velocity);
        }
    }
}
