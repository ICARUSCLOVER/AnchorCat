using UnityEngine;

public class CatAnimation : MonoBehaviour
{
    [Header("Animator")]
    public Animator catAnimator;

    [Header("Trigger 名称")]
    public string startTrigger = "Start";
    public string uncomfortableTrigger = "Uncomfortable";
    public string veryUncomfortableTrigger = "VeryUncomfortable";
    public string angryTrigger = "Angry";
    public string successTrigger = "Success";

    [Header("调试")]
    public bool showDebugLog = true;

    void Start()
    {
        if (catAnimator == null) catAnimator = GetComponent<Animator>();
        
        if (showDebugLog)
            Debug.Log($"✅ CatAnimation 启动, Animator: {(catAnimator != null ? catAnimator.name : "NULL")}");

        // 订阅事件
        if (CatEventManager.Instance != null)
        {
            CatEventManager.Instance.OnCatEventTriggered += HandleEvent;
            if (showDebugLog) Debug.Log("✅ 订阅 CatEventManager.OnCatEventTriggered");
        }
        else
        {
            Debug.LogError("❌ CatEventManager.Instance 是 null!");
        }
    }

    void OnDestroy()
    {
        if (CatEventManager.Instance != null)
        {
            CatEventManager.Instance.OnCatEventTriggered -= HandleEvent;
        }
    }

    void HandleEvent(CatEventType type)
    {
        if (showDebugLog) Debug.Log($"🐱 收到事件: {type}");

        if (catAnimator == null)
        {
            Debug.LogError("❌ catAnimator 是 null!");
            return;
        }

        switch (type)
        {
            case CatEventType.Start:
                catAnimator.SetTrigger(startTrigger);
                if (showDebugLog) Debug.Log($"🔄 SetTrigger: {startTrigger}");
                break;
            case CatEventType.Uncomfortable:
                catAnimator.SetTrigger(uncomfortableTrigger);
                if (showDebugLog) Debug.Log($"🔄 SetTrigger: {uncomfortableTrigger}");
                break;
            case CatEventType.VeryUncomfortable:
                catAnimator.SetTrigger(veryUncomfortableTrigger);
                if (showDebugLog) Debug.Log($"🔄 SetTrigger: {veryUncomfortableTrigger}");
                break;
            case CatEventType.Angry:
                catAnimator.SetTrigger(angryTrigger);
                if (showDebugLog) Debug.Log($"🔄 SetTrigger: {angryTrigger}");
                break;
            case CatEventType.Success:
                catAnimator.SetTrigger(successTrigger);
                if (showDebugLog) Debug.Log($"🔄 SetTrigger: {successTrigger}");
                break;
        }
    }
}