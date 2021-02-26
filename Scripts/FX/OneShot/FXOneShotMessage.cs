using DOTSNET;
using Unity.Mathematics;

public struct FXOneShotMessage : NetworkMessage
{
    public ulong netId;

    public int fxPoolIndex;
    public float timer;
    public float3 position;
    public quaternion rotation;

    public ushort GetID() { return MessageIds.fxOneShot; }

    public FXOneShotMessage(ulong netId, int fxIndex, float timer, float3 position, quaternion rotation)
    {
        this.netId = netId;
        this.fxPoolIndex = fxIndex;
        this.timer = timer;
        this.position = position;
        this.rotation = rotation;
    }
}