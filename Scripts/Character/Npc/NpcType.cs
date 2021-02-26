//Note: keep size lower then 11 otherwise npcTypesStorage in KeepSpawnPointComponent will break since it relies of bitshifting.
public enum NpcType
{
    Tier1,
    Tier2,
    Tier3,
    Tier4,
    CommanderTier1,
    CommanderTier2,
}
