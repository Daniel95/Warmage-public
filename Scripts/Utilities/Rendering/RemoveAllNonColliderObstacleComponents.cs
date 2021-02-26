using UnityEngine;
using UnityEngine.AI;

public class RemoveAllNonColliderObstacleComponents : MonoBehaviour
{
    void Start()
    {
        CoroutineHelper.DelayFrames(3, () => 
        {
            foreach (var comp in gameObject.GetComponents<Component>())
            {
                if (!(comp is Transform) && !(comp is Collider) && !(comp is NavMeshObstacle))
                {
                    Destroy(comp);
                }
            }
        });
    }
}
