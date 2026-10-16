/// <summary>
/// Stores story state across scenes (the Page 2 choice affects later pages).
/// </summary>
public static class StoryState
{
    /// <summary>Whether the player chose to share the balloon on Page 2.</summary>
    public static bool SharedBalloon = true;

    /// <summary>Whether the "share" ending has been seen (used for the differing Page 3/4 texts).</summary>
    public static int PlayCount = 0;

    public static void ResetToDefault()
    {
        SharedBalloon = true;
    }
}
