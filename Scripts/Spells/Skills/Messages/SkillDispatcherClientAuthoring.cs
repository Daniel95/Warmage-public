using DOTSNET;
using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[DisallowMultipleComponent]
public class SkillDispatcherClientAuthoring : MonoBehaviour, SelectiveSystemAuthoring
{
    public Type GetSystemType() { return typeof(SkillDispatcherClient); }
}

[ClientWorld]
[UpdateInGroup(typeof(ClientConnectedSimulationSystemGroup))]
[DisableAutoCreation]
public class SkillDispatcherClient : SystemBase
{
    [AutoAssign] protected NetworkClientSystem client;

    public float interval = 0.1f;

    public void SendSkill(Guid skillId, ulong targetNetId, FactionType factionType)
    {
        SkillMessage playerSkillMessage = new SkillMessage(PlayerLocalInfo.netId, targetNetId, skillId, factionType);
        client.Send(playerSkillMessage);
    }

    public void SendCirclePlacementSkill(Guid skillId, ulong targetNetId, FactionType factionType, float3 placePosition)
    {
        CirclePlacementSkillMessage playerSkillMessage = new CirclePlacementSkillMessage(PlayerLocalInfo.netId, targetNetId, skillId, factionType, placePosition);
        client.Send(playerSkillMessage);
    }

    public void SendConePlacementSkill(Guid skillId, ulong targetNetId, FactionType factionType, float3 placePosition, quaternion rotation, float3 playerPosition)
    {
        ConePlacementSkillMessage playerSkillMessage = new ConePlacementSkillMessage(PlayerLocalInfo.netId, 
            targetNetId,
            skillId,
            factionType,
            placePosition,
            rotation,
            playerPosition);
        client.Send(playerSkillMessage);
    }

    protected override void OnUpdate() { }
}
