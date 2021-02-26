using UnityEngine;
using UnityEngine.UI;

public class UiActionPointIcon : MonoBehaviour
{
    [SerializeField] private Image image;

    private float cooldown;

    public void SetCooldown(float cooldown)
    {
        this.cooldown = cooldown;
    }

    public void UpdateRemainingCooldown(float remainingCooldown)
    {
        float fill = 0;

        if (remainingCooldown > 0)
        {
            fill = remainingCooldown / cooldown;
        } 

        image.fillAmount = fill;
    }
}
