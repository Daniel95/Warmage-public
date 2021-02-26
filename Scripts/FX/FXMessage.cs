using DOTSNET;

public struct FXMessage : NetworkMessage
{
    public enum AddOrRemoveType
    {
        Add,
        Remove
    }

    public ulong netId;

    public int fxIndex;
    public AddOrRemoveType addOrRemoveType;

    public ushort GetID() { return MessageIds.fxMessage; }

    public FXMessage(ulong netId, AddOrRemoveType addOrRemoveType, int fxIndex)
    {
        this.netId = netId;
        this.fxIndex = fxIndex;
        this.addOrRemoveType = addOrRemoveType;
    }
}