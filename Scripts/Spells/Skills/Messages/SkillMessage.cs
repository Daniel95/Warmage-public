using DOTSNET;
using System;

public struct SkillMessage : NetworkMessage
{
    public ulong netId;

    public Guid skillId;
    public ulong targetNetId;
    public FactionType factionType;

    public ushort GetID() { return MessageIds.skillMessage; }

    public SkillMessage(ulong netId, ulong targetNetId, Guid skillId, FactionType factionType)
    {
        this.netId = netId;
        this.skillId = skillId;
        this.targetNetId = targetNetId;
        this.factionType = factionType;
    }
}
