using UnityEngine;

public enum CatEventType
{
    Start,
    Uncomfortable,
    VeryUncomfortable,
    Angry,
    Success
}

public enum QTEType
{
    None,
    Click,
    Hold,
    Mash,
    Swipe
}

[System.Serializable]
public class FloatRange
{
    public float min;
    public float max;
}

[CreateAssetMenu(fileName = "CatEvent", menuName = "Game/CatEventData")]
public class CatEventData : ScriptableObject
{
    [Header("基础")]
    public string eventName = "Cat Event";
    public CatEventType eventType = CatEventType.Uncomfortable;
    public Sprite catSprite;
    public AudioClip catSound;

    [Header("触发条件")]
    public float triggerTime = 5f;
    [Range(0f, 1f)] public float triggerChance = 0.5f;

    [Header("QTE 设置")]
    public QTEType qteType = QTEType.Click;
    public float qteDuration = 3f;
    public int qteTargetCount = 5;
    public KeyCode qteKey = KeyCode.Space;

    [Header("成功奖励(随机区间)")]
    [Tooltip("成功时 currentSpeed 增量")]
    public FloatRange successSpeedRange = new FloatRange { min = 5f, max = 15f };
    
    [Tooltip("成功时 currentTime 增量(秒)")]
    public FloatRange successTimeRange = new FloatRange { min = 3f, max = 8f };
    
    [Tooltip("成功时 currentBoogerLength 减量")]
    public FloatRange successLengthReduceRange = new FloatRange { min = 0.5f, max = 2f };
    
    [Tooltip("成功时 dangerLimit 增量")]
    public FloatRange successDangerLimitRange = new FloatRange { min = 0.5f, max = 1.5f };

    [Header("失败惩罚(随机区间)")]
    [Tooltip("失败时 currentSpeed 减量")]
    public FloatRange failSpeedRange = new FloatRange { min = 5f, max = 15f };
    
    [Tooltip("失败时 currentTime 减量(秒)")]
    public FloatRange failTimeRange = new FloatRange { min = 3f, max = 8f };
    
    [Tooltip("失败时 currentBoogerLength 增量(变长)")]
    public FloatRange failLengthIncreaseRange = new FloatRange { min = 0.5f, max = 2f };
    
    [Tooltip("失败时 dangerLimit 减量")]
    public FloatRange failDangerLimitRange = new FloatRange { min = 0.5f, max = 1.5f };

    [Header("动画触发(预留)")]
    public string animatorTrigger = "Uncomfortable";
}