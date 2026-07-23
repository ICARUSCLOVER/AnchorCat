using UnityEngine;

public class MenuController : MonoBehaviour
{
    public void StartGame()    { if (GameManager.Instance != null) GameManager.Instance.StartGame(); }
    public void BackToMenu()   { if (GameManager.Instance != null) GameManager.Instance.BackToMenu(); }
    public void GoToSettings() { if (GameManager.Instance != null) GameManager.Instance.GoToSettings(); }
    public void RestartGame()  { if (GameManager.Instance != null) GameManager.Instance.Restart(); }
    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}