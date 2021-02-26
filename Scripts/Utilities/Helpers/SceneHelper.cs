using UnityEngine;
using System.Collections;
using System;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;

public class SceneHelper
{
    public static List<Scenes> Scenes;
    private static int SceneLoadCounter;
    private static int ScenesToLoad;

    static SceneHelper()
    {
        Array sceneArray = Enum.GetValues(typeof(Scenes));
        Scenes = sceneArray.Cast<Scenes>().ToList();
    }

    public static void LoadSceneOverTime(Scenes scenes, Action onLoaded = null)
    {
        CoroutineHelper.Start(LoadAdditive(scenes, onLoaded));
    }

    public static void LoadScenesOverTime(List<Scenes> scenesList, Action onLoaded = null)
    {
        Debug.Assert(SceneLoadCounter == 0, "SceneLoadCounter should always be zero when loading a new scene.");
        Debug.Assert(ScenesToLoad == 0, "ScenesToLoad should always be zero when loading a new scene.");

        ScenesToLoad = scenesList.Count;

        foreach (var sceneName in scenesList)
        {
            CoroutineHelper.Start(LoadAdditive(sceneName, () =>
            {
                SceneLoadCounter++;
                if (SceneLoadCounter >= ScenesToLoad)
                {
                    ScenesToLoad = SceneLoadCounter = 0;

                    onLoaded();
                }
            }));
        }
    }

    public static void UnloadSceneOverTime(Scenes scenes, Action onLoaded = null)
    {
        CoroutineHelper.Start(UnloadAdditive(scenes, onLoaded));
    }

    public static void UnloadScenesOverTime(List<Scenes> scenesList, Action onUnloaded = null)
    {
        Debug.Assert(SceneLoadCounter == 0, "SceneLoadCounter should always be zero when loading a new scene.");
        Debug.Assert(ScenesToLoad == 0, "ScenesToLoad should always be zero when loading a new scene.");

        ScenesToLoad = scenesList.Count;

        foreach (var sceneName in scenesList)
        {
            CoroutineHelper.Start(UnloadAdditive(sceneName, () =>
            {
                SceneLoadCounter++;
                if (SceneLoadCounter >= ScenesToLoad)
                {
                    ScenesToLoad = SceneLoadCounter = 0;

                    onUnloaded();
                }
            }));
        }
    }

    private static IEnumerator LoadAdditive(Scenes scenes, Action onLoaded = null)
    {
        yield return SceneManager.LoadSceneAsync(scenes.ToString(), LoadSceneMode.Additive);

        Scene scene = SceneManager.GetSceneByName(scenes.ToString());
        SceneManager.SetActiveScene(scene);

        if (onLoaded != null)
        {
            onLoaded();
        }
    }

    private static IEnumerator UnloadAdditive(Scenes scenes, Action onDone = null)
    {
        yield return SceneManager.UnloadSceneAsync(scenes.ToString());

        Scene scene = SceneManager.GetSceneByName(global::Scenes.Default.ToString());
        SceneManager.SetActiveScene(scene);

        GC.Collect();
        Resources.UnloadUnusedAssets();
        GC.Collect();

        if (onDone != null)
        {
            onDone();
        }
    }

}
