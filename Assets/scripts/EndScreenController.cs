using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class EndScreenController : MonoBehaviour
{

    public static EndScreenController Instance { get; set; }

    [Header("End Screen UI")]
    public GameObject endScreenUI;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI minutesText;
    public TextMeshProUGUI rankingText;
    public TextMeshProUGUI hitTimes;
    public GameObject gameManager;
    
    
    [Header("Leaderboard Integration")]
    public LeaderboardInit leaderboard;
    public RectTransform leaderboardContainer;
    
    [Header("Buttons")]
    public Button playAgainButton;
    public Button mainMenuButton;
    public Button submitScoreButton;
    
    [Header("Animation")]
    public float scoreCountUpDuration = 2f;
    public AnimationCurve scoreCountCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Scene Management")]
    public string mainMenuSceneName = "MainMenu";
    public string gameSceneName = "SampleScene";
    
    private EndScreenData currentData;
    private bool scoreSubmitted = false;

    void Start()
    {
        // Set up button listeners
        if (playAgainButton != null)
            playAgainButton.onClick.AddListener(PlayAgain);
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(GoToMainMenu);
        if (submitScoreButton != null)
            submitScoreButton.onClick.AddListener(SubmitScore);
            
        // Find leaderboard if not assigned
        if (leaderboard == null)
            leaderboard = FindObjectOfType<LeaderboardInit>();
    }

    public void ShowEndScreen(EndScreenData data)
    {
        currentData = data;
        
        // Set title based on completion status
        if (titleText != null)
        {
            titleText.text = data.gameCompleted ? "🏆 GAME COMPLETED!" : "💀 GAME OVER";
            titleText.color = data.gameCompleted ? Color.gold : Color.red;
        }
        
        // Start score count-up animation
        if (finalScoreText != null)
            StartCoroutine(AnimateScoreCountUp(data.finalScore));
        
        // Set other stats immediately
        if (timeText != null)
            timeText.text = $"Time Survived: {data.formattedTime}";
        if (minutesText != null)
            minutesText.text = $"Minutes: {data.minutesSurvived}";

        if (hitTimes != null)
            hitTimes.text = $"Times hit: {data.hitTimes}";
        
        // Set initial ranking text
        if (rankingText != null)
            rankingText.text = "Calculating ranking...";
        
        
        // Show leaderboard
        if (leaderboard != null)
        {
            leaderboard.RefreshLeaderboard();
            StartCoroutine(DelayedRankingCheck(2f)); // Check ranking after leaderboard loads
        }
        
        Debug.Log($"🎬 End screen shown - Score: {data.finalScore}, Time: {data.formattedTime}, Completed: {data.gameCompleted}");
    }

    private IEnumerator AnimateScoreCountUp(int targetScore)
    {
        if (finalScoreText == null) yield break;
        
        float elapsed = 0f;
        int startScore = 0;
        
        while (elapsed < scoreCountUpDuration)
        {
            elapsed += Time.unscaledDeltaTime; // Use unscaled time in case game is paused
            float t = elapsed / scoreCountUpDuration;
            float curveValue = scoreCountCurve.Evaluate(t);
            
            int currentScore = Mathf.RoundToInt(Mathf.Lerp(startScore, targetScore, curveValue));
            finalScoreText.text = $"Final Score: {FormatScore(currentScore)}";
            
            yield return null;
        }
        
        // Ensure final score is exact
        finalScoreText.text = $"Final Score: {FormatScore(targetScore)}";
        Destroy(gameManager);
    }

    private IEnumerator DelayedRankingCheck(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        
        // This is where you could check the player's ranking
        // For now, we'll show a placeholder
        if (rankingText != null)
        {
            if (currentData.gameCompleted)
            {
                rankingText.text = "🎉 Check leaderboard below!";
                rankingText.color = Color.green;
                
               
            }
            else
            {
                rankingText.text = "💪 Try again to improve!";
                rankingText.color = Color.yellow;
                
               
            }
        }
    }

    private string FormatScore(int score)
    {
        if (score >= 1000000)
            return $"{score / 1000000f:F1}M";
        if (score >= 1000)
            return $"{score / 1000f:F1}K";
        return score.ToString("N0");
    }

    public void PlayAgain()
    {
        Debug.Log("🔄 Restarting game...");
        
        // Reset ScoreManager if it exists
        if (ScoreManager.Instance != null)
        {
            //ScoreManager.Instance.ResetForNewRound();
        }
        
        // Reload the game scene
        SceneManager.LoadScene(gameSceneName);
    }

    public void GoToMainMenu()
    {
        Debug.Log("🏠 Going to main menu...");
        SceneManager.LoadScene(mainMenuSceneName);
       
    }

    public void SubmitScore()
    {
        if (scoreSubmitted)
        {
            Debug.Log("⚠️ Score already submitted!");
            return;
        }
        
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.SubmitScoreManually();
            scoreSubmitted = true;
            
            if (submitScoreButton != null)
            {
                submitScoreButton.interactable = false;
                submitScoreButton.GetComponentInChildren<TextMeshProUGUI>().text = "Submitted!";
            }
            
            // Refresh leaderboard after submission
            if (leaderboard != null)
            {
                StartCoroutine(DelayedLeaderboardRefresh(1f));
            }
        }
        else
        {
            Debug.LogError("❌ ScoreManager not found!");
        }
    }

    private IEnumerator DelayedLeaderboardRefresh(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        if (leaderboard != null)
        {
            leaderboard.RefreshLeaderboard();
            Debug.Log("🔄 Leaderboard refreshed after score submission");
        }
    }
}