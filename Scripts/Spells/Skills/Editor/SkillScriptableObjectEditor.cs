using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[CustomEditor(typeof(SkillScriptableObject), true)]
public class SkillScriptableObjectEditor : Editor
{
    private SkillScriptableObject skill;

    private void Awake()
    {
        skill = (SkillScriptableObject)target;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Generate Id"))
        {
            skill.GenerateId();

            EditorUtility.SetDirty(skill);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }
    }
}
