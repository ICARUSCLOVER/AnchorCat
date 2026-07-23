using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("音乐")]
    public AudioSource bgmSource;
    public AudioClip[] bgmClips;  // ⬅ 多个
    public float bgmVolume = 0.5f;

    [Header("音效")]
    public AudioSource sfxSource;
    public AudioClip[] sfxClips;
    public float sfxVolume = 0.7f;

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

    /// <summary>
    /// 随机播放一首 BGM
    /// </summary>
    public void PlayRandomBGM()
    {
        if (bgmSource == null || bgmClips == null || bgmClips.Length == 0) return;

        int index = Random.Range(0, bgmClips.Length);
        
        // 避免重复同一首
        if (bgmClips.Length > 1 && index == lastClipIndex)
        {
            index = (index + 1) % bgmClips.Length;
        }

        lastClipIndex = index;

        bgmSource.clip = bgmClips[index];
        bgmSource.loop = true;
        bgmSource.volume = bgmVolume;
        bgmSource.Play();
    }

    /// <summary>
    /// 下一首
    /// </summary>
    public void NextBGM()
    {
        if (bgmClips == null || bgmClips.Length == 0) return;
        
        bgmSource.Stop();
        PlayRandomBGM();
    }

    /// <summary>
    /// 播放指定索引
    /// </summary>
    public void PlayBGM(int index)
    {
        if (bgmSource == null || bgmClips == null) return;
        if (index < 0 || index >= bgmClips.Length) return;

        bgmSource.clip = bgmClips[index];
        bgmSource.loop = true;
        bgmSource.volume = bgmVolume;
        bgmSource.Play();
    }

    /// <summary>
    /// 停止 BGM
    /// </summary>
    public void StopBGM()
    {
        if (bgmSource != null) bgmSource.Stop();
    }

    /// <summary>
    /// 暂停 BGM
    /// </summary>
    public void PauseBGM()
    {
        if (bgmSource != null) bgmSource.Pause();
    }

    /// <summary>
    /// 恢复 BGM
    /// </summary>
    public void ResumeBGM()
    {
        if (bgmSource != null) bgmSource.UnPause();
    }

    /// <summary>
    /// 设置音量
    /// </summary>
    public void SetBGMVolume(float volume)
    {
        if (bgmSource != null) bgmSource.volume = volume;
    }

    // ========== 音效 ==========
    public void PlaySFX(int index)
    {
        if (sfxSource == null || sfxClips == null) return;
        if (index < 0 || index >= sfxClips.Length) return;

        sfxSource.PlayOneShot(sfxClips[index], sfxVolume);
    }

    public void PlaySFX(AudioClip clip)
    {
        if (sfxSource == null || clip == null) return;
        sfxSource.PlayOneShot(clip, sfxVolume);
    }
}