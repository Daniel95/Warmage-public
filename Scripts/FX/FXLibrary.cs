using DOTSNET;
using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(FXObjectPool))]
public class FXLibrary : MonoBehaviour
{
    #region Singleton
    public static FXLibrary GetInstance()
    {
        if (instance == null)
        {
            instance = FindObjectOfType<FXLibrary>();
        }
        return instance;
    }

    private static FXLibrary instance;
    #endregion
    public NetworkEntityAuthoring fxOneShotEntityPrefab => oneShotPrefab;
    public NetworkEntityAuthoring fxProjectilePrefab => projectilePrefab;

    public FXObjectPool fxPool { get; private set; }

    [SerializeField] private NetworkEntityAuthoring oneShotPrefab = null;
    [SerializeField] private NetworkEntityAuthoring projectilePrefab = null;

    private Dictionary<Entity, Dictionary<int, FXObject>> activeFXEffects = new Dictionary<Entity, Dictionary<int, FXObject>>();

    public void Spawn(Vector3 position, quaternion rotation, Entity entity, int id)
    {
        if (activeFXEffects.ContainsKey(entity) && activeFXEffects[entity].ContainsKey(id)) { Debug.LogWarning("FX already exists on this entity!"); return; }

        FXObject fxObject = fxPool.GetPooledObject(id);
        fxObject.transform.position = position;
        fxObject.transform.rotation = rotation;

        fxObject.Play();

        if (activeFXEffects.ContainsKey(entity))
        {
            activeFXEffects[entity].Add(id, fxObject);
        } 
        else
        {
            Dictionary<int, FXObject> dictionary = new Dictionary<int, FXObject>()
            {
                { id, fxObject }
            };

            activeFXEffects.Add(entity, dictionary);
        }
    }

    public void UpdatePosition(Vector3 position, quaternion rotation, Entity entity, int index)
    {
        Debug.Assert(activeFXEffects.ContainsKey(entity) && activeFXEffects[entity].ContainsKey(index), "doesn't exist!");

        Transform fxTransform = activeFXEffects[entity][index].gameObject.transform;

        fxTransform.position = position;
        fxTransform.rotation = rotation;
    }

    public void Unspawn(Entity entity, int index)
    {
        if (!activeFXEffects.ContainsKey(entity) || !activeFXEffects[entity].ContainsKey(index)) { Debug.LogWarning("FX doesn't exists on this entity!"); return; }

        FXObject fxObject = activeFXEffects[entity][index];

        fxObject.Stop();

        //Unregister in activeFXEffects
        {
            activeFXEffects[entity].Remove(index);

            if (activeFXEffects[entity].Count == 0)
            {
                activeFXEffects.Remove(entity);
            }
        }
    }

    private void Awake()
    {
        fxPool = GetComponent<FXObjectPool>();
    }
}
