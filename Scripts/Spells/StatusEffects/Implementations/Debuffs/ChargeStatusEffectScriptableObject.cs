using Unity.Entities;
using UnityEngine;

[CreateAssetMenu(fileName = "Charge", menuName = "ScriptableObjects/StatusEffects/Charge", order = 1)]
public class ChargeStatusEffectScriptableObject : StatusEffectScriptableObject
{
    public override void StartEffect(EntityCommandBuffer ecb) { }
    public override void UpdateEffect(EntityCommandBuffer ecb, float deltaTime) { }
    public override void EndEffect(EntityCommandBuffer ecb) { }
}
