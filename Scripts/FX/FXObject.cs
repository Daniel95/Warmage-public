using System;
using UnityEngine;

[RequireComponent(typeof(ObjectId))]
public class FXObject : MonoBehaviour
{
    public Guid id => objectId.Id;
    public bool hasScript => optionalFXScript != null;
    public FXScriptBase script => optionalFXScript;

    [SerializeField] private FXScriptBase optionalFXScript = null;

    private bool autoPool = false;
    private float timer = 0;
    private ObjectId objectId;

    public void Play()
    {
        gameObject.SetActive(true);

        if(hasScript)
        {
            script.Play();
        }
    }

    public void Play(float autoPoolTimer)
    {
        gameObject.SetActive(true);
        autoPool = true;
        timer = autoPoolTimer;

        if (hasScript)
        {
            script.Play();
        }
    }

    public void Stop()
    {
        autoPool = false;

        if (hasScript)
        {
            script.Stop();
        }

        gameObject.SetActive(false);
    }

    public int GetPoolIndex()
    {
        return FXLibrary.GetInstance().fxPool.GetIndex(GetComponent<ObjectId>().Id);
    }

    private void Update()
    {
        if(!autoPool) { return; }

        timer -= Time.deltaTime;

        if(timer < 0)
        {
            if (hasScript)
            {
                script.Stop();
            }

            autoPool = false;
            gameObject.SetActive(false);
        }
    }

    private void Awake()
    {
        objectId = GetComponent<ObjectId>();
    }
}
