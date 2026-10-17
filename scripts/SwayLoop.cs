using DG.Tweening;
using UnityEngine;

/// <summary>
/// Gentle side-to-side sway for decorative plants (leaves, flowers, grass).
/// Used to fill empty areas of the pages with subtle life, without distracting
/// from the interactive elements.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class SwayLoop : MonoBehaviour
{
    [SerializeField] private float angle = 6f;
    [SerializeField] private float duration = 1.8f;
    [SerializeField] private float delay = 0f;

    private RectTransform rect;
    private Tween tween;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        if (rect == null) rect = GetComponent<RectTransform>();
        tween = rect.DOLocalRotate(new Vector3(0f, 0f, angle), duration)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine)
            .SetDelay(delay);
    }

    private void OnDisable()
    {
        if (tween != null) tween.Kill();
        if (rect != null) rect.localRotation = Quaternion.identity;
    }
}
