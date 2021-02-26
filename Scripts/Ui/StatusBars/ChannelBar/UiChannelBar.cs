using UnityEngine;
using UnityEngine.UI;

public class UiChannelBar : MonoBehaviour
{
    public bool channeling { get; private set; }

    [SerializeField] private Slider slider = null;
    [SerializeField] private Image interruptImage = null;
    [SerializeField] private ScriptedAnimationController interruptScriptedAnimationController = null;

    private float timer;

    public void StartChanneling(float time)
    {
        if (channeling)
        {
            Debug.LogWarning("already channeling!");
            return;
        }

        interruptScriptedAnimationController.CancelAnimation(ScriptedAnimationType.Out);
        interruptScriptedAnimationController.gameObject.SetActive(false);

        channeling = true;
        slider.maxValue = timer = time;
    }

    public void UpdateChannelTime(float time)
    {
        timer = time;
        slider.value = slider.maxValue - timer;
    }

    public void InterruptChanneling()
    {
        if (!channeling)
        {
            Debug.LogWarning("not channeling!");
            return;
        }

        StopChanneling();

        Color color = interruptImage.color;
        color.a = 1;
        interruptImage.color = color;

        interruptScriptedAnimationController.gameObject.SetActive(true);
        interruptScriptedAnimationController.StartAnimation(ScriptedAnimationType.Out, () =>
        {
            interruptScriptedAnimationController.gameObject.SetActive(false);
        });
    }

    public void StopChanneling()
    {
        if (!channeling)
        {
            Debug.LogWarning("not channeling!");
            return;
        }

        slider.value = slider.maxValue = timer = 0;
        channeling = false;
    }

    protected virtual void Update()
    {
        if (!channeling) { return; }

        timer -= Time.deltaTime;
        slider.value = slider.maxValue - timer;

        if (timer <= 0)
        {
            StopChanneling();
        }
    }

    private void Awake()
    {
        slider.value = 0;
        interruptScriptedAnimationController.gameObject.SetActive(false);
    }
}
