using Unity.Mathematics;
using UnityEngine;

public class PlayerTargetHightlight : MonoBehaviour
{
    [SerializeField] private float yOffset = 0;

    private void Update()
    {
        if(PlayerLocalInfo.targetInfo.hasTarget)
        {
            float3 targetPosition = PlayerLocalInfo.targetInfo.position;

            transform.position = new Vector3(targetPosition.x, targetPosition.y + yOffset, targetPosition.z);
        } 
        else
        {
            transform.position = new Vector3(0, -10, 0);
        }
    }
}
