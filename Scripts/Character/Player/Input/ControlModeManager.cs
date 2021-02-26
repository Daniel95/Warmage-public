using Unity.Entities;
using UnityEngine;

public enum ControlMode
{
    CharacterControl,
    MouseControl
}

public class ControlModeManager : MonoBehaviour
{
    #region Singleton
    public static ControlModeManager GetInstance()
    {
        if (instance == null)
        {
            instance = FindObjectOfType<ControlModeManager>();
        }
        return instance;
    }

    private static ControlModeManager instance;
    #endregion

    public static ControlMode mode { get; private set; }

    [SerializeField] private KeyCode mouseModeSwitchKey = KeyCode.Escape;

    public void SetControlMode(ControlMode newMouseMode)
    {
        if (newMouseMode == ControlMode.CharacterControl)
        {
            mode = ControlMode.CharacterControl;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            mode = ControlMode.MouseControl;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private void Update()
    {
        if (PlayerLocalInfo.entity == Entity.Null)
        {
            SetControlMode(ControlMode.MouseControl);
            return;
        }

        if (Input.GetKeyDown(mouseModeSwitchKey))
        {
            if(mode == ControlMode.CharacterControl)
            {
                SetControlMode(ControlMode.MouseControl);
            } 
            else
            {
                SetControlMode(ControlMode.CharacterControl);
            }
        }
    }
}
