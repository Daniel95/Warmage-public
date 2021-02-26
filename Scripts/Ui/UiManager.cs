using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum UiLayerType
{
    Game,
    MainMenu
}

[Serializable]
public class UiLayer
{
    public UiLayerType uiLayerType;
    public GameObject ui;
}

public class UiManager : MonoBehaviour
{
    #region Singleton
    public static UiManager GetInstance()
    {
        if (instance == null)
        {
            instance = FindObjectOfType<UiManager>();
        }
        return instance;
    }

    private static UiManager instance;
    #endregion

    public ScriptedAnimationController SceneFadeScriptedAnimationController => sceneFadeScriptedAnimationController;

    [SerializeField] private ScriptedAnimationController sceneFadeScriptedAnimationController = null;
    [SerializeField] private Image fadeImage = null;
    [SerializeField] private List<UiLayer> uiLayers = null;

    private List<UiLayerType> activeUILayers = new List<UiLayerType>();

    /// <summary>
    /// Fades the Image in so it appears the scene is 'faded'.
    /// </summary>
    /// <param name="fadeSceneOutCompleted"></param>
    public void FadeSceneOut(Action fadeSceneOutCompleted = null)
    {
        fadeImage.raycastTarget = true;

        sceneFadeScriptedAnimationController.CancelAnimation(ScriptedAnimationType.Out);
        sceneFadeScriptedAnimationController.StartAnimation(ScriptedAnimationType.In, fadeSceneOutCompleted);
    }

    /// <summary>
    /// Fades the Image out so it appears the scene is 'clear'.
    /// </summary>
    /// <param name="fadeSceneInCompleted"></param>
    public void FadeSceneIn(Action fadeSceneInCompleted = null)
    {
        sceneFadeScriptedAnimationController.CancelAnimation(ScriptedAnimationType.In);
        sceneFadeScriptedAnimationController.StartAnimation(ScriptedAnimationType.Out, () =>
            {
                fadeImage.raycastTarget = false;
                if (fadeSceneInCompleted != null)
                {
                    fadeSceneInCompleted();
                }
            }
        );
    }

    public void ActivateLayer(UiLayerType uiLayerType)
    {
        UiLayer uILayer = uiLayers.Find(x => x.uiLayerType == uiLayerType);

        uILayer?.ui.SetActive(true);
    }

    public void DeactivateLayer(UiLayerType uiLayerType)
    {
        UiLayer uILayer = uiLayers.Find(x => x.uiLayerType == uiLayerType);

        uILayer?.ui.SetActive(false);
    }

    private void Awake()
    {
        fadeImage.raycastTarget = false;

        foreach (var layer in uiLayers)
        {
            layer.ui.SetActive(false);
        }
    }
}
