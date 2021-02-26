using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(TextMesh))]
public class PlayerName : MonoBehaviour
{
    private static Dictionary<ulong, PlayerName> playerNames = new Dictionary<ulong, PlayerName>();

    [SerializeField] private float yOffset = 1;

    private ulong netId;

    public static void SetPosition(ulong netId, float3 position)
    {
        if(playerNames.ContainsKey(netId))
        {
            playerNames[netId].transform.position = position + new float3(0, playerNames[netId].yOffset, 0);
        } 
    }

    public void Init(ulong netId)
    {
        playerNames.Add(netId, this);
        GetComponent<TextMesh>().text = netId.ToString();
    }
}


