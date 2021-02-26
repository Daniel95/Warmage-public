using DOTSNET;
using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[DisallowMultipleComponent]
public class InterpolateTransformClientReceiverAuthoring : MonoBehaviour, SelectiveSystemAuthoring
{
    public Type GetSystemType() { return typeof(InterpolateTransformClientReceiver); }

    public float positionInterpolateSpeed = 0.1f;
    public float rotationInterpolateSpeed = 0.1f;

    private void Awake()
    {
        InterpolateTransformClientReceiver system = Bootstrap.ClientWorld.GetExistingSystem<InterpolateTransformClientReceiver>();

        system.positionInterpolateSpeed = positionInterpolateSpeed;
        system.rotationInterpolateSpeed = rotationInterpolateSpeed;
    }
}

[DisableAutoCreation]
public class InterpolateTransformClientReceiver : NetworkClientMessageSystem<TransformMessage>
{
    public float positionInterpolateSpeed;
    public float rotationInterpolateSpeed;

    private NativeHashMap<ulong, TransformMessage> messages;

    protected override void OnDestroy()
    {
        messages.Dispose(Dependency);
        base.OnDestroy();
    }

    protected override void OnCreate()
    {
        base.OnCreate();
        messages = new NativeHashMap<ulong, TransformMessage>(1000, Allocator.Persistent);
    }

    protected override void OnMessage(TransformMessage message)
    {
        messages[message.netId] = message;
    }

    protected override void OnUpdate()
    {
        NativeHashMap<ulong, TransformMessage> _messages = messages;

        Entities.ForEach((ref InterpolateTransformComponent newTransformInterpolationComponent,
            in NetworkEntity networkEntity) =>
        {
            if (_messages.ContainsKey(networkEntity.netId))
            {
                TransformMessage message = _messages[networkEntity.netId];
                newTransformInterpolationComponent.position = message.position;
                newTransformInterpolationComponent.rotation = message.rotation;
            }
        })
        .Run();

        messages.Clear();

        float _positionInterpolateSpeed = positionInterpolateSpeed;
        float _rotationInterpolateSpeed = rotationInterpolateSpeed;

        Entities.ForEach((
            ref Translation translation,
            ref Rotation rotation,
            in InterpolateTransformComponent interpolateTransformComponent) =>
        {
            if(interpolateTransformComponent.position.x != 0 && interpolateTransformComponent.position.y != 0 && interpolateTransformComponent.position.z != 0)
            {
                translation.Value = (float3)math.select(
                   translation.Value,
                   math.lerp(translation.Value, interpolateTransformComponent.position, _positionInterpolateSpeed),
                   true);

                rotation.Value.value = math.select(
                        rotation.Value.value,
                        math.slerp(rotation.Value, interpolateTransformComponent.rotation, _rotationInterpolateSpeed).value,
                        true);
            } 
        })
        .Run();
    }
}