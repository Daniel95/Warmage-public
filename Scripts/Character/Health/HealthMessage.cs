using DOTSNET;

public struct HealthMessage : NetworkMessage
{
    public ulong netId;

    public int currentHealth;
    public int maxHealth;

    public ushort GetID() { return MessageIds.health; }

    public HealthMessage(ulong netId, int currentHealth, int maxHealth)
    {
        this.netId = netId;
        this.currentHealth = currentHealth;
        this.maxHealth = maxHealth;
    }
}
