using UnityEngine;

public class SnapToSurface : MonoBehaviour
{
    [Header("Snap To Terrain Settings")]
    public LayerMask layerMask = 0;
    [SerializeField] private Vector3 snapToTerrainOffset = new Vector3(0, 1, 0);

    public void Snap()
    {
        if (Physics.Raycast(transform.position + Vector3.up * 1000, -Vector3.up, out RaycastHit raycastHit, 2000, layerMask))
        {
            transform.position = raycastHit.point + snapToTerrainOffset;
        }
        else
        {
            Debug.Log("Point: " + name + " didn't find a suitable surface to snap to!", gameObject);
        }
    }
}
