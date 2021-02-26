using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Can switch from scene to scene using async loading. Also plays an scene fade in/out animation when switching.
/// The default scene should always be active so that scene is never loaded/unloaded.
/// </summary>
[RequireComponent(typeof(LevelLibrary))]
public class LevelLoader : MonoBehaviour
{
    public LevelType CurrentLevel => currentLevelType;
    public List<Scenes> CurrentScenes => currentScenes;
    public LevelLibrary LevelLibrary => levelLibrary;

    /// <summary>
    /// Old scene, New Scene
    /// </summary>
    public static Action<LevelType, LevelType> LevelSwitchStartedEvent;
    public static Action<LevelType, LevelType> LevelSwitchCompletedEvent;
    public static Action<LevelType, LevelType> FadeSceneOutStartedEvent;
    public static Action<LevelType, LevelType> FadeSceneInStartedEvent;
    public static Action<LevelType, LevelType> FadeSceneInCompletedEvent;

    #region Singleton
    public static LevelLoader Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<LevelLoader>();
            }
            return instance;
        }
    }

    private static LevelLoader instance;
    #endregion

    [SerializeField] private LevelType startLevelType = LevelType.None;
    [SerializeField] private bool autoLoad = false;

    //private static Scenes? currentScene;
    private List<Scenes> currentScenes = new List<Scenes>();
    private LevelType currentLevelType = LevelType.None;

    private LevelLibrary levelLibrary;

    /// <summary>
    /// Load the scene indicated by the startScene assigned in the editor.
    /// </summary>
    /// <param name="_newScene"></param>
    public void LoadStartScene()
    {
        if (startLevelType == LevelType.None)
        {
            Debug.Assert(true, "Cannot load the Default scene as start scene.");
            return;
        }

        SwitchScene(startLevelType);
    }

    /// <summary>
    /// Switch to the new scene.
    /// </summary>
    /// <param name="newScene"></param>
    public void SwitchScene(LevelType newLevelType)
    {
        Debug.Assert(enabled, "GameObject is not enabled!");

        if (newLevelType == LevelType.None)
        {
            Debug.LogWarning("Cannot switch to the None level.");
            return;
        }

        List<Scenes> newScenes = levelLibrary.GetLevel(newLevelType);
        List<Scenes> scenesToUnload = currentScenes.Except(newScenes).ToList();
        List<Scenes> scenesToLoad = newScenes.Except(currentScenes).ToList();

        LevelType previousLevelType = currentLevelType;

        if (currentScenes.Count != 0)
        {
            if (FadeSceneOutStartedEvent != null)
            {
                FadeSceneOutStartedEvent(currentLevelType, newLevelType);
            }
            UiManager.GetInstance().FadeSceneOut(() =>
            {
                if (LevelSwitchStartedEvent != null)
                {
                    LevelSwitchStartedEvent(currentLevelType, newLevelType);
                }

                SceneHelper.UnloadScenesOverTime(scenesToUnload, () =>
                {
                    SceneHelper.LoadScenesOverTime(scenesToLoad, () =>
                    {
                        if (FadeSceneInStartedEvent != null)
                        {
                            FadeSceneInStartedEvent(currentLevelType, newLevelType);
                        }

                        UiManager.GetInstance().FadeSceneIn(() =>
                        {
                            if (FadeSceneInCompletedEvent != null)
                            {
                                FadeSceneInCompletedEvent(currentLevelType, newLevelType);
                            }

                            if (LevelSwitchCompletedEvent != null)
                            {
                                LevelSwitchCompletedEvent(previousLevelType, newLevelType); ;
                            }
                        });
                    });
                });
            });
        }
        else
        {
            SceneHelper.LoadScenesOverTime(scenesToLoad, () =>
            {
                if (FadeSceneInStartedEvent != null)
                {
                    FadeSceneInStartedEvent(currentLevelType, newLevelType);
                }

                UiManager.GetInstance().FadeSceneIn(() =>
                {
                    if (LevelSwitchCompletedEvent != null)
                    {
                        LevelSwitchCompletedEvent(currentLevelType, newLevelType);
                    }

                    if (FadeSceneInCompletedEvent != null)
                    {
                        FadeSceneInCompletedEvent(currentLevelType, newLevelType);
                    }
                });
            });
        }

        currentLevelType = newLevelType;
        currentScenes = newScenes;
    }

    private void Awake()
    {
        levelLibrary = GetComponent<LevelLibrary>();

        if(autoLoad)
        {
            LoadStartScene();
        }
    }
}