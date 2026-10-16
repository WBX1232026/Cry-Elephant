using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 按钮按下反馈：变暗 + 缩小到 pressScale。
/// 符合 Norman 可供性与 Nielsen 系统状态可见性。
/// 与 PulseLoop 呼吸动画共用同一 RectTransform，故按下时暂停呼吸、松开后恢复，避免两个补间抢 scale。
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

        if (pulse != null) pulse.enabled = false;   // 触发 OnDisable，scale 复位到基准
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
