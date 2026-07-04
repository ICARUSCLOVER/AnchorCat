using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneController : MonoBehaviour
{
    public static SceneController Instance;

    [Header("常驻场景")]
    public string persistentSceneName = "Persistent";

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
        // 启动时加载 MainMenu
        if (SceneManager.GetActiveScene().name == persistentSceneName)
        {
            StartCoroutine(LoadSceneAdditive("MainMenu"));
        }
    }

    /// <summary>
    /// 加载场景(Additive 模式)
    /// </summary>
    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadSceneAdditive(sceneName));
    }

    /// <summary>
    /// 切换场景:Additive 加载新场景 + 卸载其他场景
    /// </summary>
    public void SwitchToScene(string sceneName)
    {
        StartCoroutine(SwitchSceneRoutine(sceneName));
    }

    /// <summary>
    /// 卸载指定场景
    /// </summary>
    public void UnloadScene(string sceneName)
    {
        StartCoroutine(UnloadSceneRoutine(sceneName));
    }

    private IEnumerator LoadSceneAdditive(string sceneName)
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        yield return op;
    }

    private IEnumerator SwitchSceneRoutine(string sceneName)
    {
        // 1. Additive 加载新场景
        AsyncOperation loadOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        yield return loadOp;

        // 2. 卸载除 Persistent 和新场景外的所有场景
        for (int i = SceneManager.sceneCount - 1; i >= 0; i--)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (scene.name != persistentSceneName && scene.name != sceneName && scene.isLoaded)
            {
                AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(scene);
                if (unloadOp != null)
                    yield return unloadOp;
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