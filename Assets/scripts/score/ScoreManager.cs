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

    [Header("Game Progress Tracking")]
    private bool gameCompleted = false;
    private bool lastGroupCompleted = false;
    
    [Header("UI References")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI survivalTimeText;
    public TextMeshProUGUI stageText; // Optional: show current stage
    public TextMeshProUGUI progressText; // Optional: show progress
    
    [Header("Score Display Settings")]
    public bool animateScoreChanges = true;
    public float scoreAnimationSpeed = 2f;
    
    [Header("Score Submission")]
    public ScoreSubmitter scoreSubmitter;
    public bool autoSubmitOnGameEnd = true;
    public bool submitOnlyOnGameCompletion = true; // Only submit when all stages/groups complete
    
    private int displayedScore = 0;
    private float targetScore = 0;
    
    // Stage spawner reference
    private StageSpawner stageSpawner;

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
        if (progressText == null)
            progressText = GameObject.Find("ProgressText")?.GetComponent<TextMeshProUGUI>();
            
        // Auto-find ScoreSubmitter if not assigned
        if (scoreSubmitter == null)
            scoreSubmitter = FindObjectOfType<ScoreSubmitter>();
            
        // Find StageSpawner
        stageSpawner = FindObjectOfType<StageSpawner>();
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
        UpdateProgressDisplay();
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
            int totalStages = stageSpawner != null ? stageSpawner.GetTotalStages() : 0;
            stageText.text = $"Stage: {currentStage}/{totalStages}";
        }
    }
    
    private void UpdateProgressDisplay()
    {
        if (progressText != null && stageSpawner != null)
        {
            int currentGroup = stageSpawner.GetCurrentGroupIndex() + 1; // +1 for display
            int totalGroups = stageSpawner.GetTotalGroupsInCurrentStage();
            progressText.text = $"Group: {currentGroup}/{totalGroups}";
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
        // Debug.Log($"+{scoreAmount} points!");
    }

    // Called by StageSpawner when a stage starts
    public void OnStageStarted(int stageIndex)
    {
        currentStage = stageIndex + 1; // +1 for display (0-based to 1-based)
        currentStageTimer = 0f;
        Debug.Log($"▶️ Stage {currentStage} started");
    }

    // Called by StageSpawner when a group starts
    public void OnGroupStarted(int stageIndex, int groupIndex)
    {
        Debug.Log($"🎯 Group {groupIndex + 1} of Stage {stageIndex + 1} started");
    }

    // Called by StageSpawner when a pattern completes
    public void OnPatternCompleted(int stageIndex, int groupIndex, int patternIndex)
    {
        // Add small pattern completion bonus
        int patternBonus = 50;
        totalScore += patternBonus;
        Debug.Log($"✨ Pattern {patternIndex + 1} completed! +{patternBonus} points");
    }

    // Called by StageSpawner when a group completes
    public void OnGroupCompleted(int stageIndex, int groupIndex, bool isLastStage, bool isLastGroup)
    {
        // Add group completion bonus
        int groupBonus = (stageIndex + 1) * 100 + (groupIndex + 1) * 50;
        totalScore += groupBonus;
        
        Debug.Log($"💥 Group {groupIndex + 1} of Stage {stageIndex + 1} completed! +{groupBonus} points");
        
        // Check if this is the last group of the last stage
        if (isLastStage && isLastGroup)
        {
            lastGroupCompleted = true;
            Debug.Log($"🎊 LAST GROUP OF LAST STAGE COMPLETED!");
            
            // Don't submit yet - wait for OnGameCompleted to ensure everything is properly finished
        }
    }

    // Called by StageSpawner when all groups in a stage are processed
    public void OnAllGroupsCompleted(int stageIndex, bool isLastStage)
    {
        Debug.Log($"🏁 All groups in Stage {stageIndex + 1} completed");
        
        if (isLastStage)
        {
            Debug.Log($"🎉 All groups in FINAL STAGE completed! Waiting for game completion...");
        }
    }

    // Called by StageSpawner when the entire game is completed
    public void OnGameCompleted()
    {
        gameCompleted = true;
        gameActive = false;
        
        // Add massive game completion bonus
        int completionBonus = 10000;
        totalScore += completionBonus;
        
        Debug.Log($"🏆 ENTIRE GAME COMPLETED!");
        Debug.Log($"🎊 Game completion bonus: +{completionBonus} points");
        Debug.Log($"🏁 Final Score: {totalScore}");
        Debug.Log($"⏱️ Total Time: {FormatTime(totalSurvivalTimer)}");
        Debug.Log($"📈 Minutes Survived: {minutesSurvived}");
        
        // Final score update
        if (animateScoreChanges)
        {
            displayedScore = totalScore;
            UpdateScoreDisplay();
        }
        
        // Submit score only if the entire game was completed
        if (autoSubmitOnGameEnd && scoreSubmitter != null)
        {
            if (submitOnlyOnGameCompletion)
            {
                Debug.Log($"📤 Submitting score after complete game finish: {totalScore}");
                scoreSubmitter.SubmitScore(totalScore);
            }
            else
            {
                Debug.Log($"⚠️ submitOnlyOnGameCompletion is false, but game was completed");
                scoreSubmitter.SubmitScore(totalScore);
            }
        }
        else if (autoSubmitOnGameEnd && scoreSubmitter == null)
        {
            Debug.LogWarning("⚠️ Auto-submit enabled but ScoreSubmitter not found!");
        }
    }

    // Called when a stage ends, but game continues (legacy method, still used by StageSpawner)
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
        
        // DON'T stop gameActive - the game continues!
        // DON'T submit score here - only submit when entire game is completed
    }

    // Called when the entire game ends due to game over (player death, etc.)
    public void EndGame()
    {
        gameActive = false;
        
        // Final score update
        if (animateScoreChanges)
        {
            displayedScore = totalScore;
            UpdateScoreDisplay();
        }
        
        Debug.Log($"💀 Game Over! Final Score: {totalScore}");
        Debug.Log($"⏱️ Total Time: {FormatTime(totalSurvivalTimer)}");
        Debug.Log($"📈 Minutes Survived: {minutesSurvived}");
        
        // Only submit score if game completion is not required, or if it was actually completed
        if (autoSubmitOnGameEnd && scoreSubmitter != null)
        {
            if (!submitOnlyOnGameCompletion || gameCompleted)
            {
                Debug.Log($"📤 Submitting score after game over: {totalScore}");
                scoreSubmitter.SubmitScore(totalScore);
            }
            else
            {
                Debug.Log($"⚠️ Game over but submitOnlyOnGameCompletion is true and game was not completed. Score not submitted.");
            }
        }
        else if (autoSubmitOnGameEnd && scoreSubmitter == null)
        {
            Debug.LogWarning("⚠️ Auto-submit enabled but ScoreSubmitter not found!");
        }
    }

    // Manual score submission method
    public void SubmitScoreManually()
    {
        if (scoreSubmitter != null)
        {
            Debug.Log($"📤 Manually submitting score: {totalScore}");
            scoreSubmitter.SubmitScore(totalScore);
        }
        else
        {
            Debug.LogWarning("❌ ScoreSubmitter not found! Cannot submit score.");
        }
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
    public bool IsGameCompleted() => gameCompleted;
    public bool IsLastGroupCompleted() => lastGroupCompleted;
    
    // Additional utility methods for score submission
    public bool IsScoreSubmitterAvailable() => scoreSubmitter != null;
    
    public void SetScoreSubmitter(ScoreSubmitter submitter)
    {
        scoreSubmitter = submitter;
        Debug.Log($"✅ ScoreSubmitter assigned to ScoreManager");
    }
    
    // Context menu methods for testing
    [ContextMenu("Test Score Submission")]
    public void TestScoreSubmission()
    {
        if (Application.isPlaying)
        {
            SubmitScoreManually();
        }
        else
        {
            Debug.LogWarning("⚠️ Score submission test only works in Play Mode");
        }
    }
    
    [ContextMenu("Add Test Score")]
    public void AddTestScore()
    {
        if (Application.isPlaying)
        {
            AddBulletScore(10); // Add 1000 points for testing
            Debug.Log($"🎯 Added test score! Current total: {totalScore}");
        }
    }
    
    [ContextMenu("Simulate Game Completion")]
    public void SimulateGameCompletion()
    {
        if (Application.isPlaying)
        {
            OnGameCompleted();
        }
    }
    
    [ContextMenu("Simulate Game Over")]
    public void SimulateGameOver()
    {
        if (Application.isPlaying)
        {
            EndGame();
        }
    }
}