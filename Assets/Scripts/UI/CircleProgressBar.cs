using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class CircleProgressBar : MonoBehaviour
{
    [Header("进度")]
    [Range(0f, 1f)] public float progress = 0f;

    [Header("填充速度")]
    public float fillSpeed = 0.5f;

    [Header("颜色变化(可选)")]
    public bool changeColor = true;
    public Color lowColor = Color.yellow;
    public Color fullColor = Color.green;

    private Image fillImage;

    void Start()
    {
        fillImage = GetComponent<Image>();
        ConfigureFill();
    }

    void Update()
    {
        if (fillImage == null) return;
        fillImage.fillAmount = progress;

        if (changeColor)
        {
            fillImage.color = Color.Lerp(lowColor, fullColor, progress);
        }
    }

    void ConfigureFill()
    {
        if (fillImage == null) return;
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Radial360;
        fillImage.fillAmount = 0f;
        fillImage.fillClockwise = true;
        fillImage.fillOrigin = (int)Image.Origin360.Top;
    }

    public void AddProgress(float amount)
    {
        progress = Mathf.Clamp01(progress + amount);
    }

    public void ResetProgress()
    {
        progress = 0f;
    }

    public bool IsFull()
    {
        return progress >= 0.99f;
    }
}