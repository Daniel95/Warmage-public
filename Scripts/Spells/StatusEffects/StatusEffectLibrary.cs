using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class StatusEffectLibrary  : MonoBehaviour
{
    #region Singleton
    public static StatusEffectLibrary GetInstance()
    {
        if (instance == null)
        {
            instance = FindObjectOfType<StatusEffectLibrary>();
        }
        return instance;
    }

    private static StatusEffectLibrary instance;
    #endregion

    [SerializeField] private string statusEffectsPath = "StatusEffects";

    private Dictionary<Guid, IStatusEffect> statusEffects = new Dictionary<Guid, IStatusEffect>();

    private Dictionary<Entity, Dictionary<Guid, IStatusEffect>> statusEffectInstances = new Dictionary<Entity, Dictionary<Guid, IStatusEffect>>();

    public IStatusEffect Instantiate(Entity owner, Guid id)
    {
        Debug.Assert(!(statusEffectInstances.ContainsKey(owner) && statusEffectInstances[owner].ContainsKey(id)), "statusId instance on this entity already exists: " + id.ToString());

        IStatusEffect statusEffectInstance = statusEffects[id].ShallowCopy();

        if (!statusEffectInstances.ContainsKey(owner))
        {
            Dictionary<Guid, IStatusEffect> dictionary = new Dictionary<Guid, IStatusEffect>()
            {
                { id, statusEffectInstance }
            };

            statusEffectInstances.Add(owner, dictionary);
        } 
        else
        {
            statusEffectInstances[owner].Add(id, statusEffectInstance);
        }

        return statusEffectInstance;
    }

    public void DestroyInstance(Entity owner, Guid id)
    {
        Debug.Assert(statusEffectInstances.ContainsKey(owner), "no statuses found on this entity: " + id.ToString());
        Debug.Assert(statusEffectInstances[owner].ContainsKey(id), "statusId instance on this entity not found: " + id.ToString());

        statusEffectInstances[owner].Remove(id);

        if(statusEffectInstances[owner].Count == 0)
        {
            statusEffectInstances.Remove(owner);
        }
    }

    public bool InstanceExists(Entity owner, Guid id)
    {
        return statusEffectInstances.ContainsKey(owner) && statusEffectInstances[owner].ContainsKey(id);
    }

    public IStatusEffect GetInstance(Entity owner, Guid id)
    {
        Debug.Assert(statusEffectInstances.ContainsKey(owner), "no statuses found on this entity: " + id.ToString());
        Debug.Assert(statusEffectInstances[owner].ContainsKey(id), "statusId instance on this entity not found: " + id.ToString());
        return statusEffectInstances[owner][id];
    }

    public IStatusEffect GetStatusEffectTemplate(Guid id) 
    {
        return statusEffects[id];
    }

    private void Start()
    {
        UnityEngine.Object[] statusEffectObject = Resources.LoadAll(statusEffectsPath, typeof(IStatusEffect));

        for (int i = 0; i < statusEffectObject.Length; i++)
        {
            IStatusEffect statusEffect = (IStatusEffect)statusEffectObject[i];
            statusEffects.Add(statusEffect.GetId(), statusEffect);
        }
    }
}

