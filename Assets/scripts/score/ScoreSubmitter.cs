using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;

[System.Serializable]
public class ScoreSubmissionData
{
    public string gameID;
    public int score;
    public string userID;
    public string username;
}

[System.Serializable]
public class ScoreResponse
{
    public bool success;
    public string message;
    public ScoreData data;
}

[System.Serializable]
public class ScoreData
{
    public string rid;
    public string gameID;
    public int ranking;
    public int score;
    public string userID;
    public string username;
    public string timestamp;
}

public class ScoreSubmitter : MonoBehaviour
{
    [Header("API Settings")]
    public string apiBaseUrl = "https://api.fouhou.stoyanography.com/api";
    
    [Header("Game Settings")]
    public string gameID = "bullet-hell-v1";
    
    private string currentUserID;
    private string currentUsername;
    
    void Start()
    {
        // Generate or get user ID (you might want to save this persistently)
        currentUserID = SystemInfo.deviceUniqueIdentifier;
        currentUsername = "Player_" + UnityEngine.Random.Range(1000, 9999);
        
        Debug.Log($"Player initialized: {currentUsername} ({currentUserID})");
    }
    
    public void SubmitScore(int score)
    {
        StartCoroutine(SubmitScoreCoroutine(score));
    }
    
    private IEnumerator SubmitScoreCoroutine(int score)
    {
        ScoreSubmissionData submissionData = new ScoreSubmissionData
        {
            gameID = gameID,
            score = score,
            userID = currentUserID,
            username = currentUsername
        };
        
        string jsonData = JsonUtility.ToJson(submissionData);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        
        using (UnityWebRequest request = new UnityWebRequest($"{apiBaseUrl}/scores", "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            
            Debug.Log($"Submitting score: {score} for {currentUsername}");
            
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseText = request.downloadHandler.text;
                ScoreResponse response = JsonUtility.FromJson<ScoreResponse>(responseText);
                
                if (response.success)
                {
                    Debug.Log($"✅ Score submitted successfully!");
                    Debug.Log($"🏆 Ranking: #{response.data.ranking}");
                    Debug.Log($"📊 Score: {response.data.score:N0}");
                    
                    // Notify UI or other systems
                    OnScoreSubmitted(response.data);
                }
                else
                {
                    Debug.LogError($"❌ Score submission failed: {response.message}");
                }
            }
            else
            {
                Debug.LogError($"❌ Network error: {request.error}");
                Debug.LogError($"Response: {request.downloadHandler.text}");
            }
        }
    }
    
    private void OnScoreSubmitted(ScoreData scoreData)
    {
        // Handle successful score submission
        // Update UI, show ranking, etc.
        
        if (ScoreManager.Instance != null)
        {
            // You could add a method to ScoreManager to handle this
            Debug.Log($"Score submitted! Rank: {scoreData.ranking}");
        }
    }
    
    // Call this from your ScoreManager when game ends
    public void SubmitFinalScore()
    {
        if (ScoreManager.Instance != null)
        {
            int finalScore = ScoreManager.Instance.GetScore();
            SubmitScore(finalScore);
        }
    }
    
    // Test method you can call from inspector
    [ContextMenu("Test Score Submission")]
    public void TestSubmission()
    {
        SubmitScore(UnityEngine.Random.Range(1000, 50000));
    }
}