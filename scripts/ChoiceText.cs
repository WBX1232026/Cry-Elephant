using TMPro;
using UnityEngine;

/// <summary>
/// Switches the bubble text at runtime based on the Page 2 choice (StoryState.SharedBalloon).
/// Makes the "share / keep" choice actually affect the later story instead of being hard-coded at editor build time.
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
