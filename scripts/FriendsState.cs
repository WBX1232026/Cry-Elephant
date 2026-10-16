/// <summary>
/// Friendship progress (across scenes): the new rabbit/turtle friendship lines plus the original monkey share choice.
/// Does not modify the original StoryState file; only reads its SharedBalloon.
/// </summary>
public static class FriendsState
{
    /// <summary>Whether the player tapped the rabbit twice in 1_Play to make a friend (easy).</summary>
    public static bool MetRabbit = false;

    /// <summary>Whether the player patiently tapped for three rounds in 3_Cry to make the turtle come out (takes effort).</summary>
    public static bool MetTurtle = false;

    /// <summary>The total number of friends made (the monkey = the original share choice).</summary>
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
