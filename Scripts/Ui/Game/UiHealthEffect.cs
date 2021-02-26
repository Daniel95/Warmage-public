using UnityEngine;

public class UiHealthEffect : MonoBehaviour
{
    [SerializeField] private ScriptedAnimationController scriptedAnimationController = null;
    [SerializeField] private ImageFadeAnimation inAnimation = null;
    [SerializeField] private ImageFadeAnimation outAnimation = null;
    [SerializeField] private Color damageColor = Color.red;
    [SerializeField] private Color healColor = Color.green;
    [SerializeField] private int minHealthChange = 20;
    [SerializeField] private float minAlpha = 0.02f;
    [SerializeField] private int maxHealthChange = 1000;
    [SerializeField] private float maxAlpha = 0.2f;

    private bool isDamage;

    public void Play(int healthChange)
    {
        bool changed = false;

        if(healthChange < 0)
        {
            inAnimation.GetImage().color = damageColor;
            outAnimation.GetImage().color = damageColor;

            if(!isDamage)
            {
                changed = true;
                isDamage = true;
            }
        }
        else
        {
            inAnimation.GetImage().color = healColor;
            outAnimation.GetImage().color = healColor;

            if (isDamage)
            {
                changed = true;
                isDamage = false;
            }
        }

        healthChange = Mathf.Clamp(Mathf.Abs(healthChange), 0, maxHealthChange);

        if (healthChange < minHealthChange) { return; }

        scriptedAnimationController.gameObject.SetActive(true);

        float damageRange = maxHealthChange - minHealthChange;
        float damageFactor = healthChange / damageRange;

        float alphaRange = maxAlpha - minAlpha;
        float alpha = minAlpha + alphaRange * damageFactor;

        if(scriptedAnimationController.IsAnimating && !changed)
        {
            inAnimation.TargetValue += alpha;
            inAnimation.TargetValue = Mathf.Clamp(inAnimation.TargetValue, minAlpha, maxAlpha);
        } 
        else
        {
            inAnimation.TargetValue = alpha;
        }

        scriptedAnimationController.StartAnimation(ScriptedAnimationType.In, () => 
        {
            scriptedAnimationController.StartAnimation(ScriptedAnimationType.Out); 
        });
    }

    private void Awake()
    {
        scriptedAnimationController.gameObject.SetActive(false);
    }
}
