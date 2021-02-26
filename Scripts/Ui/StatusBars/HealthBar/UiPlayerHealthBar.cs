public class UiPlayerHealthBar : UiHealthBar
{
    private void Update()
    {
        SetHealth(PlayerLocalInfo.currentHealth, PlayerLocalInfo.maxHealth);
    }
}
