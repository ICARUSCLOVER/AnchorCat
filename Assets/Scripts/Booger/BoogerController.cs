using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class BoogerController : MonoBehaviour
{
    [Header("A 点(起点)")]
    public Transform catNose;

    [Header("鼻涕动画(演示用)")]
    public float boogerInitialLength = 1.0f;
    public float boogerLengthVariation = 0.2f;
    public float animationSpeed = 1f;

    [Header("线段")]
    public int segments = 20;
    public float baseWidth = 0.15f;
    public float maxWidth = 0.3f;

    [Header("下垂")]
    public float droopAmount = 0.2f;

    [Header("甩动(按子状态)")]
    [Tooltip("钟摆角度(度),越大越夸张")]
    public float introSwayAngle = 5f;        // Intro 轻微摆
    public float introSwaySpeed = 1.5f;
    public float stickingSwayAngle = 15f;     // Sticking 大幅摆
    public float stickingSwaySpeed = 3f;
    public float rollingSwayAngle = 8f;       // Rolling 摆(虽然跟鼠标,但视觉惯性)
    public float rollingSwaySpeed = 4f;

    [Header("速度加成(角度)")]
    public float speedAngleBoost = 10f;       // 速度越高,摆得越厉害

    [Header("颜色")]
    public Color normalColor = Color.yellow;
    public Color successColor = Color.green;
    public Color dangerColor = Color.red;

    private LineRenderer line;
    private Vector3[] positions;
    private Vector3 currentBoogerTip;

    void Start()
    {
        line = GetComponent<LineRenderer>();
        line.positionCount = segments;
        line.startColor = Color.yellow;
        line.endColor = Color.yellow;
        line.material = new Material(Shader.Find("Sprites/Default"));
        positions = new Vector3[segments];
    }

    void Update()
    {
        if (catNose == null) return;
        if (GameStateJudge.Instance == null) return;
        if (GameManager.Instance == null) return;

        var subState = GameStateJudge.Instance.subState;
        var gmState = GameManager.Instance.state;

        // 1. 鼻涕颜色
        Color color = CalculateColor(gmState, subState);
        line.startColor = color;
        line.endColor = color;

        // 2. 鼻涕演示长度(动画用)
        float demoLength = CalculateDemoLength();

        // 3. 鼻涕粗细(基于演示长度)
        float lengthT = Mathf.InverseLerp(0.6f, 1.4f, demoLength);  // 0.6-1.4 映射 0-1
        float width = Mathf.Lerp(maxWidth, baseWidth, lengthT);
        line.startWidth = width;
        line.endWidth = width;

        // 4. 计算 B 点(根据子状态)
        currentBoogerTip = CalculateBPoint(subState, demoLength);

        // 5. 画线段(钟摆感)
        DrawBoogerPendulum(demoLength);
    }

    // ==================== 鼻涕演示长度 ====================
    float CalculateDemoLength()
    {
        var subState = GameStateJudge.Instance.subState;

        if (subState == GameSubState.Intro || subState == GameSubState.Sticking)
        {
            // Intro/Sticking:动画演示
            float t = (Mathf.Sin(Time.time * animationSpeed) + 1f) * 0.5f;  // 0-1
            return Mathf.Lerp(
                boogerInitialLength - boogerLengthVariation,
                boogerInitialLength + boogerLengthVariation,
                t
            );
        }

        // Sticked/Rolling:固定
        return boogerInitialLength;
    }

    // ==================== 计算 B 点(钟摆) ====================
    Vector3 CalculateBPoint(GameSubState subState, float demoLength)
    {
        if (subState == GameSubState.Sticked || subState == GameSubState.Rolling)
        {
            // Sticked 后:B 点 = 鼠标位置
            return GetMouseWorldPos();
        }

        // Intro/Sticking:钟摆摆动
        float swayAngle, swaySpeed;
        SelectSwayParams(subState, out swayAngle, out swaySpeed);

        // 钟摆:绕 A 点旋转
        // angle = 0 时,鼻涕垂直下垂
        // angle 正负 → 左右摆
        float angleDeg = Mathf.Sin(Time.time * swaySpeed) * swayAngle;
        Vector3 dir = Quaternion.Euler(0, 0, angleDeg) * Vector3.down;
        return catNose.position + dir * demoLength;
    }

    void SelectSwayParams(GameSubState subState, out float swayAngle, out float swaySpeed)
    {
        switch (subState)
        {
            case GameSubState.Intro:
                swayAngle = introSwayAngle;
                swaySpeed = introSwaySpeed;
                break;
            case GameSubState.Sticking:
                swayAngle = stickingSwayAngle;
                swaySpeed = stickingSwaySpeed;
                break;
            case GameSubState.Sticked:
            case GameSubState.Rolling:
                swayAngle = rollingSwayAngle;
                swaySpeed = rollingSwaySpeed;
                break;
            default:
                swayAngle = 0;
                swaySpeed = 1;
                break;
        }
    }

    Vector3 GetMouseWorldPos()
    {
        if (CursorManager.Instance != null)
        {
            return CursorManager.Instance.GetQtipPosition();
        }

        Vector3 screenPos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0);
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
        worldPos.z = 0;
        return worldPos;
    }

    // ==================== 颜色 ====================
    Color CalculateColor(GameState gmState, GameSubState subState)
    {
        if (gmState == GameState.Success) return successColor;
        if (gmState == GameState.GameOver) return dangerColor;
        if (subState == GameSubState.Rolling)
        {
            if (GameplayData.Instance != null && GameplayData.Instance.IsInDangerZone())
            {
                return dangerColor;
            }
        }
        return normalColor;
    }

    // ==================== 画线段(钟摆感) ====================
    void DrawBoogerPendulum(float demoLength)
    {
        for (int i = 0; i < segments; i++)
        {
            float ratio = (float)i / (segments - 1);  // 0-1
            positions[i] = Vector3.Lerp(catNose.position, currentBoogerTip, ratio);

            // 下垂(中间弧度)
            float droop = Mathf.Sin(ratio * Mathf.PI) * droopAmount * demoLength;
            positions[i].y -= droop;

            // ⬅ 不再叠加左右摆!钟摆感由 B 点决定
        }

        line.SetPositions(positions);
    }

    // ==================== 公开方法 ====================
    public Vector3 GetBoogerTip()
    {
        return currentBoogerTip;
    }

    void OnDrawGizmos()
    {
        if (catNose == null) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(currentBoogerTip, 0.1f);
    }
}