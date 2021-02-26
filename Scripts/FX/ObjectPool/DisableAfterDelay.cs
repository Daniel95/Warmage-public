using UnityEngine;

/// <summary>
/// This class is used for object pooled items.
/// Items that are enabled will immediately be disabled after a set amount a time to be reused again.
/// </summary>
public class DisableAfterDelay : MonoBehaviour
{
    [SerializeField] float lifeTime = 4f;

    private void OnEnable()
    {
        Invoke("DisableSelf", lifeTime);
    }

    private void DisableSelf()
    {
        gameObject.SetActive(false);
    }
}
