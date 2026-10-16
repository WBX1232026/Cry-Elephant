using UnityEngine;

/// <summary>
/// 全局音效播放器（常驻场景，用于点击音、气球声、旁白等一次性音效）。
/// </summary>
public class Sfx : MonoBehaviour
{
    public static Sfx Instance { get; private set; }

    [SerializeField] private AudioClip clickClip;
    [SerializeField] private AudioClip balloonPopClip;

    private AudioSource source;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
    }

    public void Play(AudioClip clip, float volume = 1f)
    {
        if (clip == null || source == null) return;
        source.PlayOneShot(clip, volume);
    }

    /// <summary>独占播放：先停掉当前音效再播（用于旁白，避免跨场景叠加）。</summary>
    public void PlayExclusive(AudioClip clip, float volume = 1f)
    {
        if (clip == null || source == null) return;
        source.Stop();
        source.PlayOneShot(clip, volume);
    }

    public void PlayClick()
    {
        Play(clickClip);
    }

    public void PlayBalloonPop()
    {
        Play(balloonPopClip != null ? balloonPopClip : clickClip);
    }
}
