using DOTSNET;
using Unity.Mathematics;

public struct PlayerMovementMessage : NetworkMessage
{
    public ulong netId;

    public float2 movement;

    public ushort GetID() { return MessageIds.playerMovement; }

    public PlayerMovementMessage(ulong netId, float2 movement)
    {
        this.netId = netId;
        this.movement = movement;
    }
}
