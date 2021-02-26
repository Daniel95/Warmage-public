using Unity.Entities;
using UnityEngine;

[GenerateAuthoringComponent]
public struct NpcCombatComponent : IComponentData
{
    public float combatSpeed;
    public float outOfCombatSpeed;
    public float returnSpeed;
    public float pathfindInterval;
    public float globalCooldown;
    public float attackRange;
    public float maxDistanceFromSpawn;

    [HideInInspector] public bool isChasing;
    [HideInInspector] public bool inAttackRange;
    [HideInInspector] public float globalCooldownTimer;
    [HideInInspector] public int activeSkillIndex;
    [HideInInspector] public float activeSkillRange;
    [HideInInspector] public float pathfindTimer;
}
