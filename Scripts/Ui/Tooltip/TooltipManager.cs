using UnityEngine;

public class TooltipManager : MonoBehaviour
{
    public struct SkillInfo
    {
        public string name;
        public float range;
        public float cooldown;
        public float castTime;
        public float mana;
        public string description;
    };

    public struct StatusEffectInfo
    {
        public string name;
        public string description;
    };

    #region Singleton
    public static TooltipManager GetInstance()
    {
        if (instance == null)
        {
            instance = FindObjectOfType<TooltipManager>();
        }
        return instance;
    }

    private static TooltipManager instance;
    #endregion

    public enum TooltipMode
    {
        None,
        Skill,
        StatusEffect
    }

    public TooltipMode tooltipMode { get; private set; }

    [SerializeField] private RectTransform canvasRectTransform = null;
    [SerializeField] private SkillTooltip skillTooltip = null;
    [SerializeField] private StatusEffectTooltip statusEffectTooltip = null;
    [SerializeField] private Vector2 offset = Vector2.zero;

    private TooltipBase activeTooltip;
    private bool show;

    public void Show(StatusEffectInfo statusEffectInfo)
    {
        SetTooltipActive(statusEffectTooltip);
        statusEffectTooltip.SetInfo(statusEffectInfo);
        tooltipMode = TooltipMode.StatusEffect;
    }

    public void Show(SkillInfo skillInfo)
    {
        SetTooltipActive(skillTooltip);
        skillTooltip.SetInfo(skillInfo);
        tooltipMode = TooltipMode.Skill;
    }

    public void Hide()
    {
        activeTooltip.gameObject.SetActive(false);
        show = false;
    }

    private void SetTooltipActive(TooltipBase tooltipBase)
    {
        tooltipBase.gameObject.SetActive(true);
        activeTooltip = tooltipBase;
        show = true;
    }

    private void Update()
    {
        if(!show) { return; }

        Vector2 mousePosition = Input.mousePosition;
        activeTooltip.transform.position = mousePosition + offset;
        RectTransform backgroundRectTransform = activeTooltip.BackgroundRectTransform;

        Vector2 anchoredPosition = activeTooltip.TooltipRectTransform.anchoredPosition;

        if (anchoredPosition.x + backgroundRectTransform.rect.width > canvasRectTransform.rect.width)
        {
            anchoredPosition.x = canvasRectTransform.rect.width - backgroundRectTransform.rect.width;
        } 
        else if(anchoredPosition.x < 0)
        {
            anchoredPosition.x = 0;
        }

        if (anchoredPosition.y + backgroundRectTransform.rect.height > canvasRectTransform.rect.height)
        {
            anchoredPosition.y = canvasRectTransform.rect.height - backgroundRectTransform.rect.height;
        } 
        else if(anchoredPosition.y < 0)
        {
            anchoredPosition.y = 0;
        }

        activeTooltip.TooltipRectTransform.anchoredPosition = anchoredPosition;
    }
}
