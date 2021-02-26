using System;
using System.Collections.Generic;
using UnityEngine;

public class UiInitializer : MonoBehaviour
{
    public enum InitializeType
    {
        Enable,
        Disable
    }

    [Serializable]
    public class UiLayer
    {
        public UiLayerType uiLayerType;
        public InitializeType initializeType;
    }

    [SerializeField] private List<UiLayer> uiLayers = null;

    private void Awake()
    {
        foreach (UiLayer uiLayer in uiLayers)
        {
            if (uiLayer.initializeType == InitializeType.Enable)
            {
                UiManager.GetInstance().ActivateLayer(uiLayer.uiLayerType);
            }
            else
            {
                UiManager.GetInstance().DeactivateLayer(uiLayer.uiLayerType);
            }
        }
    }
}