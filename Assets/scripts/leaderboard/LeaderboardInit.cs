using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;

public class LeaderboardInit : MonoBehaviour
{
    [Header("API Configuration")]
    public string serverUrl = "https://api.fouhou.stoyanography.com/api";
    public string gameID = "fouhou-v1";
    
    [Header("Simple Settings")]
    public Vector2 leaderboardSize = new Vector2(800f, 600f); // Bigger for readability
    
    [Header("Auto Refresh")]
    public bool autoRefresh = true;
    public float refreshInterval = 30f;
    
    [Header("Debug")]
    public bool useTestData = false;
    
    // SUPER SIMPLE - Just 5 entries, no fancy scaling
    private Canvas canvas;
    private GameObject leaderboardPanel;
    private TextMeshProUGUI titleText;
    private Button refreshButton;
    private GameObject loadingText;
    private GameObject errorText;
    
    // Just 5 simple entry objects
    private GameObject[] entries = new GameObject[5];
    private TextMeshProUGUI[] rankTexts = new TextMeshProUGUI[5];
    private TextMeshProUGUI[] usernameTexts = new TextMeshProUGUI[5];
    private TextMeshProUGUI[] scoreTexts = new TextMeshProUGUI[5];
    private Image[] backgrounds = new Image[5];
    
    private Coroutine refreshCoroutine;
    private bool isUpdating = false;

    void Start()
    {
        Debug.Log("🎮 FOUHOU SIMPLE TOP 5");
        
        CreateSimpleUI();
        
        if (autoRefresh)
        {
            refreshCoroutine = StartCoroutine(AutoRefreshCoroutine());
        }
        
        if (useTestData)
        {
            CreateTestData();
        }
        else
        {
            RefreshLeaderboard();
        }
    }

    void CreateSimpleUI()
    {
        // Canvas
        GameObject canvasObj = new GameObject("SimpleCanvas");
        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // Main panel
        leaderboardPanel = new GameObject("SimpleLeaderboard");
        leaderboardPanel.transform.SetParent(canvas.transform, false);
        
        Image bg = leaderboardPanel.AddComponent<Image>();
        bg.color = new Color(0.1f, 0.1f, 0.2f, 0.95f);
        
        RectTransform panelRect = leaderboardPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = leaderboardSize;
        
        CreateTitle();
        CreateRefreshButton();
        CreateEntries();
        CreateFooter();
        
        Debug.Log("✅ Simple UI created!");
    }

