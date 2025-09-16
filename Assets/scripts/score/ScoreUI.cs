using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScoreUI : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI survivalTimeText;
    public TextMeshProUGUI stageText;
    
    [Header("Layout Settings")]
    public RectTransform uiPanel;
    
    void Start()
    {
        SetupUI();
    }
    
    private void SetupUI()
    {
        // Create UI panel if it doesn't exist
        if (uiPanel == null)
        {
            CreateScorePanel();
        }
        
        // Link to ScoreManager
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.scoreText = scoreText;
            ScoreManager.Instance.survivalTimeText = survivalTimeText;
        }
    }
    
    private void CreateScorePanel()
    {
        // This method can create the UI programmatically if needed
        // For now, it's recommended to create it manually in the scene
        Debug.Log("Please create UI elements manually in the scene");
    }
    
    public void UpdateStageDisplay(int stageNumber)
    {
        if (stageText != null)
        {
            stageText.text = $"Stage: {stageNumber}";
        }
    }
}