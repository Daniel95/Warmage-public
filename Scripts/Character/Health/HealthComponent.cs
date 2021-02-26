using Unity.Entities;
using UnityEngine;

[GenerateAuthoringComponent]
public struct HealthComponent : IComponentData
{
    public int currentHealth;
    public int maxHealth;
    [HideInInspector] public FactionType lastDamagerFaction;
}
