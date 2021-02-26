using DOTSNET;
using System;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class SpawnPointServerSystemAuthoring : MonoBehaviour, SelectiveSystemAuthoring
{
    public Type GetSystemType() => typeof(SpawnPointServerSystem);
}

[ServerWorld]
[UpdateInGroup(typeof(ServerActiveSimulationSystemGroup))]
[DisableAutoCreation]
public class SpawnPointServerSystem : SystemBase
{
    protected override void OnUpdate()
    {
        float deltaTime = Time.DeltaTime;

        Entities.ForEach((ref SpawnPointComponent spawnPointComponent,
            in Entity entity,
            in LocalToWorld localToWorld) =>
        {
            if (!spawnPointComponent.isFull)
            {
                if (spawnPointComponent.respawnTimer < 0)
                {
                    spawnPointComponent.readyToRespawn = true;
                } 
                else
                {
                    spawnPointComponent.respawnTimer -= deltaTime;
                }
            }
        })
        .ScheduleParallel();
    }
}
