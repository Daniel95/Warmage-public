using DOTSNET;
using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerMovementClientSystemAuthoring : MonoBehaviour, SelectiveSystemAuthoring
{
    public Type GetSystemType() => typeof(PlayerMovementClientSystem);
}

[ClientWorld]
[UpdateInGroup(typeof(ClientConnectedSimulationSystemGroup))]
[DisableAutoCreation]
public class PlayerMovementClientSystem : SystemBase
{
    public static quaternion rotation;

    private EntityQuery m_CharacterControllerInputQuery;

    protected override void OnCreate()
    {
        base.OnCreate();

        m_CharacterControllerInputQuery = GetEntityQuery(typeof(CharacterControllerInput));

        if (m_CharacterControllerInputQuery.CalculateEntityCount() == 0)
        {
            EntityManager.CreateEntity(typeof(CharacterControllerInput));
        }
    }

    protected override void OnUpdate()
    {
        if (ControlModeManager.mode != ControlMode.CharacterControl) 
        {
            m_CharacterControllerInputQuery.SetSingleton(new CharacterControllerInput
            {
                Looking = Vector2.zero,
                Movement = Vector2.zero
            });

            return; 
        }

        float inputX = Input.GetAxis("Horizontal");
        float inputZ = Input.GetAxis("Vertical");

        float2 movement = new float2(inputX, inputZ);

        if (inputZ < -0.5f)
        {
            movement *= 0.5f;
        }

        StatsComponent statsComponent = EntityManager.GetComponentData<StatsComponent>(PlayerLocalInfo.entity);
        CharacterControllerComponentData characterControllerComponentData = EntityManager.GetComponentData<CharacterControllerComponentData>(PlayerLocalInfo.entity);

        characterControllerComponentData.MovementSpeed = characterControllerComponentData.MaxMovementSpeed = statsComponent.Speed;

        EntityManager.SetComponentData(PlayerLocalInfo.entity, characterControllerComponentData);

        int jumped = 0;

        if(Input.GetKeyDown(KeyCode.Space))
        {
            jumped = 1;
        }

        float2 looking = new float2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

        if (!PlayerLocalInfo.isAlive)
        {
            movement = Vector2.zero;
            jumped = 0;
        }

        m_CharacterControllerInputQuery.SetSingleton(new CharacterControllerInput
        {
            Looking = looking,
            Movement = movement,
            Jumped = jumped
        });

        rotation = EntityManager.GetComponentData<Rotation>(PlayerLocalInfo.entity).Value;
    }
}