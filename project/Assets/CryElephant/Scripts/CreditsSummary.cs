using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Credits-page friendship summary: shows how many new friends the child made
/// based on their choices along the way. Adds new text without touching the
/// original 5_Credits content.
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
