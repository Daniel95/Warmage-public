using DOTSNET;
using System;
using UnityEngine;

public class NpcLibrary : MonoBehaviour
{
    [Serializable]
    public struct NpcTypePrefab
    {
        public NetworkEntityAuthoring prefab;
        public NpcType type;
    }

    #region Singleton
    public static NpcLibrary GetInstance()
    {
        if (instance == null)
        {
            instance = FindObjectOfType<NpcLibrary>();
        }
        return instance;
    }

    private static NpcLibrary instance;
    #endregion

    [SerializeField] private NpcTypePrefab[] npcTypePrefabs = null;

    public NetworkEntityAuthoring GetNpcPrefab(NpcType npcType)
    {
        foreach (NpcTypePrefab npcTypePrefab in npcTypePrefabs)
        {
            if(npcTypePrefab.type == npcType)
            {
                return npcTypePrefab.prefab;
            }
        }

        Debug.Assert(true, "No prefab of npc type " + npcType + " is registered in the npc library!");

        return null;
    }
}
