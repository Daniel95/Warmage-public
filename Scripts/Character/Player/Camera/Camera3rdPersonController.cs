using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters;
using System;
using UnityEngine;
using Unity.Physics.Authoring;
using Unity.Physics;
using DOTSNET;
using Unity.Physics.Systems;
using Unity.Mathematics;
using Unity.Entities;

public class Camera3rdPersonController : MonoBehaviour
{
    public static float xDelta { get; private set; }
    public static float yDelta { get; private set; }

    //camera holder
    [Header("Camera Distance")]
    [SerializeField] private float cameraDistance = 6.6f;
    [SerializeField] private float cameraDistanceScrollSpeed = 2.0f;
    [SerializeField] private float minCameraDistance = 3.0f;
    [SerializeField] private float maxCameraDistance = 15.0f;
    [Header("Camera Y")]
    [SerializeField] private float cameraYOffset = 0.8f;
    [SerializeField] private float yRotate = 120.0f;
    [SerializeField] private float yMinLimit = -35f;
    [SerializeField] private float yMaxLimit = 80f;

    //For camera colliding
    [SerializeField] private PhysicsCategoryTags physicsCategoryTags = PhysicsCategoryTags.Everything;

    private float distanceHit;
    private float y = 0.0f;

    void Start()
    {
        var angles = transform.eulerAngles;
        y = angles.x;
    }

    private void LateUpdate()
    {
        if(ControlModeManager.mode != ControlMode.CharacterControl) { return; }

        float3 playerPosition = PlayerLocalInfo.position;

        float3 targetPos = playerPosition + new float3(0, (distanceHit - 2) / 3f + cameraYOffset, 0);

        cameraDistance -= Input.GetAxis("Mouse ScrollWheel") * cameraDistanceScrollSpeed;

        cameraDistance = Mathf.Clamp(cameraDistance, minCameraDistance, maxCameraDistance);

        var pos = Input.mousePosition;
        float dpiScale = 1;
        if (Screen.dpi < 1) dpiScale = 1;
        if (Screen.dpi < 200) dpiScale = 1;
        else dpiScale = Screen.dpi / 200f;
        if (pos.x < 380 * dpiScale && Screen.height - pos.y < 250 * dpiScale) return;

        yDelta = (float)(Input.GetAxis("Mouse Y") * yRotate);

        y -= yDelta;
        y = ClampAngle(y, yMinLimit, yMaxLimit);

        Quaternion rotation = PlayerMovementClientSystem.rotation * Quaternion.AngleAxis(y, Vector3.right);

        float3 position = (float3)(rotation * new Vector3(0, 0, -cameraDistance)) + targetPos;

        PhysicsWorld physicsWorld = Bootstrap.ClientWorld.GetExistingSystem<BuildPhysicsWorld>().PhysicsWorld;

        CollisionFilter collisionFilter = new CollisionFilter()
        {
            BelongsTo = unchecked((uint)~0),
            CollidesWith = physicsCategoryTags.Value
        };

        RaycastInput cameraToPlayerInput = new RaycastInput
        {
            Start = targetPos,
            End = position,
            Filter = collisionFilter
        };

        if (physicsWorld.CastRay(cameraToPlayerInput, out Unity.Physics.RaycastHit hit))
        {
            transform.position = hit.Position;

            //Min(4) distance from ground for camera target point
            distanceHit = Mathf.Clamp(Vector3.Distance(targetPos, hit.Position), 4, 600);
        }
        else
        {
            transform.position = position;
            distanceHit = cameraDistance;
        }
        transform.rotation = rotation;

        /*
        if (prevDistance != currDistance)
        {
            prevDistance = currDistance;
            var rot = Quaternion.Euler(y, x, 0);
            // (currDistance - 2) / 3.5f - constant for far camera position
            var po = (float3)(rot * new Vector3(0, 0, -currDistance)) + targetPos;
            transform.rotation = rot;
            transform.position = po;
        }
         */
    }

    static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360)
        {
            angle += 360;
        }
        if (angle > 360)
        {
            angle -= 360;
        }
        return Mathf.Clamp(angle, min, max);
    }
}