using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class BoogerController : MonoBehaviour
{
    [Header("A 点(起点)")]
    public Transform pointA;

    [Header("B 点控制(独立脚本)")]
    public BPointController bPointController;

    [Header("线段")]
    public int segments = 20;
    public float minWidth = 0.05f;
    public float maxWidth = 0.3f;

    [Header("视觉下垂(在 B 点下垂基础上)")]
    public float visualDroop = 0.1f;

    [Header("视觉摆动")]
    public float visualSwayAmount = 0.02f;
    public float visualSwaySpeed = 2f;

    [Header("颜色")]
    public Color normalColor = Color.yellow;
    public Color successColor = Color.green;
    public Color dangerColor = Color.red;

    [Header("粗细控制")]
    public float speedWidthFactor = 0.1f;

    private LineRenderer line;
    private Vector3[] positions;

    void Start()
    {
        line = GetComponent<LineRenderer>();
        line.positionCount = segments;
        line.material = new Material(Shader.Find("Sprites/Default"));
        positions = new Vector3[segments];
    }

    void Update()
    {
        if (pointA == null || bPointController == null) return;
        if (GameplayData.Instance == null) return;
        if (GameStateJudge.Instance == null) return;
        if (GameManager.Instance == null) return;

        var data = GameplayData.Instance;
        var subState = GameStateJudge.Instance.subState;
        var gmState = GameManager.Instance.state;

        // 1. 从 BPointController 获取 B 点位置
        Vector3 pointB = bPointController.GetBPointPosition();

        // 2. 计算鼻涕宽度
        float width = CalculateWidth(data);
        line.startWidth = width;
        line.endWidth = width;

        // 3. 计算鼻涕颜色
        Color color = CalculateColor(gmState, subState, data);
        line.startColor = color;
        line.endColor = color;

        // 4. 画线段
        DrawBooger(pointB, data);
    }

    /// <summary>
    /// 鼻涕宽度:剩余长度 + 速度影响
    /// </summary>
    float CalculateWidth(GameplayData data)
    {
        // 基础:跟剩余长度成反比
        float lengthT = data.currentBoogerLength / data.maxBoogerLength;
        float baseWidth = Mathf.Lerp(maxWidth, minWidth, lengthT);

        // 速度影响:速度高 → 紧绷 → 细
        float speedRatio = data.currentSpeed / data.maxSpeed;
        float speedWidth = baseWidth * (1f - speedRatio * speedWidthFactor);

        return Mathf.Max(speedWidth, minWidth);
    }

    /// <summary>
    /// 鼻涕颜色
    /// </summary>
    Color CalculateColor(GameState gmState, GameSubState subState, GameplayData data)
    {
        if (gmState == GameState.Success) return successColor;
        if (gmState == GameState.GameOver) return dangerColor;
        if (subState == GameSubState.Rolling && data.IsInDangerZone())
            return dangerColor;
        return normalColor;
    }

    /// <summary>
    /// 画鼻涕线段
    /// </summary>
    void DrawBooger(Vector3 pointB, GameplayData data)
    {
        float lengthT = data.currentBoogerLength / data.maxBoogerLength;

        for (int i = 0; i < segments; i++)
        {
            float ratio = (float)i / (segments - 1);
            positions[i] = Vector3.Lerp(pointA.position, pointB, ratio);

            // 视觉下垂(在 B 点下垂基础上,中间弧度更大)
            float droop = Mathf.Sin(ratio * Mathf.PI) * visualDroop * lengthT;
            positions[i].y -= droop;

            // 视觉摆动(Sticked 前轻微摆,Sticked 后归零)
            float mouseControlWeight = bPointController.IsMouseControlled() ? 0f : 1f;
            float swayWeight = Mathf.Sin(ratio * Mathf.PI);
            float sway = Mathf.Sin(Time.time * visualSwaySpeed + i * 0.3f)
                         * visualSwayAmount * swayWeight * mouseControlWeight;
            positions[i].x += sway;
        }

        line.SetPositions(positions);
    }

    /// <summary>
    /// 获取当前 B 点位置(给 QtipController 粘住检测用)
    /// </summary>
    public Vector3 GetBoogerTip()
    {
        return bPointController != null ? bPointController.GetBPointPosition() : Vector3.zero;
    }

    void OnDrawGizmos()
    {
        if (pointA == null || bPointController == null) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(bPointController.GetBPointPosition(), 0.1f);
    }
}