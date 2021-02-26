using System;
using UnityEngine;
using UnityEngine.UI;

public class StatusEffectIconUI : MonoBehaviour
{
    [SerializeField] private Image icon = null;
    [SerializeField] private Text countText = null;
    [SerializeField] private ShowStatusEffectTooltip showTooltip = null;
    [SerializeField] private Image duration = null;

    private UiStatusEffect statusEffectUI;
    private ulong casterNetId;
    private Guid statusEffectId;
    private float totalTime;
    private float timeLeft;

    public void UpdateTimeLeft(float newTimeLeft)
    {
        timeLeft = newTimeLeft;
    }

    public void UpdateCount(int count)
    {
        if (count == 1)
        {
            countText.text = "";
        }
        else
        {
            countText.text = count.ToString();
        }
    }

    public void Init(UiStatusEffect statusEffectUI, Guid statusEffectId, ulong casterNetId, float timeLeft, int count)
    {
        this.statusEffectUI = statusEffectUI;
        this.statusEffectId = statusEffectId;
        this.casterNetId = casterNetId;

        IStatusEffect statusEffect = StatusEffectLibrary.GetInstance().GetStatusEffectTemplate(statusEffectId);

        totalTime = statusEffect.GetTime();
        this.timeLeft = timeLeft;

        icon.sprite = statusEffect.GetIcon();

        UpdateCount(count);

        TooltipManager.StatusEffectInfo statusEffectInfo = new TooltipManager.StatusEffectInfo()
        {
            name = statusEffect.GetName(),
            description = statusEffect.GetDescription()
        };

        showTooltip.SetInfo(statusEffectInfo);
    }

    private void Update()
    {
        timeLeft -= Time.deltaTime;

        if(timeLeft > 0)
        {
            if(timeLeft == float.PositiveInfinity)
            {
                duration.fillAmount = 0;
            }
            else
            {
                duration.fillAmount = timeLeft / totalTime;
            }
        } 
        else
        {
            statusEffectUI.RemoveStatusEffect(casterNetId, statusEffectId);
        }
    }
}
