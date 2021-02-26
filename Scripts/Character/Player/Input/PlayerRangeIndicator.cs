using Unity.Mathematics;
using UnityEngine;

public class PlayerRangeIndicator : MonoBehaviour
{
    #region Singleton
    public static PlayerRangeIndicator GetInstance()
    {
        if (instance == null)
        {
            instance = FindObjectOfType<PlayerRangeIndicator>();
        }
        return instance;
    }

    private static PlayerRangeIndicator instance;
    #endregion

    [SerializeField] private float activeTime = 1;
    [SerializeField] private float activeAlpha = 0.5f;
    [SerializeField] private float yOffset = -0.5f;

    private float timer;
    private Renderer rangeIndicatorRenderer;
    private MaterialPropertyBlock materialPropertyBlock;
    private Color defaultColor;

    public void Activate(float range)
    {
        rangeIndicatorRenderer.enabled = true;

        transform.localScale = new Vector3(range * 2, transform.position.y, range * 2);

        timer = activeTime;
    }

    private void Deactivate()
    {
        rangeIndicatorRenderer.enabled = false;
        timer = 0;
    }

    private void Update()
    {
        if(timer <= 0) 
        {
            rangeIndicatorRenderer.enabled = false;
            return; 
        }

        transform.position = PlayerLocalInfo.position + new float3(0, yOffset, 0);

        //Lower alpha over time
        {
            float progress = timer / activeTime;
            float alpha = Mathf.Lerp(0, activeAlpha, progress);

            Color color = defaultColor;
            color.a = alpha;

            materialPropertyBlock.SetColor("_BaseColor", color);

            rangeIndicatorRenderer.SetPropertyBlock(materialPropertyBlock);
        }

        timer -= Time.deltaTime;
    }

    private void Awake()
    {
        materialPropertyBlock = new MaterialPropertyBlock();
        rangeIndicatorRenderer = GetComponent<Renderer>();
        defaultColor = rangeIndicatorRenderer.material.GetColor("_BaseColor");
    }
}
