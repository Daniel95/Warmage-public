using Unity.Mathematics;
using Unity.Physics.Authoring;
using UnityEngine;

public class ConeSkillPlacement : SkillPlacementBase
{
    [SerializeField] private PhysicsCategoryTags physicsCategoryTags;
    [SerializeField] private float coneDistance = 2.0f;

    public override void OnUpdate()
    {
        float3 forwardCamera = cam.transform.forward;
        forwardCamera.y = 0.0f;
        placementVisual.transform.rotation = Quaternion.LookRotation(forwardCamera);

        float3 vecPos = PlayerLocalInfo.position + math.normalize(forwardCamera) * coneDistance;
        placementVisual.transform.position = vecPos;

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
