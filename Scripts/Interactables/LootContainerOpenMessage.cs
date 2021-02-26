using DOTSNET;

public struct LootContainerOpenMessage : NetworkMessage
{
    public ulong netId;

    public ulong lootContainerNetId;

    public ushort GetID() { return MessageIds.lootContainerOpenMessage; }
}