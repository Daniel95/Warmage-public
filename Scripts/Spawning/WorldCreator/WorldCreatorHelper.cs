using UnityEngine;

public class WorldCreatorHelper : MonoBehaviour
{
    [SerializeField] private float length = 602;
    [SerializeField] private Vector3 offset = new Vector3(0, 50, 0);

    [Header("Corner Keeps Size Settings)")]
    [SerializeField] [Range(0, 100)] private float spawnSize = 30;
    [SerializeField] [Range(0, 100)] private float firstLineSize = 20;
    [SerializeField] [Range(0, 100)] private float secondLineSize = 10;

    [Header("Corner Keeps Position Settings)")]
    [SerializeField] [Range(0, 1)] private float spawnCenterOffsetFactor = 0.1f;
    [SerializeField] [Range(0, 1)] private float firstLineCenterOffsetFactor = 0.35f;
    [SerializeField] [Range(0, 1)] private float secondLineCenterOffsetFactor = 0.65f;
    [SerializeField] [Range(0, 100)] private float secondLineSeperationOffset = 45;

    [Header("Center Keep Settings)")]
    [SerializeField] [Range(0, 100)] private float centerSize = 75;
    [SerializeField] [Range(0, 100)] private float outerCentersSize = 40;
    [SerializeField] [Range(0, 1)] private float outerCentersCenterOffsetFactor = 0.1f;

    private bool show;

    public void Hide()
    {
        show = false;
    }

    public void Show()
    {
        show = true;
    }

    private void OnDrawGizmos()
    {
        if(!show) { return; }

        GetEquilateralTriangle(out Vector3 bottomLeftCorner, out Vector3 bottomRightCorner, out Vector3 topCorner, length, offset);

        Vector3 bottomCenter = (bottomLeftCorner + bottomRightCorner) * 0.5f;
        Vector3 rightCenter = (bottomRightCorner + topCorner) * 0.5f;
        Vector3 leftCenter = (topCorner + bottomLeftCorner) * 0.5f;

        Vector3 center = (bottomLeftCorner + bottomRightCorner + topCorner) / 3;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(bottomLeftCorner, bottomRightCorner);
        Gizmos.DrawLine(bottomRightCorner, topCorner);
        Gizmos.DrawLine(topCorner, bottomLeftCorner);

        Gizmos.DrawWireSphere(center, centerSize);

        //Outer Center Keeps
        DrawOuterCenterWireSpheres(bottomCenter, center);
        DrawOuterCenterWireSpheres(rightCenter, center);
        DrawOuterCenterWireSpheres(leftCenter, center);

        //Corner Keeps
        DrawCornerWireSpheres(bottomLeftCorner, center);
        DrawCornerWireSpheres(bottomRightCorner, center);
        DrawCornerWireSpheres(topCorner, center);
    }

    private void DrawOuterCenterWireSpheres(Vector3 outerCenter, Vector3 center)
    {
        Vector3 offset = center - outerCenter;
        Vector3 pos = outerCenter + offset * outerCentersCenterOffsetFactor;

        Gizmos.DrawWireSphere(pos, outerCentersSize);
    }

    private void DrawCornerWireSpheres(Vector3 corner, Vector3 center)
    {
        Vector3 offset = center - corner;
        Vector3 spawnPosition = corner + offset * spawnCenterOffsetFactor;
        Gizmos.DrawWireSphere(spawnPosition, spawnSize);

        Vector3 firstLine = corner + offset * firstLineCenterOffsetFactor;
        Gizmos.DrawWireSphere(firstLine, firstLineSize);

        Vector3 secondLine = corner + offset * secondLineCenterOffsetFactor;
        Vector3 dirToCenter = offset.normalized;
        Vector3 right = Vector3.Cross(dirToCenter, Vector3.up);
        Gizmos.DrawWireSphere(secondLine + right * secondLineSeperationOffset, secondLineSize);
        Gizmos.DrawWireSphere(secondLine - right * secondLineSeperationOffset, 10);
    }

    private void GetEquilateralTriangle(out Vector3 point1, out Vector3 point2, out Vector3 point3, float length, Vector3 offset)
    {
        point1 = offset + Vector3.zero;
        point2 = point1 + new Vector3(length, 0, 0);
        Vector3 direction = Quaternion.AngleAxis(60, Vector3.up) * new Vector3(-1, 0, 0);
        point3 = point2 + direction * length;
    }
}
