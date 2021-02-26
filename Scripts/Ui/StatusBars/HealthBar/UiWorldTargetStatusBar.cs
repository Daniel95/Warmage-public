using UnityEngine;
using Unity.Mathematics;
using UnityEngine.UI;
using Unity.Entities;
using DOTSNET;

public class UiWorldTargetStatusBar : UiStatusBar
{
    #region Singleton
    public static UiWorldTargetStatusBar GetInstance()
    {
        if (instance == null)
        {
            instance = FindObjectOfType<UiWorldTargetStatusBar>();
        }
        return instance;
    }

    private static UiWorldTargetStatusBar instance;
    #endregion

    [SerializeField] private float yOffset = 0;

    private Camera playerCamera;
    private ShowMode mode = ShowMode.Npc;
    private bool show;

    public enum ShowMode
    {
        Npc,
        Player
    }

    public void SetMode(ShowMode mode)
    {
        this.mode = mode;
    }

    private void OnTarget(Entity entity)
    {
        EntityManager entityManager = Bootstrap.ClientWorld.EntityManager;

        if (entityManager.HasComponent<PlayerComponent>(entity))
        {
            Show(ShowMode.Player);
        } 
        else
        {
            Show(ShowMode.Npc);
        }

        HealthComponent healthComponent = entityManager.GetComponentData<HealthComponent>(entity);

        healthBar.SetHealth(healthComponent.currentHealth, healthComponent.maxHealth);
    }

    private void OnUntarget()
    {
        Hide();
    }

    private void Update()
    {
        if (PlayerLocalInfo.targetInfo.hasTarget)
        {
            PlayerLocalInfo.TargetInfo targetInfo = PlayerLocalInfo.targetInfo;

            transform.position = targetInfo.position + new float3(0, yOffset, 0);

            healthBar.SetHealth(targetInfo.currentHealth, targetInfo.maxHealth);

            Vector3 cameraPosition = playerCamera.transform.position;

            transform.LookAt(cameraPosition);
        }
        else
        {
            Hide();

            transform.position = new Vector3(0, -10, 0);
        }
    }

    private void OnEnable()
    {
        PlayerLocalInfo.targetInfo.targetEvent += OnTarget;
        PlayerLocalInfo.targetInfo.untargetEvent += OnUntarget;
    }

    private void OnDisable()
    {
        PlayerLocalInfo.targetInfo.targetEvent -= OnTarget;
        PlayerLocalInfo.targetInfo.untargetEvent -= OnUntarget;
    }

    private void Awake()
    {
        playerCamera = Camera.main;

        Hide();
    }

    private void Show(ShowMode mode)
    {
        this.mode = mode;
        show = true;

        if (mode == ShowMode.Npc)
        {
            healthBar.gameObject.SetActive(show);
            channelBar.gameObject.SetActive(show);
            statusEffect.gameObject.SetActive(show);
            actionPoints.gameObject.SetActive(false);
        }
        else
        {
            healthBar.gameObject.SetActive(show);
            channelBar.gameObject.SetActive(show);
            statusEffect.gameObject.SetActive(show);
            actionPoints.gameObject.SetActive(false);
        }
    }

    private void Hide()
    {
        healthBar.gameObject.SetActive(false);
        channelBar.gameObject.SetActive(false);
        statusEffect.gameObject.SetActive(false);
        actionPoints.gameObject.SetActive(false);
    }
}
