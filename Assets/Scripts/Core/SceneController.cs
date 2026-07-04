using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneController : MonoBehaviour
{
    public static SceneController Instance;

    [Header("常驻场景")]
    public string persistentSceneName = "Persistent";

    [Header("启动场景")]
    public string firstScene = "MainMenu";

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
        Bootstrap();
    }

    public void Bootstrap()
    {
        if (SceneManager.GetActiveScene().name == persistentSceneName)
        {
            StartCoroutine(SwitchSceneRoutine(firstScene));
        }
    }

    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadSceneAdditive(sceneName));
    }

    public void SwitchToScene(string sceneName)
    {
        StartCoroutine(SwitchSceneRoutine(sceneName));
    }

    public void UnloadScene(string sceneName)
    {
        StartCoroutine(UnloadSceneRoutine(sceneName));
    }

    private IEnumerator LoadSceneAdditive(string sceneName)
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        if (op != null) yield return op;
    }

    private IEnumerator SwitchSceneRoutine(string sceneName)
    {
        // 1. 加载新场景
        AsyncOperation loadOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        if (loadOp != null) yield return loadOp;

        // 2. 卸载其他场景
        for (int i = SceneManager.sceneCount - 1; i >= 0; i--)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (scene.name != persistentSceneName && scene.name != sceneName && scene.isLoaded)
            {
                AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(scene);
                if (unloadOp != null) yield return unloadOp;
            }
        }
    }

    private IEnumerator UnloadSceneRoutine(string sceneName)
    {
        Scene scene = SceneManager.GetSceneByName(sceneName);
        if (scene.isLoaded)
        {
            AsyncOperation op = SceneManager.UnloadSceneAsync(scene);
            if (op != null) yield return op;
        }
    }
}