using UnityEngine;

public enum GameSubState
{
    Intro,
    Sticking,
    Sticked,
    Rolling
}

public class GameStateJudge : MonoBehaviour
{
    public static GameStateJudge Instance;

    [Header("子状态")]
    public GameSubState subState = GameSubState.Intro;

    [Header("Intro 计时")]
    public float appearDelay = 3f;
    private float appearTimer = 0f;
    private bool hasAppeared = false;

    [Header("调试")]
    public bool showDebugLog = true;

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
        UpdateIntroTimer();
    }

    // ========== Intro 计时 ==========
    void UpdateIntroTimer()
    {
        if (!IsInGame()) return;
        if (!IsInIntro()) return;
        if (hasAppeared) return;

        appearTimer += Time.deltaTime;
        if (appearTimer >= appearDelay)
        {
            hasAppeared = true;
            SetSubState(GameSubState.Sticking);
            Debug.Log("[Judge] Intro 计时结束 → Sticking");
        }
    }

    // ========== 子状态管理 ==========
    public void ResetSubState()
    {
        subState = GameSubState.Intro;
        hasAppeared = false;
        appearTimer = 0f;
    }

    public void SetSubState(GameSubState newSubState)
    {
        if (showDebugLog)
            Debug.Log($"[Judge] SubState: {subState} -> {newSubState}");
        subState = newSubState;
    }

    // ========== 状态查询 ==========
    public bool IsInGame()
    {
        return GameManager.Instance != null &&
               GameManager.Instance.state == GameState.Playing;
    }

    public bool IsInIntro() => subState == GameSubState.Intro;
    public bool IsInSticking() => subState == GameSubState.Sticking;
    public bool IsInSticked() => subState == GameSubState.Sticked;
    public bool IsInRolling() => subState == GameSubState.Rolling;

    // ========== 状态判定 ==========

    public void OnBoogerCleared()
    {
        if (GameManager.Instance.state == GameState.Playing)
        {
            GameManager.Instance.OnSuccess();
        }
    }

    public void OnSpeedTooHigh()
    {
        if (GameManager.Instance.state == GameState.Playing)
        {
            GameManager.Instance.OnGameOver();
            Debug.Log("[Judge] 速度红区 2.5s, 鼻涕断!");
        }
    }

    public void OnTimeUp()
    {
        if (GameManager.Instance.state != GameState.Playing) return;

        if (GameplayData.Instance.currentBoogerLength > 0)
        {
            GameManager.Instance.OnGameOver();
            Debug.Log("[Judge] 60s 到, 鼻涕没拉完, 猫逃跑!");
        }
        else
        {
            GameManager.Instance.OnSuccess();
        }
    }

    // ========== 事件用 ==========

    public void ForceSpeedBoost(float amount)
    {
        if (GameplayData.Instance != null)
        {
            GameplayData.Instance.currentSpeed += amount;
            GameplayData.Instance.currentSpeed = Mathf.Clamp(
                GameplayData.Instance.currentSpeed,
                0, GameplayData.Instance.maxSpeed);
        }
    }

    public void ForceSpeedReduce(float amount)
    {
        if (GameplayData.Instance != null)
        {
            GameplayData.Instance.currentSpeed -= amount;
            GameplayData.Instance.currentSpeed = Mathf.Max(0, GameplayData.Instance.currentSpeed);
        }
    }
}