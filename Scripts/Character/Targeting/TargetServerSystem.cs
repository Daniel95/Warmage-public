using DOTSNET;
using System;
using Unity.Entities;
using UnityEngine;

public class TargetServerSystemAuthoring : MonoBehaviour, SelectiveSystemAuthoring
{
    public Type GetSystemType() => typeof(TargetServerSystem);

}

public class TargetServerSystem : SystemBase
{
    private EndServerSimulationEntityCommandBufferSystem endServerSimulationEntityCommandBufferSystem;

    protected override void OnCreate()
    {
        base.OnCreate();

        endServerSimulationEntityCommandBufferSystem = World.GetOrCreateSystem<EndServerSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        //Entities.ForEach(() => { });
    }
} 
