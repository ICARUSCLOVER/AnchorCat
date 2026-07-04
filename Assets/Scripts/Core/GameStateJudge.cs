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

    [Header("Rolling 设置")]
    public float maxTime = 60f;
    public float dangerLimit = 2.5f;

    [Header("滚轮加速")]
    public float scrollSpeedStep = 5f;

    [Header("胜利条件")]
    [Tooltip("拉完多少鼻涕算胜利(0-1 比例)")]
    public float winThreshold = 0.99f;

    [Header("事件系统")]
    public bool enableEvents = true;
    private bool eventsStarted = false;

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
        if (!IsInGame()) return;

        switch (subState)
        {
            case GameSubState.Intro:
                UpdateIntroTimer();
                break;

            case GameSubState.Sticking:
                // 等 QtipController 触发
                break;

            case GameSubState.Rolling:
                UpdateRolling();
                break;
        }
    }

    // ========== Intro ==========
    void UpdateIntroTimer()
    {
        if (hasAppeared) return;

        appearTimer += Time.deltaTime;
        if (appearTimer >= appearDelay)
        {
            hasAppeared = true;
            SetSubState(GameSubState.Sticking);
        }
    }

    // ========== Rolling ==========
    void UpdateRolling()
    {
        if (GameplayData.Instance == null) return;

        // 1. 时间到 → 失败
        if (GameplayData.Instance.currentTime <= 0)
        {
            OnTimeUp();
            return;
        }

        // 2. 危险区
        if (GameplayData.Instance.IsInDangerZone())
        {
            GameplayData.Instance.dangerTime += Time.deltaTime;
            if (GameplayData.Instance.dangerTime >= dangerLimit)
            {
                OnSpeedTooHigh();
                return;
            }
        }
        else
        {
            GameplayData.Instance.dangerTime = Mathf.Max(0,
                GameplayData.Instance.dangerTime - Time.deltaTime * 0.5f);
        }

        // 3. 滚轮
        HandleScroll();

        // 4. 胜利
        if (IsBoogerCleared())
        {
            OnBoogerCleared();
        }

        // 调试 log
        if (Time.frameCount % 60 == 0 && showDebugLog)
        {
            Debug.Log($"📏 length={GameplayData.Instance.currentBoogerLength:F2}/{GameplayData.Instance.maxBoogerLength}, " +
                      $"speed={GameplayData.Instance.currentSpeed:F1}, time={GameplayData.Instance.currentTime:F1}");
        }
    }

    bool IsBoogerCleared()
    {
        float progress = 1f - (GameplayData.Instance.currentBoogerLength / GameplayData.Instance.maxBoogerLength);
        return progress >= winThreshold;
    }

    // ========== 滚轮 ==========
    void HandleScroll()
    {
        float scrollDelta = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scrollDelta) > 0.001f)
        {
            float currentSpeed = GameplayData.Instance.currentSpeed;
            float newSpeed = currentSpeed + scrollDelta * scrollSpeedStep * 10f;
            newSpeed = Mathf.Clamp(newSpeed, 0f, GameplayData.Instance.maxSpeed);
            GameplayData.Instance.currentSpeed = newSpeed;
        }
    }

    // ========== 子状态管理 ==========
    public void ResetSubState()
    {
        subState = GameSubState.Intro;
        hasAppeared = false;
        appearTimer = 0f;
        eventsStarted = false;  // ⬅ 重置事件标志

        // 停止事件
        if (enableEvents && CatEventManager.Instance != null)
        {
            CatEventManager.Instance.StopEvents();
        }

        if (GameplayData.Instance != null)
        {
            GameplayData.Instance.ResetData();
        }
    }

    public void SetSubState(GameSubState newSubState)
    {
        if (showDebugLog)
            Debug.Log($"[Judge] SubState: {subState} -> {newSubState}");
        subState = newSubState;

        if (newSubState == GameSubState.Rolling)
        {
            if (GameplayData.Instance != null)
            {
                GameplayData.Instance.ResetData();
                Debug.Log($"[Judge] ResetData: length={GameplayData.Instance.currentBoogerLength}, " +
                          $"speed={GameplayData.Instance.currentSpeed}, time={GameplayData.Instance.currentTime}");
            }

            // ⬅ 启动事件
            if (enableEvents && CatEventManager.Instance != null && !eventsStarted)
            {
                CatEventManager.Instance.StartEvents();
                eventsStarted = true;
            }
        }

        if (newSubState == GameSubState.Intro)
        {
            ResetSubState();
        }
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
            // ⬅ 触发成功事件 + 停止
            if (enableEvents && CatEventManager.Instance != null)
            {
                CatEventManager.Instance.TriggerSuccessEvent();
                CatEventManager.Instance.StopEvents();
            }

            GameManager.Instance.OnSuccess();
            if (showDebugLog) Debug.Log("[Judge] 鼻涕拉完,通关!");
        }
    }

    public void OnSpeedTooHigh()
    {
        if (GameManager.Instance.state == GameState.Playing)
        {
            // ⬅ 停止事件
            if (enableEvents && CatEventManager.Instance != null)
            {
                CatEventManager.Instance.StopEvents();
            }

            GameManager.Instance.OnGameOver();
            if (showDebugLog) Debug.Log("[Judge] 速度红区 2.5s, 鼻涕断!");
        }
    }

    public void OnTimeUp()
    {
        if (GameManager.Instance.state != GameState.Playing) return;

        // ⬅ 停止事件
        if (enableEvents && CatEventManager.Instance != null)
        {
            CatEventManager.Instance.StopEvents();
        }

        GameManager.Instance.OnGameOver();
        if (showDebugLog) Debug.Log("[Judge] 60s 到,鼻涕没拉完,猫逃跑!");
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