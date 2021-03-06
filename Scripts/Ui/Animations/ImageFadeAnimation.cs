﻿using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Fades an images in or out.
/// </summary>
public class ImageFadeAnimation : LerpAnimation
{
    [SerializeField] private Image image = null;

    public Image GetImage() => image;

    public override void StartAnimation(Action animationStoppedEvent = null)
    {
        startValue = image.color.a;
        base.StartAnimation(animationStoppedEvent);
    }

    protected override void Apply(float _value)
    {
        Color _tempColor = image.color;
        _tempColor.a = _value;
        image.color = _tempColor;
    }
}
