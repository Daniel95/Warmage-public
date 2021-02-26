using System;
using System.Collections.Generic;

[Serializable]
public struct PlayerProfile
{
    public int globalXp;
    public List<SerializableGuid> skills;
    public SerializableGuid[] skillBar;
}
