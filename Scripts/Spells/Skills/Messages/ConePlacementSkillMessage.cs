using DOTSNET;
using System;
using Unity.Mathematics;

public struct ConePlacementSkillMessage : NetworkMessage
{
    public ulong netId;

    public Guid skillId;
    public ulong targetNetId;
    public float3 placePosition;
    public FactionType factionType;
    public quaternion rotation;
    public float3 playerPosition;

    public ushort GetID() { return MessageIds.conePlacementSkill; }

    public ConePlacementSkillMessage(ulong netId, ulong targetNetId, Guid skillId, FactionType factionType, float3 placePosition, quaternion rotation, float3 playerPosition)
    {
        this.netId = netId;
        this.skillId = skillId;
        this.targetNetId = targetNetId;
        this.placePosition = placePosition;
        this.factionType = factionType;
        this.rotation = rotation;
        this.playerPosition = playerPosition;
    }
}
