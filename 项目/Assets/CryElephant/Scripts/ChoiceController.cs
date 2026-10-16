using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Page 2 选择型交互：分享气球 / 自己玩。
/// 结果写入 StoryState，并驱动猴子情绪变化后进入下一页。
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
                // 开心：跳两下
                Sequence happy = DOTween.Sequence();
                for (int i = 0; i < 2; i++)
                {
                    happy.Append(monkey.DOAnchorPos(basePos + new Vector2(0f, 70f), 0.22f).SetEase(Ease.OutQuad));
                    happy.Append(monkey.DOAnchorPos(basePos, 0.3f).SetEase(Ease.OutBounce));
                }
            }
            else
            {
                // 没被分享气球：委屈地左右摇头三下（绕图片中心 pivot 旋转）
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
