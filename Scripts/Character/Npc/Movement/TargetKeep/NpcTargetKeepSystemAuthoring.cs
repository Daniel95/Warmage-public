using DOTSNET;
using Reese.Nav;
using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.AI;

[DisallowMultipleComponent]
public class NpcTargetKeepSystemAuthoring : MonoBehaviour, SelectiveSystemAuthoring
{
    public Type GetSystemType() => typeof(NpcTargetKeepSystem);

    [Range(0, 150)] [SerializeField] private float closestKeepDistanceAllowance = 80.0f;
    [Range(0, 30)] [SerializeField] private float minDistanceToTargetKeepReached = 15.0f;

    private void Awake()
    {
        NpcTargetKeepSystem npcPatrolServerSystem = Bootstrap.ServerWorld.GetExistingSystem<NpcTargetKeepSystem>();

        npcPatrolServerSystem.keepDistanceAllowance = closestKeepDistanceAllowance;
        npcPatrolServerSystem.minDistanceToTargetKeepReached = minDistanceToTargetKeepReached;
    }
}

[ServerWorld]
[UpdateInGroup(typeof(ServerActiveSimulationSystemGroup))]
[DisableAutoCreation]
public class NpcTargetKeepSystem : SystemBase
{
    private struct KeepData
    {
        public float3 position;
        public float threatFactor;
        public FactionType factionType;
    }

    private struct KeepTarget
    {
        public float3 position;
        public float distance;
    }

    public float keepDistanceAllowance;
    public float minDistanceToTargetKeepReached;

    private NativeList<KeepData> allKeepDatas;
    private NativeList<KeepTarget> keepTargetCandidates;

    protected override void OnCreate()
    {
        base.OnCreate();

        allKeepDatas = new NativeList<KeepData>(Allocator.Persistent);
        keepTargetCandidates = new NativeList<KeepTarget>(Allocator.Persistent);
    }

    protected override void OnDestroy()
    {
        allKeepDatas.Dispose();
        keepTargetCandidates.Dispose();

        base.OnDestroy();
    }

    protected override void OnUpdate()
    {
        NativeList<KeepData> _allKeepDatas = allKeepDatas;
        NativeList<KeepTarget> _keepTargetCandidates = keepTargetCandidates;

        uint seed = 1 + (uint)UnityEngine.Random.Range(int.MinValue, int.MaxValue - 1);
        Unity.Mathematics.Random random = new Unity.Mathematics.Random(seed);

        Entities
            .ForEach((
                in Translation translation,
                in KeepComponent keepComponent
            ) =>
            {
                _allKeepDatas.Add(new KeepData
                {
                    position = translation.Value,
                    threatFactor = keepComponent.threatFactor,
                    factionType = keepComponent.factionType
                });
            }).Run();

        float _keepDistanceAllowance = keepDistanceAllowance;
        float _minDistanceToTargetKeepReached = minDistanceToTargetKeepReached;

        Entities
            .WithReadOnly(_allKeepDatas)
            .WithNone<NavLerping>()
            .ForEach((
                            ref NpcTargetKeepComponent npcTargetKeepComponent,
                            in FactionComponent factionComponent,
                            in Entity entity,
                            in Translation translation) =>
            {
                float2 worldPosition2d = new float2(translation.Value.x, translation.Value.z);

                if (npcTargetKeepComponent.hasTarget) 
                {
                    float distanceToKeep = math.distance(worldPosition2d, new float2(npcTargetKeepComponent.targetKeepPosition.x, npcTargetKeepComponent.targetKeepPosition.z));

                    if (distanceToKeep > _minDistanceToTargetKeepReached)
                    {
                        return;
                    } 
                }

                //Find closest keep
                float3 closestKeepPosition = float3.zero;
                float closetKeepDistance = int.MaxValue;

                for (int i = 0; i < _allKeepDatas.Length; i++)
                {
                    KeepData keepData = _allKeepDatas[i];

                    if (factionComponent.factionType != keepData.factionType)
                    {
                        float3 keepPosition = keepData.position;

                        float distanceToKeep = math.distance(worldPosition2d, new float2(keepPosition.x, keepPosition.z));

                        _keepTargetCandidates.Add(new KeepTarget
                        {
                            position = keepData.position,
                            distance = distanceToKeep
                        });

                        if (distanceToKeep < closetKeepDistance)
                        {
                            closetKeepDistance = distanceToKeep;
                            closestKeepPosition = keepData.position;
                        }
                    }
                }

                for (int i = _keepTargetCandidates.Length - 1; i >= 0; i--)
                {
                    KeepTarget keepTargetCandidate = _keepTargetCandidates[i];

                    //If the distance difference is too big, discard this candidate.
                    if (keepTargetCandidate.distance - closetKeepDistance > _keepDistanceAllowance)
                    {

                        _keepTargetCandidates.RemoveAt(i);
                    }
                }

                npcTargetKeepComponent.hasTarget = true;

                int randomIndex = random.NextInt(_keepTargetCandidates.Length);

                npcTargetKeepComponent.targetKeepPosition = _keepTargetCandidates[randomIndex].position;

                _keepTargetCandidates.Clear();
            })
        .Run();

        allKeepDatas.Clear();
        keepTargetCandidates.Clear();
    }
}
