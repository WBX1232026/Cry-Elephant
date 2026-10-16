/// <summary>
/// 友谊进度（跨场景）：新增的兔子/乌龟友谊线 + 原有猴子分享选择。
/// 不改动 StoryState 原文件，只读它的 SharedBalloon。
/// </summary>
public static class FriendsState
{
    /// <summary>1_Play 是否点兔子两次交到朋友（容易）。</summary>
    public static bool MetRabbit = false;

    /// <summary>3_Cry 是否耐心点三轮让乌龟出来（需要努力）。</summary>
    public static bool MetTurtle = false;

    /// <summary>总共交到几个朋友（猴子 = 原分享选择）。</summary>
    public static int FriendsMade
    {
        get
        {
            int n = 0;
            if (MetRabbit) n++;
            if (MetTurtle) n++;
            if (StoryState.SharedBalloon) n++;
            return n;
        }
    }
}
