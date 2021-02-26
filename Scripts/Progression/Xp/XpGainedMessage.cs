using DOTSNET;

public struct XpGainedMessage : NetworkMessage
{
    public ulong netId;

    public int amount;

    public ushort GetID() { return MessageIds.xpGained; }

    public XpGainedMessage(ulong netId, int amount)
    {
        this.netId = netId;
        this.amount = amount;
    }
}