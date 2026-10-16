using UnityEngine;

/// <summary>
/// Global SFX player (persistent across scenes) for one-shot sounds such as
/// clicks, balloon pops and narration.
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

    /// <summary>Exclusive playback: stops the current sound first (used for narration so pages never overlap).</summary>
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
