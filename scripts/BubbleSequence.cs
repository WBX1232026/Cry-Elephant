using System.Collections;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// Pops bubbles one by one (Page 3 friend suggestions / Page 4 elephant lines).
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
