using DOTSNET;
using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ObjectPoolFXItem
{
    public FXObject fxObjectPrefab;
    public int amountToPool;
}

public class FXObjectPool : MonoBehaviour
{
    public const int CirclePlacementIndex = 0;
    public const int ConePlacementIndex = 1;

    [SerializeField] private List<ObjectPoolFXItem> itemsToPool = null;

    private List<List<FXObject>> pooledObjects;

    // Use this for initialization
    protected virtual void Start()
    {
        pooledObjects = new List<List<FXObject>>();

        for (int i = 0; i < itemsToPool.Count; i++)
        {
            ObjectPoolFXItem item = itemsToPool[i];

            pooledObjects.Add(new List<FXObject>());

            for (int j = 0; j < item.amountToPool; j++)
            {
                GameObject instance = Instantiate(item.fxObjectPrefab.gameObject);
                instance.SetActive(false);

                FXObject fxObject = instance.GetComponent<FXObject>();

                Debug.Assert(fxObject != null, "No FXObject script added to " + instance.name);

                pooledObjects[i].Add(fxObject);
            }
        }
    }

    public int GetIndex(Guid id)
    {
        for (int i = 0; i < pooledObjects.Count; i++)
        {
            if(id == pooledObjects[i][0].id)
            {
                return i;
            }
        }

        Debug.Assert(false, "ID " + id.ToString() + " does not exists!");

        return -1;
    }

    public FXObject GetPooledObject(int index)
    {
        Debug.Assert(index < pooledObjects.Count, "sub pool with index " + index + " does not exists!");

        List<FXObject> subPool = pooledObjects[index];

        for (int i = 0; i < subPool.Count; i++)
        {
            if (!subPool[i].gameObject.activeInHierarchy)
            {
                return subPool[i];
            }
        }

        GameObject obj = Instantiate(subPool[0].gameObject);

        FXObject fxObject = obj.GetComponent<FXObject>();

        obj.SetActive(false);
        subPool.Add(fxObject);

        return fxObject;
    }

    private void OnValidate()
    {
        foreach (var item in itemsToPool)
        {
            if(item.amountToPool < 1)
            {
                item.amountToPool = 1;
            }
        }
    }
}
