using System;
using Unity.Entities;

public class StartChannelingComponent : IComponentData
{
    public Action onCompleteEvent;
    public float time;
}
