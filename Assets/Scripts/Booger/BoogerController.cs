using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class BoogerController : MonoBehaviour
{
    [Header("连接点")]
    public Transform catNose;

    [Header("线段")]
    public int segments = 20;
    public float baseWidth = 0.1f;
    public float maxWidth = 0.3f;

    [Header("下垂")]
    public float droopAmount = 0.3f;

    [Header("甩动(按子状态)")]
    public float introSwayAmount = 0.03f;
    public float introSwaySpeed = 1f;
    public float stickingSwayAmount = 0.15f;
    public float stickingSwaySpeed = 3f;
    public float rollingSwayAmount = 0.1f;
    public float rollingSwaySpeed = 3f;

    [Header("速度加成")]
    public float speedSwayBoost = 0.1f;

    [Header("B 点参考(粘住检测)")]
    public float boogerTipDownOffset = 0.5f;

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
        if (GameplayData.Instance == null) return;
        if (GameManager.Instance == null) return;
        if (GameStateJudge.Instance == null) return;

        var data = GameplayData.Instance;
        var subState = GameStateJudge.Instance.subState;
        var gmState = GameManager.Instance.state;

        // 1. 鼻涕宽度
        float lengthT = data.currentBoogerLength / data.maxBoogerLength;
        float width = Mathf.Lerp(maxWidth, baseWidth, lengthT);
        line.startWidth = width;
        line.endWidth = width;

        // 2. 鼻涕颜色
        Color boogerColor = Color.yellow;
        if (gmState == GameState.Success)
        {
            boogerColor = Color.green;
        }
        else if (gmState == GameState.GameOver)
        {
            boogerColor = Color.red;
        }
        else if (subState == GameSubState.Rolling && data.IsInDangerZone())
        {
            boogerColor = Color.red;
        }
        line.startColor = boogerColor;
        line.endColor = boogerColor;

        // 3. 选甩动幅度
        float swayAmount = 0f;
        float swaySpeed = 1f;

        switch (subState)
        {
            case GameSubState.Intro:
                swayAmount = introSwayAmount;
                swaySpeed = introSwaySpeed;
                break;
            case GameSubState.Sticking:
                swayAmount = stickingSwayAmount;
                swaySpeed = stickingSwaySpeed;
                break;
            case GameSubState.Sticked:
            case GameSubState.Rolling:
                swayAmount = rollingSwayAmount +
                             (data.currentSpeed / data.maxSpeed) * speedSwayBoost;
                swaySpeed = rollingSwaySpeed + data.currentSpeed / 20f;
                break;
        }

        // 4. 计算 B 点(用于粘住检测)
        float baseDownOffset = boogerTipDownOffset * lengthT;
        float baseSwayX = Mathf.Sin(Time.time * swaySpeed) * swayAmount * 1f;

        currentBoogerTip = new Vector3(
            catNose.position.x + baseSwayX,
            catNose.position.y - baseDownOffset,
            catNose.position.z
        );

        // 5. 画线段
        for (int i = 0; i < segments; i++)
        {
            float ratio = (float)i / (segments - 1);
            positions[i] = Vector3.Lerp(catNose.position, currentBoogerTip, ratio);

            float droop = Mathf.Sin(ratio * Mathf.PI) * droopAmount * lengthT;
            positions[i].y -= droop;

            float swayWeight = Mathf.Sin(ratio * Mathf.PI);
            float sway = Mathf.Sin(Time.time * swaySpeed + i * 0.3f) * swayAmount * swayWeight;
            positions[i].x += sway;
        }

        line.SetPositions(positions);
    }

    /// <summary>
    /// 获取当前 B 点位置(给 QtipController 粘住检测用)
    /// </summary>
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