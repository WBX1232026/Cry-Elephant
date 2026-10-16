using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// Generic clickable prop: plays a sound and one DOTween reaction animation when clicked.
/// Used for in-page objects such as balloons, the elephant, monkey, rabbit, and bubbles.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class TapReaction : MonoBehaviour, IPointerClickHandler
{
    public enum ReactionKind
    {
        Pop,        // Scale up then bounce back (bubbles, balloons)
        Bounce,     // Bounce up and come back down (balloons)
        FloatUp,    // Float up for about 2 seconds, then come back down (balloons, Page 1)
        EarWiggle,  // Wiggle left and right (elephant ears)
        Jump,       // Jump once (monkey)
        Shake,      // Shake left and right
        Nod,        // Nod up and down
        Cry,        // Cry: sink down and twitch (elephant, Page 3)
        Comfort,    // Comfort: sway gently from side to side (friend, Page 3)
        FloatAway   // Float away, fade out, then hide (balloon, Page 4)
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
        // Pause this object's own up-and-down float so two tweens don't fight over anchoredPosition
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
                // Float up for about 2 seconds, then come back down
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
                // Cry: sink down first, twitch left and right a few times, then return to the start position
                seq.Append(rect.DOAnchorPos(basePos - new Vector2(0f, 26f * strength), 0.18f));
                seq.Append(rect.DOAnchorPos(basePos - new Vector2(14f, 20f) * strength, 0.1f));
                seq.Append(rect.DOAnchorPos(basePos - new Vector2(-14f, 20f) * strength, 0.1f));
                seq.Append(rect.DOAnchorPos(basePos - new Vector2(10f, 20f) * strength, 0.1f));
                seq.Append(rect.DOAnchorPos(basePos - new Vector2(-10f, 20f) * strength, 0.1f));
                seq.Append(rect.DOAnchorPos(basePos, 0.25f).SetEase(Ease.OutBack));
                break;

            case ReactionKind.Comfort:
                // Comfort: sway gently from side to side twice
                seq.Append(rect.DORotate(new Vector3(0f, 0f, 7f * strength), 0.35f).SetEase(Ease.InOutSine));
                seq.Append(rect.DORotate(new Vector3(0f, 0f, -4f * strength), 0.35f).SetEase(Ease.InOutSine));
                seq.Append(rect.DORotate(new Vector3(0f, 0f, baseAngle), 0.3f).SetEase(Ease.InOutSine));
                break;

            case ReactionKind.FloatAway:
                // Float away: drift upward while fading out, then hide when finished (balloon, Page 4)
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
                gameObject.SetActive(false);   // once floated away it stays hidden and the float is not restored
                return;
            }
            if (floatLoop != null) floatLoop.enabled = true;   // resume the up-and-down float
        });
    }
}
