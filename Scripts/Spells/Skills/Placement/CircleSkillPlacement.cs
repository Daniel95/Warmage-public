using DOTSNET;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Authoring;
using Unity.Physics.Systems;
using UnityEngine;

public class CircleSkillPlacement : SkillPlacementBase
{
    [SerializeField] private PhysicsCategoryTags placementCollisionPhysicsCategoryTags;
    [SerializeField] private PhysicsCategoryTags surfacePhysicsCategoryTag;

    public override void OnUpdate()
    {
        PhysicsWorld physicsWorld = Bootstrap.ClientWorld.GetExistingSystem<BuildPhysicsWorld>().PhysicsWorld;

        CollisionFilter placementCollisionFilter = new CollisionFilter()
        {
            BelongsTo = unchecked((uint)~0),
            CollidesWith = placementCollisionPhysicsCategoryTags.Value
        };

        if (!GetMousePositionOnSurface(out float3 mouseWorldPosition, placementCollisionFilter, physicsWorld)) { return; }

        mouseWorldPosition = ClampToMaxSkillRange(out bool clampedRange, mouseWorldPosition);
        mouseWorldPosition = ClampToCameraVision(out bool clampedVision, mouseWorldPosition, placementCollisionFilter, physicsWorld);

        CollisionFilter surfaceCollisionFilter = new CollisionFilter()
        {
            BelongsTo = unchecked((uint)~0),
            CollidesWith = placementCollisionPhysicsCategoryTags.Value
        };

        if(clampedRange || clampedVision)
        {
            RaycastInput downRaycastInput = new RaycastInput
            {
                Start = mouseWorldPosition,
                End = new float3(mouseWorldPosition.x, mouseWorldPosition.y - 100, mouseWorldPosition.z),
                Filter = surfaceCollisionFilter
            };

            if (physicsWorld.CastRay(downRaycastInput, out Unity.Physics.RaycastHit newPositionOnGround))
            {
                mouseWorldPosition.y = newPositionOnGround.Position.y;
            } 
        }

        placementVisual.transform.position = mouseWorldPosition + new float3(0, 0.5f, 0);

        if (Input.GetMouseButtonDown(0))
        {
            if (placedEvent != null)
            {
                placedEvent(placementVisual.transform.position, placementVisual.transform.rotation);
            }

            StopPlacement();
        }
    }
}
