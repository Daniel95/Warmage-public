using System;
using System.Collections.Generic;
using UnityEngine;

public class ObjectId : MonoBehaviour {

    public SerializableGuid Id { get { return id; } }

    [SerializeField] private SerializableGuid id;

    public void GenerateId() 
    {
        id = Guid.NewGuid();
    }

    private void Reset() 
    {
        GenerateId();
    }
}
