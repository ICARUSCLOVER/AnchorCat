using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

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
    public TextMeshProUGUI qteInstructionText;
    public TextMeshProUGUI qteResultText;     // ⬅ 新加
    public Image qteKeyIcon;

    [Header("按键图标")]
    public Sprite[] keySprites;

    [Header("QTE 反馈")]
    public float resultDisplayTime = 0.6f;     // ⬅ 结果显示时长

    [Header("屏幕震动")]
    public float successShakeIntensity = 0.05f;
    public float successShakeDuration = 0.15f;
    public float failShakeIntensity = 0.2f;
    public float failShakeDuration = 0.4f;

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
        Debug.Log($"[CatEvent.Start] qtePanel={(qtePanel != null ? qtePanel.name : "NULL")}, " +
                  $"progressBar={qteProgressBar != null}, " +
                  $"instructionText={qteInstructionText != null}, " +
                  $"resultText={qteResultText != null}, " +
                  $"keyIcon={qteKeyIcon != null}");

        if (qtePanel != null)
        {
            qtePanel.SetActive(false);
            Debug.Log($"[CatEvent.Start] qtePanel.SetActive(false), ActiveSelf={qtePanel.activeSelf}");
        }
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
        if (showDebugLog) Debug.Log($"🐱 触发事件: {eventData.eventName}, qteType={eventData.qteType}");

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

        if (qtePanel == null)
        {
            Debug.LogError($"❌ StartQTE: qtePanel 是 NULL!QTE 无法显示");
            return;
        }

        // ⬅ 重置 UI 状态
        ResetQTEUI();

        qtePanel.SetActive(true);

        Debug.Log($"[StartQTE] {eventData.eventName} 已启动, ActiveSelf={qtePanel.activeSelf}");

        UpdateQTEUI(eventData);

        if (showDebugLog) Debug.Log($"⏱️ QTE 已启动: {eventData.eventName}");
    }

    void ResetQTEUI()
    {
        // ⬅ 重置所有子元素到初始状态
        if (qteInstructionText != null) qteInstructionText.gameObject.SetActive(true);
        if (qteProgressBar != null) qteProgressBar.gameObject.SetActive(true);
        if (qteKeyIcon != null) qteKeyIcon.gameObject.SetActive(true);
        if (qteResultText != null) qteResultText.gameObject.SetActive(false);
    }

    void UpdateQTEUI(CatEventData eventData)
    {
        if (qteInstructionText != null)
        {
            qteInstructionText.text = GetQTEInstruction(eventData);
        }
        else
        {
            Debug.LogError("❌ qteInstructionText 是 NULL");
        }

        if (qteProgressBar != null)
        {
            float t = qteTimer / eventData.qteDuration;
            qteProgressBar.value = t;
            var fillImage = qteProgressBar.fillRect?.GetComponent<Image>();
            if (fillImage != null)
                fillImage.color = Color.Lerp(Color.red, Color.green, t);
        }
        else
        {
            Debug.LogError("❌ qteProgressBar 是 NULL");
        }

        if (qteKeyIcon != null)
        {
            if (currentEvent == null)
            {
                qteKeyIcon.gameObject.SetActive(false);
                return;
            }

            int idx = GetKeySpriteIndex(currentEvent.qteKey);
            if (idx >= 0 && keySprites != null && idx < keySprites.Length && keySprites[idx] != null)
            {
                qteKeyIcon.gameObject.SetActive(true);
                qteKeyIcon.sprite = keySprites[idx];
            }
            else
            {
                qteKeyIcon.gameObject.SetActive(false);
            }
        }
        else
        {
            Debug.LogError("❌ qteKeyIcon 是 NULL");
        }
    }

    int GetKeySpriteIndex(KeyCode key)
    {
        switch (key)
        {
            case KeyCode.S: return 0;
            case KeyCode.A: return 1;
            case KeyCode.W: return 2;
            case KeyCode.Space: return 3;
            case KeyCode.Mouse0: return 4;
            default: return -1;
        }
    }

    string GetKeyDisplayName(KeyCode key)
    {
        switch (key)
        {
            case KeyCode.Space: return "SPACE";
            case KeyCode.Mouse0: return "CLICK";
            default: return key.ToString();
        }
    }

    string GetQTEInstruction(CatEventData eventData)
    {
        string keyName = GetKeyDisplayName(eventData.qteKey);
        switch (eventData.qteType)
        {
            case QTEType.Click:
                return $"Tap {keyName} {eventData.qteTargetCount} times!";
            case QTEType.Hold:
                return $"Hold {keyName}!";
            case QTEType.Mash:
                return $"Mash {keyName}!";
            case QTEType.Swipe:
                return "Swipe the mouse!";
            default:
                return "";
        }
    }

    void UpdateQTE()
    {
        if (currentEvent == null) return;

        qteTimer -= Time.deltaTime;
        if (qteProgressBar != null)
        {
            float t = qteTimer / currentEvent.qteDuration;
            qteProgressBar.value = t;
            var fillImage = qteProgressBar.fillRect?.GetComponent<Image>();
            if (fillImage != null)
                fillImage.color = Color.Lerp(Color.red, Color.green, t);
        }

        HandleQTEInput();

        if (currentQTECount >= currentEvent.qteTargetCount)
            OnQTESuccessInternal();
        else if (qteTimer <= 0)
            OnQTEFailInternal();
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
                }
                break;
            case QTEType.Hold:
                if (Input.GetKey(currentEvent.qteKey))
                {
                    currentQTECount = Mathf.FloorToInt(currentEvent.qteTargetCount *
                        (1f - qteTimer / currentEvent.qteDuration));
                }
                break;
        }
    }

    void OnQTESuccessInternal()
    {
        isQTETime = false;

        // ⬅ 显示成功反馈
        ShowResult("SUCCESS!", new Color(0.2f, 0.9f, 0.2f, 1f));

        // ⬅ 震动(轻微)
        if (CameraShake.Instance != null)
            CameraShake.Instance.Shake(successShakeIntensity, successShakeDuration);

        // ⬅ 延迟关闭 panel
        Invoke(nameof(HideResultPanelDelayed), resultDisplayTime);

        ApplySuccess(currentEvent);
        OnQTESuccess?.Invoke(currentEvent);
        if (showDebugLog) Debug.Log($"✅ QTE 成功!");
        currentEvent = null;
    }

    void OnQTEFailInternal()
    {
        isQTETime = false;

        // ⬅ 显示失败反馈
        ShowResult("FAILED!", new Color(0.9f, 0.2f, 0.2f, 1f));

        // ⬅ 震动(强烈)
        if (CameraShake.Instance != null)
            CameraShake.Instance.Shake(failShakeIntensity, failShakeDuration);

        // ⬅ 延迟关闭 panel
        Invoke(nameof(HideResultPanelDelayed), resultDisplayTime);

        ApplyFail(currentEvent);
        OnQTEFail?.Invoke(currentEvent);
        if (showDebugLog) Debug.Log($"❌ QTE 失败!");
        currentEvent = null;
    }

    // ==================== 反馈系统 ====================

    void ShowResult(string text, Color color)
    {
        if (qtePanel != null) qtePanel.SetActive(true);

        // ⬅ 隐藏原本 QTE 元素
        if (qteInstructionText != null) qteInstructionText.gameObject.SetActive(false);
        if (qteProgressBar != null) qteProgressBar.gameObject.SetActive(false);
        if (qteKeyIcon != null) qteKeyIcon.gameObject.SetActive(false);

        // ⬅ 显示结果文字
        if (qteResultText != null)
        {
            qteResultText.gameObject.SetActive(true);
            qteResultText.text = text;
            qteResultText.color = color;
            StartCoroutine(AnimateResultIn());
        }
        else
        {
            Debug.LogWarning("⚠️ qteResultText 为空,跳过文字反馈");
        }
    }

    IEnumerator AnimateResultIn()
    {
        if (qteResultText == null) yield break;

        float duration = 0.25f;
        float elapsed = 0f;
        Vector3 startScale = Vector3.one * 1.5f;
        Vector3 endScale = Vector3.one * 1.0f;

        Color startColor = qteResultText.color;
        startColor.a = 0f;
        Color endColor = qteResultText.color;
        endColor.a = 1f;
        qteResultText.color = startColor;
        qteResultText.transform.localScale = startScale;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            qteResultText.transform.localScale = Vector3.Lerp(startScale, endScale, t);
            qteResultText.color = Color.Lerp(startColor, endColor, t);
            yield return null;
        }

        qteResultText.transform.localScale = endScale;
        qteResultText.color = endColor;
    }

    void HideResultPanelDelayed()
    {
        if (qtePanel != null) qtePanel.SetActive(false);
        ResetQTEUI();
    }

    // ==================== 成功奖励 ====================
    void ApplySuccess(CatEventData eventData)
    {
        if (GameplayData.Instance == null) return;

        float speedBoost = Random.Range(eventData.successSpeedRange.min, eventData.successSpeedRange.max);
        GameplayData.Instance.AddSpeed(speedBoost);
        if (showDebugLog) Debug.Log($"  ⚡ Speed +{speedBoost:F1}");

        float timeBoost = Random.Range(eventData.successTimeRange.min, eventData.successTimeRange.max);
        GameplayData.Instance.currentTime += timeBoost;
        if (showDebugLog) Debug.Log($"  ⏰ Time +{timeBoost:F1}s");

        float lengthReduce = Random.Range(eventData.successLengthReduceRange.min, eventData.successLengthReduceRange.max);
        GameplayData.Instance.currentBoogerLength -= lengthReduce;
        GameplayData.Instance.currentBoogerLength = Mathf.Max(0, GameplayData.Instance.currentBoogerLength);
        if (showDebugLog) Debug.Log($"  📏 Length -{lengthReduce:F1}");

        float dangerBoost = Random.Range(eventData.successDangerLimitRange.min, eventData.successDangerLimitRange.max);
        if (GameStateJudge.Instance != null)
            GameStateJudge.Instance.dangerLimit += dangerBoost;
        if (showDebugLog) Debug.Log($"  ⚠️ DangerLimit +{dangerBoost:F1}");
    }

    // ==================== 失败惩罚 ====================
    void ApplyFail(CatEventData eventData)
    {
        if (GameplayData.Instance == null) return;

        float speedReduce = Random.Range(eventData.failSpeedRange.min, eventData.failSpeedRange.max);
        GameplayData.Instance.AddSpeed(-speedReduce);
        if (showDebugLog) Debug.Log($"  ⚡ Speed -{speedReduce:F1}");

        float timeReduce = Random.Range(eventData.failTimeRange.min, eventData.failTimeRange.max);
        GameplayData.Instance.currentTime -= timeReduce;
        GameplayData.Instance.currentTime = Mathf.Max(0, GameplayData.Instance.currentTime);
        if (showDebugLog) Debug.Log($"  ⏰ Time -{timeReduce:F1}s");

        float lengthIncrease = Random.Range(eventData.failLengthIncreaseRange.min, eventData.failLengthIncreaseRange.max);
        GameplayData.Instance.currentBoogerLength += lengthIncrease;
        if (showDebugLog) Debug.Log($"  📏 Length +{lengthIncrease:F1}");

        float dangerReduce = Random.Range(eventData.failDangerLimitRange.min, eventData.failDangerLimitRange.max);
        if (GameStateJudge.Instance != null)
        {
            GameStateJudge.Instance.dangerLimit -= dangerReduce;
            GameStateJudge.Instance.dangerLimit = Mathf.Max(0.5f, GameStateJudge.Instance.dangerLimit);
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