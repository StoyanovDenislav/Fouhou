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

    void Start()
    {
        currentUserID = SystemInfo.deviceUniqueIdentifier;
    }
    
    public void SubmitScore(int score)
    {
        StartCoroutine(SubmitScoreCoroutine(score));
    }
    
    private IEnumerator SubmitScoreCoroutine(int score)
    {
        string username = UsernameService.HasUsername
            ? UsernameService.Username
            : UsernameService.GetSuggestion(); // Fallback (shouldn't happen if we gate correctly)

        ScoreSubmissionData submissionData = new ScoreSubmissionData
        {
            gameID = gameID,
            score = score,
            userID = currentUserID,
            username = username
        };
        
        string jsonData = JsonUtility.ToJson(submissionData);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        
        using (UnityWebRequest request = new UnityWebRequest($"{apiBaseUrl}/scores", "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            
            Debug.Log($"Submitting score: {score} for {username}");
            
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseText = request.downloadHandler.text;
                ScoreResponse response = JsonUtility.FromJson<ScoreResponse>(responseText);
                
                if (response != null && response.success)
                {
                    Debug.Log($"✅ Score submitted successfully! Rank: #{response.data.ranking} Score: {response.data.score:N0}");
                    OnScoreSubmitted(response.data);
                }
                else
                {
                    string msg = (response != null) ? response.message : "Invalid server response";
                    Debug.LogError($"❌ Score submission failed: {msg}");
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
        if (ScoreManager.Instance != null)
        {
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
    
    [ContextMenu("Test Score Submission")]
    public void TestSubmission()
    {
        SubmitScore(UnityEngine.Random.Range(1000, 50000));
    }
}