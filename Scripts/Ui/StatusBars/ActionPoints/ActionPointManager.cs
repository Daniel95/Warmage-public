using System.Collections.Generic;
using UnityEngine;

public class ActionPointManager : MonoBehaviour
{
    public int availableCount { get; private set; }
    public int totalCount => totalActionPointCount;
    public float cooldown => actionPointCooldown;
    public List<ActionPoint> actionPoints { get; private set; }

    #region Singleton
    public static ActionPointManager GetInstance()
    {
        if (instance == null)
        {
            instance = FindObjectOfType<ActionPointManager>();
        }
        return instance;
    }

    private static ActionPointManager instance;
    #endregion

    public class ActionPoint
    {
        public bool isOffCooldown => remainingCooldown <= 0;

        public float remainingCooldown;

        public void ResetTimer(float cooldown) => remainingCooldown = cooldown;
    }

    [SerializeField] private int totalActionPointCount = 6;
    [SerializeField] private float actionPointCooldown = 3;

    public float GetCooldown() => actionPointCooldown;

    public void Spend(int amount)
    {
        Debug.Assert(amount <= totalActionPointCount, "cannot spend that many points!");
        Debug.Assert(amount <= availableCount, "cannot spend that many points!");

        availableCount -= amount;

        int spendCount = 0;

        for (int i = 0; i < totalActionPointCount; i++)
        {
            if (actionPoints[i].isOffCooldown)
            {
                if (spendCount >= amount)
                {
                    break;
                }

                actionPoints[i].ResetTimer(actionPointCooldown);
                spendCount++;
            }
        }

        Debug.Assert(spendCount == amount, "Couldn't spend all points! spend: " + spendCount);
    }

    public float GetRemainingCooldown(int cost)
    {
        //Init shortestCooldownTimes array
        float[] shortestCooldownTimes = new float[cost];

        for (int i = 0; i < shortestCooldownTimes.Length; i++)
        {
            shortestCooldownTimes[i] = actionPointCooldown;
        }

        //Try and replace the highest cooldown in shortestCooldownTimes, using the cooldowns from actionPointIcons
        {
            for (int i = 0; i < totalActionPointCount; i++)
            {
                float highestCooldownInShortestCooldowns = int.MinValue;
                int highestCooldownIndex = 0;

                for (int j = 0; j < shortestCooldownTimes.Length; j++)
                {
                    if (shortestCooldownTimes[j] > highestCooldownInShortestCooldowns)
                    {
                        highestCooldownInShortestCooldowns = shortestCooldownTimes[j];
                        highestCooldownIndex = j;
                    }
                }

                if (actionPoints[i].remainingCooldown < highestCooldownInShortestCooldowns)
                {
                    shortestCooldownTimes[highestCooldownIndex] = actionPoints[i].remainingCooldown;
                }
            }
        }

        //Return the highest cooldown in the shortestCooldownTimes array.
        {
            float highestCooldown = int.MinValue;

            for (int j = 0; j < shortestCooldownTimes.Length; j++)
            {
                if (shortestCooldownTimes[j] > highestCooldown)
                {
                    highestCooldown = shortestCooldownTimes[j];
                }
            }

            return highestCooldown;
        }
    }

    private void Awake()
    {
        actionPoints = new List<ActionPoint>(totalActionPointCount);

        for (int i = 0; i < totalActionPointCount; i++)
        {
            actionPoints.Add(new ActionPoint { });
        }
    }

    private void Update()
    {
        int actionPointsOffCooldownCount = 0;

        for (int i = 0; i < totalActionPointCount; i++)
        {
            ActionPoint actionPoint = actionPoints[i];

            if (actionPoints[i].isOffCooldown)
            {
                actionPointsOffCooldownCount++;
            } 
            else
            {
                actionPoint.remainingCooldown -= Time.deltaTime;
            }
        }

        availableCount = actionPointsOffCooldownCount;
    }
}
