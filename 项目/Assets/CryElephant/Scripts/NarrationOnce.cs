using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 每页第一次进入时播放旁白提示（只播一次）。
/// </summary>
public class NarrationOnce : MonoBehaviour
{
    private static readonly HashSet<string> PlayedKeys = new HashSet<string>();

    [SerializeField] private AudioClip clip;
    [SerializeField] private float delay = 0.6f;

    private void Start()
    {
        if (clip == null) return;

        string key = SceneManager.GetActiveScene().name + "|" + clip.name;
        if (PlayedKeys.Contains(key)) return;

        PlayedKeys.Add(key);
        Invoke(nameof(Play), delay);
    }

    private void Play()
    {
        // 独占播放：停掉上一页还没播完的旁白，避免声音叠加
        if (Sfx.Instance != null) Sfx.Instance.PlayExclusive(clip);
        StartCoroutine(DuckBgm());
    }

    /// <summary>旁白期间压低背景音乐，播完恢复。</summary>
    private IEnumerator DuckBgm()
    {
        if (clip == null || BgmManager.Instance == null) yield break;

        float original = BgmManager.Instance.Volume;
        BgmManager.Instance.SetVolume(0.08f);
        yield return new WaitForSeconds(clip.length + 0.3f);
        BgmManager.Instance.SetVolume(original);
    }
}
