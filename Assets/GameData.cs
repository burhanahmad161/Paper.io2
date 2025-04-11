using UnityEngine;

public static class GameData
{
    private static float playerScore = 0f;
    private static float enemy1Score = 0f;
    private static float enemy2Score = 0f;
    private static float enemy3Score = 0f;
    private static float enemy4Score = 0f; // Default health score
    public static float PlayerScore
    {
        get { return playerScore; }
        set { playerScore = value; }
    }
    public static float Enemy1Score
    {
        get { return enemy1Score; }
        set { enemy1Score = value; }
    }
    public static float Enemy2Score
    {
        get { return enemy2Score; }
        set { enemy2Score = value; }
    }
    public static float Enemy3Score
    {
        get { return enemy3Score; }
        set { enemy3Score = value; }
    }
    public static float Enemy4Score
    {
        get { return enemy4Score; }
        set { enemy4Score = value; }
    }
    public static void ResetScores()
    {
        playerScore = 0;
        enemy1Score = 0;
        enemy2Score = 0;
        enemy3Score = 0;
        enemy4Score = 0;
    }
}
