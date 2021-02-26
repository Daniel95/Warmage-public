using UnityEngine;

public class UiStatusBar : MonoBehaviour
{
    public UiHealthBar healthBar => healthbarUi;
    public UiChannelBar channelBar => channelBarUi;
    public UiStatusEffect statusEffect => statusEffectUi;
    public UiActionPoints actionPoints => actionPointsUi;

    [SerializeField] private UiHealthBar healthbarUi = null;
    [SerializeField] private UiChannelBar channelBarUi = null;
    [SerializeField] private UiStatusEffect statusEffectUi;
    [SerializeField] private UiActionPoints actionPointsUi;
}
