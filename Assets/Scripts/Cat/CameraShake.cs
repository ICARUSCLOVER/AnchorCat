using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;

    private Camera cam;
    private Vector3 originalPos;
    private float intensity = 0.2f;
    private float duration = 0.3f;
    private float timer = 0f;
    private bool shaking = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        cam = GetComponent<Camera>();
        if (cam == null) cam = Camera.main;
        originalPos = transform.localPosition;
    }

    public void Shake()
    {
        StartShake(intensity, duration);
    }

    public void Shake(float shakeIntensity, float shakeDuration)
    {
        StartShake(shakeIntensity, shakeDuration);
    }

    void StartShake(float shakeIntensity, float shakeDuration)
    {
        intensity = shakeIntensity;
        duration = shakeDuration;
        timer = shakeDuration;
        shaking = true;
    }

    void LateUpdate()
    {
        if (!shaking) return;

        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            shaking = false;
            transform.localPosition = originalPos;
            return;
        }

        float t = timer / duration;
        Vector3 offset = Random.insideUnitSphere * intensity * t;
        offset.z = 0;
        transform.localPosition = originalPos + offset;
    }
}