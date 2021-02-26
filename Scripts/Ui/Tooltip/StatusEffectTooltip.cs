using UnityEngine;
using UnityEngine.UI;

class StatusEffectTooltip : TooltipBase
{
    [SerializeField] private new Text name = null;
    [SerializeField] private Text description = null;

    public void SetInfo(TooltipManager.StatusEffectInfo statusEffectInfo)
    {
        description.text = statusEffectInfo.description;
        name.text = statusEffectInfo.name;

        float textPaddingSize = 4f;
        Vector2 backgroundSize = new Vector2(name.preferredWidth + textPaddingSize * 2f, name.preferredHeight + textPaddingSize * 2f);
        BackgroundRectTransform.sizeDelta = backgroundSize;
    }
}
