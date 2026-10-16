/// <summary>
/// 跨场景保存剧情状态（Page 2 的选择会影响后续页面）。
/// </summary>
public static class StoryState
{
    /// <summary>Page 2 是否选择分享气球。</summary>
    public static bool SharedBalloon = true;

    /// <summary>是否已经见过“分享”结局（用于 Page 3/4 的差异文本）。</summary>
    public static int PlayCount = 0;

    public static void ResetToDefault()
    {
        SharedBalloon = true;
    }
}
