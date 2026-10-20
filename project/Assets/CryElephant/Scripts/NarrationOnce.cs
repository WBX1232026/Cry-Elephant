using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Plays the page narration every time the scene is entered, after a short
/// delay. The background music ducks while the voice speaks.
/// </summary>
public class NarrationOnce : MonoBehaviour
{
    [SerializeField] private AudioClip clip;
    [SerializeField] private float delay = 0.6f;

    private void Start()
    {
        if (clip == null) return;
        Invoke(nameof(Play), delay);
    }

    private void Play()
    {
        // Exclusive playback: stop any narration still playing from the previous page so voices never stack
        if (Sfx.Instance != null) Sfx.Instance.PlayExclusive(clip);
        StartCoroutine(DuckBgm());
    }

    /// <summary>Ducks the background music while the narration plays, then restores it.</summary>
    private IEnumerator DuckBgm()
    {
        if (clip == null || BgmManager.Instance == null) yield break;

        float original = BgmManager.Instance.Volume;
        BgmManager.Instance.SetVolume(0.08f);
        yield return new WaitForSeconds(clip.length + 0.3f);
        BgmManager.Instance.SetVolume(original);
    }
}
