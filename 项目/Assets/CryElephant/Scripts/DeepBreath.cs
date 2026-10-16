using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Page 4 深呼吸：点击小象触发缓慢放大再恢复，并开始播放背景音乐。
/// 直接实现点击接口，避免依赖外部事件绑定（此前 Breathe() 无人调用）。
/// 同时驱动：呼吸光环缩放、兔子同伴轻微起伏、引导文字"吸气/呼气"。
/// </summary>
public class DeepBreath : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private RectTransform target;
    [SerializeField] private AudioClip breathSound;
    [SerializeField] private AudioClip bgm;

    [Header("呼吸引导")]
    [SerializeField] private RectTransform halo;        // 呼吸光环（随呼吸放大缩小）
    [SerializeField] private RectTransform companion;   // 同伴（跟着一起呼吸）
    [SerializeField] private TMP_Text hintLabel;        // 引导文字
    [SerializeField] private string inhaleText = "吸气…";
    [SerializeField] private string exhaleText = "呼气…";

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
            // 吸气：放大
            seq.AppendCallback(() => SetHint(inhaleText));
            seq.Append(target.DOScale(Vector3.one * 1.12f, 1.1f).SetEase(Ease.InOutSine));
            if (halo != null) seq.Join(halo.DOScale(Vector3.one * 1.35f, 1.1f).SetEase(Ease.InOutSine));
            if (companion != null) seq.Join(companion.DOScale(Vector3.one * 1.06f, 1.1f).SetEase(Ease.InOutSine));

            // 呼气：回缩
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
