using System.Collections;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// 气泡逐个弹出（Page 3 朋友建议 / Page 4 小象台词）。
/// </summary>
public class BubbleSequence : MonoBehaviour
{
    [SerializeField] private RectTransform[] bubbles;
    [SerializeField] private float startDelay = 0.4f;
    [SerializeField] private float interval = 1.0f;
    [SerializeField] private float popDuration = 0.35f;

    private void Awake()
    {
        HideAll();
    }

    private void OnEnable()
    {
        StopAllCoroutines();
        StartCoroutine(Run());
    }

    private void HideAll()
    {
        if (bubbles == null) return;
        foreach (RectTransform b in bubbles)
        {
            if (b != null) b.localScale = Vector3.zero;
        }
    }

    private IEnumerator Run()
    {
        HideAll();
        yield return new WaitForSeconds(startDelay);

        if (bubbles == null) yield break;

        foreach (RectTransform b in bubbles)
        {
            if (b == null) continue;
            b.localScale = Vector3.zero;
            b.DOScale(Vector3.one, popDuration).SetEase(Ease.OutBack);
            yield return new WaitForSeconds(interval);
        }
    }
}
