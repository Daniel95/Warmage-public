using DOTSNET;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[CustomEditor(typeof(PrefabSystemAuthoring), true)]
public class PrefabSystemAuthoringEditor : Editor
{
    private const string networkPrefabs = "NetworkPrefabs";

    private PrefabSystemAuthoring prefabSystemAuthoring;

    private void Awake()
    {
        prefabSystemAuthoring = (PrefabSystemAuthoring)target;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Update prefabs"))
        {
            UnityEngine.Object[] skillbjects = Resources.LoadAll(networkPrefabs, typeof(NetworkEntityAuthoring));

            NetworkEntityAuthoring[] networkEntityAuthorings = new NetworkEntityAuthoring[skillbjects.Length];

            for (int i = 0; i < skillbjects.Length; i++)
            {
                NetworkEntityAuthoring networkEntityAuthoring = (NetworkEntityAuthoring)skillbjects[i];
                networkEntityAuthorings[i] = networkEntityAuthoring;
            }

            prefabSystemAuthoring.prefabs = networkEntityAuthorings.ToList();

            EditorUtility.SetDirty(prefabSystemAuthoring);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }
    }
}