using System.Collections.Generic;
using UnityEngine;

public class PlayerRespawnPoint : MonoBehaviour
{
    private static readonly Dictionary<FactionType, List<Vector3>> respawnPoints = new Dictionary<FactionType, List<Vector3>>();

    [SerializeField] private FactionType faction = FactionType.Blue;

    public static Vector3 GetNearest(FactionType factionType, Vector3 deathPosition) 
    {
        List<Vector3> factionRespawnPositions = respawnPoints[factionType];

        Vector3 closestPoint = new Vector3();
        float smallestDistanceToPoint = float.MaxValue;

        foreach (var respawnPoint in factionRespawnPositions)
        {
            if (Vector3.Distance(respawnPoint, deathPosition) < smallestDistanceToPoint)
            {
                closestPoint = respawnPoint;
            }
        }

        return closestPoint;
    }

    private void Awake()
    {
        if(respawnPoints.ContainsKey(faction))
        {
            respawnPoints[faction].Add(transform.position);
        } 
        else
        {
            respawnPoints[faction] = new List<Vector3>() { transform.position };
        }
    }
}
