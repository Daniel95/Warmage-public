using System;
using Unity.Entities;
using UnityEngine;

public interface ISkill 
{
    string GetName();
    string GetDescription();
    float GetRange();
    Guid GetId();
    int GetManaCost();
    float GetCooldown();
    bool HasGlobalCooldown();
    float GetCastTime();
    int GetActionPointCost();
    int GetMaxCharges();
    bool ActivateOnFriendly();
    SkillPlacementType GetPlacementType();
    bool IsManuallyPlaced();
    bool IsPlayerOwned();
    Sprite GetIcon();
    ISkill ShallowCopy();

    void Execute(Entity casterEntity, ulong casterNetId, FactionType casterFactionType, Entity targetEntity, EntityCommandBuffer ecb, SpellEcsData systemInfo);
}
