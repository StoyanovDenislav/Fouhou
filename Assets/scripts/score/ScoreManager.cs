using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("Score Settings")]
    private float totalSurvivalTimer = 0f;
    private float currentStageTimer = 0f;
    private int totalScore = 0;
    private bool gameActive = true;
    private int currentStage = 1;
    private int minutesSurvived = 0;

    [Header("Game Progress Tracking")]
    private bool gameCompleted = false;
    private bool lastGroupCompleted = false;
    
    [Header("UI References")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI survivalTimeText;
    public TextMeshProUGUI stageText;
    public TextMeshProUGUI progressText;
    
    [Header("End Screen")]
    public GameObject endScreenCanvas; // NEW: Reference to end screen
    public EndScreenController endScreenController; // NEW: Reference to end screen controller
    
    [Header("Score Display Settings")]
    public bool animateScoreChanges = true;
    public float scoreAnimationSpeed = 2f;
    
    [Header("Score Submission")]
    public ScoreSubmitter scoreSubmitter;
    public bool autoSubmitOnGameEnd = true;
    public bool submitOnlyOnGameCompletion = true;
    
    private int displayedScore = 0;
    private float targetScore = 0;
    private StageSpawner stageSpawner;
    public int hitTimes;
    

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
            
        // Find end screen components
        if (endScreenCanvas == null)
            endScreenCanvas = GameObject.Find("EndScreenCanvas");
        if (endScreenController == null)
            endScreenController = FindObjectOfType<EndScreenController>();
            
        // Auto-find ScoreSubmitter if not assigned
        if (scoreSubmitter == null)
            scoreSubmitter = FindObjectOfType<ScoreSubmitter>();
            
        // Find StageSpawner
        stageSpawner = FindObjectOfType<StageSpawner>();
        
        // Make sure end screen is initially hidden
        if (endScreenCanvas != null)
            endScreenCanvas.SetActive(false);
    }

    void Update()
    {
        if (!gameActive) return;

        totalSurvivalTimer += Time.deltaTime;
        currentStageTimer += Time.deltaTime;

        // Check for new complete minutes
        int currentMinutes = Mathf.FloorToInt(totalSurvivalTimer / 60f);
        
        if (currentMinutes > minutesSurvived)
        {
            int newMinutes = currentMinutes - minutesSurvived;
            totalScore += newMinutes * 1000;
            minutesSurvived = currentMinutes;
            
            Debug.Log($"⏰ Survived {minutesSurvived} minute(s)! +{newMinutes * 1000} bonus points");
        }

        UpdateScoreDisplay();
        UpdateSurvivalTimeDisplay();
        UpdateStageDisplay();
        UpdateProgressDisplay();
    }

    // ... [All existing methods remain the same until OnGameCompleted] ...

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
        
        // NEW: Show end screen instead of just submitting
        ShowEndScreen();
        
        // Submit score only if the entire game was completed
        if (autoSubmitOnGameEnd && scoreSubmitter != null)
        {
            if (submitOnlyOnGameCompletion)
            {
                Debug.Log($"📤 Submitting score after complete game finish: {totalScore}");
                scoreSubmitter.SubmitScore(totalScore);
            }
        }
    }

    public void EndGame()
    {
        gameActive = false;
        
        if (animateScoreChanges)
        {
            displayedScore = totalScore;
            UpdateScoreDisplay();
        }
        
        Debug.Log($"💀 Game Over! Final Score: {totalScore}");
        Debug.Log($"⏱️ Total Time: {FormatTime(totalSurvivalTimer)}");
        Debug.Log($"📈 Minutes Survived: {minutesSurvived}");
        
        // NEW: Show end screen for game over too
        ShowEndScreen();
        
        // Submit score logic
        if (autoSubmitOnGameEnd && scoreSubmitter != null)
        {
            if (!submitOnlyOnGameCompletion || gameCompleted)
            {
                Debug.Log($"📤 Submitting score after game over: {totalScore}");
                scoreSubmitter.SubmitScore(totalScore);
            }
        }
    }

    // NEW: Show the end screen
    private void ShowEndScreen()
    {
        if (endScreenCanvas != null)
        {
            endScreenCanvas.SetActive(true);
            Debug.Log("🎬 End screen activated");
        }
        
        if (endScreenController != null)
        {
            // Pass all the data to the end screen
            EndScreenData data = new EndScreenData
            {
                finalScore = totalScore,
                minutesSurvived = minutesSurvived,
                gameCompleted = gameCompleted,
                formattedTime = FormatTime(totalSurvivalTimer),
                hitTimes = hitTimes
            };
            
            endScreenController.ShowEndScreen(data);
            Debug.Log("📊 End screen data sent");
        }
        else
        {
            Debug.LogWarning("⚠️ EndScreenController not found!");
        }
    }

    // ... [Rest of existing methods remain the same] ...

    private void UpdateScoreDisplay()
    {
        if (scoreText == null) return;

        if (animateScoreChanges)
        {
            if (displayedScore != totalScore)
            {
                displayedScore = Mathf.RoundToInt(Mathf.MoveTowards(displayedScore, totalScore, scoreAnimationSpeed * Time.deltaTime * 1000));
            }
        }
        else
        {
            displayedScore = totalScore;
        }

        int scoreToDisplay = displayedScore;
        int minWidth = GetMinimumScoreWidth(scoreToDisplay);
        string formattedScore = scoreToDisplay.ToString($"D{minWidth}");
        scoreText.text = $"Score: {formattedScore}";
    }

    private int GetMinimumScoreWidth(int score)
    {
        int digits = Mathf.Max(8, score.ToString().Length);
        return digits;
    }

    private void UpdateSurvivalTimeDisplay()
    {
        if (survivalTimeText == null) return;

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
            int currentGroup = stageSpawner.GetCurrentGroupIndex() + 1;
            int totalGroups = stageSpawner.GetTotalGroupsInCurrentStage();
            progressText.text = $"Group: {currentGroup}/{totalGroups}";
        }
    }

    public void AddBulletScore(int count)
    {
        totalScore += count * 100;
        
        if (animateScoreChanges)
        {
            ShowFloatingScore(count * 100);
        }
    }

    private void ShowFloatingScore(int scoreAmount)
    {
        // Optional floating score implementation
    }

    public void OnStageStarted(int stageIndex)
    {
        currentStage = stageIndex + 1;
        currentStageTimer = 0f;
        Debug.Log($"▶️ Stage {currentStage} started");
    }

    public void OnGroupStarted(int stageIndex, int groupIndex)
    {
        Debug.Log($"🎯 Group {groupIndex + 1} of Stage {stageIndex + 1} started");
    }

    public void OnPatternCompleted(int stageIndex, int groupIndex, int patternIndex)
    {
        int patternBonus = 50;
        totalScore += patternBonus;
        Debug.Log($"✨ Pattern {patternIndex + 1} completed! +{patternBonus} points");
    }

    public void OnGroupCompleted(int stageIndex, int groupIndex, bool isLastStage, bool isLastGroup)
    {
        int groupBonus = (stageIndex + 1) * 100 + (groupIndex + 1) * 50;
        totalScore += groupBonus;
        
        Debug.Log($"💥 Group {groupIndex + 1} of Stage {stageIndex + 1} completed! +{groupBonus} points");
        
        if (isLastStage && isLastGroup)
        {
            lastGroupCompleted = true;
            Debug.Log($"🎊 LAST GROUP OF LAST STAGE COMPLETED!");
        }
    }

    public void OnAllGroupsCompleted(int stageIndex, bool isLastStage)
    {
        Debug.Log($"🏁 All groups in Stage {stageIndex + 1} completed");
        
        if (isLastStage)
        {
            Debug.Log($"🎉 All groups in FINAL STAGE completed! Waiting for game completion...");
        }
    }

    public void EndStage()
    {
        int stageBonus = currentStage * 500;
        totalScore += stageBonus;
        
        Debug.Log($"🎉 Stage {currentStage} completed! Bonus: {stageBonus} points");
        Debug.Log($"📊 Stage time: {FormatTime(currentStageTimer)}");
        Debug.Log($"📊 Total time: {FormatTime(totalSurvivalTimer)}");
        
        currentStageTimer = 0f;
    }

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
    
    public bool IsScoreSubmitterAvailable() => scoreSubmitter != null;
    
    public void SetScoreSubmitter(ScoreSubmitter submitter)
    {
        scoreSubmitter = submitter;
        Debug.Log($"✅ ScoreSubmitter assigned to ScoreManager");
    }
    
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
            AddBulletScore(10);
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