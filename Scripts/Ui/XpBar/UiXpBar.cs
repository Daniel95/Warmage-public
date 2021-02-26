using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class UiXpBar : MonoBehaviour
{
    [SerializeField] private Text text = null;

    private Slider slider;
    private LevelXpRequirementLibrary levelXpRequirementLibrary;

    public void UpdateXp()
    {
        int globalXp = PlayerProfileManager.GetInstance().playerProfile.globalXp;
        int levelIndex = levelXpRequirementLibrary.GetLevelIndex(globalXp);

        int level = levelIndex + 1;

        if (levelIndex == levelXpRequirementLibrary.GetMaxLevelIndex()) 
        {
            text.text = "Level " + level;
            slider.value = slider.maxValue;

            return;
        }

        int nextLevelIndex = levelIndex + 1;

        int localLevelRequirement = levelXpRequirementLibrary.GetLocalXpRequirement(nextLevelIndex);

        int localXp = 0;

        if(levelIndex != -1)
        {
            localXp = globalXp - levelXpRequirementLibrary.GetGlobalXpRequirement(levelIndex);
        }

        slider.maxValue = localLevelRequirement;
        slider.value = localXp;
        text.text = "Level: " + level + ", XP: " + localXp + " / " + localLevelRequirement;
    }

    private void Start()
    {
        levelXpRequirementLibrary = LevelXpRequirementLibrary.GetInstance();
        slider = GetComponent<Slider>();

        UpdateXp();
    }
}
