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

    void Start()
    {
        if (panel != null) panel.SetActive(false);

        if (retryButton != null)
            retryButton.onClick.AddListener(OnRetryClicked);

        if (menuButton != null)
            menuButton.onClick.AddListener(OnMenuClicked);
    }

    public void ShowSuccess(string message = "")
    {
        if (panel == null) return;
        panel.SetActive(true);

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
        GameManager.Instance.Restart();
    }

    void OnMenuClicked()
    {
        GameManager.Instance.BackToMenu();
    }
}