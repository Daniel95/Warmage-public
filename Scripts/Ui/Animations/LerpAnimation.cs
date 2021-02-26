using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// This animation can be used to lerp certain values by overriding this class.
/// </summary>
public abstract class LerpAnimation : ScriptedAnimation
{
    [Range(0, 1)] public float TargetValue = 1;
    public float Speed = 1;

    protected float startValue = 1;
    protected float currentValue;

    protected abstract void Apply(float _value);

    public override void StartAnimation(Action _animationStoppedEvent = null)
    {
        base.StartAnimation(_animationStoppedEvent);
    }

    protected override IEnumerator Animate()
    {
        float _progress = 0;
        currentValue = startValue;
        while (currentValue != TargetValue)
        {
            _progress += Speed * Time.deltaTime;
            currentValue = Mathf.Lerp(startValue, TargetValue, _progress);
            Apply(currentValue);
            yield return null;
        }

        Apply(TargetValue);
        startValue = TargetValue;

        StopAnimation(true);
    }
}