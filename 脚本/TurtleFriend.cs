using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 乌龟交友（需要努力）：点壳 → 露头又缩回 → 耐心等 → 三轮后才出来。
/// 用于 3_Cry 的 Friend_Fall 下挂的乌龟子对象。
/// </summary>
public class TurtleFriend : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private RectTransform shell;
    [SerializeField] private RectTransform turtleOut;
    [SerializeField] private RectTransform heart;
    [SerializeField] private Text hint;
    [SerializeField] private string firstText = "The turtle is shy. Tap the shell gently.";
    [SerializeField] private string waitingText = "The turtle is still shy. Be patient...";
    [SerializeField] private string doneText = "The turtle trusts you now!";

    private int round;
    private bool waiting;
    private bool done;

    private void OnEnable()
    {
        if (!done && hint != null) hint.text = firstText;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (done || waiting) return;
        StartCoroutine(Peek());
    }

    private IEnumerator Peek()
    {
        waiting = true;
        round++;

        // 露头看看
        if (shell != null) shell.gameObject.SetActive(false);
        if (turtleOut != null)
        {
            turtleOut.gameObject.SetActive(true);
            turtleOut.localScale = Vector3.one * 0.55f;
            turtleOut.DOScale(1f, 0.25f).SetEase(Ease.OutBack);
        }
        if (Sfx.Instance != null) Sfx.Instance.PlayClick();
        if (hint != null && round < 3) hint.text = waitingText;

        yield return new WaitForSeconds(0.8f);

        if (round >= 3)
        {
            // 第三轮：信任孩子，彻底出来
            done = true;
            FriendsState.MetTurtle = true;
            if (hint != null) hint.text = doneText;
            HeartPop();
            if (turtleOut != null)
            {
                turtleOut.DOKill();
                turtleOut.DOLocalRotate(new Vector3(0f, 0f, 8f), 0.2f)
                    .SetLoops(6, LoopType.Yoyo).SetEase(Ease.InOutSine);
            }
            yield break;
        }

        // 又缩回去，等待时间一轮比一轮长（孩子需要耐心）
        if (turtleOut != null)
        {
            turtleOut.DOScale(0.55f, 0.2f).SetEase(Ease.InBack)
                .OnComplete(() =>
                {
                    turtleOut.gameObject.SetActive(false);
                    if (shell != null) shell.gameObject.SetActive(true);
                });
        }
        yield return new WaitForSeconds(1.5f + round);
        waiting = false;
    }

    private void HeartPop()
    {
        if (heart == null) return;
        heart.DOKill();
        heart.gameObject.SetActive(true);
        heart.localScale = Vector3.zero;
        heart.DOScale(1.35f, 0.22f).SetEase(Ease.OutBack)
            .OnComplete(() => heart.DOScale(0f, 0.28f).SetEase(Ease.InBack).SetDelay(0.4f)
                .OnComplete(() => heart.gameObject.SetActive(false)));
    }
}
