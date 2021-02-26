using System;
using UnityEngine;

[Serializable]
public struct HealOverTimeData
{
    public int intervalHeal => _intervalHeal;
    public float intervalTime => _intervalTime;

    public int totalHeal;
    public int count;

    private int _intervalHeal;
    private float _intervalTime;

    public void Init(float time)
    {
#if UNITY_EDITOR
        if (totalHeal == 0) { Debug.LogError("totalHeal = 0"); return; }
        if (time == 0) { Debug.LogError("time = 0"); return; }
        if (count == 0) { Debug.LogError("count = 0"); return; }
#endif

        _intervalHeal = totalHeal / count;
        _intervalTime = time / count;

#if UNITY_EDITOR
        if ((float)totalHeal % (float)count != 0.0f)
        {
            Debug.LogError("division of " + totalHeal + " by " + count + " does not results in zero: " + (float)totalHeal % (float)count);
        }
#endif
    }
}