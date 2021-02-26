using System;
using System.Collections.Generic;
using UnityEngine;

public class TemplatePlayerProfiles : MonoBehaviour
{
    [Serializable]
    public struct TemplatePlayerProfileEditor
    {
        public TemplatePlayerProfileType type;
        public PlayerProfileEditor playerProfileEditor;
    }

    [Serializable]
    public struct PlayerProfileEditor
    {
        public int globalXp;
        public List<SkillScriptableObject> skills;
        public SkillScriptableObject[] skillBar;
    }

    public enum TemplatePlayerProfileType
    {
        Start,
        Max,
        Testing
    }

    [SerializeField] private string playerSkillsPath = "Skills/Player";
    [SerializeField] private TemplatePlayerProfileEditor[] defaultPlayerProfileEditors = new TemplatePlayerProfileEditor[0];

    private Dictionary<TemplatePlayerProfileType, PlayerProfile> defaultPlayerProfiles = new Dictionary<TemplatePlayerProfileType, PlayerProfile>();

    public PlayerProfile GetProfile(TemplatePlayerProfileType type)
    {
        if(defaultPlayerProfiles.ContainsKey(type))
        {
            return defaultPlayerProfiles[type];
        } 
        else
        {
            Debug.Assert(false, "Default profile: " + type + " does not exist!");
            return new PlayerProfile();
        }
    }

    public void UpdateMaxPlayerProfileSkills()
    {
        UnityEngine.Object[] skillbjects = Resources.LoadAll(playerSkillsPath, typeof(ISkill));

        int maxProfileIndex = -1;

        for (int i = 0; i < defaultPlayerProfileEditors.Length; i++)
        {
            if(defaultPlayerProfileEditors[i].type == TemplatePlayerProfileType.Max)
            {
                maxProfileIndex = i;
            }
        }

        Debug.Assert(maxProfileIndex != -1, "Max default player profile not set!");

        PlayerProfileEditor playerProfileEditor = defaultPlayerProfileEditors[maxProfileIndex].playerProfileEditor;

        playerProfileEditor.skills.Clear();

        for (int i = 0; i < skillbjects.Length; i++)
        {
            SkillScriptableObject skill = (SkillScriptableObject)skillbjects[i];

            playerProfileEditor.skills.Add(skill);
        }

        defaultPlayerProfileEditors[maxProfileIndex].playerProfileEditor = playerProfileEditor;
    }

    public void GenerateProfiles()
    {
        foreach (TemplatePlayerProfileEditor templatePlayerProfile in defaultPlayerProfileEditors)
        {
            PlayerProfileEditor playerProfileEditor = templatePlayerProfile.playerProfileEditor;

            List<SerializableGuid> skillsIds = new List<SerializableGuid>();

            for (int i = 0; i < playerProfileEditor.skills.Count; i++)
            {
                skillsIds.Add(playerProfileEditor.skills[i].GetId());
            }

            SerializableGuid[] skillBarIds = new SerializableGuid[16];

            for (int i = 0; i < playerProfileEditor.skillBar.Length; i++)
            {
                if(playerProfileEditor.skillBar[i] != null)
                {
                    skillBarIds[i] = playerProfileEditor.skillBar[i].GetId();
                } 
                else
                {
                    skillBarIds[i].Value = string.Empty;
                }
            }

            defaultPlayerProfiles.Add(templatePlayerProfile.type, new PlayerProfile
            {
                skillBar = skillBarIds,
                skills = skillsIds,
                globalXp = templatePlayerProfile.playerProfileEditor.globalXp
            });
        }
    }
}
