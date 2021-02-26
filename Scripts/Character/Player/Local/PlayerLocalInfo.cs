using System;
using Unity.Entities;
using Unity.Mathematics;

public struct PlayerLocalInfo
{
    public struct TargetInfo
    {
        public void Update(Entity entity, ulong netId, float3 position, int currentHealth, int maxHealth, FactionType factionType, float distance)
        {
            this.entity = entity;
            this.netId = netId;
            this.position = position;
            this.currentHealth = currentHealth;
            this.maxHealth = maxHealth;
            this.factionType = factionType;
            this.distance = distance;
        }

        public bool hasTarget;
        public bool isPlayer;

        public Entity entity;
        public ulong netId;
        public float3 position;
        public int currentHealth;
        public int maxHealth;
        public FactionType factionType;
        public float distance;

        public Action<Entity> targetEvent;
        public Action untargetEvent;
    }

    public static float3 position;
    public static float3 forward;
    public static int currentHealth;
    public static int maxHealth;
    public static TargetInfo targetInfo;
    public static Entity entity = Entity.Null;
    public static ulong netId;
    public static FactionType factionType;
    public static bool isAlive;
}
