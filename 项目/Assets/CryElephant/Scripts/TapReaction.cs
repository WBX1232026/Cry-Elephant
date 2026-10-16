using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// 通用可点击道具：点击后播放音效 + 播放一种 DOTween 反应动画。
/// 用于气球、小象、猴子、兔子、气泡等页面内对象。
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class TapReaction : MonoBehaviour, IPointerClickHandler
{
    public enum ReactionKind
    {
        Pop,        // 放大再回弹（气泡、气球）
        Bounce,     // 向上弹跳再回落（气球）
        FloatUp,    // 向上飘约 2 秒后回落（气球，Page 1）
        EarWiggle,  // 左右摇摆（小象耳朵）
        Jump,       // 跳一下（猴子）
        Shake,      // 左右抖动
        Nod,        // 点头（上下）
        Cry,        // 哭：下沉 + 抽动（小象，Page 3）
        Comfort,    // 安慰：轻轻侧身摆动（朋友，Page 3）
        FloatAway   // 飘走并淡出后隐藏（气球，Page 4）
    }

    [SerializeField] private ReactionKind kind = ReactionKind.Pop;
    [SerializeField] private AudioClip sound;
    [SerializeField] private float strength = 1f;

    private RectTransform rect;
    private Graphic graphic;
    private Vector2 basePos;
    private float baseAngle;
    private bool busy;
    private FloatLoop floatLoop;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        graphic = GetComponent<Graphic>();
        basePos = rect.anchoredPosition;
        baseAngle = rect.localEulerAngles.z;
        floatLoop = GetComponent<FloatLoop>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Trigger();
    }

    public void Trigger()
    {
        if (Sfx.Instance != null && sound != null) Sfx.Instance.Play(sound);
        if (busy) return;
        Play();
    }

    private void Play()
    {
        // 暂停同物体的上下浮动，避免两个补间抢 anchoredPosition
        if (floatLoop != null) floatLoop.enabled = false;

        rect.DOKill();
        rect.anchoredPosition = basePos;
        rect.localRotation = Quaternion.Euler(0f, 0f, baseAngle);
        rect.localScale = Vector3.one;

        Sequence seq = DOTween.Sequence();
        seq.SetTarget(rect);

        switch (kind)
        {
            case ReactionKind.Pop:
                seq.Append(rect.DOScale(Vector3.one * (1.15f * strength), 0.18f).SetEase(Ease.OutQuad));
                seq.Append(rect.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBack));
                break;

            case ReactionKind.Bounce:
                seq.Append(rect.DOAnchorPos(basePos + new Vector2(0f, 160f * strength), 0.45f).SetEase(Ease.OutQuad));
                seq.Append(rect.DOAnchorPos(basePos, 0.55f).SetEase(Ease.OutBounce));
                break;

            case ReactionKind.FloatUp:
                // 向上飘约 2 秒后回落
                seq.Append(rect.DOAnchorPos(basePos + new Vector2(0f, 360f * strength), 1.15f).SetEase(Ease.OutSine));
                seq.Append(rect.DOAnchorPos(basePos, 0.9f).SetEase(Ease.InQuad));
                break;

            case ReactionKind.EarWiggle:
                seq.Append(rect.DORotate(new Vector3(0f, 0f, 12f * strength), 0.12f));
                seq.Append(rect.DORotate(new Vector3(0f, 0f, -12f * strength), 0.18f));
                seq.Append(rect.DORotate(new Vector3(0f, 0f, 8f * strength), 0.14f));
                seq.Append(rect.DORotate(new Vector3(0f, 0f, baseAngle), 0.12f));
                break;

            case ReactionKind.Jump:
                seq.Append(rect.DOAnchorPos(basePos + new Vector2(0f, 110f * strength), 0.25f).SetEase(Ease.OutQuad));
                seq.Append(rect.DOAnchorPos(basePos, 0.35f).SetEase(Ease.OutBounce));
                break;

            case ReactionKind.Shake:
                seq.Append(rect.DOAnchorPos(basePos + new Vector2(28f * strength, 0f), 0.08f));
                seq.Append(rect.DOAnchorPos(basePos - new Vector2(28f * strength, 0f), 0.12f));
                seq.Append(rect.DOAnchorPos(basePos + new Vector2(18f * strength, 0f), 0.09f));
                seq.Append(rect.DOAnchorPos(basePos, 0.08f));
                break;

            case ReactionKind.Nod:
                seq.Append(rect.DOAnchorPos(basePos - new Vector2(0f, 34f * strength), 0.16f));
                seq.Append(rect.DOAnchorPos(basePos, 0.24f).SetEase(Ease.OutBack));
                break;

            case ReactionKind.Cry:
                // 哭：先下沉，再左右抽动几下，最后回到原位
                seq.Append(rect.DOAnchorPos(basePos - new Vector2(0f, 26f * strength), 0.18f));
                seq.Append(rect.DOAnchorPos(basePos - new Vector2(14f, 20f) * strength, 0.1f));
                seq.Append(rect.DOAnchorPos(basePos - new Vector2(-14f, 20f) * strength, 0.1f));
                seq.Append(rect.DOAnchorPos(basePos - new Vector2(10f, 20f) * strength, 0.1f));
                seq.Append(rect.DOAnchorPos(basePos - new Vector2(-10f, 20f) * strength, 0.1f));
                seq.Append(rect.DOAnchorPos(basePos, 0.25f).SetEase(Ease.OutBack));
                break;

            case ReactionKind.Comfort:
                // 安慰：轻轻侧身摆动两下
                seq.Append(rect.DORotate(new Vector3(0f, 0f, 7f * strength), 0.35f).SetEase(Ease.InOutSine));
                seq.Append(rect.DORotate(new Vector3(0f, 0f, -4f * strength), 0.35f).SetEase(Ease.InOutSine));
                seq.Append(rect.DORotate(new Vector3(0f, 0f, baseAngle), 0.3f).SetEase(Ease.InOutSine));
                break;

            case ReactionKind.FloatAway:
                // 飘走：向上飘并淡出，结束后隐藏（气球，Page 4）
                seq.Append(rect.DOAnchorPos(basePos + new Vector2(30f * strength, 640f * strength), 1.5f).SetEase(Ease.InQuad));
                if (graphic != null) seq.Join(graphic.DOFade(0f, 1.35f).SetEase(Ease.InQuad));
                break;
        }

        busy = true;
        seq.OnComplete(() =>
        {
            busy = false;
            if (kind == ReactionKind.FloatAway)
            {
                gameObject.SetActive(false);   // 飘走后不再出现，也不恢复浮动
                return;
            }
            if (floatLoop != null) floatLoop.enabled = true;   // 恢复上下浮动
        });
    }
}
