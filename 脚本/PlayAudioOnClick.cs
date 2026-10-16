using UnityEngine;
using UnityEngine.UI;

public class PlayAudioOnClick : MonoBehaviour
{
    [SerializeField] private AudioClip clip;

    private AudioSource source;

    private void Awake()
    {
        source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;

        var btn = GetComponent<Button>();
        if (btn != null)
            btn.onClick.AddListener(Play);
    }

    public void Play()
    {
        if (clip != null)
            source.PlayOneShot(clip);
    }
}
