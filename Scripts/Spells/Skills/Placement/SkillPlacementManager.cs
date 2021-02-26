using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class SkillPlacementManager : MonoBehaviour
{
    #region Singleton
    public static SkillPlacementManager GetInstance()
    {
        if (instance == null)
        {
            instance = FindObjectOfType<SkillPlacementManager>();
        }
        return instance;
    }

    private static SkillPlacementManager instance;
    #endregion

    private Dictionary<SkillPlacementType, SkillPlacementBase> skillPlacementMethods = new Dictionary<SkillPlacementType, SkillPlacementBase>();

    private SkillPlacementBase activeSkillPlacement;

    public bool GetIsPlacing()
    {
        foreach (SkillPlacementBase skillPlacementBase in skillPlacementMethods.Values)
        {
            if(skillPlacementBase.isPlacing)
            {
                return true;
            }
        }

        return false;
    }

    public void StartPlacement(ISkill skill, Action<float3, quaternion> onPlaced)
    {
        StopPlacement();

        activeSkillPlacement = skillPlacementMethods[skill.GetPlacementType()];

        activeSkillPlacement.StartPlacement(skill, onPlaced);
    }

    public void StopPlacement()
    {
        if (activeSkillPlacement != null)
        {
            activeSkillPlacement.StopPlacement();
        }
    }

    private void Awake()
    {
        SkillPlacementBase[] skillPlacementBases = GetComponents<SkillPlacementBase>();

        foreach (SkillPlacementBase skillPlacementBase in skillPlacementBases)
        {
            skillPlacementMethods.Add(skillPlacementBase.placementType, skillPlacementBase);
        }
    }
}
