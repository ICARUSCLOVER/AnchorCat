using UnityEngine;

public class GameplayData : MonoBehaviour
{
    public static GameplayData Instance;

    [Header("鼻涕")]
    public float currentBoogerLength = 10f;     // ⬅ 初始 10
    public float maxBoogerLength = 10f;

    [Header("倒计时")]
    public float currentTime = 60f;
    public float totalTime = 60f;

    [Header("速度")]
    public float currentSpeed = 0f;
    public float maxSpeed = 100f;
    public float speedDecayPerSecond = 10f;
    public float speedAddPerScroll = 5f;

    [Header("鼻涕消耗")]
    public float boogerConsumeRate = 0.4f;     // 最大消耗率

    [Header("危险区域")]
    public float dangerSpeed = 85f;
    public float dangerTime = 0f;
    public float dangerLimit = 2.5f;

    [Header("安全区")]
    public float safeSpeedMax = 60f;            // 绿色区上限

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Update()
    {
        if (GameManager.Instance == null) return;
        if (GameStateJudge.Instance == null) return;
        if (GameManager.Instance.state != GameState.Playing) return;

        // 只在 Rolling 阶段跑
        if (!GameStateJudge.Instance.IsInRolling()) return;

        // 1. 倒计时
        currentTime -= Time.deltaTime;
        if (currentTime <= 0)
        {
            currentTime = 0;
            GameStateJudge.Instance.OnTimeUp();
            return;
        }

        // 2. 速度自然衰减
        currentSpeed = Mathf.Max(0, currentSpeed - speedDecayPerSecond * Time.deltaTime);

        // 3. 鼻涕消耗(根据速度)
        float consumeRate = (currentSpeed / maxSpeed) * boogerConsumeRate;
        currentBoogerLength = Mathf.Max(0, currentBoogerLength - consumeRate * Time.deltaTime);

        // 4. 鼻涕拉完 → 成功
        if (currentBoogerLength <= 0)
        {
            GameStateJudge.Instance.OnBoogerCleared();
        }

        // 5. 危险区判定
        if (currentSpeed >= dangerSpeed)
        {
            dangerTime += Time.deltaTime;
            if (dangerTime >= dangerLimit)
            {
                GameStateJudge.Instance.OnSpeedTooHigh();
            }
        }
        else
        {
            dangerTime = 0f;
        }
    }

    public void AddSpeed(float amount)
    {
        if (GameStateJudge.Instance == null) return;
        if (!GameStateJudge.Instance.IsInRolling()) return;

        currentSpeed = Mathf.Clamp(currentSpeed + amount, 0, maxSpeed);
    }

    public bool IsInDangerZone() => currentSpeed >= dangerSpeed;
    public bool IsInSafeZone() => currentSpeed <= safeSpeedMax;
    public bool IsInWarningZone() => currentSpeed > safeSpeedMax && currentSpeed < dangerSpeed;

    public Color GetSpeedColor()
    {
        if (IsInDangerZone()) return Color.red;
        if (IsInWarningZone()) return Color.yellow;
        return Color.green;
    }

    public void ResetData()
    {
        currentBoogerLength = maxBoogerLength;
        currentTime = totalTime;
        currentSpeed = 0f;
        dangerTime = 0f;
    }
}