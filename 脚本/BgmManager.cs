using UnityEngine;

/// <summary>
/// 全局背景音乐：常驻（DontDestroyOnLoad）单例，跨场景连续播放、自动循环。
/// 音乐来源二选一：
///   1) Inspector 里指定 bgmClip；
///   2) 放到 Assets/Resources/bgm_happy.mp3，运行时自动加载（无需改场景）。
/// </summary>
public class BgmManager : MonoBehaviour
{
    public static BgmManager Instance { get; private set; }

    [SerializeField] private AudioClip bgmClip;
    [SerializeField, Range(0f, 1f)] private float volume = 0.45f;
    [SerializeField] private string resourcesName = "bgm_happy";

    private AudioSource bgmSource;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.playOnAwake = false;
        bgmSource.loop = true;      // 背景音乐循环
        bgmSource.volume = volume;
    }

    private void Start()
    {
        // 未在 Inspector 指定时，尝试从 Resources 自动加载
        if (bgmClip == null && !string.IsNullOrEmpty(resourcesName))
            bgmClip = Resources.Load<AudioClip>(resourcesName);

        Play(bgmClip);
    }

    /// <summary>当前实际音量（供旁白压低/恢复）。</summary>
    public float Volume => bgmSource != null ? bgmSource.volume : volume;

    public void SetVolume(float v)
    {
        if (bgmSource != null) bgmSource.volume = v;
    }

    public void Play(AudioClip clip)
    {
        if (clip == null || bgmSource == null) return;

        // 同一首已在播放时不打断，保证跨场景连续
        if (bgmSource.isPlaying && bgmSource.clip == clip) return;

        bgmSource.Stop();
        bgmSource.clip = clip;
        bgmSource.Play();
    }
}
