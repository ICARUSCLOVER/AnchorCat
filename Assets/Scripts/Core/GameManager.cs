using UnityEngine;
using UnityEngine.SceneManagement;
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("游戏状态")]
    public GameState state = GameState.MainMenu;

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
        if (SceneManager.GetActiveScene().name == "Persistent")
        {
            if (SceneController.Instance != null)
                SceneController.Instance.SwitchToScene("MainMenu");
        }
    }

    // ========== 状态切换(被 GameStateJudge 调用) ==========
    public void OnSuccess()
    {
        if (state == GameState.Success) return;
        state = GameState.Success;
        Debug.Log("[GameManager] 通关!");
    }

    public void OnGameOver()
    {
        if (state == GameState.GameOver) return;
        state = GameState.GameOver;
        Debug.Log("[GameManager] 失败!");
    }

    // ========== 状态查询 ==========
    public bool IsPlaying() => state == GameState.Playing;
    public bool IsGameOver() => state == GameState.GameOver;
    public bool IsSuccess() => state == GameState.Success;
    public bool IsMainMenu() => state == GameState.MainMenu;

    // ========== 场景切换 ==========
    public void StartGame()
    {
        state = GameState.Playing;
        if (GameplayData.Instance != null)
            GameplayData.Instance.ResetData();
        if (GameStateJudge.Instance != null)
            GameStateJudge.Instance.ResetSubState();
        if (SceneController.Instance != null)
            SceneController.Instance.SwitchToScene("MainGame");
    }
    
    public void Restart()
    {
        state = GameState.Playing;
        if (GameplayData.Instance != null)
            GameplayData.Instance.ResetData();
        if (GameStateJudge.Instance != null)
            GameStateJudge.Instance.ResetSubState();
        if (SceneController.Instance != null)
            SceneController.Instance.SwitchToScene("MainGame");
    }

    public void BackToMenu()
    {
        state = GameState.MainMenu;
        if (SceneController.Instance != null)
            SceneController.Instance.SwitchToScene("MainMenu");
    }

    public void GoToSettings()
    {
        if (SceneController.Instance != null)
            SceneController.Instance.SwitchToScene("Settings");
    }
}