using Unity.Entities;
using Unity.Rendering;
using UnityEngine;

public class DisableHybridRenderingComponentAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.RemoveComponent<RenderMesh>(entity);
        dstManager.RemoveComponent<PerInstanceCullingTag>(entity);
        dstManager.RemoveComponent<ChunkWorldRenderBounds>(entity);
        dstManager.RemoveComponent<WorldRenderBounds>(entity);
        dstManager.RemoveComponent<RenderBounds>(entity);
    }
}
