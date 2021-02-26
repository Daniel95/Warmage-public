using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ItemLibrary : MonoBehaviour
{
    #region Singleton
    public static ItemLibrary GetInstance()
    {
        if (instance == null)
        {
            instance = FindObjectOfType<ItemLibrary>();
        }
        return instance;
    }

    private static ItemLibrary instance;
    #endregion

    [SerializeField] private string itemPath = "Item";

    private Dictionary<Guid, IItem> items = new Dictionary<Guid, IItem>();

    //public ILoot GetSkillTemplate(Guid id)
    //{
    //    Debug.Assert(skills.ContainsKey(id), "skillId not found: " + id.ToString());
    //    return skills[id].ShallowCopy();
    //}

    private void Start()
    {
        UnityEngine.Object[] skillbjects = Resources.LoadAll(itemPath, typeof(IItem));

        for (int i = 0; i < skillbjects.Length; i++)
        {
            IItem item = (IItem)skillbjects[i];
            items.Add(item.GetId(), item);
        }
    }
}