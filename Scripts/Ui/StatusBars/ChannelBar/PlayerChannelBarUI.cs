public class PlayerChannelBarUI : UiChannelBar
{
    public static bool isChanneling { get; private set; }

    protected override void Update()
    {
        base.Update();

        isChanneling = channeling;
    }
}
