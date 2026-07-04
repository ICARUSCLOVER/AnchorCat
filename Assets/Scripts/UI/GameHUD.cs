using UnityEngine;
using UnityEngine.UI;
using TMPro;  // ⬅ 加这行

public class GameHUD : MonoBehaviour
{
    [Header("速度条")]
    public RectTransform barRect;
    public RectTransform pointerRect;

    [Header("Timer")]
    public TextMeshProUGUI timerText;  

    void Update()
    {
        if (GameplayData.Instance == null) return;
        if (GameStateJudge.Instance == null) return;

        UpdatePointer();
        UpdateTimer();
    }

    void UpdatePointer()
    {
        if (barRect == null || pointerRect == null) return;
        var data = GameplayData.Instance;

        bool showBar = GameStateJudge.Instance.IsInRolling();
        barRect.gameObject.SetActive(showBar);
        pointerRect.gameObject.SetActive(showBar);
        if (!showBar) return;

        float barHeight = barRect.rect.height;
        float maxSpeed = data.maxSpeed;
        float t = Mathf.Clamp01(data.currentSpeed / maxSpeed);
        float yPos = t * barHeight - barHeight / 2f;

        pointerRect.anchoredPosition = new Vector2(
            pointerRect.anchoredPosition.x,
            yPos
        );
    }

    void UpdateTimer()
    {
        if (timerText == null) return;

        bool showTimer = GameStateJudge.Instance.IsInRolling();
        timerText.gameObject.SetActive(showTimer);

        if (showTimer)
        {
            int timeLeft = Mathf.CeilToInt(GameplayData.Instance.currentTime);
            timerText.text = $"{timeLeft:D2}";
            timerText.color = timeLeft <= 10 ? Color.red : Color.black;
        }
    }
}