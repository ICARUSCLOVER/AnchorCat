using UnityEngine;

public class CursorManager : MonoBehaviour
{
    [Header("设置")]
    public bool hideSystemCursor = true;
    public bool showDebugLog = true;

    [Header("Qtip 平滑")]
    public float moveSpeed = 15f;

    private Transform qtipCursor;

    public static CursorManager Instance { get; private set; }

    public Vector3 MouseWorldPos
    {
        get
        {
            Vector3 screenPos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0);
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
            worldPos.z = 0;
            return worldPos;
        }
    }

    // ⬇⬇⬇ 加这一段 ⬇⬇⬇
    /// <summary>
    /// 获取 Qtip 当前世界位置(给 BoogerController 等用)
    /// </summary>
    public Vector3 GetQtipPosition()
    {
        return qtipCursor != null ? qtipCursor.position : Vector3.zero;
    }
    // ⬆⬆⬆ 加这一段 ⬆⬆⬆

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

    void Start()
    {
        // 自动找子物体 Qtip
        if (qtipCursor == null)
            qtipCursor = transform.Find("Qtip");

        if (qtipCursor != null)
            qtipCursor.position = MouseWorldPos;   

        if (hideSystemCursor)
            Cursor.visible = false;
    }

    void OnDestroy()
    {
        Cursor.visible = true;
        if (Instance == this) Instance = null;
    }

    void Update()
    {
        UpdateQtip();
        DetectClick();
    }

    private void UpdateQtip()
    {
        if (qtipCursor == null) return;

        // 直接跟手,没限制
        qtipCursor.position = Vector3.Lerp(
            qtipCursor.position,
            MouseWorldPos,
            moveSpeed * Time.deltaTime
        );
    }

    private void DetectClick()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        Collider2D hit = Physics2D.OverlapPoint(MouseWorldPos);
        if (hit != null)
        {
            IClickable clickable = hit.GetComponent<IClickable>();
            if (clickable != null)
            {
                clickable.OnClick();
                if (showDebugLog)
                    Debug.Log($"点击了: {hit.gameObject.name}");
            }
            else if (showDebugLog)
            {
                Debug.Log($"点击了无响应物体: {hit.gameObject.name}");
            }
        }
    }
}