using System.Collections.Generic;
using Unity.Mathematics;

public class FactionManager
{
    private static Dictionary<FactionType, int> factionCount = new Dictionary<FactionType, int>() { { FactionType.Blue, 0 }, { FactionType.Red, 0 }, { FactionType.Yellow, 0 } };

    public static FactionType GetSmallestFaction() 
    {
        int smallestFactionCount = int.MaxValue;
        FactionType smallestFaction = FactionType.Blue;

        foreach (var pair in factionCount)
        {
            if(pair.Value < smallestFactionCount)
            {
                smallestFactionCount = pair.Value;
            }
        }

        return smallestFaction;
    }

    public static void RegisterAtFaction(FactionType factionType)
    {
        factionCount[factionType]++;
    }

    public static float4 GetFactionColor(FactionType factionType)
    {
        float4 factionColor = new float4(1, 1, 1, 1);

        if (factionType == FactionType.Blue)
        {
            factionColor = new float4(0, 0, 1, 1);
        }
        else if (factionType == FactionType.Red)
        {
            factionColor = new float4(1, 0, 0, 1);
        }
        else if (factionType == FactionType.Yellow)
        {
            factionColor = new float4(1, 1, 0, 1);
        }

        return factionColor;
    }
}
