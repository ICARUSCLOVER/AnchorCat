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
    }

    void Start()
    {
        // SceneController 负责加载 MainMenu
    }

    public void OnSuccess()
    {
        if (state == GameState.Success) return;
        state = GameState.Success;

        ResultPanel panel = GetResultPanel();
        if (panel != null) panel.ShowSuccess(BuildSuccessMessage());
    }

    public void OnGameOver()
    {
        if (state == GameState.GameOver) return;
        state = GameState.GameOver;

        ResultPanel panel = GetResultPanel();
        if (panel != null) panel.ShowFailure(BuildFailureMessage());
    }

    ResultPanel GetResultPanel()
    {
        GameObject found = GameObject.FindGameObjectWithTag(resultPanelTag);
        if (found != null) return found.GetComponent<ResultPanel>();
        return null;
    }

    string BuildSuccessMessage()
    {
        float timeUsed = 60f;
        if (GameplayData.Instance != null)
            timeUsed = 60f - GameplayData.Instance.currentTime;
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

        if (SceneController.Instance != null)
            SceneController.Instance.SwitchToScene("MainGame");
    }

    public void Restart()
    {
        state = GameState.Playing;
        ResetGameData();

        GameObject panelObj = GameObject.FindGameObjectWithTag(resultPanelTag);
        if (panelObj != null) panelObj.SetActive(false);

        if (SceneController.Instance != null)
            SceneController.Instance.SwitchToScene("MainGame");
    }

    public void RestartGame() => Restart();

    public void BackToMenu()
    {
        state = GameState.MainMenu;

        GameObject panelObj = GameObject.FindGameObjectWithTag(resultPanelTag);
        if (panelObj != null) panelObj.SetActive(false);

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
        if (GameplayData.Instance != null) GameplayData.Instance.ResetData();
        if (GameStateJudge.Instance != null) GameStateJudge.Instance.ResetSubState();
    }
}