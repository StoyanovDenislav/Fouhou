using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("Score Settings")]
    private float totalSurvivalTimer = 0f; // Total time across all stages - NEVER RESET
    private float currentStageTimer = 0f;   // Time for current stage only
    private int totalScore = 0;
    private bool gameActive = true;         // Changed from stageActive to gameActive
    private int currentStage = 1;
    private int minutesSurvived = 0; // Track how many complete minutes we've survived

    [Header("UI References")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI survivalTimeText;
    public TextMeshProUGUI stageText; // Optional: show current stage
    
    [Header("Score Display Settings")]
    public bool animateScoreChanges = true;
    public float scoreAnimationSpeed = 2f;
    
    private int displayedScore = 0;
    private float targetScore = 0;

    void Awake()
    {
        if (Instance != null && Instance != this) 
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        // Find UI elements if not assigned
        if (scoreText == null)
            scoreText = GameObject.Find("ScoreText")?.GetComponent<TextMeshProUGUI>();
        if (survivalTimeText == null)
            survivalTimeText = GameObject.Find("SurvivalTimeText")?.GetComponent<TextMeshProUGUI>();
        if (stageText == null)
            stageText = GameObject.Find("StageText")?.GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        if (!gameActive) return;

        // Update both timers - totalSurvivalTimer NEVER resets
        totalSurvivalTimer += Time.deltaTime;
        currentStageTimer += Time.deltaTime;

        // Check for new complete minutes
        int currentMinutes = Mathf.FloorToInt(totalSurvivalTimer / 60f);
        
        // If we've survived a new complete minute, award points
        if (currentMinutes > minutesSurvived)
        {
            int newMinutes = currentMinutes - minutesSurvived;
            totalScore += newMinutes * 1000; // +1000 per new minute
            minutesSurvived = currentMinutes;
            
            Debug.Log($"⏰ Survived {minutesSurvived} minute(s)! +{newMinutes * 1000} bonus points");
        }

        // Update UI
        UpdateScoreDisplay();
        UpdateSurvivalTimeDisplay();
        UpdateStageDisplay();
    }

    private void UpdateScoreDisplay()
    {
        if (scoreText == null) return;

        if (animateScoreChanges)
        {
            // Smooth score animation
            if (displayedScore != totalScore)
            {
                displayedScore = Mathf.RoundToInt(Mathf.MoveTowards(displayedScore, totalScore, scoreAnimationSpeed * Time.deltaTime * 1000));
            }
        }
        else
        {
            // Instant score update
            displayedScore = totalScore;
        }

        // Use displayedScore for both animated and non-animated modes
        int scoreToDisplay = displayedScore;
    
        // Determine minimum width based on score magnitude
        int minWidth = GetMinimumScoreWidth(scoreToDisplay);
    
        // Format with leading zeros
        string formattedScore = scoreToDisplay.ToString($"D{minWidth}");
    
        scoreText.text = $"Score: {formattedScore}";
    }

    private int GetMinimumScoreWidth(int score)
    {
        // Start with 8 digits minimum, expand as needed
        int digits = Mathf.Max(8, score.ToString().Length);
        return digits;
    }

    private void UpdateSurvivalTimeDisplay()
    {
        if (survivalTimeText == null) return;

        // Show total survival time - this now continuously increases
        int minutes = Mathf.FloorToInt(totalSurvivalTimer / 60f);
        int seconds = Mathf.FloorToInt(totalSurvivalTimer % 60f);
        survivalTimeText.text = $"Time: {minutes:00}:{seconds:00}";
    }

    private void UpdateStageDisplay()
    {
        if (stageText != null)
        {
            stageText.text = $"Stage: {currentStage}";
        }
    }

    public void AddBulletScore(int count)
    {
        totalScore += count * 100; // +100 per bullet
        
        // Optional: Show floating score text for bullet hits
        if (animateScoreChanges)
        {
            ShowFloatingScore(count * 100);
        }
    }

    private void ShowFloatingScore(int scoreAmount)
    {
        // This could create floating score text effects
        // For now, just log it - you can expand this later
        Debug.Log($"+{scoreAmount} points!");
    }

    // Called when a stage ends, but game continues
    public void EndStage()
    {
        // Add stage completion bonus
        int stageBonus = currentStage * 500; // Bonus increases per stage
        totalScore += stageBonus;
        
        Debug.Log($"🎉 Stage {currentStage} completed! Bonus: {stageBonus} points");
        Debug.Log($"📊 Stage time: {FormatTime(currentStageTimer)}");
        Debug.Log($"📊 Total time: {FormatTime(totalSurvivalTimer)}");
        
        // Reset ONLY stage timer for next stage - totalSurvivalTimer keeps running
        currentStageTimer = 0f;
        currentStage++;
        
        // DON'T stop gameActive - the game continues!
    }

    // Called when starting a new stage
    public void StartNewStage(int stageNumber = -1)
    {
        if (stageNumber > 0)
        {
            currentStage = stageNumber;
        }
        currentStageTimer = 0f; // Reset stage timer only
        // totalSurvivalTimer and minutesSurvived keep their values
        // gameActive remains true
        
        Debug.Log($"▶️ Starting Stage {currentStage}");
    }

    // Called when the entire game ends (all stages complete or game over)
    public void EndGame()
    {
        gameActive = false;
        
        // Final score update
        if (animateScoreChanges)
        {
            displayedScore = totalScore;
            UpdateScoreDisplay();
        }
        
        Debug.Log($"🏁 Game Over! Final Score: {totalScore}");
        Debug.Log($"⏱️ Total Time: {FormatTime(totalSurvivalTimer)}");
        Debug.Log($"📈 Minutes Survived: {minutesSurvived}");
    }

    // Helper method to format time nicely
    private string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
        return $"{minutes:00}:{seconds:00}";
    }

    // Public getters
    public int GetScore() => totalScore;
    public float GetTotalSurvivalTime() => totalSurvivalTimer;
    public float GetCurrentStageTime() => currentStageTimer;
    public int GetCurrentStage() => currentStage;
    public int GetMinutesSurvived() => minutesSurvived;
}