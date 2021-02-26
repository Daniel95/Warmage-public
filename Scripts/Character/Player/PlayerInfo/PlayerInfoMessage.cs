using DOTSNET;
using Unity.Collections;

public struct PlayerInfoMessage : NetworkMessage
{
    public ulong netId;

    public FixedString32 name;
    public FactionType faction;

    public ushort GetID() { return MessageIds.playerInfoMessage; }

    public PlayerInfoMessage(ulong netId, FixedString32 name, FactionType faction)
    {
        this.netId = netId;
        this.name = name;
        this.faction = faction;
    }
}
