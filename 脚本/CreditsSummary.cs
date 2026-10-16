using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 结局页友谊总结：按孩子一路的选择显示交到几个新朋友。
/// 新增文字，不修改 5_Credits 原有内容。
/// </summary>
public class CreditsSummary : MonoBehaviour
{
    [SerializeField] private Text label;

    private void Start()
    {
        if (label == null) return;

        int n = FriendsState.FriendsMade;
        if (n <= 0)
            label.text = "Try again and make some friends!";
        else
            label.text = "You made " + n + " new friend" + (n > 1 ? "s" : "") + " today!";
    }
}
