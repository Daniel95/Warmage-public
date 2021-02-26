using Reese.Nav;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[DisallowMultipleComponent]
public class NavAgentAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    [SerializeField] private float jumpDegrees = 45;
    [SerializeField] private float jumpGravity = 200;
    [SerializeField] private float translationSpeed = 20;
    [SerializeField] private float3 offset = new float3(0, 1, 0);

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new NavAgent
        {
            JumpDegrees = jumpDegrees,
            JumpGravity = jumpGravity,
            TranslationSpeed = translationSpeed,
            RotationSpeed = 0.3f,
            TypeID = NavUtil.GetAgentType(NavConstants.HUMANOID),
            Offset = offset
        });

        dstManager.AddComponentData(entity, new NavNeedsSurface { });
        dstManager.AddComponentData(entity, new NavTerrainCapable { });
        dstManager.AddComponentData(entity, new Parent { });
        dstManager.AddComponentData(entity, new LocalToParent { });
        dstManager.AddComponentData(entity, new LocalToWorld { });
    }
}