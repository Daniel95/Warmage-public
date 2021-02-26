using Unity.Entities;

public struct DispatchChannelBarMessageComponent : IComponentData
{
    public ChannelBarMessage.MessageType messageType;
    public float time;
}
