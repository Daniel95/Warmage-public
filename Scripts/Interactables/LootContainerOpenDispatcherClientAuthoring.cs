using DOTSNET;
using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Authoring;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;

[DisallowMultipleComponent]
public class LootContainerOpenDispatcherClientAuthoring : MonoBehaviour, SelectiveSystemAuthoring
{
    public Type GetSystemType() { return typeof(LootContainerOpenDispatcherClient); }

    [SerializeField] private PhysicsCategoryTags physicsCategory = PhysicsCategoryTags.Everything;
    [SerializeField] private float minDistance = 4;

    private void Awake()
    {
        LootContainerOpenDispatcherClient lootContainerDispatcherClient = Bootstrap.ClientWorld.GetExistingSystem<LootContainerOpenDispatcherClient>();

        lootContainerDispatcherClient.physicsCategory = physicsCategory;
        lootContainerDispatcherClient.minDistance = minDistance;
    }
}

[ClientWorld]
[UpdateInGroup(typeof(ClientConnectedSimulationSystemGroup))]
[DisableAutoCreation]
public class LootContainerOpenDispatcherClient : SystemBase
{
    [AutoAssign] protected NetworkClientSystem client = null;

    public PhysicsCategoryTags physicsCategory;
    public float minDistance;

    protected override void OnUpdate() 
    {
        if (Input.GetMouseButton(0) && !PlayerChannelBarUI.isChanneling && GetTarget(out Entity targetedEntity))
        {
            if (EntityManager.HasComponent<LootContainerComponent>(targetedEntity))
            {
                float3 lootContainerPosition = EntityManager.GetComponentData<Translation>(targetedEntity).Value;

                if (math.distance(PlayerLocalInfo.position, lootContainerPosition) <= minDistance)
                {
                    //LootContainerComponent lootContainerComponent = EntityManager.GetComponentData<LootContainerComponent>(targetedEntity);
                    ulong lootContainerNetId = EntityManager.GetComponentData<NetworkEntity>(targetedEntity).netId;

                    client.Send(new LootContainerOpenMessage
                    {
                        netId = PlayerLocalInfo.netId,
                        lootContainerNetId = lootContainerNetId
                    });
                }
            }
        }
    }

    private bool GetTarget(out Entity entityHit)
    {
        PhysicsWorld physicsWorld = Bootstrap.ClientWorld.GetExistingSystem<BuildPhysicsWorld>().PhysicsWorld;

        UnityEngine.Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        CollisionFilter collisionFilter = new CollisionFilter()
        {
            BelongsTo = unchecked((uint)~0),
            CollidesWith = physicsCategory.Value
        };

        RaycastInput raycastInput = new RaycastInput
        {
            Start = ray.origin,
            End = ray.origin + (ray.direction * 1000),
            Filter = collisionFilter
        };

        bool hitSomething = physicsWorld.CastRay(raycastInput, out Unity.Physics.RaycastHit hit);

        entityHit = hit.Entity;

        return hitSomething;
    }
}
