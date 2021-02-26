using DOTSNET;
using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[DisallowMultipleComponent]
public class FXClientSystemAuthoring : MonoBehaviour, SelectiveSystemAuthoring
{
    public Type GetSystemType() { return typeof(FXClientSystem); }
}

[ClientWorld]
[UpdateInGroup(typeof(ClientConnectedSimulationSystemGroup))]
[UpdateAfter(typeof(FXMessageClientReceiver))]
[DisableAutoCreation]
public class FXClientSystem : SystemBase
{
    public struct FXTransformCommand
    {
        public Entity entity;
        public int fxIndex;
        public float3 position;
        public quaternion rotation;
    }

    private NativeList<FXTransformCommand> fxTransformCommands;

    protected override void OnCreate()
    {
        base.OnCreate();

        fxTransformCommands = new NativeList<FXTransformCommand>(100, Allocator.Persistent);
    }

    protected override void OnDestroy()
    {
        fxTransformCommands.Dispose();

        base.OnDestroy();
    }

    protected override void OnUpdate()
    {
        NativeList<FXTransformCommand> _fxTransformCommands = fxTransformCommands;

        Entities.ForEach((ref DynamicBuffer<FXBufferElement> fxBuffer,
            in Entity entity,
            in Translation translation,
            in Rotation rotation) =>
        {
            for (int i = 0; i < fxBuffer.Length; i++)
            {
                FXBufferElement fxBufferElement = fxBuffer[i];

                _fxTransformCommands.Add(new FXTransformCommand 
                { 
                    entity = entity,
                    fxIndex = fxBufferElement.fxPoolIndex,
                    position = translation.Value,
                    rotation = rotation.Value
                });
            }
        })
        .Run();

        foreach (FXTransformCommand fxTransformCommand in fxTransformCommands)
        {
            FXLibrary.GetInstance().UpdatePosition(fxTransformCommand.position, fxTransformCommand.rotation, fxTransformCommand.entity, fxTransformCommand.fxIndex);
        }

        fxTransformCommands.Clear();
    }
}