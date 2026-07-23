using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ResultPanel : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject panel;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI messageText;
    public Button retryButton;
    public Button menuButton;

    [Header("动画")]
    public float showAnimDuration = 0.3f;
    public float startScale = 0.7f;

    [Header("样式")]
    public Color successColor = new Color(0.2f, 0.8f, 0.2f);
    public Color failColor = new Color(0.9f, 0.2f, 0.2f);
    public Color defaultMessageColor = Color.white;

    void Start()
    {
        // ⬅ 启动时隐藏(不要让 panel 默认 active)
        if (panel != null) panel.SetActive(false);

        if (retryButton != null)
        {
            retryButton.onClick.RemoveAllListeners();
            retryButton.onClick.AddListener(OnRetryClicked);
        }

        if (menuButton != null)
        {
            menuButton.onClick.RemoveAllListeners();
            menuButton.onClick.AddListener(OnMenuClicked);
        }
    }

    public void ShowSuccess(string message = "")
    {
        string title = "Success!";
        string msg = string.IsNullOrEmpty(message) ? "Cleared!" : message;
        Show(title, successColor, msg);
    }

    public void ShowFailure(string message = "")
    {
        string title = "Failed";
        string msg = string.IsNullOrEmpty(message) ? "Try Again!" : message;
        Show(title, failColor, msg);
    }

    void Show(string title, Color titleColor, string message)
    {
        if (panel == null) return;

        // ⬅ 强制激活整个祖先链
        Transform t = panel.transform;
        while (t != null)
        {
            if (!t.gameObject.activeSelf)
                t.gameObject.SetActive(true);
            t = t.parent;
        }

        // 重置 transform
        panel.transform.localScale = Vector3.one;

        // 设内容
        if (titleText != null)
        {
            titleText.text = title;
            titleText.color = titleColor;
        }
        if (messageText != null)
        {
            messageText.text = message;
            messageText.color = defaultMessageColor;
        }

        // CanvasGroup 重置
        CanvasGroup cg = panel.GetComponent<CanvasGroup>();
        if (cg == null) cg = panel.AddComponent<CanvasGroup>();
        cg.alpha = 1f;
        cg.interactable = true;
        cg.blocksRaycasts = true;

        StopAllCoroutines();
        StartCoroutine(ShowAnim());
    }

    IEnumerator ShowAnim()
    {
        if (panel == null) yield break;

        Vector3 endScale = Vector3.one;
        Vector3 startScaleVec = Vector3.one * startScale;
        panel.transform.localScale = startScaleVec;

        CanvasGroup cg = panel.GetComponent<CanvasGroup>();
        if (cg == null) cg = panel.AddComponent<CanvasGroup>();
        cg.alpha = 0f;

        float elapsed = 0f;
        while (elapsed < showAnimDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / showAnimDuration);
            panel.transform.localScale = Vector3.Lerp(startScaleVec, endScale, t);
            cg.alpha = t;
            yield return null;
        }

        panel.transform.localScale = endScale;
        cg.alpha = 1f;
    }

    // ⬅ 用 CanvasGroup alpha 隐藏,不用 SetActive(false)
    public void Hide()
    {
        if (panel == null) return;

        CanvasGroup cg = panel.GetComponent<CanvasGroup>();
        if (cg == null) cg = panel.AddComponent<CanvasGroup>();
        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;

        StopAllCoroutines();
    }

    void OnRetryClicked()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.Restart();
    }

    void OnMenuClicked()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.BackToMenu();
    }
}