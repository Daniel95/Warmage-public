using DOTSNET;
using Unity.Collections;

public struct ChatMessage : NetworkMessage
{
    public FixedString32 sender;
    public FixedString128 text;

    public ushort GetID() { return MessageIds.chat; }

    public ChatMessage(FixedString32 sender, FixedString128 text)
    {
        this.sender = sender;
        this.text = text;
    }
}
