using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;

//public class FXManager : FXPool
//{
//    // singleton reference
//    private static FXManager instance;
//    public static FXManager GetInstance()
//    {
//        if (instance == null)
//        {
//            instance = FindObjectOfType<FXManager>();
//        }

//        return instance;
//    }

//    // explosion to pool
//    [SerializeField] private GameObject explosionPrefab = null;

//    // tag is used to find object in pool
//    [ReadOnly] private string explosionTag = "Explosion";

//    // size of the initial object pool
//    [SerializeField] private int poolSize = 40;

//    // simple Singleton
//    protected virtual void Awake()
//    {
//        if (instance != null && instance != this)
//        {
//            Destroy(gameObject);
//        }
//        else
//        {
//            instance = this;
//        }

//        // create a new entry in the object pool
//        if (explosionPrefab != null)
//        {
//            ObjectPoolFXItem explosionPoolItem = new ObjectPoolFXItem
//            {
//                prefab = explosionPrefab,
//                amountToPool = poolSize,
//                shouldExpand = true
//            };

//            // add to the current pool
//            itemsToPool.Add(explosionPoolItem);
//        }
//    }

//    // move the pooled explosion prefab into place (particles are set to Play on Awake)
//    public void CreateExplosion(Vector3 a_pos, Quaternion a_rot)
//    {
//        GameObject instance = GetPooledObject(explosionTag);
//        if (instance != null)
//        {
//            instance.SetActive(false);
//            instance.transform.position = a_pos;
//            instance.transform.rotation = a_rot;
//            instance.SetActive(true);
//        }

//    }
//}
