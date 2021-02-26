using DOTSNET;
using System;
using Unity.Mathematics;

public struct CirclePlacementSkillMessage : NetworkMessage
{
    public ulong netId;

    public Guid skillId;
    public ulong targetNetId;
    public float3 placePosition;
    public FactionType factionType;

    public ushort GetID() { return MessageIds.circlePlacementSkill; }

    public CirclePlacementSkillMessage(ulong netId, ulong targetNetId, Guid skillId, FactionType factionType, float3 placePosition)
    {
        this.netId = netId;
        this.skillId = skillId;
        this.targetNetId = targetNetId;
        this.placePosition = placePosition;
        this.factionType = factionType;
    }
}
