using DOTSNET;

public struct PlayerDeathMessage : NetworkMessage
{
    public ulong netId;

    public ushort GetID() { return MessageIds.playerDeath; }

    public PlayerDeathMessage(ulong netId)
    {
        this.netId = netId;
    }
}