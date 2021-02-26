using DOTSNET;
using System;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[DisallowMultipleComponent]
public class CameraFollowTargetClientSystemAuthoring : MonoBehaviour, SelectiveSystemAuthoring
{
    public Type GetSystemType() => typeof(CameraFollowTargetClientSystem);
}

[ClientWorld]
[UpdateInGroup(typeof(ClientConnectedSimulationSystemGroup))]
[DisableAutoCreation]
public class CameraFollowTargetClientSystem : SystemBase
{
    private Camera camera = null;

    protected override void OnUpdate()
    {
        float3 cameraPosition = new float3();

        Entities.ForEach((
            in CameraFollowTargetComponent cameraFollowTargetComponent,
            in LocalPlayerComponent localPlayerComponent,
            in Translation translation) => 
        {
            cameraPosition = translation.Value + cameraFollowTargetComponent.offset;
        })
        .Run();

        if(camera == null)
        {
            camera = Camera.main;
        }

        //If there is no CameraFollowTargetComponent, this system will be set to sleep and not modify the main camera position.
        camera.transform.position = cameraPosition;
    }
}