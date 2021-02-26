using DOTSNET;
using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Authoring;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class PlayerTargetClientSystemAuthoring : MonoBehaviour, SelectiveSystemAuthoring
{
    // add system if Authoring is used
    public Type GetSystemType() => typeof(PlayerTargetClientSystem);

    [SerializeField] private PhysicsCategoryTags physicsCategoryTags = PhysicsCategoryTags.Everything;
    [SerializeField] private float minLookPercentage = 0.95f;
    [SerializeField] private float maxDistance = 50.0f;
    [SerializeField] private Image aimIcon = null;

    void Start()
    {
        PlayerTargetClientSystem playerTargetClientSystem = Bootstrap.ClientWorld.GetExistingSystem<PlayerTargetClientSystem>();

        playerTargetClientSystem.physicsCategoryTags = physicsCategoryTags;
        playerTargetClientSystem.minLookPercentage = minLookPercentage;
        playerTargetClientSystem.maxDistance = maxDistance;
        playerTargetClientSystem.aimIcon = aimIcon;
    }
}

[ClientWorld]
[UpdateInGroup(typeof(ClientConnectedSimulationSystemGroup))]
[DisableAutoCreation]
public class PlayerTargetClientSystem : SystemBase
{
    private struct TargetData
    {
        public TargetData(Entity entity, ulong netId, float3 position, int currentHealth, int maxHealth, FactionType factionType, float distance)
        {
            this.entity = entity;
            this.netId = netId;
            this.position = position;
            this.currentHealth = currentHealth;
            this.maxHealth = maxHealth;
            this.factionType = factionType;
            this.distance = distance;
        }

        public Entity entity;
        public ulong netId;
        public float3 position;
        public int currentHealth;
        public int maxHealth;
        public FactionType factionType;
        public float distance;
    }

    public Image aimIcon;
    public PhysicsCategoryTags physicsCategoryTags;
    public float minLookPercentage;
    public float maxDistance;

    private Camera cam;

    protected override void OnStartRunning()
    {
        base.OnStartRunning();

        cam = Camera.main;
    }

    protected override void OnUpdate()
    {
        PhysicsWorld physicsWorld = Bootstrap.ClientWorld.GetExistingSystem<BuildPhysicsWorld>().PhysicsWorld;

        CollisionFilter collisionFilter = new CollisionFilter()
        {
            BelongsTo = unchecked((uint)~0),
            CollidesWith = physicsCategoryTags.Value
        };

        float _minViewAlignment = minLookPercentage;
        float _maxDistance = maxDistance;

        FactionType playerFactionType = PlayerLocalInfo.factionType;

        float3 cameraForward = cam.transform.forward;
        float3 cameraPosition = cam.transform.position;

        bool validTarget = false;
        TargetData bestTargetData = new TargetData();
        bool validEnemyTarget = false;

        float bestScore = 0;

        Entities.WithNone<LocalPlayerComponent>().ForEach((in Entity entity,
            in NetworkEntity networkEntity,
            in HealthComponent healthComponent,
            in Translation translation,
            in FactionComponent factionComponent) =>
        {
            bool isEnemy = factionComponent.factionType != playerFactionType;

            if (validEnemyTarget && !isEnemy) { return; }

            float distance = math.distance(cameraPosition, translation.Value);

            if (distance > _maxDistance) { return; }

            float viewAlignment = math.dot(cameraForward, math.normalize(translation.Value - cameraPosition));

            if (viewAlignment <= _minViewAlignment) { return; }

            RaycastInput canSeeTargetInput = new RaycastInput
            {
                Start = cameraPosition,
                End = translation.Value,
                Filter = collisionFilter
            };

            if (physicsWorld.CastRay(canSeeTargetInput)) { return; }

            float viewAlignmentOffset = viewAlignment - _minViewAlignment;
            float maxViewAlignmentOffset = 1 - _minViewAlignment;

            float distanceFactor = 1 - (distance / _maxDistance);
            float viewOffsetFactor = viewAlignmentOffset / maxViewAlignmentOffset;

            float score = distanceFactor + viewOffsetFactor;

            //Update best target when: Score is higher OR if the target is a enemy and we haven't found a valid enemy yet.
            if (score > bestScore || (isEnemy && !validEnemyTarget))
            {
                if(isEnemy)
                {
                    validEnemyTarget = true;
                }

                validTarget = true;
                bestScore = score;

                bestTargetData = new TargetData(entity,
                    networkEntity.netId,
                    translation.Value,
                    healthComponent.currentHealth,
                    healthComponent.maxHealth,
                    factionComponent.factionType,
                    distance);
            }
        })
        .Run();

        if (!validTarget)
        {
            if (PlayerLocalInfo.targetInfo.hasTarget)
            {
                aimIcon.transform.position = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0);

                PlayerLocalInfo.targetInfo.untargetEvent?.Invoke();
                PlayerLocalInfo.targetInfo.hasTarget = false;
            }

            return;
        }

        PlayerLocalInfo.targetInfo.Update(bestTargetData.entity,
            bestTargetData.netId,
            bestTargetData.position,
            bestTargetData.currentHealth,
            bestTargetData.maxHealth,
            bestTargetData.factionType,
            bestTargetData.distance);

        Vector3 screenPos = Camera.main.WorldToScreenPoint(bestTargetData.position);
        aimIcon.transform.position = Vector3.MoveTowards(aimIcon.transform.position, screenPos, Time.DeltaTime * 3000);

        if (!PlayerLocalInfo.targetInfo.hasTarget)
        {
            PlayerLocalInfo.targetInfo.targetEvent?.Invoke(bestTargetData.entity);
            PlayerLocalInfo.targetInfo.hasTarget = true;
        }
    }
}
