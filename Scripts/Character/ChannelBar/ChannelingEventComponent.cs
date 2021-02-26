using System;
using Unity.Entities;
using UnityEngine;

[GenerateAuthoringComponent]
public class ChannelingEventComponent : IComponentData
{
    [HideInInspector] public Action onCompleteEvent;
}
