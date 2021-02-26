using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class UiHealthBar : MonoBehaviour
{
    private Text healthText;
    private Slider slider;

    public virtual void SetHealth(int currentHealth, int maxHealth)
    {
        if(maxHealth == 0) { return; }

        healthText.text = currentHealth + "/" + maxHealth;

        float scale = (float)currentHealth / (float)maxHealth;
        slider.value = scale;
    }

    protected virtual void Awake()
    {
        slider = GetComponent<Slider>();
        healthText = GetComponentInChildren<Text>();
    }
}
