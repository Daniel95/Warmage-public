using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[CustomEditor(typeof(StatusEffectScriptableObject), true)]
public class StatusEffectScriptableObjectEditor : Editor
{
    private StatusEffectScriptableObject statusEffect;

    private void Awake()
    {
        statusEffect = (StatusEffectScriptableObject)target;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Generate Id"))
        {
            statusEffect.GenerateId();

            EditorUtility.SetDirty(statusEffect);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }
    }
}