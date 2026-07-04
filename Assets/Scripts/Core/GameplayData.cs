using UnityEngine;

public class GameplayData : MonoBehaviour
{
    public static GameplayData Instance;

    [Header("鼻涕")]
    public float currentBoogerLength = 10f;
    public float maxBoogerLength = 10f;

    [Header("倒计时")]
    public float currentTime = 60f;
    public float totalTime = 60f;

    [Header("速度")]
    public float currentSpeed = 0f;
    public float maxSpeed = 100f;
    public float speedDecayPerSecond = 10f;

    [Header("鼻涕消耗")]
    public float boogerConsumeRate = 0.4f;

    [Header("危险区域")]
    public float dangerSpeed = 85f;
    public float dangerTime = 0f;
    public float dangerLimit = 2.5f;

    [Header("安全区")]
    public float safeSpeedMax = 60f;

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

        ResetData();
    }

    void Update()
    {
        // ⬅ 只在 Rolling 阶段更新数据
        if (GameManager.Instance == null) return;
        if (GameStateJudge.Instance == null) return;
        if (GameManager.Instance.state != GameState.Playing) return;
        if (!GameStateJudge.Instance.IsInRolling()) return;

        // 1. 倒计时
        currentTime -= Time.deltaTime;
        currentTime = Mathf.Max(0, currentTime);

        // 2. 速度衰减
        currentSpeed = Mathf.Max(0, currentSpeed - speedDecayPerSecond * Time.deltaTime);

        // 3. 鼻涕消耗
        float consumeRate = (currentSpeed / maxSpeed) * boogerConsumeRate;
        currentBoogerLength = Mathf.Max(0, currentBoogerLength - consumeRate * Time.deltaTime);
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