    void CreateTitle()
    {
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(leaderboardPanel.transform, false);
        
        titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "FOUHOU TOP 5";
        titleText.fontSize = 40; // BIG title
        titleText.fontStyle = FontStyles.Bold;
        titleText.color = Color.white;
        titleText.alignment = TextAlignmentOptions.Center;
        
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 0.85f);
        titleRect.anchorMax = new Vector2(0.8f, 1f);
        titleRect.sizeDelta = Vector2.zero;
        titleRect.anchoredPosition = Vector2.zero;
    }

    void CreateRefreshButton()
    {
        GameObject buttonObj = new GameObject("RefreshButton");
        buttonObj.transform.SetParent(leaderboardPanel.transform, false);
        
        refreshButton = buttonObj.AddComponent<Button>();
        Image buttonImg = buttonObj.AddComponent<Image>();
        buttonImg.color = new Color(0.3f, 0.3f, 0.5f, 1f);
        
        RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.8f, 0.85f);
        buttonRect.anchorMax = new Vector2(1f, 1f);
        buttonRect.sizeDelta = Vector2.zero;
        buttonRect.anchoredPosition = Vector2.zero;
        
        GameObject buttonTextObj = new GameObject("ButtonText");
        buttonTextObj.transform.SetParent(buttonObj.transform, false);
        
        TextMeshProUGUI buttonText = buttonTextObj.AddComponent<TextMeshProUGUI>();
        buttonText.text = "REFRESH";
        buttonText.fontSize = 20;
        buttonText.color = Color.white;
        buttonText.alignment = TextAlignmentOptions.Center;
        
        RectTransform buttonTextRect = buttonTextObj.GetComponent<RectTransform>();
        buttonTextRect.anchorMin = Vector2.zero;
        buttonTextRect.anchorMax = Vector2.one;
        buttonTextRect.sizeDelta = Vector2.zero;
        buttonTextRect.anchoredPosition = Vector2.zero;
        
        refreshButton.onClick.AddListener(() => {
            if (!isUpdating) RefreshLeaderboard();
        });
    }

    void CreateEntries()
    {
        for (int i = 0; i < 5; i++)
        {
            CreateSingleEntry(i);
        }
    }

    void CreateSingleEntry(int index)
    {
        // Entry container
        entries[index] = new GameObject($"Entry{index + 1}");
        entries[index].transform.SetParent(leaderboardPanel.transform, false);
        
        // Position each entry
        float entryHeight = 0.12f; // 12% of panel height each
        float startY = 0.8f; // Start below title
        float yPos = startY - (index * entryHeight);
        
        RectTransform entryRect = entries[index].AddComponent<RectTransform>();
        entryRect.anchorMin = new Vector2(0.05f, yPos - entryHeight);
        entryRect.anchorMax = new Vector2(0.95f, yPos);
        entryRect.sizeDelta = Vector2.zero;
        entryRect.anchoredPosition = Vector2.zero;
        
        // Background
        backgrounds[index] = entries[index].AddComponent<Image>();
        SetEntryColor(index + 1);
        
        // Rank text
        GameObject rankObj = new GameObject("Rank");
        rankObj.transform.SetParent(entries[index].transform, false);
        
        rankTexts[index] = rankObj.AddComponent<TextMeshProUGUI>();
        rankTexts[index].text = $"#{index + 1}";
        rankTexts[index].fontSize = 32; // BIG font
        rankTexts[index].fontStyle = FontStyles.Bold;
        rankTexts[index].color = Color.black;
        rankTexts[index].alignment = TextAlignmentOptions.Center;
        
        RectTransform rankRect = rankObj.GetComponent<RectTransform>();
        rankRect.anchorMin = new Vector2(0f, 0f);
        rankRect.anchorMax = new Vector2(0.15f, 1f);
        rankRect.sizeDelta = Vector2.zero;
        rankRect.anchoredPosition = Vector2.zero;
        
        // Username text
        GameObject usernameObj = new GameObject("Username");
        usernameObj.transform.SetParent(entries[index].transform, false);
        
        usernameTexts[index] = usernameObj.AddComponent<TextMeshProUGUI>();
        usernameTexts[index].text = "Loading...";
        usernameTexts[index].fontSize = 28; // BIG font
        usernameTexts[index].fontStyle = FontStyles.Normal;
        usernameTexts[index].color = Color.black;
        usernameTexts[index].alignment = TextAlignmentOptions.Left;
        
        RectTransform usernameRect = usernameObj.GetComponent<RectTransform>();
        usernameRect.anchorMin = new Vector2(0.15f, 0f);
        usernameRect.anchorMax = new Vector2(0.65f, 1f);
        usernameRect.sizeDelta = Vector2.zero;
        usernameRect.anchoredPosition = Vector2.zero;
        usernameRect.offsetMin = new Vector2(10, 0); // Small padding
        
        // Score text
        GameObject scoreObj = new GameObject("Score");
        scoreObj.transform.SetParent(entries[index].transform, false);
        
        scoreTexts[index] = scoreObj.AddComponent<TextMeshProUGUI>();
        scoreTexts[index].text = "---";
        scoreTexts[index].fontSize = 28; // BIG font
        scoreTexts[index].fontStyle = FontStyles.Bold;
        scoreTexts[index].color = Color.black;
        scoreTexts[index].alignment = TextAlignmentOptions.Right;
        
        RectTransform scoreRect = scoreObj.GetComponent<RectTransform>();
        scoreRect.anchorMin = new Vector2(0.65f, 0f);
        scoreRect.anchorMax = new Vector2(1f, 1f);
        scoreRect.sizeDelta = Vector2.zero;
        scoreRect.anchoredPosition = Vector2.zero;
        scoreRect.offsetMax = new Vector2(-10, 0); // Small padding
    }

    void SetEntryColor(int rank)
    {
        switch (rank)
        {
            case 1: backgrounds[rank-1].color = new Color(1f, 0.8f, 0f, 1f); break; // Gold
            case 2: backgrounds[rank-1].color = new Color(0.8f, 0.8f, 0.8f, 1f); break; // Silver
            case 3: backgrounds[rank-1].color = new Color(0.8f, 0.4f, 0f, 1f); break; // Bronze
            case 4: backgrounds[rank-1].color = new Color(0.5f, 0.7f, 1f, 1f); break; // Blue
            case 5: backgrounds[rank-1].color = new Color(0.7f, 0.5f, 1f, 1f); break; // Purple
        }
    }

    void CreateFooter()
    {
        // Loading text
        GameObject loadingObj = new GameObject("Loading");
        loadingObj.transform.SetParent(leaderboardPanel.transform, false);
        
        loadingText = loadingObj;
        TextMeshProUGUI loading = loadingObj.AddComponent<TextMeshProUGUI>();
        loading.text = "Loading...";
        loading.fontSize = 24;
        loading.color = Color.white;
        loading.alignment = TextAlignmentOptions.Center;
        
        RectTransform loadingRect = loadingObj.GetComponent<RectTransform>();
        loadingRect.anchorMin = new Vector2(0f, 0f);
        loadingRect.anchorMax = new Vector2(1f, 0.1f);
        loadingRect.sizeDelta = Vector2.zero;
        loadingRect.anchoredPosition = Vector2.zero;
        
        // Error text
        GameObject errorObj = new GameObject("Error");
        errorObj.transform.SetParent(leaderboardPanel.transform, false);
        
        errorText = errorObj;
        TextMeshProUGUI error = errorObj.AddComponent<TextMeshProUGUI>();
        error.text = "";
        error.fontSize = 20;
        error.color = Color.red;
        error.alignment = TextAlignmentOptions.Center;
        
        RectTransform errorRect = errorObj.GetComponent<RectTransform>();
        errorRect.anchorMin = new Vector2(0f, 0f);
        errorRect.anchorMax = new Vector2(1f, 0.1f);
        errorRect.sizeDelta = Vector2.zero;
        errorRect.anchoredPosition = Vector2.zero;
        
        errorObj.SetActive(false);
    }

    void CreateTestData()
    {
        Debug.Log("Creating test data...");
        
        string[] testNames = {"RhythmKing", "BeatMaster", "SoundWave", "MusicLord", "TuneHero"};
        int[] testScores = {999999, 888888, 777777, 666666, 555555};
        
        for (int i = 0; i < 5; i++)
        {
            rankTexts[i].text = $"#{i + 1}";
            usernameTexts[i].text = testNames[i];
            scoreTexts[i].text = FormatScore(testScores[i]);
        }
        
        if (loadingText) loadingText.SetActive(false);
    }

    public void RefreshLeaderboard()
    {
        StartCoroutine(FetchData());
    }

    IEnumerator AutoRefreshCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(refreshInterval);
            if (!isUpdating) RefreshLeaderboard();
        }
    }

    IEnumerator FetchData()
    {
        isUpdating = true;
        
        if (loadingText) loadingText.SetActive(true);
        if (errorText) errorText.SetActive(false);

        string url = $"{serverUrl}/scores/top/{gameID}?limit=5";
        Debug.Log($"Fetching: {url}");

        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();

        if (loadingText) loadingText.SetActive(false);

        if (request.result == UnityWebRequest.Result.Success)
        {
            try
            {
                FouhouResponse response = JsonUtility.FromJson<FouhouResponse>(request.downloadHandler.text);
                
                if (response.success && response.data != null)
                {
                    UpdateEntries(response.data.topScores);
                    Debug.Log($"Updated with {response.data.topScores.Length} entries");
                }
                else
                {
                    ShowError("Failed to get data");
                }
            }
            catch (Exception e)
            {
                ShowError($"Error: {e.Message}");
            }
        }
        else
        {
            ShowError($"Connection failed: {request.error}");
        }

        request.Dispose();
        isUpdating = false;
    }

    void UpdateEntries(FouhouEntry[] data)
    {
        for (int i = 0; i < 5; i++)
        {
            if (i < data.Length)
            {
                rankTexts[i].text = $"#{data[i].ranking}";
                usernameTexts[i].text = data[i].username;
                scoreTexts[i].text = FormatScore(data[i].score);
            }
            else
            {
                rankTexts[i].text = $"#{i + 1}";
                usernameTexts[i].text = "---";
                scoreTexts[i].text = "---";
            }
        }
    }

    void ShowError(string message)
    {
        Debug.LogError(message);
        if (errorText)
        {
            errorText.GetComponent<TextMeshProUGUI>().text = message;
            errorText.SetActive(true);
        }
    }

    string FormatScore(int score)
    {
        if (score >= 1000000) return $"{score / 1000000f:F1}M";
        if (score >= 1000) return $"{score / 1000f:F1}K";
        return score.ToString("N0");
    }

    [ContextMenu("Toggle Test Data")]
    public void ToggleTestData()
    {
        useTestData = !useTestData;
        if (useTestData)
        {
            CreateTestData();
        }
        else
        {
            RefreshLeaderboard();
        }
    }

    void OnDestroy()
    {
        if (refreshCoroutine != null)
        {
            StopCoroutine(refreshCoroutine);
        }
    }

    [System.Serializable]
    public class FouhouEntry
    {
        public int ranking;
        public int score;
        public string userID;
        public string username;
        public bool isRanked;
    }

    [System.Serializable]
    public class FouhouResponse
    {
        public bool success;
        public FouhouData data;
    }

    [System.Serializable]
    public class FouhouData
    {
        public string gameID;
        public int limit;
        public FouhouEntry[] topScores;
    }
}