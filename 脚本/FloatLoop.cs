using DG.Tweening;
using UnityEngine;

/// <summary>
/// 上下浮动动画：用于气球、气泡、手指图标。
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class FloatLoop : MonoBehaviour
{
    [SerializeField] private float distance = 24f;
    [SerializeField] private float duration = 1.2f;
    [SerializeField] private float delay = 0f;

    private RectTransform rect;
    private Vector2 basePos;
    private Tween tween;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        basePos = rect.anchoredPosition;
    }

    private void OnEnable()
    {
        StartFloat();
    }

    private void OnDisable()
    {
        if (tween != null) tween.Kill();
        if (rect != null) rect.anchoredPosition = basePos;
    }

    private void StartFloat()
    {
        if (rect == null) rect = GetComponent<RectTransform>();
        rect.anchoredPosition = basePos;
        tween = rect.DOAnchorPos(basePos + new Vector2(0f, distance), duration)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine)
            .SetDelay(delay);
    }
}
