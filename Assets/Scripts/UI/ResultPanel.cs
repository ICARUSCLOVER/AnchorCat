using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResultPanel : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject panel;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI messageText;
    public Button retryButton;
    public Button menuButton;

    [Header("Debug")]
    public bool showDebugLog = true;

    void Start()
    {
        if (panel == null) panel = gameObject;
        if (panel != null) panel.SetActive(false);

        if (retryButton != null)
            retryButton.onClick.AddListener(OnRetryClicked);

        if (menuButton != null)
            menuButton.onClick.AddListener(OnMenuClicked);

        if (showDebugLog)
            Debug.Log($"✅ ResultPanel 启动: {gameObject.name}");
    }

    public void ShowSuccess(string message = "")
    {
        if (panel == null) return;
        panel.SetActive(true);
        if (showDebugLog) Debug.Log($"✅ 显示成功: {message}");

        if (titleText != null)
        {
            titleText.text = "Success!";
            titleText.color = new Color(0.2f, 0.8f, 0.2f);
        }

        if (messageText != null)
        {
            messageText.text = string.IsNullOrEmpty(message) ? "Cleared!" : message;
        }
    }

    public void ShowFailure(string message = "")
    {
        if (panel == null) return;
        panel.SetActive(true);
        if (showDebugLog) Debug.Log($"❌ 显示失败: {message}");

        if (titleText != null)
        {
            titleText.text = "Failed";
            titleText.color = new Color(0.9f, 0.2f, 0.2f);
        }

        if (messageText != null)
        {
            messageText.text = string.IsNullOrEmpty(message) ? "Try Again!" : message;
        }
    }

    public void Hide()
    {
        if (panel != null) panel.SetActive(false);
    }

    void OnRetryClicked()
    {
        if (showDebugLog) Debug.Log("🔄 点重试");
        GameManager.Instance.Restart();
    }

    void OnMenuClicked()
    {
        if (showDebugLog) Debug.Log("📋 点主菜单");
        GameManager.Instance.BackToMenu();
    }
}