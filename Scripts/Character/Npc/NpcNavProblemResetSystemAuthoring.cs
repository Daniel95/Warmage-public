using DOTSNET;
using System;
using Unity.Entities;
using UnityEngine;

public class NpcNavProblemResetSystemAuthoring : MonoBehaviour, SelectiveSystemAuthoring
{
    public Type GetSystemType() => typeof(NpcNavProblemResetSystem);
}

[ServerWorld]
[UpdateInGroup(typeof(ServerActiveSimulationSystemGroup))]
[DisableAutoCreation]
public class NpcNavProblemResetSystem : SystemBase
{
    protected override void OnUpdate()
    {

    }
}