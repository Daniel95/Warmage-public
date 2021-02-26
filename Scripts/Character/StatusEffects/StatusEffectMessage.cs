using DOTSNET;
using System;

public struct StatusEffectMessage : NetworkMessage
{
    public enum MessageType
    {
        Add,
        Update, 
        Remove
    }

    public ulong netId;

    public Guid statusEffectId;
    public ulong casterNetId;
    public MessageType messageType;
    public float timeLeft;
    public int count;

    public ushort GetID() { return MessageIds.statusEffect; }

    public StatusEffectMessage(ulong netId, Guid statusEffectId, ulong casterNetId, float timeLeft, int count, MessageType messageType)
    {
        this.netId = netId;
        this.casterNetId = casterNetId;
        this.statusEffectId = statusEffectId;
        this.messageType = messageType;
        this.timeLeft = timeLeft;
        this.count = count;
    }
}