using DG.Tweening;
using UnityEngine;

/// <summary>
/// Breathing pulse animation, used on all buttons and bubbles to hint "tap here".
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class PulseLoop : MonoBehaviour
{
    [SerializeField] private float scaleTo = 1.05f;
    [SerializeField] private float duration = 0.8f;
    [SerializeField] private float delay = 0f;

    private RectTransform rect;
    private Vector3 baseScale;
    private Tween tween;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        baseScale = rect.localScale;
    }

    private void OnEnable()
    {
        StartPulse();
    }

    private void OnDisable()
    {
        if (tween != null) tween.Kill();
        if (rect != null) rect.localScale = baseScale;
    }

    private void StartPulse()
    {
        if (rect == null) rect = GetComponent<RectTransform>();
        rect.localScale = baseScale;
        tween = rect.DOScale(baseScale * scaleTo, duration)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine)
            .SetDelay(delay);
    }
}
