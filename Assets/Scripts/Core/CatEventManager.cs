using UnityEngine;
using UnityEngine.UI;

public class CatEventManager : MonoBehaviour
{
    public static CatEventManager Instance;

    [Header("事件配置")]
    public CatEventData[] events;
    public float minTimeBetweenEvents = 8f;
    public float maxTimeBetweenEvents = 15f;

    [Header("QTE UI")]
    public GameObject qtePanel;
    public Slider qteProgressBar;
    public Text qteInstructionText;
    public Image qteKeyIcon;

    [Header("调试")]
    public bool showDebugLog = true;

    private CatEventData currentEvent;
    private bool isQTETime = false;
    private int currentQTECount = 0;
    private float qteTimer = 0f;
    private float nextEventTime = 0f;
    private bool isActive = false;

    public event System.Action<CatEventType> OnCatEventTriggered;
    public event System.Action<CatEventData> OnQTESuccess;
    public event System.Action<CatEventData> OnQTEFail;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (qtePanel != null) qtePanel.SetActive(false);
    }

    void Update()
    {
        if (!IsRollingPhase()) return;
        if (!isActive) return;

        if (isQTETime)
        {
            UpdateQTE();
        }
        else if (Time.time >= nextEventTime)
        {
            TryTriggerEvent();
        }
    }

    bool IsRollingPhase()
    {
        return GameStateJudge.Instance != null &&
               GameStateJudge.Instance.IsInRolling();
    }

    void TryTriggerEvent()
    {
        var availableEvents = System.Array.FindAll(events, e => e != null);
        if (availableEvents.Length == 0) return;

        if (Random.value > GetAverageChance(availableEvents)) return;

        currentEvent = availableEvents[Random.Range(0, availableEvents.Length)];
        TriggerEvent(currentEvent);
    }

    float GetAverageChance(CatEventData[] events)
    {
        float sum = 0;
        foreach (var e in events) sum += e.triggerChance;
        return sum / events.Length;
    }

    void TriggerEvent(CatEventData eventData)
    {
        if (showDebugLog) Debug.Log($"🐱 触发事件: {eventData.eventName}");

        OnCatEventTriggered?.Invoke(eventData.eventType);

        if (eventData.catSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(eventData.catSound);
        }

        if (eventData.qteType != QTEType.None) StartQTE(eventData);
        ScheduleNextEvent();
    }

    void ScheduleNextEvent()
    {
        nextEventTime = Time.time + Random.Range(minTimeBetweenEvents, maxTimeBetweenEvents);
    }

    // ==================== QTE ====================

    void StartQTE(CatEventData eventData)
    {
        isQTETime = true;
        currentQTECount = 0;
        qteTimer = eventData.qteDuration;

        if (qtePanel != null) qtePanel.SetActive(true);
        UpdateQTEUI(eventData);

        if (showDebugLog) Debug.Log($"⏱️ QTE 开始: {eventData.eventName}");
    }

    void UpdateQTEUI(CatEventData eventData)
    {
        if (qteInstructionText != null)
            qteInstructionText.text = GetQTEInstruction(eventData);

        if (qteProgressBar != null)
            qteProgressBar.value = (float)currentQTECount / eventData.qteTargetCount;
    }

    string GetQTEInstruction(CatEventData eventData)
    {
        switch (eventData.qteType)
        {
            case QTEType.Click: return $"Tap {eventData.qteKey}!";
            case QTEType.Hold: return $"Hold {eventData.qteKey}!";
            case QTEType.Mash: return $"Mash {eventData.qteKey}!";
            case QTEType.Swipe: return "Swipe!";
            default: return "";
        }
    }

    void UpdateQTE()
    {
        if (currentEvent == null) return;

        qteTimer -= Time.deltaTime;
        if (qteProgressBar != null)
            qteProgressBar.value = qteTimer / currentEvent.qteDuration;

        HandleQTEInput();

        if (currentQTECount >= currentEvent.qteTargetCount) OnQTESuccessInternal();
        else if (qteTimer <= 0) OnQTEFailInternal();
    }

    void HandleQTEInput()
    {
        if (currentEvent == null) return;

        switch (currentEvent.qteType)
        {
            case QTEType.Click:
            case QTEType.Mash:
                if (Input.GetKeyDown(currentEvent.qteKey))
                {
                    currentQTECount++;
                    UpdateQTEUI(currentEvent);
                }
                break;
            case QTEType.Hold:
                if (Input.GetKey(currentEvent.qteKey))
                {
                    currentQTECount = (int)(currentEvent.qteTargetCount *
                        (1f - qteTimer / currentEvent.qteDuration));
                    UpdateQTEUI(currentEvent);
                }
                break;
        }
    }

    void OnQTESuccessInternal()
    {
        isQTETime = false;
        if (qtePanel != null) qtePanel.SetActive(false);
        ApplySuccess(currentEvent);
        OnQTESuccess?.Invoke(currentEvent);
        if (showDebugLog) Debug.Log($"✅ QTE 成功!");
        currentEvent = null;
    }

    void OnQTEFailInternal()
    {
        isQTETime = false;
        if (qtePanel != null) qtePanel.SetActive(false);
        ApplyFail(currentEvent);
        OnQTEFail?.Invoke(currentEvent);
        if (showDebugLog) Debug.Log($"❌ QTE 失败!");
        currentEvent = null;
    }

    // ==================== 成功奖励 ====================

    void ApplySuccess(CatEventData eventData)
    {
        if (GameplayData.Instance == null) return;

        // 1. Speed 增加
        float speedBoost = Random.Range(
            eventData.successSpeedRange.min,
            eventData.successSpeedRange.max
        );
        GameplayData.Instance.AddSpeed(speedBoost);
        if (showDebugLog) Debug.Log($"  ⚡ Speed +{speedBoost:F1}");

        // 2. Time 增加
        float timeBoost = Random.Range(
            eventData.successTimeRange.min,
            eventData.successTimeRange.max
        );
        GameplayData.Instance.currentTime += timeBoost;
        if (showDebugLog) Debug.Log($"  ⏰ Time +{timeBoost:F1}s");

        // 3. BoogerLength 减少
        float lengthReduce = Random.Range(
            eventData.successLengthReduceRange.min,
            eventData.successLengthReduceRange.max
        );
        GameplayData.Instance.currentBoogerLength -= lengthReduce;
        GameplayData.Instance.currentBoogerLength = Mathf.Max(0,
            GameplayData.Instance.currentBoogerLength);
        if (showDebugLog) Debug.Log($"  📏 Length -{lengthReduce:F1}");

        // 4. DangerLimit 增加
        float dangerBoost = Random.Range(
            eventData.successDangerLimitRange.min,
            eventData.successDangerLimitRange.max
        );
        if (GameStateJudge.Instance != null)
            GameStateJudge.Instance.dangerLimit += dangerBoost;
        if (showDebugLog) Debug.Log($"  ⚠️ DangerLimit +{dangerBoost:F1}");
    }

    // ==================== 失败惩罚 ====================

    void ApplyFail(CatEventData eventData)
    {
        if (GameplayData.Instance == null) return;

        // 1. Speed 减少
        float speedReduce = Random.Range(
            eventData.failSpeedRange.min,
            eventData.failSpeedRange.max
        );
        GameplayData.Instance.AddSpeed(-speedReduce);
        if (showDebugLog) Debug.Log($"  ⚡ Speed -{speedReduce:F1}");

        // 2. Time 减少
        float timeReduce = Random.Range(
            eventData.failTimeRange.min,
            eventData.failTimeRange.max
        );
        GameplayData.Instance.currentTime -= timeReduce;
        GameplayData.Instance.currentTime = Mathf.Max(0,
            GameplayData.Instance.currentTime);
        if (showDebugLog) Debug.Log($"  ⏰ Time -{timeReduce:F1}s");

        // 3. BoogerLength 增加
        float lengthIncrease = Random.Range(
            eventData.failLengthIncreaseRange.min,
            eventData.failLengthIncreaseRange.max
        );
        GameplayData.Instance.currentBoogerLength += lengthIncrease;
        if (showDebugLog) Debug.Log($"  📏 Length +{lengthIncrease:F1}");

        // 4. DangerLimit 减少
        float dangerReduce = Random.Range(
            eventData.failDangerLimitRange.min,
            eventData.failDangerLimitRange.max
        );
        if (GameStateJudge.Instance != null)
        {
            GameStateJudge.Instance.dangerLimit -= dangerReduce;
            GameStateJudge.Instance.dangerLimit = Mathf.Max(0.5f,
                GameStateJudge.Instance.dangerLimit);
        }
        if (showDebugLog) Debug.Log($"  ⚠️ DangerLimit -{dangerReduce:F1}");
    }

    // ==================== 控制 ====================

    public void StartEvents()
    {
        isActive = true;
        ScheduleNextEvent();
        if (showDebugLog) Debug.Log("🎬 事件系统启动");
    }

    public void StopEvents()
    {
        isActive = false;
        isQTETime = false;
        if (qtePanel != null) qtePanel.SetActive(false);
    }

    public void TriggerSuccessEvent()
    {
        var successEvent = System.Array.Find(events, e => e != null && e.eventType == CatEventType.Success);
        if (successEvent != null) TriggerEvent(successEvent);
    }
}