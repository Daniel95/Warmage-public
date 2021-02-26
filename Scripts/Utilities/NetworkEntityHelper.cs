using DOTSNET;
using Unity.Entities;
using UnityEngine;

public static class NetworkEntityHelper
{
    public static void Destroy(Entity entity, EntityCommandBuffer beginServerEcb, NetworkServerSystem server)
    {
        server.Unspawn(entity);
        beginServerEcb.DestroyEntity(entity);
    }
}
