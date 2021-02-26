using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UiGame : MonoBehaviour
{
    #region Singleton
    public static UiGame GetInstance()
    {
        if (instance == null)
        {
            instance = FindObjectOfType<UiGame>();
        }
        return instance;
    }

    private static UiGame instance;
    #endregion

    public GraphicRaycaster graphicsRaycaster => _graphicsRaycaster;
    public EventSystem eventSystem => _eventSystem;
    public UiStatusBar playerStatusBar => playerStatusBarUi;
    public UiSkillbar skillBar => uiSkillBar;
    public UiXpBar XpBar => uiXpBar;
    public UiFloatingTextManager floatingTextManager => uiFloatingTextManager;
    public UiHealthEffect healthEffect => uiDamageEffect;
    public UiRespawn respawn => uiRespawn;

    [SerializeField] private GraphicRaycaster _graphicsRaycaster = null;
    [SerializeField] private EventSystem _eventSystem = null;
    [SerializeField] private UiStatusBar playerStatusBarUi = null;
    [SerializeField] private UiSkillbar uiSkillBar = null;
    [SerializeField] private UiXpBar uiXpBar = null;
    [SerializeField] private UiFloatingTextManager uiFloatingTextManager = null;
    [SerializeField] private UiHealthEffect uiDamageEffect = null;
    [SerializeField] private UiRespawn uiRespawn = null;
}
