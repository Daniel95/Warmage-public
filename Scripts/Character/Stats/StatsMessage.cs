using DOTSNET;

public struct StatsMessage : NetworkMessage
{
    public ulong netId;

    public float speedFactor;

    public ushort GetID() { return MessageIds.stats; }

    public StatsMessage(ulong netId, float speedFactor)
    {
        this.netId = netId;
        this.speedFactor = speedFactor;
    }
}
