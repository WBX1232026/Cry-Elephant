using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Page 4 deep breathing: tapping the elephant triggers a slow scale-up and back, and starts the background music.
/// Implements the pointer-click interface directly instead of relying on external event wiring (previously nothing called Breathe()).
/// Also drives: the breathing halo scaling, the rabbit companion's gentle bobbing, and the "Breathe in / Breathe out" hint text.
/// </summary>
public class DeepBreath : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private RectTransform target;
    [SerializeField] private AudioClip breathSound;
    [SerializeField] private AudioClip bgm;

    [Header("Breathing Guide")]
    [SerializeField] private RectTransform halo;        // breathing halo (scales with each breath)
    [SerializeField] private RectTransform companion;   // companion (breathes along)
    [SerializeField] private TMP_Text hintLabel;        // hint label
    [SerializeField] private string inhaleText = "Breathe in...";
    [SerializeField] private string exhaleText = "Breathe out...";

    private bool breathing;

    public void OnPointerClick(PointerEventData eventData)
    {
        Breathe();
    }

    public void Breathe()
    {
        if (Sfx.Instance != null && breathSound != null) Sfx.Instance.Play(breathSound);

        if (bgm != null && BgmManager.Instance != null) BgmManager.Instance.Play(bgm);

        if (target == null || breathing) return;
        breathing = true;

        Reset(target);
        if (halo != null) Reset(halo);
        if (companion != null) Reset(companion);

        Sequence seq = DOTween.Sequence();
        for (int i = 0; i < 2; i++)
        {
            // Breathe in: scale up
            seq.AppendCallback(() => SetHint(inhaleText));
            seq.Append(target.DOScale(Vector3.one * 1.12f, 1.1f).SetEase(Ease.InOutSine));
            if (halo != null) seq.Join(halo.DOScale(Vector3.one * 1.35f, 1.1f).SetEase(Ease.InOutSine));
            if (companion != null) seq.Join(companion.DOScale(Vector3.one * 1.06f, 1.1f).SetEase(Ease.InOutSine));

            // Breathe out: scale back down
            seq.AppendCallback(() => SetHint(exhaleText));
            seq.Append(target.DOScale(Vector3.one, 1.3f).SetEase(Ease.InOutSine));
            if (halo != null) seq.Join(halo.DOScale(Vector3.one, 1.3f).SetEase(Ease.InOutSine));
            if (companion != null) seq.Join(companion.DOScale(Vector3.one, 1.3f).SetEase(Ease.InOutSine));
        }
        seq.OnComplete(() => breathing = false);
    }

    private static void Reset(RectTransform rt)
    {
        rt.DOKill();
        rt.localScale = Vector3.one;
    }

    private void SetHint(string text)
    {
        if (hintLabel != null) hintLabel.text = text;
    }
}
