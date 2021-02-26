using System;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;

public abstract class SkillPlacementBase : MonoBehaviour
{
    public SkillPlacementType placementType => skillPlacementType;
    public bool isPlacing { get; private set; }

    protected Action<float3, quaternion> placedEvent;
    protected ISkill skill;
    protected Camera cam;

    [SerializeField] protected GameObject placementVisual = null;
    [SerializeField] private SkillPlacementType skillPlacementType = SkillPlacementType.None;

    public abstract void OnUpdate();

    public virtual void StartPlacement(ISkill skill, Action<float3, quaternion> onPlaced)
    {
        placedEvent = onPlaced;
        isPlacing = true;
        this.skill = skill;
        placementVisual.SetActive(true);
    }

    public virtual void StopPlacement()
    {
        placementVisual.SetActive(false);
        placedEvent = null;
    }

    private void Update()
    {
        //Delay setting isPlacing by one frame in order to stop other inputs registering this frame.
        if(!placementVisual.activeSelf)
        {
            isPlacing = false;
            return; 
        }

        OnUpdate();
    }

    protected virtual void Awake()
    {
        placementVisual.SetActive(false);

        cam = Camera.main;
    }

    protected bool GetMousePositionOnSurface(out float3 clickPosition, CollisionFilter collisionFilter, PhysicsWorld physicsWorld)
    {
        UnityEngine.Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        RaycastInput clickRaycastInput = new RaycastInput
        {
            Start = ray.origin,
            End = ray.origin + (ray.direction * 1000),
            Filter = collisionFilter
        };

        if (physicsWorld.CastRay(clickRaycastInput, out Unity.Physics.RaycastHit clickHit)) 
        {
            clickPosition = clickHit.Position;

            return true;
        } 
        else
        {
            clickPosition = float3.zero;

            return false;
        }
    }

    protected float3 ClampToMaxSkillRange(out bool clamped, float3 position)
    {
        float3 playerPosition = PlayerLocalInfo.position;

        float2 position2d = new float2(position.x, position.z);
        float2 playerPosition2d = new float2(playerPosition.x, playerPosition.z);

        if (math.distance(position2d, playerPosition2d) > skill.GetRange())
        {
            float2 direction = math.normalize(position2d - playerPosition2d);
            float2 newPosition2d = playerPosition2d + direction * skill.GetRange();
            float3 newPosition = new float3(newPosition2d.x, position.y, newPosition2d.y);

            clamped = true;

            return newPosition;
        } 
        else
        {
            clamped = false;
            return position;
        }
    }

    protected float3 ClampToCameraVision(out bool clamped, float3 position, CollisionFilter collisionFilter, PhysicsWorld physicsWorld)
    {
        RaycastInput canSeeRaycastInput = new RaycastInput
        {
            Start = cam.transform.position,
            End = position,
            Filter = collisionFilter
        };

        if (physicsWorld.CastRay(canSeeRaycastInput, out Unity.Physics.RaycastHit canSeeHit))
        {
            clamped = true;

            return canSeeHit.Position;
        } 
        else
        {
            clamped = false;

            return position;
        }
    }
}
