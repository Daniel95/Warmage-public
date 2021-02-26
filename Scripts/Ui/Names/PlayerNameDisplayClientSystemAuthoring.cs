using DOTSNET;
using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerNameDisplayClientSystemAuthoring : MonoBehaviour, SelectiveSystemAuthoring
{
    public Type GetSystemType() => typeof(PlayerNameDisplayClientSystem);
}

[ClientWorld]
[UpdateInGroup(typeof(ClientConnectedSimulationSystemGroup))]
[DisableAutoCreation]
public class PlayerNameDisplayClientSystem : SystemBase
{
    private NativeList<NameDisplayInfo> names;

    protected override void OnCreate()
    {
        base.OnCreate();

        names = new NativeList<NameDisplayInfo>(25, Allocator.Persistent);
    }

    protected override void OnDestroy()
    {
        names.Dispose();

        base.OnDestroy();
    }

    protected override void OnUpdate()
    {
        NativeList<NameDisplayInfo> _names = names;

        Entities.WithNone<LocalPlayerComponent>().ForEach((in Translation translation,
                    in PlayerInfoComponent playerInfoComponent) =>
        {
            _names.Add(new NameDisplayInfo { position = translation.Value, name = playerInfoComponent.name });
        })
        .Run();

        foreach (NameDisplayInfo nameDisplayInfo in _names)
        {
            PlayerNameUI.GetInstance().Show(nameDisplayInfo.name.ToString(), nameDisplayInfo.position);
        }

        names.Clear();
    }

    private struct NameDisplayInfo 
    {
        public float3 position;
        public FixedString32 name;
    }
}