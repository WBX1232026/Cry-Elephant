using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Button press feedback: darkens and shrinks to pressScale.
/// Follows Norman's affordance and Nielsen's visibility of system status.
/// Shares the same RectTransform with the PulseLoop breathing animation, so the pulse pauses while pressed and resumes on release, preventing the two tweens from fighting over scale.
/// </summary>
public class PressFeedback : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private float darken = 0.85f;
    [SerializeField] private float pressScale = 0.95f;

    private Graphic graphic;
    private Color baseColor;
    private RectTransform rect;
    private PulseLoop pulse;

    private void Awake()
    {
        graphic = GetComponent<Graphic>();
        if (graphic != null) baseColor = graphic.color;
        rect = transform as RectTransform;
        pulse = GetComponent<PulseLoop>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        ApplyColor(darken);

        if (pulse != null) pulse.enabled = false;   // triggers OnDisable, which resets the scale to the baseline
        if (rect != null) rect.DOScale(Vector3.one * pressScale, 0.08f).SetEase(Ease.OutQuad);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        ApplyColor(1f);

        if (rect != null)
        {
            rect.DOScale(Vector3.one, 0.12f).SetEase(Ease.OutQuad)
                .OnComplete(() => { if (pulse != null) pulse.enabled = true; });
        }
        else if (pulse != null)
        {
            pulse.enabled = true;
        }
    }

    private void ApplyColor(float factor)
    {
        if (graphic == null) return;
        graphic.color = new Color(baseColor.r * factor, baseColor.g * factor, baseColor.b * factor, baseColor.a);
    }
}
