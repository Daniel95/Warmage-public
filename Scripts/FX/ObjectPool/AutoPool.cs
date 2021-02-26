using UnityEngine;

/// <summary>
/// This class is used for object pooled items.
/// Items that are enabled will be disabled after a set amount a time to be reused again.
/// </summary>
public class AutoPool : MonoBehaviour
{
    [SerializeField] float lifeTime = 4f;

    private float timer;

    private void OnEnable()
    {
        timer = lifeTime;
    }

    private void Update()
    {
        timer -= Time.deltaTime;
        if(timer < 0)
        {
            ObjectPool.GetInstance().PoolObject(gameObject);
        }
    }
}
