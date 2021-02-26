using DOTSNET;
using Unity.Mathematics;

public struct DamageMessage : NetworkMessage
{
    public ulong netId;

    public short damage;
    public ulong damagerNetId;
    public float3 position;

    public ushort GetID() { return MessageIds.damage; }

    public DamageMessage(ulong netId, short damage, ulong damagerNetId, float3 position)
    {
        this.netId = netId;
        this.damage = damage;
        this.damagerNetId = damagerNetId;
        this.position = position;
    }
}
