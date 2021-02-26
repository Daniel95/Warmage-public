using System;
using System.Collections.Generic;
using UnityEngine;

public class UiStatusEffect : MonoBehaviour
{
    [SerializeField] private StatusEffectIconUI statusEffectIconUIPrefab = null;

    private Dictionary<ulong, Dictionary<Guid, StatusEffectIconUI>> statusEffectIconUIs = new Dictionary<ulong, Dictionary<Guid, StatusEffectIconUI>>();

    public void AddStatusEffect(ulong casterNetId, Guid statusEffectId, float timeLeft, int count)
    {
        //Debug.Assert(!(statusEffectIconUIs.ContainsKey(casterNetId) && statusEffectIconUIs[casterNetId].ContainsKey(statusEffectId)), "statusId instance on this entity already exists: " + statusEffectId.ToString());
        if (Exists(casterNetId, statusEffectId)) { return; }

        GameObject statusEffectIconGameObject = Instantiate(statusEffectIconUIPrefab.gameObject, transform);
        StatusEffectIconUI statusEffectIconUI = statusEffectIconGameObject.GetComponent<StatusEffectIconUI>();

        statusEffectIconUI.Init(this, statusEffectId, casterNetId, timeLeft, count);

        //Register in statusEffectIconUIs
        {
            if (!statusEffectIconUIs.ContainsKey(casterNetId))
            {
                Dictionary<Guid, StatusEffectIconUI> dictionary = new Dictionary<Guid, StatusEffectIconUI>()
                {
                    { statusEffectId, statusEffectIconUI }
                };

                statusEffectIconUIs.Add(casterNetId, dictionary);
            }
            else
            {
                statusEffectIconUIs[casterNetId].Add(statusEffectId, statusEffectIconUI);
            }
        }
    }

    public void UpdateStatusEffect(ulong casterNetId, Guid statusEffectId, float timeLeft, int count)
    {
        if (!Exists(casterNetId, statusEffectId)) { return; }

        StatusEffectIconUI statusEffectIconUI = statusEffectIconUIs[casterNetId][statusEffectId];
        statusEffectIconUI.UpdateTimeLeft(timeLeft);
        statusEffectIconUI.UpdateCount(count);
    }

    public void RemoveStatusEffect(ulong casterNetId, Guid statusEffectId)
    {
        //Debug.Assert(statusEffectIconUIs.ContainsKey(casterNetId), "no statuses found on this entity: " + statusEffectId.ToString());
        //Debug.Assert(statusEffectIconUIs[casterNetId].ContainsKey(statusEffectId), "statusId instance on this entity not found: " + statusEffectId.ToString());

        if (!Exists(casterNetId, statusEffectId)) { return; }

        Destroy(statusEffectIconUIs[casterNetId][statusEffectId].gameObject);

        //Unregister in statusEffectIconUIs
        {
            statusEffectIconUIs[casterNetId].Remove(statusEffectId);

            if (statusEffectIconUIs[casterNetId].Count == 0)
            {
                statusEffectIconUIs.Remove(casterNetId);
            }
        }
    }

    public bool Exists(ulong casterNetId, Guid statusEffectId)
    {
        return statusEffectIconUIs.ContainsKey(casterNetId) && statusEffectIconUIs[casterNetId].ContainsKey(statusEffectId);
    }

    public void Clear()
    {
        List<ulong> netIdsToRemove = new List<ulong>();
        List<Guid> statusEffectIdsToRemove = new List<Guid>();

        foreach (var statusEffects in statusEffectIconUIs)
        {
            foreach (var statusEffect in statusEffects.Value)
            {
                statusEffectIdsToRemove.Add(statusEffect.Key);
                netIdsToRemove.Add(statusEffects.Key);
            }
        }

        for (int i = 0; i < netIdsToRemove.Count; i++)
        {
            RemoveStatusEffect(netIdsToRemove[i], statusEffectIdsToRemove[i]);
        }
    }
}

/*
public class UiStatusEffect : MonoBehaviour
{
    [SerializeField] private StatusEffectIconUI statusEffectIconUIPrefab = null;

    private Dictionary<ulong, Dictionary<Guid, StatusEffectIconUI>> statusEffectIconUIs = new Dictionary<ulong, Dictionary<Guid, StatusEffectIconUI>>();

    public void AddStatusEffect(ulong casterNetId, Guid statusEffectId, float timeLeft)
    {
        //Debug.Assert(!(statusEffectIconUIs.ContainsKey(casterNetId) && statusEffectIconUIs[casterNetId].ContainsKey(statusEffectId)), "statusId instance on this entity already exists: " + statusEffectId.ToString());
        if(Exists(casterNetId, statusEffectId)) { return; }

        GameObject statusEffectIconGameObject = Instantiate(statusEffectIconUIPrefab.gameObject, transform);
        StatusEffectIconUI statusEffectIconUI = statusEffectIconGameObject.GetComponent<StatusEffectIconUI>();

        statusEffectIconUI.Init(this, statusEffectId, casterNetId, timeLeft);

        //Register in statusEffectIconUIs
        {
            if (!statusEffectIconUIs.ContainsKey(casterNetId))
            {
                Dictionary<Guid, StatusEffectIconUI> dictionary = new Dictionary<Guid, StatusEffectIconUI>()
                {
                    { statusEffectId, statusEffectIconUI }
                };

                statusEffectIconUIs.Add(casterNetId, dictionary);
            }
            else
            {
                statusEffectIconUIs[casterNetId].Add(statusEffectId, statusEffectIconUI);
            }
        }
    }

    public void RemoveStatusEffect(ulong casterNetId, Guid statusEffectId)
    {
        //Debug.Assert(statusEffectIconUIs.ContainsKey(casterNetId), "no statuses found on this entity: " + statusEffectId.ToString());
        //Debug.Assert(statusEffectIconUIs[casterNetId].ContainsKey(statusEffectId), "statusId instance on this entity not found: " + statusEffectId.ToString());

        if (!Exists(casterNetId, statusEffectId)) { return; }

        Destroy(statusEffectIconUIs[casterNetId][statusEffectId].gameObject);

        //Unregister in statusEffectIconUIs
        {
            statusEffectIconUIs[casterNetId].Remove(statusEffectId);

            if (statusEffectIconUIs[casterNetId].Count == 0)
            {
                statusEffectIconUIs.Remove(casterNetId);
            }
        }
    }

    public bool Exists(ulong casterNetId, Guid statusEffectId)
    {
        return statusEffectIconUIs.ContainsKey(casterNetId) && statusEffectIconUIs[casterNetId].ContainsKey(statusEffectId);
    }

    public void Clear()
    {
        List<ulong> netIdsToRemove = new List<ulong>();
        List<Guid> statusEffectIdsToRemove = new List<Guid>();

        foreach (var statusEffects in statusEffectIconUIs)
        {
            foreach (var statusEffect in statusEffects.Value)
            {
                statusEffectIdsToRemove.Add(statusEffect.Key);
                netIdsToRemove.Add(statusEffects.Key);
            }
        }

        for (int i = 0; i < netIdsToRemove.Count; i++)
        {
            RemoveStatusEffect(netIdsToRemove[i], statusEffectIdsToRemove[i]);
        }
    }
}
 */