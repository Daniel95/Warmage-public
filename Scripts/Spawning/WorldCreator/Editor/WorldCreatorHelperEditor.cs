using DOTSNET;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[CustomEditor(typeof(WorldCreatorHelper), true)]

public class WorldCreatorHelperEditor : Editor
{
    private WorldCreatorHelper worldCreatorHelper;

    private void Awake()
    {
        worldCreatorHelper = (WorldCreatorHelper)target;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Show"))
        {
            worldCreatorHelper.Show();

            EditorUtility.SetDirty(worldCreatorHelper);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }

        if (GUILayout.Button("Hide"))
        {
            worldCreatorHelper.Hide();

            EditorUtility.SetDirty(worldCreatorHelper);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }
    }
}