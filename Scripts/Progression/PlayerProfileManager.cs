using System;
using UnityEngine;

[RequireComponent(typeof(TemplatePlayerProfiles))]
public class PlayerProfileManager : MonoBehaviour
{
    public static Action profileLoadedEvent;

    #region Singleton
    public static PlayerProfileManager GetInstance()
    {
        if (instance == null)
        {
            instance = FindObjectOfType<PlayerProfileManager>();
        }
        return instance;
    }

    private static PlayerProfileManager instance;
    #endregion

    public PlayerProfile playerProfile { get; private set; }

    [SerializeField] private bool loadTemplateProfile = false;
    [SerializeField] private TemplatePlayerProfiles.TemplatePlayerProfileType templateToLoad = TemplatePlayerProfiles.TemplatePlayerProfileType.Max;

    public void Save(PlayerProfile playerProfile)
    {
        this.playerProfile = playerProfile;

        SaveSystem.Save(playerProfile);
    }

    private void Start()
    {
        TemplatePlayerProfiles templatePlayerProfiles = GetComponent<TemplatePlayerProfiles>();

        templatePlayerProfiles.GenerateProfiles();

        if (loadTemplateProfile)
        {
            playerProfile = templatePlayerProfiles.GetProfile(templateToLoad);
        } 
        else if(SaveSystem.SaveExists())
        {
            playerProfile = SaveSystem.Load();
        } 
        else 
        {
            playerProfile = templatePlayerProfiles.GetProfile(TemplatePlayerProfiles.TemplatePlayerProfileType.Start);
            SaveSystem.Save(playerProfile);
        }

        if(profileLoadedEvent != null) 
        {
            profileLoadedEvent();
        }
    }
}
