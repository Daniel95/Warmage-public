using DOTSNET;
using Unity.Entities;
using UnityEngine;

[ServerWorld]
[AlwaysUpdateSystem]
[UpdateInGroup(typeof(ServerActiveSimulationSystemGroup), OrderFirst = true, OrderLast = false)]
//[DisableAutoCreation]
public class BeginServerSimulationEntityCommandBufferSystem : EntityCommandBufferSystem
{

    //protected override void OnUpdate()
    //{
    //    base.OnUpdate();
    //}

    protected override void OnStopRunning()
    {
        // Must call OnUpdate before stopping in order to flush remaining buffers and avoid memory leak.    
        base.OnUpdate();
        base.OnStopRunning();
    }
}

[ServerWorld]
[AlwaysUpdateSystem]
[UpdateInGroup(typeof(ServerActiveSimulationSystemGroup), OrderFirst = false, OrderLast = true)]
//[DisableAutoCreation]
public class EndServerSimulationEntityCommandBufferSystem : EntityCommandBufferSystem
{
    protected override void OnStopRunning()
    {
        // Must call OnUpdate before stopping in order to flush remaining buffers and avoid memory leak.    
        base.OnUpdate();
        base.OnStopRunning();
    }
}
