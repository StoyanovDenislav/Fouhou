using System;

[Serializable]
public class EndScreenData
{
    public int finalScore;
    public int minutesSurvived;
    public bool gameCompleted;
    public string formattedTime;
    public int hitTimes;
    
    // Additional data for rankings (will be populated by EndScreenController)
    public int playerRanking = -1;
    public bool isNewRecord = false;
    public string congratsMessage = "";
}