using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Rabbit friendship (easy): two taps make a friend, and every tap gives feedback.
/// Used by Rabbit_Play in 1_Play.
/// </summary>
public class RabbitFriend : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private RectTransform rabbit;
    [SerializeField] private RectTransform heart;
    [SerializeField] private Text hint;
    [SerializeField] private string helloText = "Tap the rabbit to say hello!";
    [SerializeField] private string friendText = "Rabbit is your friend now!";

    private int taps;
    private bool done;

    private void OnEnable()
    {
        if (hint != null && !done) hint.text = helloText;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (done) return;

        taps++;
        if (Sfx.Instance != null) Sfx.Instance.PlayClick();
        Hop();
        HeartPop();

        if (taps >= 2)
        {
            done = true;
            FriendsState.MetRabbit = true;
            if (hint != null) hint.text = friendText;

            // Jump happily twice
            rabbit.DOKill();
            Sequence happy = DOTween.Sequence();
            happy.Append(rabbit.DOScale(1.18f, 0.18f).SetEase(Ease.OutQuad));
            happy.Append(rabbit.DOScale(1f, 0.22f).SetEase(Ease.OutBounce));
            happy.Append(rabbit.DOScale(1.1f, 0.16f).SetEase(Ease.OutQuad));
            happy.Append(rabbit.DOScale(1f, 0.18f).SetEase(Ease.OutBounce));
        }
    }

    private void Hop()
    {
        if (rabbit == null) return;
        rabbit.DOKill();
        Vector2 p = rabbit.anchoredPosition;
        rabbit.DOAnchorPos(p + new Vector2(0f, 42f), 0.18f).SetEase(Ease.OutQuad)
            .OnComplete(() => rabbit.DOAnchorPos(p, 0.24f).SetEase(Ease.OutBounce));
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
