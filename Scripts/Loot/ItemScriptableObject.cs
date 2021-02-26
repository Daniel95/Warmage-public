using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "ScriptableObjects/Item/Item", order = 1)]
public class ItemScriptableObject : ScriptableObject, IItem
{
    public Guid GetId()
    {
        throw new NotImplementedException();
    }

    public Guid GetStatusEffectId()
    {
        throw new NotImplementedException();
    }
}