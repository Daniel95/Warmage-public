using System;
using System.Collections.Generic;
using UnityEngine;

public class UiFloatingTextManager : MonoBehaviour
{
    [SerializeField] private GameObject floatingTextPrefab = null;
    [SerializeField] private List<FloatingTextPreset> floatingTextPresets = null;

    [Header("Scale by damage")]
    [SerializeField] [Range(0, 1)] private float minScaleFactor = 0.5f;
    [SerializeField] [Range(0, 100)] private int minScaleDamage = 0;
    [SerializeField] [Range(1, 3)] private float maxScaleFactor = 2f;
    [SerializeField] [Range(100, 1000)] private int maxScaleDamage = 500;

    private Camera cam;

    public void SpawnText(Vector3 worldPosition, string value, FloatingTextType floatingTextType)
    {
        GameObject floatingTextGameObject = ObjectPool.GetInstance().GetObjectForType(floatingTextPrefab.name);
        floatingTextGameObject.transform.SetParent(transform);
        floatingTextGameObject.transform.position = worldPosition;

        UiFloatingTextElement floatingTextElement = floatingTextGameObject.GetComponent<UiFloatingTextElement>();

        FloatingTextPreset floatingTextPreset = floatingTextPresets.Find(x => x.floatingTextType == floatingTextType);

        floatingTextElement.Init(value, floatingTextPreset);
    }

    public void SpawnDamageText(Vector3 worldPosition, int damage, FloatingTextType floatingTextType)
    {
        GameObject floatingTextGameObject = ObjectPool.GetInstance().GetObjectForType(floatingTextPrefab.name);
        floatingTextGameObject.transform.SetParent(transform);
        floatingTextGameObject.transform.position = worldPosition;

        UiFloatingTextElement floatingTextElement = floatingTextGameObject.GetComponent<UiFloatingTextElement>();

        FloatingTextPreset floatingTextPreset = floatingTextPresets.Find(x => x.floatingTextType == floatingTextType);

        if (floatingTextType == FloatingTextType.Damage || floatingTextType == FloatingTextType.DamageByThisPlayer)
        {
            int damageRange = maxScaleDamage - minScaleDamage;
            float damageScaleFactor = (float)Math.Abs(damage - minScaleDamage) / damageRange;

            float scaleRange = maxScaleFactor - minScaleFactor;
            float scaler = minScaleFactor + scaleRange * damageScaleFactor;

            floatingTextPreset.scale *= scaler;
        }

        floatingTextElement.Init(damage.ToString(), floatingTextPreset);
    }

    private void Awake()
    {
        cam = Camera.main;
    }
}

[Serializable]
public struct FloatingTextPreset
{
    public FloatingTextType floatingTextType;
    public Color color;
    [Range(1, 10)] public float speed;
    [Range(0.005f, 0.05f)] public float scale;
    [Range(1, 4)] public float lifeTime;
    public FontStyle fontStyle;
}

public enum FloatingTextType
{
    Experience,
    DamageByThisPlayer,
    Damage,
}