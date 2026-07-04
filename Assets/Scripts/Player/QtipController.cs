using UnityEngine;

public class QtipController : MonoBehaviour
{
    [Header("Sticking 检测")]
    public string triggerTag = "BoogerTip";
    public float stickHoldTime = 5f;

    [Header("圆圈进度条")]
    public CircleProgressBar circleProgress;
    public Transform progressFollowTarget;

    [Header("调试")]
    public bool showDebugLog = true;

    private Transform currentTrigger;
    private Collider2D currentTriggerCollider;
    private bool wasInZone = false;
    private float lastLogTime = 0f;

    void Start()
    {
        if (circleProgress != null)
        {
            circleProgress.gameObject.SetActive(false);
            circleProgress.ResetProgress();
        }

        FindBoogerTipTrigger();
    }

    void Update()
    {
        if (GameStateJudge.Instance == null) return;
        if (CursorManager.Instance == null) return;
        if (circleProgress == null) return;

        var subState = GameStateJudge.Instance.subState;

        // 只在 Sticking 阶段检测
        if (subState != GameSubState.Sticking)
        {
            if (wasInZone) HideProgress();
            return;
        }

        // 没找到触发器就找
        if (currentTrigger == null)
        {
            FindBoogerTipTrigger();
            if (currentTrigger == null) return;
        }

        // 检测是否在触发区
        bool isInZone = IsQtipInTriggerZone();

        // 调试 log(每秒最多 2 次)
        if (showDebugLog && Time.time - lastLogTime > 0.5f)
        {
            lastLogTime = Time.time;
            Debug.Log($"🔍 触发区: {isInZone}, 按住左键: {Input.GetMouseButton(0)}, 进度: {(circleProgress.progress * 100f):F0}%");
        }

        if (isInZone)
        {
            // 进入触发区 → 显示进度条
            if (!wasInZone)
            {
                circleProgress.gameObject.SetActive(true);
                circleProgress.ResetProgress();
                if (showDebugLog) Debug.Log("✅ 进入触发区");
            }

            // 跟随鼻涕末端
            circleProgress.transform.position = currentTrigger.position;

            // 按住左键 → 进度增加
            if (Input.GetMouseButton(0))
            {
                circleProgress.AddProgress(Time.deltaTime / stickHoldTime);

                // 进度满 → Sticked
                if (circleProgress.IsFull())
                {
                    if (showDebugLog) Debug.Log("🎯 进度满了,进入 Sticked");
                    GameStateJudge.Instance.SetSubState(GameSubState.Rolling);
                    HideProgress();
                    wasInZone = false;
                    return;
                }
            }
        }
        else
        {
            // 离开触发区 → 隐藏进度条
            if (wasInZone)
            {
                HideProgress();
                if (showDebugLog) Debug.Log("❌ 离开触发区");
            }
        }

        wasInZone = isInZone;
    }

    void FindBoogerTipTrigger()
    {
        GameObject found = GameObject.FindGameObjectWithTag(triggerTag);
        if (found != null)
        {
            currentTrigger = found.transform;
            currentTriggerCollider = found.GetComponent<Collider2D>();
        }
    }

    bool IsQtipInTriggerZone()
    {
        if (currentTrigger == null || currentTriggerCollider == null) return false;

        Transform cursor = CursorManager.Instance.GetCursorTransform();
        if (cursor == null) return false;

        return currentTriggerCollider.bounds.Contains(cursor.position);
    }

    void HideProgress()
    {
        if (circleProgress != null && circleProgress.gameObject.activeSelf)
        {
            circleProgress.gameObject.SetActive(false);
            circleProgress.ResetProgress();
        }
    }
}