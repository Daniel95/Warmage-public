using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[CustomEditor(typeof(SnapToSurface), true)]
public class SnapToSurfaceEditor : Editor
{
    private SnapToSurface snapToSurface;

    private void Awake()
    {
        snapToSurface = (SnapToSurface)target;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Snap To Surface"))
        {
            snapToSurface.Snap();

            EditorUtility.SetDirty(snapToSurface);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }
    }
}