using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class CircleProgressBar : MonoBehaviour
{
    [Header("进度")]
    [Range(0f, 1f)] public float progress = 0f;

    [Header("填充速度")]
    public float fillSpeed = 0.5f;  // 每秒填充 0.5(2 秒满)

    private Image fillImage;

    void Start()
    {
        fillImage = GetComponent<Image>();
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Radial360;
        fillImage.fillAmount = 0f;
        fillImage.fillClockwise = true;
    }

    void Update()
    {
        if (fillImage == null) return;
        fillImage.fillAmount = progress;
    }

    /// <summary>
    /// 增加进度
    /// </summary>
    public void AddProgress(float amount)
    {
        progress = Mathf.Clamp01(progress + amount);
    }

    /// <summary>
    /// 重置进度
    /// </summary>
    public void ResetProgress()
    {
        progress = 0f;
    }

    /// <summary>
    /// 进度是否满了
    /// </summary>
    public bool IsFull()
    {
        return progress >= 0.99f;
    }
}