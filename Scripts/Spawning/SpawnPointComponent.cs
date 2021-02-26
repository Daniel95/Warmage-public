using System.Net.Security;
using Unity.Entities;
using UnityEngine;

[GenerateAuthoringComponent]
public struct SpawnPointComponent : IComponentData
{
    [HideInInspector] public float respawnTimer;
    [HideInInspector] public bool isFull;
    [HideInInspector] public bool readyToRespawn;

    public void ResetTimer(float respawnTimer)
    {
        readyToRespawn = false;
        this.respawnTimer = respawnTimer;
    }
}