using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class EditorHelper
{
    [MenuItem("Tools/Generate Scene List")]
    public static void GenerateSceneList()
    {
        SceneListCheck.Generate();
    }

    [MenuItem("Tools/Delete PlayerPrefs")]
    public static void DeletePlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
    }

    [MenuItem("Tools/Go to Persistant Data path folder")]
    public static void OpenPersistantDataPathFolder()
    {
        EditorUtility.RevealInFinder(Application.persistentDataPath);
    }

    [MenuItem("Tools/Go to Data path folder")]
    public static void OpenDataPathFolder()
    {
        EditorUtility.RevealInFinder(Application.dataPath);
    }

    [MenuItem("Tools/Remove Debug Objects")]
    public static void RemoveDebugObjects()
    {
        DebugHelper.RemoveAllDebugPositions();
    }

    [MenuItem("Tools/Check duplicate Object Id's")]
    public static void CheckDuplicateObjectIds()
    {
        List<Guid> guids = new List<Guid>();

        ObjectId[] objectIds = UnityEngine.Object.FindObjectsOfType<ObjectId>();

        for (int i = 0; i < objectIds.Length; i++)
        {
            ObjectId objectId = objectIds[i];

            if (guids.Contains(objectId.Id))
            {
                Debug.LogWarning("Id " + objectId.Id + " is duplicate on " + objectId.name, objectId.gameObject);
            } 
            else
            {
                guids.Add(objectId.Id);
            }
        }
    }

    [MenuItem("Tools/Snap all to surface")]
    public static void SnapAllToSurface()
    {
        SnapToSurface[] snapToSurfaces = UnityEngine.Object.FindObjectsOfType<SnapToSurface>();

        for (int i = 0; i < snapToSurfaces.Length; i++)
        {
            snapToSurfaces[i].layerMask.value = 1 << LayerMask.NameToLayer("NavSurface");
            snapToSurfaces[i].Snap();
        }
    }
}
