using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[CustomEditor(typeof(TemplatePlayerProfiles), true)]
public class DefaultPlayerProfilesEditor : Editor
{
    private TemplatePlayerProfiles defaultPlayerProfiles;

    private void Awake()
    {
        defaultPlayerProfiles = (TemplatePlayerProfiles)target;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Update max PlayerProfile skills"))
        {
            defaultPlayerProfiles.UpdateMaxPlayerProfileSkills();

            EditorUtility.SetDirty(defaultPlayerProfiles);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }
    }
}