using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game State")]
    public GameState state = GameState.MainMenu;

    [Header("Tag Config")]
    public string resultPanelTag = "ResultPanel";

    [Header("Debug")]
    public bool showDebugLog = true;

    private ResultPanel cachedResultPanel;

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
        if (showDebugLog) Debug.Log($"🔄 场景加载: {scene.name}");
        // ⬅ 场景变了,缓存失效
        cachedResultPanel = null;
    }

    void Start()
    {
    }

    public void OnSuccess()
    {
        if (state == GameState.Success) return;
        state = GameState.Success;
        if (showDebugLog) Debug.Log($"[OnSuccess] state=Success");

        ResultPanel panel = GetResultPanel();
        if (panel == null)
        {
            if (showDebugLog) Debug.LogWarning($"[OnSuccess] ResultPanel 找不到");
            return;
        }

        string msg = BuildSuccessMessage();
        if (showDebugLog) Debug.Log($"[OnSuccess] msg='{msg}'");
        panel.ShowSuccess(msg);
    }

    public void OnGameOver()
    {
        if (state == GameState.GameOver) return;
        state = GameState.GameOver;
        if (showDebugLog) Debug.Log($"[OnGameOver] state=GameOver");

        ResultPanel panel = GetResultPanel();
        if (panel == null)
        {
            if (showDebugLog) Debug.LogWarning($"[OnGameOver] ResultPanel 找不到");
            return;
        }

        string msg = BuildFailureMessage();
        if (showDebugLog) Debug.Log($"[OnGameOver] msg='{msg}'");
        panel.ShowFailure(msg);
    }

    ResultPanel GetResultPanel()
    {
        // ⬅ 缓存有效 + 物体存活
        if (cachedResultPanel != null &&
            cachedResultPanel.gameObject != null &&
            cachedResultPanel.gameObject.activeInHierarchy)
            return cachedResultPanel;

        // ⬅ 重新找
        GameObject found = GameObject.FindGameObjectWithTag(resultPanelTag);
        if (found == null)
        {
            Debug.LogWarning($"⚠️ Tag='{resultPanelTag}' 物体未找到");
            return null;
        }

        cachedResultPanel = found.GetComponent<ResultPanel>();
        return cachedResultPanel;
    }

    string BuildSuccessMessage()
    {
        if (GameplayData.Instance == null) return "Cleared!";

        float total = GameplayData.Instance.totalTime;
        float remaining = GameplayData.Instance.currentTime;
        float timeUsed = Mathf.Max(0f, total - remaining);

        return $"Time: {timeUsed:F1}s";
    }

    string BuildFailureMessage()
    {
        if (GameplayData.Instance == null) return "Try Again!";
        if (GameplayData.Instance.currentTime <= 0) return "Time Up!";
        if (GameplayData.Instance.dangerTime >= 2.5f) return "Too Fast!";
        return "Try Again!";
    }

    public bool IsPlaying() => state == GameState.Playing;
    public bool IsGameOver() => state == GameState.GameOver;
    public bool IsSuccess() => state == GameState.Success;
    public bool IsMainMenu() => state == GameState.MainMenu;

    public void StartGame()
    {
        state = GameState.Playing;
        ResetGameData();
        ResetDangerLimit();

        if (SceneController.Instance != null)
            SceneController.Instance.SwitchToScene("MainGame");
    }

    public void Restart()
    {
        if (showDebugLog) Debug.Log($"[Restart] 开始, before state={state}");
        state = GameState.Playing;
        ResetGameData();
        ResetDangerLimit();
        HideResultPanel();

        if (GameStateJudge.Instance != null)
        {
            GameStateJudge.Instance.SetSubState(GameSubState.Intro);
            if (showDebugLog) Debug.Log($"[Restart] SetSubState(Intro), subState={GameStateJudge.Instance.subState}");
        }
        else
        {
            Debug.LogError("[Restart] GameStateJudge.Instance 是 NULL!");
        }

        if (showDebugLog) Debug.Log("[Restart] 完成");
    }

    public void RestartGame() => Restart();

    public void BackToMenu()
    {
        state = GameState.MainMenu;
        HideResultPanel();

        if (SceneController.Instance != null)
            SceneController.Instance.SwitchToScene("MainMenu");
    }

    public void GoToSettings()
    {
        if (SceneController.Instance != null)
            SceneController.Instance.SwitchToScene("Settings");
    }

    void ResetGameData()
    {
        if (GameplayData.Instance != null)
        {
            GameplayData.Instance.ResetData();
            if (showDebugLog)
                Debug.Log($"[ResetGameData] GameplayData: length={GameplayData.Instance.currentBoogerLength}, " +
                          $"time={GameplayData.Instance.currentTime}");
        }

        if (GameStateJudge.Instance != null)
        {
            GameStateJudge.Instance.ResetSubState();
            if (showDebugLog) Debug.Log($"[ResetGameData] GameStateJudge.subState={GameStateJudge.Instance.subState}");
        }
    }

    // ⬅ dangerLimit 在 Reset 时恢复初始值
    void ResetDangerLimit()
    {
        if (GameStateJudge.Instance != null)
        {
            GameStateJudge.Instance.dangerLimit = 2.5f;
            if (showDebugLog) Debug.Log($"[ResetDangerLimit] dangerLimit=2.5");
        }
    }

    // ⬅ 改:用 panel.Hide(),不用 SetActive(false)
    void HideResultPanel()
    {
        ResultPanel panel = GetResultPanel();
        if (panel != null)
        {
            panel.Hide();
            if (showDebugLog) Debug.Log($"[HideResultPanel] panel.Hide()");
        }
        else
        {
            Debug.LogWarning($"[HideResultPanel] panel 找不到,直接用 GameObject tag");
            GameObject panelObj = GameObject.FindGameObjectWithTag(resultPanelTag);
            if (panelObj != null)
            {
                CanvasGroup cg = panelObj.GetComponent<CanvasGroup>();
                if (cg == null) cg = panelObj.AddComponent<CanvasGroup>();
                cg.alpha = 0f;
                cg.interactable = false;
                cg.blocksRaycasts = false;
            }
        }
    }
}