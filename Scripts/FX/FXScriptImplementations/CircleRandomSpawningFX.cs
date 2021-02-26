using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleRandomSpawningFX : FXScriptBase
{
    [SerializeField] private GameObject prefab = null;
    [SerializeField] private float radius = 8f;
    [SerializeField] private float minSpawnTime = 0.3f;
    [SerializeField] private float maxSpawnTime = 0.8f;

    private float spawnTimer = 0;
    private Coroutine coroutine;
    private ObjectPool objectPool;

    public override void Play()
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
            coroutine = null;
        }

        coroutine = StartCoroutine(StartMove());
    }

    public IEnumerator StartMove()
    {
        spawnTimer = 0;

        while (true)
        {
            spawnTimer -= Time.deltaTime;

            if (spawnTimer < 0)
            {
                spawnTimer = Random.Range(minSpawnTime, maxSpawnTime);

                Vector2 random2DDirection = new Vector2(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f)).normalized;

                Vector3 randomDirection = new Vector3(random2DDirection.x, 0, random2DDirection.y);

                float randomDistance = Random.Range(0.0f, radius);

                GameObject ray = objectPool.GetObjectForType(prefab.name);

                ray.transform.position = transform.position + randomDirection * randomDistance;
            }

            yield return null;
        }
    }

    public override void Stop()
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
            coroutine = null;
        }
    }

    private void Awake()
    {
        objectPool = ObjectPool.GetInstance();
    }
}
