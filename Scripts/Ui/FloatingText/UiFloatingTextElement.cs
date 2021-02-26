using UnityEngine;
using UnityEngine.UI;

public class UiFloatingTextElement : MonoBehaviour
{
    [SerializeField] private Text text;

    private float lifeTime;
    private float speed;
    private Camera cam;
    private ObjectPool objectPool;

    public void Init(string value, FloatingTextPreset floatingTextPreset)
    {
        text.text = value;
        text.color = floatingTextPreset.color;
        text.fontStyle = floatingTextPreset.fontStyle;
        text.transform.localScale = new Vector3(-floatingTextPreset.scale, floatingTextPreset.scale, floatingTextPreset.scale);

        speed = floatingTextPreset.speed;
        lifeTime = floatingTextPreset.lifeTime;
    }

    private void Awake()
    {
        cam = Camera.main;
        objectPool = ObjectPool.GetInstance();
    }

    private void Update()
    {
        transform.position += Vector3.up * speed * Time.deltaTime;
        transform.LookAt(cam.transform.position);

        lifeTime -= Time.deltaTime;

        if (lifeTime < 0)
        {
            objectPool.PoolObject(gameObject);
        }
    }
}