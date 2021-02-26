using UnityEngine;

public class LevelXpRequirementLibrary : MonoBehaviour
{
    #region Singleton
    public static LevelXpRequirementLibrary GetInstance()
    {
        if (instance == null)
        {
            instance = FindObjectOfType<LevelXpRequirementLibrary>();
        }
        return instance;
    }

    private static LevelXpRequirementLibrary instance;
    #endregion

    [SerializeField] private int[] levelLocalXpRequirements = new int[15];

    public int GetMaxLevel() => levelLocalXpRequirements.Length;
    public int GetMaxLevelIndex() => levelLocalXpRequirements.Length - 1;

    public int GetLevelIndex(int globalXp)
    {
        int levelIndex = -1;

        for (int i = 0; i < levelLocalXpRequirements.Length; i++)
        {
            globalXp -= levelLocalXpRequirements[i];
            if(globalXp >= 0)
            {
                levelIndex = i;
            } 
            else
            {
                break;
            }
        }

        return levelIndex;
    }

    public int GetLocalXpRequirement(int levelIndex)
    {
        Debug.Assert(levelIndex < levelLocalXpRequirements.Length, "Level index " + levelIndex + " does not exist!");

        return levelLocalXpRequirements[levelIndex];
    }

    public int GetGlobalXpRequirement(int levelIndex)
    {
        Debug.Assert(levelIndex < levelLocalXpRequirements.Length, "Level index " + levelIndex + " does not exist!");

        int totalXpRequirement = 0;

        for (int i = 0; i < levelIndex + 1; i++)
        {
            totalXpRequirement += levelLocalXpRequirements[i];
        }

        return totalXpRequirement;
    }
}
