using UnityEngine;

public class MouseDirection : MonoBehaviour
{
    //public static float Length { get; private set; }
    //public static Vector2 Direction { get; private set; }

    //[SerializeField] private float minDelta = 0.001041667f;
    //[SerializeField] private float maxDelta = 0.015625f;

    //private Vector2 mouseStartPosition;
    //private Vector2 screenSize;
    //private Vector2 unlockedMousePosition;

    //private void Awake()
    //{
    //    screenSize = new Vector2(Screen.width, Screen.height);
    //}

    //private void Update()
    //{
    //    if (Input.GetMouseButtonDown(1))
    //    {
    //        Cursor.lockState = CursorLockMode.Locked;
    //        Cursor.visible = false;

    //        unlockedMousePosition = mouseStartPosition = Input.mousePosition / screenSize;
    //    }

    //    if (Input.GetMouseButton(1))
    //    {
    //        unlockedMousePosition.x += Input.GetAxis("Mouse X") / Screen.width;
    //        unlockedMousePosition.y += Input.GetAxis("Mouse Y") / Screen.height;

    //        float distance = Vector2.Distance(mouseStartPosition, unlockedMousePosition);

    //        if (distance >= minDelta)
    //        {
    //            Direction = (unlockedMousePosition - mouseStartPosition).normalized;
    //            float minMaxOffset = maxDelta - minDelta;
    //            Length = distance / minMaxOffset;


    //            if (distance >= maxDelta)
    //            {
    //                unlockedMousePosition = mouseStartPosition + Direction * maxDelta;
    //            }
    //        }
    //    }
    //    else
    //    {
    //        Cursor.lockState = CursorLockMode.None;
    //        Cursor.visible = true;

    //        Direction = Vector2.zero;
    //        Length = 0;
    //    }
    //}
}
