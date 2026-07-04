using UnityEngine;

public class BoogerTipTrigger : MonoBehaviour
{
    [Header("鼻涕控制器")]
    public BoogerController boogerController;

    [Header("调试")]
    public bool showDebugLog = true;

    void Start()
    {
        if (showDebugLog) Debug.Log($"🎯 BoogerTipTrigger Start - boogerController: {(boogerController != null ? boogerController.name : "NULL")}");
    }

    void LateUpdate()
    {
        if (boogerController == null)
        {
            if (showDebugLog && Time.frameCount % 60 == 0) 
                Debug.LogWarning("⚠️ BoogerController 是 null!");
            return;
        }

        Vector3 newPos = boogerController.GetBoogerTip();
        transform.position = newPos;

        if (showDebugLog && Time.frameCount % 60 == 0)
        {
            Debug.Log($"🎯 Trigger 位置: {newPos}");
        }
    }
}