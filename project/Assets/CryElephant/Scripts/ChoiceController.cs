using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Page 2 choice interaction: share the balloon / keep it to yourself.
/// Writes the result to StoryState, drives the monkey's mood change, then goes to the next page.
/// </summary>
public class ChoiceController : MonoBehaviour
{
    [SerializeField] private RectTransform monkey;
    [SerializeField] private string nextScene = "3_Cry";
    [SerializeField] private AudioClip happySound;
    [SerializeField] private AudioClip sadSound;
    [SerializeField] private float nextDelay = 1.4f;

    private bool chosen;

    public void ChooseShare()
    {
        Apply(true);
    }

    public void ChooseKeep()
    {
        Apply(false);
    }

    private void Apply(bool shared)
    {
        if (chosen) return;
        chosen = true;

        StoryState.SharedBalloon = shared;

        if (Sfx.Instance != null)
        {
            Sfx.Instance.Play(shared ? happySound : sadSound);
            Sfx.Instance.PlayClick();
        }

        if (monkey != null)
        {
            monkey.DOKill();
            Vector2 basePos = monkey.anchoredPosition;

            if (shared)
            {
                // Happy: jump twice
                Sequence happy = DOTween.Sequence();
                for (int i = 0; i < 2; i++)
                {
                    happy.Append(monkey.DOAnchorPos(basePos + new Vector2(0f, 70f), 0.22f).SetEase(Ease.OutQuad));
                    happy.Append(monkey.DOAnchorPos(basePos, 0.3f).SetEase(Ease.OutBounce));
                }
            }
            else
            {
                // Balloon not shared: sadly shakes its head left and right three times (rotating around the center pivot)
                const float angle = 12f;
                const float step = 0.14f;
                Sequence sad = DOTween.Sequence();
                for (int i = 0; i < 3; i++)
                {
                    sad.Append(monkey.DOLocalRotate(new Vector3(0f, 0f, angle), step).SetEase(Ease.InOutSine));
                    sad.Append(monkey.DOLocalRotate(new Vector3(0f, 0f, -angle), step).SetEase(Ease.InOutSine));
                }
                sad.Append(monkey.DOLocalRotate(Vector3.zero, step).SetEase(Ease.InOutSine));
            }
        }

        Invoke(nameof(GoNext), nextDelay);
    }

    private void GoNext()
    {
        if (!string.IsNullOrEmpty(nextScene)) SceneManager.LoadScene(nextScene);
    }
}
