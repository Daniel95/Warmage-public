using UnityEngine;

public class LootContainerSpawnPointsManager : MonoBehaviour
{
    private void OnValidate()
    {
        LootContainerSpawnPointComponentAuthoring[] spawnPoints = GetComponentsInChildren<LootContainerSpawnPointComponentAuthoring>();
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            spawnPoints[i].spawnPointIndex = i;
        }
    }
}