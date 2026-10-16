using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Turtle friendship (takes effort): tap the shell -> the head peeks out and pulls back -> wait patiently -> after three rounds it comes out fully.
/// Used by the turtle child object under Friend_Fall in 3_Cry.
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

        // Peek out to take a look
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
            // Third round: it trusts the child and comes out completely
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

        // Hides again; the wait grows longer each round (the child needs patience)
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
