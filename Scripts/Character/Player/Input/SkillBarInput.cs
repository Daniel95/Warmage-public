using DOTSNET;
using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(SkillCooldownManager))]
public class SkillBarInput : MonoBehaviour
{
    public enum CastResult
    {
        NoSkillAssigned,
        InvalidTarget,
        Cooldown,
        OutOfRange,
        BusyChanneling,
        BusyPlacing,
        Success
    }

    #region Singleton
    public static SkillBarInput GetInstance()
    {
        if (instance == null)
        {
            instance = FindObjectOfType<SkillBarInput>();
        }
        return instance;
    }

    private static SkillBarInput instance;
    #endregion

    public SkillCooldownManager skillCooldownManager { get; private set; }

    private SkillDispatcherClient skillInputDispatcherClient;

    public static CastResult CanCastSpell(ISkill skill)
    {
        if (SkillPlacementManager.GetInstance().GetIsPlacing()) { return CastResult.BusyPlacing; }

        if (PlayerChannelBarUI.isChanneling) { return CastResult.BusyChanneling; }

        bool validTarget = skill.IsManuallyPlaced() ||
            skill.ActivateOnFriendly() ||
            (PlayerLocalInfo.targetInfo.hasTarget && PlayerLocalInfo.factionType != PlayerLocalInfo.targetInfo.factionType);

        if (!validTarget) { return CastResult.InvalidTarget; }

        if (!GetInstance().skillCooldownManager.CanUse(skill.GetId())) { return CastResult.Cooldown; }

        if (!skill.IsManuallyPlaced() && (skill.GetRange() < PlayerLocalInfo.targetInfo.distance && !skill.ActivateOnFriendly())) { return CastResult.OutOfRange; }

        return CastResult.Success;
    }

    public void SkillInput(ISkill skill)
    {
        if (!PlayerLocalInfo.isAlive) { return; }

        if (skill.IsManuallyPlaced())
        {
            SkillPlacementManager.GetInstance().StartPlacement(skill, (float3 position, quaternion rotation) =>
            {
                //Cast skill
                skillCooldownManager.RegisterSkillUsed(skill.GetId());

                ulong targetNetId = GetTargetNetId(skill);

                if (skill.GetCastTime() > 0)
                {
                    if (skill is ICirclePlacementSkill)
                    {
                        skillInputDispatcherClient.SendCirclePlacementSkill(skill.GetId(), PlayerLocalInfo.netId, PlayerLocalInfo.factionType, position);
                    }
                    else if (skill is IConePlacementSkill)
                    {
                        skillInputDispatcherClient.SendConePlacementSkill(skill.GetId(),
                            PlayerLocalInfo.netId,
                            PlayerLocalInfo.factionType,
                            position,
                            rotation,
                            PlayerLocalInfo.position);
                    }
                }
                else
                {
                    if (skill is ICirclePlacementSkill)
                    {
                        skillInputDispatcherClient.SendCirclePlacementSkill(skill.GetId(), PlayerLocalInfo.netId, PlayerLocalInfo.factionType, position);
                    }
                    else if (skill is IConePlacementSkill)
                    {
                        skillInputDispatcherClient.SendConePlacementSkill(skill.GetId(),
                                    PlayerLocalInfo.netId,
                                    PlayerLocalInfo.factionType,
                                    position,
                                    rotation,
                                    PlayerLocalInfo.position);
                    }
                }
            });
        }
        else
        {
            //Cast skill
            skillCooldownManager.RegisterSkillUsed(skill.GetId());

            ulong targetNetId = GetTargetNetId(skill);

            if (skill.GetCastTime() > 0)
            {
                skillInputDispatcherClient.SendSkill(skill.GetId(), targetNetId, PlayerLocalInfo.factionType);
            }
            else
            {
                skillInputDispatcherClient.SendSkill(skill.GetId(), targetNetId, PlayerLocalInfo.factionType);
            }
        }
    }

    private ulong GetTargetNetId(ISkill skill)
    {
        if (skill.ActivateOnFriendly())
        {
            ulong targetNetId;

            bool hasFriendlyTarget = PlayerLocalInfo.targetInfo.hasTarget && PlayerLocalInfo.factionType == PlayerLocalInfo.targetInfo.factionType;

            if (hasFriendlyTarget)
            {
                targetNetId = PlayerLocalInfo.targetInfo.netId;
            }
            else
            {
                targetNetId = PlayerLocalInfo.netId;
            }

            return targetNetId;
        }
        else
        {
            ulong targetNetId = PlayerLocalInfo.targetInfo.netId;
            return targetNetId;
        }
    }

    private void Awake()
    {
        skillInputDispatcherClient = Bootstrap.ClientWorld.GetExistingSystem<SkillDispatcherClient>();
        skillCooldownManager = GetComponent<SkillCooldownManager>();
    }
}
