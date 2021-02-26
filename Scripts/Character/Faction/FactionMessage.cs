using DOTSNET;
using Unity.Entities;

public struct SetFactionMessage : NetworkMessage
{
    public ulong netId;

    public FactionType factionType;

    public ushort GetID() { return MessageIds.setFaction; }

    public SetFactionMessage(ulong netId, FactionType factionType)
    {
        this.netId = netId;
        this.factionType = factionType;
    }
}