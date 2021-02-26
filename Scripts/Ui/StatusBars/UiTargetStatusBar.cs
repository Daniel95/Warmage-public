using Unity.Entities;

public class UiTargetStatusBar : UiStatusBar
{
    public enum ShowMode
    {
        Npc,
        Player
    }

    private ShowMode mode = ShowMode.Npc;
    private bool show;

    public void SetMode(ShowMode mode)
    {
        this.mode = mode;
        //if (mode == Mode.Npc)
        //{

        //} 
        //else
        //{

        //}
    }

    private void OnTarget(Entity entity)
    {
        Show(true);

        healthBar.SetHealth(PlayerLocalInfo.targetInfo.currentHealth, PlayerLocalInfo.targetInfo.maxHealth);
    }

    private void OnUntarget()
    {
        Show(false);
    }

    private void Update()
    {
        if (show)
        {
            healthBar.SetHealth(PlayerLocalInfo.targetInfo.currentHealth, PlayerLocalInfo.targetInfo.maxHealth);
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
        Show(false);
    }

    private void Show(bool show)
    {
        this.show = show;

        if (mode == ShowMode.Npc)
        {
            healthBar.gameObject.SetActive(show);
            channelBar.gameObject.SetActive(show);
            statusEffect.gameObject.SetActive(show);
        } 
        else
        {

        }
    }
}
