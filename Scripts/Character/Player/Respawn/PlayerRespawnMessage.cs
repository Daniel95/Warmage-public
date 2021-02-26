using DOTSNET;

public struct PlayerRespawnMessage : NetworkMessage
{
    public ulong netId;

    public ushort GetID() { return MessageIds.playerMovement; }

    public PlayerRespawnMessage(ulong netId)
    {
        this.netId = netId;
    }
}
