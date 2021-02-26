using DOTSNET;

public struct ChannelBarMessage : NetworkMessage
{
    public enum MessageType
    {
        Start,
        UpdateTime,
        Interrupt
    }

    public ulong netId;

    public MessageType messageType;
    public float time;

    public ushort GetID() { return MessageIds.channelBar; }

    public ChannelBarMessage(ulong netId, MessageType messageType, float time)
    {
        this.netId = netId;
        this.messageType = messageType;
        this.time = time;
    }
}