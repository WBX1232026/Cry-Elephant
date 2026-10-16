using UnityEngine;

/// <summary>
/// Global background music: persistent (DontDestroyOnLoad) singleton that plays
/// continuously across scenes and loops automatically.
/// Two ways to provide the music:
///   1) assign bgmClip in the Inspector, or
///   2) put bgm_happy.mp3 in Assets/Resources so it loads at runtime (no scene edits).
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
        bgmSource.loop = true;      // loop the background music
        bgmSource.volume = volume;
    }

    private void Start()
    {
        // Fall back to Resources auto-load when no clip is assigned in the Inspector
        if (bgmClip == null && !string.IsNullOrEmpty(resourcesName))
            bgmClip = Resources.Load<AudioClip>(resourcesName);

        Play(bgmClip);
    }

    /// <summary>Current effective volume (used by the narrator to duck/restore).</summary>
    public float Volume => bgmSource != null ? bgmSource.volume : volume;

    public void SetVolume(float v)
    {
        if (bgmSource != null) bgmSource.volume = v;
    }

    public void Play(AudioClip clip)
    {
        if (clip == null || bgmSource == null) return;

        // Do not interrupt the same track that is already playing, so it stays continuous across scenes
        if (bgmSource.isPlaying && bgmSource.clip == clip) return;

        bgmSource.Stop();
        bgmSource.clip = clip;
        bgmSource.Play();
    }
}
