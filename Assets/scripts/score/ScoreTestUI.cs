using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScoreTestUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject testPanel;
    public TMP_InputField scoreInput;
    public TMP_InputField gameIDInput;
    public TMP_InputField userIDInput;
    public TMP_InputField usernameInput;
    public Button submitButton;
    public Button togglePanelButton;
    public TextMeshProUGUI statusText;
    
    [Header("Settings")]
    public KeyCode toggleKey = KeyCode.F12;
    public bool startVisible = false;
    
    private ScoreSubmitter scoreSubmitter;
    
    void Start()
    {
        // Find ScoreSubmitter
        scoreSubmitter = FindObjectOfType<ScoreSubmitter>();
        
        // Setup UI
        if (testPanel != null)
        {
            testPanel.SetActive(startVisible);
        }
        
        // Setup default values
        if (scoreInput != null)
            scoreInput.text = "15420";
        if (gameIDInput != null)
            gameIDInput.text = "fouhou-v1";
        if (userIDInput != null)
            userIDInput.text = SystemInfo.deviceUniqueIdentifier;
        if (usernameInput != null)
            usernameInput.text = "TestPlayer_" + Random.Range(1000, 9999);
        
        // Setup button events
        if (submitButton != null)
            submitButton.onClick.AddListener(TestSubmitScore);
        if (togglePanelButton != null)
            togglePanelButton.onClick.AddListener(TogglePanel);
        
        UpdateStatus("Test utility ready");
    }
    
    void Update()
    {
        // Toggle panel with key
        if (Input.GetKeyDown(toggleKey))
        {
            TogglePanel();
        }
    }
    
    public void TogglePanel()
    {
        if (testPanel != null)
        {
            testPanel.SetActive(!testPanel.activeSelf);
        }
    }
    
    public void TestSubmitScore()
    {
        if (scoreSubmitter == null)
        {
            UpdateStatus("❌ ScoreSubmitter not found!");
            return;
        }
        
        // Get values from input fields
        
        int score = Random.Range(10, 100000);
        string gameID = gameIDInput.text;
        string userID = userIDInput.text;
        string username = usernameInput.text;
        
        UpdateStatus($"📤 Submitting test score: {score}...");
        
        // Create a temporary test submission
        StartCoroutine(TestSubmissionCoroutine(score, gameID, userID, username));
    }
    
    private System.Collections.IEnumerator TestSubmissionCoroutine(int score, string gameID, string userID, string username)
    {
        var submissionData = new ScoreSubmissionData
        {
            gameID = gameID,
            score = score,
            userID = userID,
            username = username
        };
        
        string jsonData = JsonUtility.ToJson(submissionData);
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        
        using (UnityEngine.Networking.UnityWebRequest request = new UnityEngine.Networking.UnityWebRequest($"{scoreSubmitter.apiBaseUrl}/scores", "POST"))
        {
            request.uploadHandler = new UnityEngine.Networking.UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new UnityEngine.Networking.DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            
            yield return request.SendWebRequest();
            
            if (request.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                string responseText = request.downloadHandler.text;
                var response = JsonUtility.FromJson<ScoreResponse>(responseText);
                
                if (response.success)
                {
                    UpdateStatus($"✅ Success! Rank: #{response.data.ranking}, Score: {response.data.score:N0}");
                }
                else
                {
                    UpdateStatus($"❌ API Error: {response.message}");
                }
            }
            else
            {
                UpdateStatus($"❌ Network Error: {request.error}");
            }
        }
    }
    
    private void UpdateStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = $"[{System.DateTime.Now:HH:mm:ss}] {message}";
        }
        Debug.Log($"ScoreTestUI: {message}");
    }
    
    [ContextMenu("Quick Test")]
    public void QuickTest()
    {
        TestSubmitScore();
    }
    
    [ContextMenu("Test High Score")]
    public void TestHighScore()
    {
        if (scoreInput != null)
            scoreInput.text = Random.Range(50000, 100000).ToString();
        TestSubmitScore();
    }
    
    [ContextMenu("Test Low Score")]
    public void TestLowScore()
    {
        if (scoreInput != null)
            scoreInput.text = Random.Range(100, 1000).ToString();
        TestSubmitScore();
    }
}