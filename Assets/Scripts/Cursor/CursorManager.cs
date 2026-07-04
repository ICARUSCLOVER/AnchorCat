using UnityEngine;
using UnityEngine.SceneManagement;

public class CursorManager : MonoBehaviour
{
    public static CursorManager Instance { get; private set; }

    [Header("鼠标配置")]
    public string cursorTag = "Cursor";
    public float moveSpeed = 15f;
    public bool hideSystemCursor = true;

    [Header("调试")]
    public bool showDebugLog = true;

    [Header("跳过场景")]
    public string[] skipScenes = { "Persistent" };

    private Transform cursorObject;

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
        if (hideSystemCursor) Cursor.visible = false;
        FindCursorInCurrentScene();
    }

    void OnDestroy()
    {
        Cursor.visible = true;
        if (Instance == this) Instance = null;
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (IsSkipScene(scene.name)) return;
        FindCursorInScene(scene);
    }

    void FindCursorInCurrentScene()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (IsSkipScene(scene.name)) return;
        FindCursorInScene(scene);
    }

    bool IsSkipScene(string sceneName)
    {
        if (skipScenes == null) return false;
        foreach (var skip in skipScenes)
        {
            if (skip == sceneName) return true;
        }
        return false;
    }

    void FindCursorInScene(Scene scene)
    {
        if (!scene.IsValid()) return;

        GameObject[] roots = scene.GetRootGameObjects();
        foreach (var root in roots)
        {
            Transform found = FindByTagRecursive(root.transform, cursorTag);
            if (found != null)
            {
                SetCursor(found);
                if (showDebugLog)
                    Debug.Log($"🖱️ 找到鼠标物体: {scene.name} / {found.name}");
                return;
            }
        }

        SetCursor(null);
        if (showDebugLog)
            Debug.Log($"🖱️ 场景 {scene.name} 没鼠标物体,用系统鼠标");
    }

    Transform FindByTagRecursive(Transform parent, string tag)
    {
        if (parent.CompareTag(tag)) return parent;

        foreach (Transform child in parent)
        {
            Transform found = FindByTagRecursive(child, tag);
            if (found != null) return found;
        }
        return null;
    }

    public void SetCursor(Transform newCursor)
    {
        if (cursorObject != null)
        {
            cursorObject.gameObject.SetActive(false);
        }

        cursorObject = newCursor;

        if (cursorObject != null)
        {
            cursorObject.gameObject.SetActive(true);
            cursorObject.position = MouseWorldPos;
            Cursor.visible = false;
        }
        else
        {
            Cursor.visible = true;
        }
    }

    /// <summary>
    /// 获取当前鼠标物体 Transform(给其他系统用,如 QtipController)
    /// </summary>
    public Transform GetCursorTransform()
    {
        return cursorObject;
    }

    /// <summary>
    /// 获取 Qtip 当前位置(给 BoogerController 等用)
    /// </summary>
    public Vector3 GetQtipPosition()
    {
        return cursorObject != null ? cursorObject.position : Vector3.zero;
    }

    void Update()
    {
        UpdateCursor();
    }

    void UpdateCursor()
    {
        if (cursorObject == null) return;

        cursorObject.position = Vector3.Lerp(
            cursorObject.position,
            MouseWorldPos,
            moveSpeed * Time.deltaTime
        );
    }
}