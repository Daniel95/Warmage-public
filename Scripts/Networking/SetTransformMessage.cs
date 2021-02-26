using DOTSNET;
using Unity.Mathematics;

public struct SetTransformMessage : NetworkMessage
{
    public ulong netId;

    public float3 position;
    public quaternion rotation;

    public ushort GetID() { return MessageIds.setPosition; }

    public SetTransformMessage(ulong netId, float3 position, quaternion rotation)
    {
        this.netId = netId;
        this.position = position;
        this.rotation = rotation;
    }
}
