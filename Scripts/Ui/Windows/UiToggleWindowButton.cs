using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UiToggleWindowButton : MonoBehaviour
{
    [SerializeField] private UiWindowBase uiWindow = null;
 
    private Button button;

    private void OnClick()
    {
        if (uiWindow.gameObject.activeSelf)
        {
            uiWindow.gameObject.SetActive(false);
        } 
        else
        {
            uiWindow.gameObject.SetActive(true);
        }
    }

    private Button GetButton()
    {
        if(button == null)
        {
            button = GetComponent<Button>();
        }

        return button;
    }

    private void OnEnable()
    {
        GetButton().onClick.AddListener(OnClick);
    }

    private void OnDisable()
    {
        GetButton().onClick.RemoveListener(OnClick);
    }
}
