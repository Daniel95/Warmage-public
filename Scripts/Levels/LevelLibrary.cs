using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class LevelDictionaryElement
{
    public LevelType levelType;
    public List<Scenes> scenes;
}

public class LevelLibrary : MonoBehaviour
{
    [SerializeField] private List<LevelDictionaryElement> levels = null;

    public List<Scenes> GetLevel(LevelType levelType)
    {
        LevelDictionaryElement levelDictionaryElement = levels.Find(x => x.levelType == levelType);

        Debug.Assert(levelDictionaryElement != null, "LevelType does not exists in LevelLibrary!");

        return levelDictionaryElement.scenes;
    }

    public List<LevelType> GetAllLevelTypes() 
    {
        List<LevelType> levelTypes = levels.Select(x => x.levelType).ToList();
        return levelTypes;
    }
}
