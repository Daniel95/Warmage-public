using DOTSNET;
using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

[DisallowMultipleComponent]
public class FXMessageClientReceiverAuthoring : MonoBehaviour, SelectiveSystemAuthoring
{
    public Type GetSystemType() { return typeof(FXMessageClientReceiver); }
}

[DisableAutoCreation]
public class FXMessageClientReceiver : NetworkClientMessageSystem<FXMessage>
{
    public struct FXSpawnCommand
    {
        public Entity entity;
        public int fxIndex;
        public float3 position;
        public quaternion rotation;
    }

    public struct FXUnspawnCommand
    {
        public Entity entity;
        public int fxIndex;
    }

    private NativeHashMap<ulong, FXMessage> messages;
    private NativeList<FXSpawnCommand> fxSpawnCommands;
    private NativeList<FXUnspawnCommand> fxUnspawnCommands;

    protected override void OnCreate()
    {
        base.OnCreate();

        fxSpawnCommands = new NativeList<FXSpawnCommand>(10, Allocator.Persistent);
        fxUnspawnCommands = new NativeList<FXUnspawnCommand>(10, Allocator.Persistent);
        messages = new NativeHashMap<ulong, FXMessage>(200, Allocator.Persistent);
    }

    protected override void OnDestroy()
    {
        messages.Dispose(Dependency);
        fxSpawnCommands.Dispose(Dependency);
        fxUnspawnCommands.Dispose(Dependency);

        base.OnDestroy();
    }

    protected override void OnMessage(FXMessage message)
    {
        messages[message.netId] = message;
    }

    protected override void OnUpdate()
    {
        NativeList<FXSpawnCommand> _fxSpawnCommands = fxSpawnCommands;
        NativeList<FXUnspawnCommand> _fxUnspawnCommands = fxUnspawnCommands;
        NativeHashMap<ulong, FXMessage> _messages = messages;

        Entities.ForEach((ref DynamicBuffer<FXBufferElement> fxBuffer,
            in Translation translation,
            in Entity entity,
            in Rotation rotation,
            in NetworkEntity networkEntity) =>
        {
            if (_messages.ContainsKey(networkEntity.netId))
            {
                FXMessage fxMessage = _messages[networkEntity.netId];

                if(fxMessage.addOrRemoveType == FXMessage.AddOrRemoveType.Add)
                {
                    bool alreadyExists = false;

                    for (int i = 0; i < fxBuffer.Length; i++)
                    {
                        if (fxBuffer[i].fxPoolIndex == fxMessage.fxIndex)
                        {
                            alreadyExists = true;
                            break;
                        }
                    }

                    if(!alreadyExists)
                    {
                        fxBuffer.Add(new FXBufferElement { fxPoolIndex = fxMessage.fxIndex });
                        _fxSpawnCommands.Add(new FXSpawnCommand { entity = entity, fxIndex = fxMessage.fxIndex, position = translation.Value, rotation = rotation.Value });
                    }
                } 
                else
                {
                    for (int i = 0; i < fxBuffer.Length; i++)
                    {
                        if(fxBuffer[i].fxPoolIndex == fxMessage.fxIndex)
                        {
                            fxBuffer.RemoveAt(i);
                            _fxUnspawnCommands.Add(new FXUnspawnCommand { entity = entity, fxIndex = fxMessage.fxIndex });
                            break;
                        }
                    }
                }
            }
        })
        .Run();

        foreach (FXSpawnCommand fxSpawnCommand in fxSpawnCommands)
        {
            FXLibrary.GetInstance().Spawn(fxSpawnCommand.position, fxSpawnCommand.rotation, fxSpawnCommand.entity, fxSpawnCommand.fxIndex);
        }

        foreach (FXUnspawnCommand fxUnspawnCommand in fxUnspawnCommands)
        {
            FXLibrary.GetInstance().Unspawn(fxUnspawnCommand.entity, fxUnspawnCommand.fxIndex);
        }

        fxSpawnCommands.Clear();
        fxUnspawnCommands.Clear();
        messages.Clear();
    }
}
