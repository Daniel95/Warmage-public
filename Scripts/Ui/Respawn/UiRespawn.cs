using System;
using UnityEngine;
using UnityEngine.UI;

public class UiRespawn : MonoBehaviour
{
    public static Action respawnButtonClickedEvent;

    [SerializeField] private Button respawnButton;

    public void Show(bool show)
    {
        respawnButton.gameObject.SetActive(show);

        if(show == true)
        {
            respawnButton.interactable = false;

            CoroutineHelper.DelayTime(3, () => respawnButton.interactable = true);
        } 
    }

    private void OnClick()
    {
        respawnButtonClickedEvent?.Invoke();
    }

    private void Awake()
    {
        Show(false);
    }

    private void OnEnable()
    {
        respawnButton.onClick.AddListener(OnClick);
    }

    private void OnDisable()
    {
        respawnButton.onClick.RemoveListener(OnClick);
    }
}
