using System;
using Unity.Entities;
using UnityEngine;

public interface IStatusEffect
{
    string GetName();
    string GetDescription();
    float GetTime();
    void SetTime(float newTime);
    bool IsInfinite();
    bool GetIsStackable();
    int GetMaxStacks();
    Guid GetId();
    Sprite GetIcon();
    IStatusEffect ShallowCopy();

    void Init(SpellEcsData spellEcsData, Entity caster, ulong casterNetId, FactionType casterFactionType, Entity target);
    void StartEffect(EntityCommandBuffer ecb);
    void UpdateEffect(EntityCommandBuffer ecb, float deltaTime);
    void EndEffect(EntityCommandBuffer ecb);
}