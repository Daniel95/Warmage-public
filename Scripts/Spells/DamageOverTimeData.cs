using System;
using UnityEngine;

[Serializable]
public struct DamageOverTimeData
{
    public int intervalDamage => _intervalDamage;
    public float intervalTime => _intervalTime;

    public int totalDamage;
    public int count;

    private int _intervalDamage;
    private float _intervalTime;

    public void Init(float time)
    {
#if UNITY_EDITOR
        if (totalDamage == 0) { Debug.LogError("totalDamage = 0"); return; }
        if (time == 0) { Debug.LogError("time = 0"); return; }
        if (count == 0) { Debug.LogError("count = 0"); return; }
#endif

        _intervalDamage = totalDamage / count;
        _intervalTime = time / count;

#if UNITY_EDITOR
        if ((float)totalDamage % (float)count != 0.0f)
        {
            Debug.LogError("division of " + totalDamage + " by " + count + " does not results in zero: " + (float)totalDamage % (float)count);
        }
#endif
    }
}