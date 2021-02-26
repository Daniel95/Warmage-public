using Unity.Mathematics;
using UnityEngine;

public class WorldBounds : MonoBehaviour
{
    #region Singleton
    public static WorldBounds GetInstance()
    {
        if (instance == null)
        {
            instance = FindObjectOfType<WorldBounds>();
        }
        return instance;
    }

    private static WorldBounds instance;
    #endregion

    public float3 min => bottomLeftBound.position;
    public float3 max => topRightBound.position;

    [SerializeField] private Transform bottomLeftBound = null;
    [SerializeField] private Transform topRightBound = null;

    public static float3 GetRandomPositionWithinBounds(float3 min, float3 max, Unity.Mathematics.Random random)
    {
        return new float3(random.NextFloat(min.x, max.x), random.NextFloat(min.y, max.y), random.NextFloat(min.z, max.z));
    }
}