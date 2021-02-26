using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerNameUI : MonoBehaviour
{
    #region Singleton
    public static PlayerNameUI GetInstance()
    {
        if (instance == null)
        {
            instance = FindObjectOfType<PlayerNameUI>();
        }
        return instance;
    }

    private static PlayerNameUI instance;
    #endregion

    [SerializeField] private GameObject playerNameTextPrefab = null;
    [SerializeField] private float yOffset = 3;

    private Dictionary<string, PlayerNameData> names = new Dictionary<string, PlayerNameData>();
    private Camera playerCamera;

    public void Show(string name, Vector3 position)
    {
        if(names.ContainsKey(name))
        {
            PlayerNameData data = names[name];

            data.textMesh.text = name;
            UpdatePosition(data.transform, position);
            data.updatedThisFrame = true;
        } 
        else
        {
            GameObject playerNameGameObject = ObjectPool.GetInstance().GetObjectForType(playerNameTextPrefab.name);
            TextMesh textMesh = playerNameGameObject.GetComponentInChildren<TextMesh>();

            textMesh.text = name;

            UpdatePosition(playerNameGameObject.transform, position);

            names.Add(name, new PlayerNameData { transform = playerNameGameObject.transform, textMesh = textMesh, updatedThisFrame = true });
        }
    }

    private void UpdatePosition(Transform playerNameTransform, Vector3 position)
    {
        playerNameTransform.position = position + new Vector3(0, yOffset, 0);

        playerNameTransform.LookAt(playerCamera.transform.position);
    }

    private void LateUpdate()
    {
        List<string> namesToRemove = new List<string>();

        foreach (var name in names)
        {
            PlayerNameData playerNameData = name.Value;

            if (!playerNameData.updatedThisFrame)
            {
                namesToRemove.Add(name.Key);
            }

            playerNameData.updatedThisFrame = false;
        }

        foreach (string nameToRemove in namesToRemove)
        {
            ObjectPool.GetInstance().PoolObject(names[nameToRemove].transform.gameObject);
            names.Remove(nameToRemove);
        }
    }

    private void Awake()
    {
        playerCamera = Camera.main;
    }

    private class PlayerNameData
    {
        public Transform transform;
        public TextMesh textMesh;
        public bool updatedThisFrame;
    }
}
