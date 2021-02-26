using DOTSNET;
using Unity.Entities;

public struct SpellEcsData
{
    public PrefabSystem prefabSystem;
    public NetworkServerSystem server;
    public EntityManager entityManager;
}
