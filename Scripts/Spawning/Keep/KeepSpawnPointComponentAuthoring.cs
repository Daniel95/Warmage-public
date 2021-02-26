using System;
using System.Reflection;
using Unity.Entities;
using UnityEditor;
using UnityEngine;

[DisallowMultipleComponent]
public class KeepSpawnPointComponentAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    [HideInInspector] public SerializableGuid keepId;
    [HideInInspector] public int spawnPointIndex;

    [SerializeField] private KeepSpawnPointComponent.KeepSpawnPointType spawnPointType = KeepSpawnPointComponent.KeepSpawnPointType.Spot;
    [Range(5, 100)] [SerializeField] private float areaRadius = 5;
    [Range(1, 100)] [SerializeField] private int capacity = 1;

    [Header("Npc Settings")]
    [SerializeField] private NpcType[] npcTypes = new NpcType[1] { NpcType.Tier2 };
    [SerializeField] private NpcBehaviourType npcBehaviourType = NpcBehaviourType.GuardPost;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        int npcTypesStorage = 0;

        for (int i = 0; i < npcTypes.Length; i++)
        {
            npcTypesStorage = BitHelper.Set(npcTypesStorage, (int)npcTypes[i], i);
        }

        dstManager.AddComponentData(entity, new KeepSpawnPointComponent
        {
            keepId = keepId,
            spawnPointIndex = spawnPointIndex,
            npcTypesStorage = npcTypesStorage,
            npcTypesStorageLength = npcTypes.Length,
            npcBehaviourType = npcBehaviourType,
            areaSize = areaRadius,
            spawnPointType = spawnPointType,
            maxOccupiers = capacity
        });
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if(npcTypes == null || npcTypes.Length == 0)
        {
            name = "Spawnpoint: ERROR";
        }
        else if(npcTypes.Length == 1)
        {
            if (spawnPointType == KeepSpawnPointComponent.KeepSpawnPointType.Area)
            {
                name = "Spawnpoint Area (" + capacity + "): " + npcBehaviourType.ToString() + " " + npcTypes[0];
            } 
            else
            {
                name = "Spawnpoint Spot : " + npcBehaviourType.ToString() + " " + npcTypes[0];
            }
        } 
        else 
        {
            if (spawnPointType == KeepSpawnPointComponent.KeepSpawnPointType.Area)
            {
                name = "Spawnpoint Area (" + capacity + "): " + npcBehaviourType.ToString();
            }
            else
            {
                name = "Spawnpoint Spot: " + npcBehaviourType.ToString();
            }
        } 

        if (npcBehaviourType == NpcBehaviourType.Commander)
        {
            DrawIcon(gameObject, 6);

            capacity = 1;
        } 
        else
        {
            DrawIcon(gameObject, 5);
        }

        if(spawnPointType == KeepSpawnPointComponent.KeepSpawnPointType.Spot)
        {
            capacity = 1;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (spawnPointType == KeepSpawnPointComponent.KeepSpawnPointType.Area)
        {
            Gizmos.DrawWireSphere(gameObject.transform.position, areaRadius);
        }
    }

    private void DrawIcon(GameObject gameObject, int idx)
    {
        var largeIcons = GetTextures("sv_label_", string.Empty, 0, 8);
        var icon = largeIcons[idx];
        var egu = typeof(EditorGUIUtility);
        var flags = BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.NonPublic;
        var args = new object[] { gameObject, icon.image };
        var setIcon = egu.GetMethod("SetIconForObject", flags, null, new Type[] { typeof(UnityEngine.Object), typeof(Texture2D) }, null);
        setIcon.Invoke(null, args);
    }

    private GUIContent[] GetTextures(string baseName, string postFix, int startIndex, int count)
    {
        GUIContent[] array = new GUIContent[count];
        for (int i = 0; i < count; i++)
        {
            array[i] = EditorGUIUtility.IconContent(baseName + (startIndex + i) + postFix);
        }
        return array;
    }
#endif
}

public struct KeepSpawnPointComponent : IComponentData
{
    public enum KeepSpawnPointType
    {
        Spot,
        Area
    }

    public Guid keepId;
    public int npcTypesStorage;
    public int npcTypesStorageLength;
    public int maxOccupiers;
    public int occupierCount;
    public int spawnPointIndex;
    public NpcBehaviourType npcBehaviourType;
    public KeepSpawnPointType spawnPointType;
    public float areaSize;
}
