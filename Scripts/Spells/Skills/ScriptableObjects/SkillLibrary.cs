using System;
using System.Collections.Generic;
using UnityEngine;

public class SkillLibrary : MonoBehaviour
{
    #region Singleton
    private static SkillLibrary instance;

    public static SkillLibrary GetInstance()
    {
        if (instance == null)
        {
            instance = FindObjectOfType<SkillLibrary>();
        }
        return instance;
    }
    #endregion

    [SerializeField] private string skillsPath = "Skills";

    private Dictionary<Guid, ISkill> skills = new Dictionary<Guid, ISkill>();

    public bool Exists(Guid id) => skills.ContainsKey(id);

    public ISkill GetSkillTemplate(Guid id)
    {
        Debug.Assert(skills.ContainsKey(id), "skillId not found: " + id.ToString());
        return skills[id].ShallowCopy();
    }

    public List<Guid> GetAllPlayerSkillIds()
    {
        List<Guid> skillIds = new List<Guid>();

        foreach (var skill in skills)
        {
            if (skill.Value.IsPlayerOwned())
            {
                skillIds.Add(skill.Value.GetId());
            }
        }

        return skillIds;
    }

    private void Start()
    {
        UnityEngine.Object[] skillbjects = Resources.LoadAll(skillsPath, typeof(ISkill));

        for (int i = 0; i < skillbjects.Length; i++)
        {
            ISkill skill = (ISkill)skillbjects[i];

            Debug.Assert(!skills.ContainsKey(skill.GetId()), "Duplicate skill id: " + skill.GetName());

            skills.Add(skill.GetId(), skill);
        }
    }
}
