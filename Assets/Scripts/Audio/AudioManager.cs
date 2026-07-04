using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("音乐")]
    public AudioSource bgmSource;
    public AudioClip[] bgmClips;
    public float bgmVolume = 0.5f;

    [Header("音效")]
    public AudioSource sfxSource;
    public AudioClip[] sfxClips;
    public float sfxVolume = 0.7f;

    [Header("全局控制")]
    [Range(0f, 1f)] public float masterVolume = 1f;  // 总音量
    public bool isMuted = false;                       // 静音

    private int lastClipIndex = -1;

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
        PlayRandomBGM();
    }

    // ==================== 全局静音 ====================

    /// <summary>
    /// 切换静音状态
    /// </summary>
    public void ToggleMute()
    {
        isMuted = !isMuted;
        ApplyAudioSettings();
    }

    /// <summary>
    /// 设置静音
    /// </summary>
    public void SetMute(bool mute)
    {
        isMuted = mute;
        ApplyAudioSettings();
    }

    // ==================== 总音量 ====================

    /// <summary>
    /// 设置总音量(0-1)
    /// </summary>
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        ApplyAudioSettings();
    }

    /// <summary>
    /// 设置 BGM 音量(0-1)
    /// </summary>
    public void SetBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        ApplyAudioSettings();
    }

    /// <summary>
    /// 设置 SFX 音量(0-1)
    /// </summary>
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        ApplyAudioSettings();
    }

    /// <summary>
    /// 应用所有音频设置
    /// </summary>
    void ApplyAudioSettings()
    {
        if (isMuted)
        {
            // 静音
            if (bgmSource != null) bgmSource.volume = 0;
            if (sfxSource != null) sfxSource.volume = 0;
        }
        else
        {
            // 应用音量
            if (bgmSource != null) bgmSource.volume = bgmVolume * masterVolume;
            if (sfxSource != null) sfxSource.volume = sfxVolume * masterVolume;
        }
    }

    // ==================== BGM ====================

    public void PlayRandomBGM()
    {
        if (bgmSource == null || bgmClips == null || bgmClips.Length == 0) return;

        int index = Random.Range(0, bgmClips.Length);
        if (bgmClips.Length > 1 && index == lastClipIndex)
        {
            index = (index + 1) % bgmClips.Length;
        }
        lastClipIndex = index;

        bgmSource.clip = bgmClips[index];
        bgmSource.loop = true;
        bgmSource.Play();
        ApplyAudioSettings();  // ⬅ 应用音量
    }

    public void PlayBGM(int index)
    {
        if (bgmSource == null || bgmClips == null) return;
        if (index < 0 || index >= bgmClips.Length) return;

        bgmSource.clip = bgmClips[index];
        bgmSource.loop = true;
        bgmSource.Play();
        ApplyAudioSettings();  // ⬅ 应用音量
    }

    public void NextBGM()
    {
        if (bgmClips == null || bgmClips.Length == 0) return;
        bgmSource.Stop();
        PlayRandomBGM();
    }

    public void StopBGM()
    {
        if (bgmSource != null) bgmSource.Stop();
    }

    public void PauseBGM()
    {
        if (bgmSource != null) bgmSource.Pause();
    }

    public void ResumeBGM()
    {
        if (bgmSource != null) bgmSource.UnPause();
    }

    // ==================== SFX ====================

    public void PlaySFX(int index)
    {
        if (sfxSource == null || sfxClips == null) return;
        if (index < 0 || index >= sfxClips.Length) return;

        // ⬅ 应用音量
        float volume = isMuted ? 0 : sfxVolume * masterVolume;
        sfxSource.PlayOneShot(sfxClips[index], volume);
    }

    public void PlaySFX(AudioClip clip)
    {
        if (sfxSource == null || clip == null) return;

        // ⬅ 应用音量
        float volume = isMuted ? 0 : sfxVolume * masterVolume;
        sfxSource.PlayOneShot(clip, volume);
    }
}