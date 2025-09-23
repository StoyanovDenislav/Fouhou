/*using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class LeaderboardEntry
{
    public int ranking;
    public int score;
    public string userID;
    public string username;
    public bool isRanked;
}

[System.Serializable]
public class LeaderboardResponse
{
    public bool success;
    public LeaderboardData data;
}

[System.Serializable]
public class LeaderboardData
{
    public string gameID;
    public int limit;
    public LeaderboardEntry[] topScores;
}

public class LeaderboardManager : MonoBehaviour
{
    [Header("API Configuration")]
    public string serverUrl = "https://api.fouhou.stoyanography.com/api";
    public string gameID = "fouhou-v1";
    
    [Header("UI References")]
    public LeaderboardLayoutManager layoutManager;
    public GameObject loadingIndicator;
    public TMPro.TextMeshProUGUI errorText;
    
    [Header("Auto Refresh")]
    public bool autoRefresh = true;
    public float refreshInterval = 30f;
    
    private Coroutine refreshCoroutine;

    void Start()
    {
        if (errorText) errorText.gameObject.SetActive(false);
        
        if (autoRefresh)
        {
            refreshCoroutine = StartCoroutine(AutoRefreshCoroutine());
        }
        
        RefreshLeaderboard();
    }

    void OnDestroy()
    {
        if (refreshCoroutine != null)
        {
            StopCoroutine(refreshCoroutine);
        }
    }

    public void RefreshLeaderboard()
    {
        StartCoroutine(FetchTopScores());
    }

    IEnumerator AutoRefreshCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(refreshInterval);
            RefreshLeaderboard();
        }
    }

    IEnumerator FetchTopScores()
    {
        if (loadingIndicator) loadingIndicator.SetActive(true);
        if (errorText) errorText.gameObject.SetActive(false);

        string url = $"{serverUrl}/scores/top/{gameID}?limit=5";
        
        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();

        if (loadingIndicator) loadingIndicator.SetActive(false);

        if (request.result == UnityWebRequest.Result.Success)
        {
            try
            {
                LeaderboardResponse response = JsonUtility.FromJson<LeaderboardResponse>(request.downloadHandler.text);
                
                if (response.success && response.data != null)
                {
                    // Use the layout manager to update all entries
                    if (layoutManager)
                    {
                        layoutManager.UpdateLeaderboard(response.data.topScores);
                    }
                }
                else
                {
                    ShowError("Failed to parse leaderboard data");
                }
            }
            catch (Exception e)
            {
                ShowError($"Parse error: {e.Message}");
            }
        }
        else
        {
            ShowError($"Connection error: {request.error}");
        }

        request.Dispose();
    }

    void ShowError(string message)
    {
        Debug.LogError($"❌ Leaderboard error: {message}");
        if (errorText)
        {
            errorText.text = message;
            errorText.gameObject.SetActive(true);
        }
    }
}*/