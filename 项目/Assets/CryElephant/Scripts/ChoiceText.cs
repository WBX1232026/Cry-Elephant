using TMPro;
using UnityEngine;

/// <summary>
/// 依据 Page 2 的选择（StoryState.SharedBalloon）在运行时切换气泡文案。
/// 让"分享 / 不分享"真正影响后续剧情，而非在编辑器构建期写死。
/// </summary>
[RequireComponent(typeof(TMP_Text))]
public class ChoiceText : MonoBehaviour
{
    [SerializeField] private string sharedText = "";
    [SerializeField] private string keepText = "";

    private void Start()
    {
        TMP_Text text = GetComponent<TMP_Text>();
        if (text == null) return;
        text.text = StoryState.SharedBalloon ? sharedText : keepText;
    }
}
