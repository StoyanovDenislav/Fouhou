/*using UnityEngine;
using UnityEngine.UI;

public class LeaderboardLayoutManager : MonoBehaviour
{
    [Header("Layout Settings")]
    public int totalEntries = 5;
    public float spacing = 100f; // Distance between entries
    public bool useVerticalLayout = true; // true = vertical stack, false = horizontal
    
    [Header("Entry Template")]
    public GameObject entryTemplate; // Drag your LeaderboardEntryUI here
    
    [Header("Container Settings")]
    public Transform entriesContainer; // Optional: specific container for entries
    public bool useLayoutGroup = true; // Use Unity's Layout Group components
    
    [Header("Auto Setup")]
    public bool createEntriesOnStart = true;
    
    private GameObject[] leaderboardEntries;
    private Transform actualContainer;
    
    void Awake()
    {
        // Determine which container to use
        SetupContainer();
    }
    
    void Start()
    {
        if (createEntriesOnStart)
        {
            SetupLeaderboardEntries();
        }
    }
    
    void SetupContainer()
    {
        // Use specified container, or create one, or use this transform
        if (entriesContainer != null)
        {
            actualContainer = entriesContainer;
        }
        else
        {
            // Create a dedicated container as child of this object
            GameObject containerObj = new GameObject("LeaderboardEntries");
            containerObj.transform.SetParent(this.transform, false);
            
            // Add RectTransform for UI positioning
            RectTransform containerRect = containerObj.AddComponent<RectTransform>();
            containerRect.anchorMin = Vector2.zero;
            containerRect.anchorMax = Vector2.one;
            containerRect.sizeDelta = Vector2.zero;
            containerRect.anchoredPosition = Vector2.zero;
            
            actualContainer = containerObj.transform;
            entriesContainer = actualContainer; // Store reference
        }
        
        // Setup layout group if requested
        if (useLayoutGroup)
        {
            SetupLayoutGroup();
        }
    }
    
    void SetupLayoutGroup()
    {
        // Remove existing layout groups
        VerticalLayoutGroup vertLayout = actualContainer.GetComponent<VerticalLayoutGroup>();
        HorizontalLayoutGroup horizLayout = actualContainer.GetComponent<HorizontalLayoutGroup>();
        
        if (vertLayout) DestroyImmediate(vertLayout);
        if (horizLayout) DestroyImmediate(horizLayout);
        
        // Add appropriate layout group
        if (useVerticalLayout)
        {
            VerticalLayoutGroup layout = actualContainer.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = spacing;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childScaleWidth = false;
            layout.childScaleHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
        }
        else
        {
            HorizontalLayoutGroup layout = actualContainer.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = spacing;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childScaleWidth = false;
            layout.childScaleHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
        }
        
        // Add Content Size Fitter to automatically adjust container size
        ContentSizeFitter sizeFitter = actualContainer.gameObject.GetComponent<ContentSizeFitter>();
        if (sizeFitter == null)
        {
            sizeFitter = actualContainer.gameObject.AddComponent<ContentSizeFitter>();
        }
        
        if (useVerticalLayout)
        {
            sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            sizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        }
        else
        {
            sizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            sizeFitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
        }
    }
    
    [ContextMenu("Setup Leaderboard Entries")]
    public void SetupLeaderboardEntries()
    {
        if (actualContainer == null)
        {
            SetupContainer();
        }
        
        // Clear existing entries
        ClearExistingEntries();
        
        // Create array to hold all entries
        leaderboardEntries = new GameObject[totalEntries];
        
        if (entryTemplate != null)
        {
            // Move the template to the container first
            entryTemplate.transform.SetParent(actualContainer, false);
            entryTemplate.name = "LeaderboardEntry_1";
            leaderboardEntries[0] = entryTemplate;
            
            // If not using layout groups, position manually
            if (!useLayoutGroup)
            {
                RectTransform templateRect = entryTemplate.GetComponent<RectTransform>();
                Vector2 startPos = CalculateStartPosition();
                templateRect.anchoredPosition = startPos;
            }
            
            // Create additional entries as children of the container
            for (int i = 1; i < totalEntries; i++)
            {
                GameObject newEntry = Instantiate(entryTemplate, actualContainer);
                newEntry.name = $"LeaderboardEntry_{i + 1}";
                
                // If not using layout groups, position manually
                if (!useLayoutGroup)
                {
                    RectTransform entryRect = newEntry.GetComponent<RectTransform>();
                    Vector2 startPos = CalculateStartPosition();
                    Vector2 position = CalculatePosition(i, startPos);
                    entryRect.anchoredPosition = position;
                }
                
                // Store reference
                leaderboardEntries[i] = newEntry;
                
                Debug.Log($"✅ Created leaderboard entry {i + 1} as child of {actualContainer.name}");
            }
            
            Debug.Log($"🏆 Setup complete! All {totalEntries} entries are children of: {actualContainer.name}");
        }
        else
        {
            Debug.LogError("❌ Entry template is not assigned!");
        }
    }
    
    Vector2 CalculateStartPosition()
    {
        // Only used when not using layout groups
        float totalSize = (totalEntries - 1) * spacing;
        
        if (useVerticalLayout)
        {
            return new Vector2(0, totalSize / 2f);
        }
        else
        {
            return new Vector2(-totalSize / 2f, 0);
        }
    }
    
    Vector2 CalculatePosition(int index, Vector2 startPos)
    {
        // Only used when not using layout groups
        if (useVerticalLayout)
        {
            return new Vector2(startPos.x, startPos.y - (index * spacing));
        }
        else
        {
            return new Vector2(startPos.x + (index * spacing), startPos.y);
        }
    }
    
    void ClearExistingEntries()
    {
        // Clear all children from the container except the template
        if (actualContainer != null)
        {
            // Find the template first
            GameObject template = null;
            if (entryTemplate != null && entryTemplate.transform.parent == actualContainer)
            {
                template = entryTemplate;
            }
            
            // Destroy all children except the template
            for (int i = actualContainer.childCount - 1; i >= 0; i--)
            {
                Transform child = actualContainer.GetChild(i);
                if (child.gameObject != template)
                {
                    if (Application.isPlaying)
                    {
                        Destroy(child.gameObject);
                    }
                    else
                    {
                        DestroyImmediate(child.gameObject);
                    }
                }
            }
        }
    }
    
    // Method to update all entries with leaderboard data
    public void UpdateLeaderboard(LeaderboardEntry[] entries)
    {
        if (leaderboardEntries == null)
        {
            Debug.LogWarning("⚠️ Leaderboard entries not initialized. Call SetupLeaderboardEntries() first.");
            SetupLeaderboardEntries();
        }
        
        for (int i = 0; i < leaderboardEntries.Length; i++)
        {
            GameObject entryObj = leaderboardEntries[i];
            if (entryObj == null) continue;
            
            LeaderboardEntryUI entryUI = entryObj.GetComponent<LeaderboardEntryUI>();
            
            if (entryUI != null)
            {
                if (i < entries.Length)
                {
                    // Show real data
                    entryUI.SetupEntry(entries[i], i + 1);
                    entryObj.SetActive(true);
                }
                else
                {
                    // Show placeholder for empty slots
                    entryUI.SetupPlaceholder(i + 1);
                    entryObj.SetActive(true);
                }
            }
        }
        
        Debug.Log($"📊 Updated {entries.Length} leaderboard entries");
    }
    
    // Optional: Method to get specific entry for individual updates
    public LeaderboardEntryUI GetEntry(int index)
    {
        if (leaderboardEntries != null && index >= 0 && index < leaderboardEntries.Length && leaderboardEntries[index] != null)
        {
            return leaderboardEntries[index].GetComponent<LeaderboardEntryUI>();
        }
        return null;
    }
    
    // Method to get the container transform (useful for external references)
    public Transform GetContainer()
    {
        return actualContainer;
    }
    
    // Method to refresh layout (useful when changing settings at runtime)
    [ContextMenu("Refresh Layout")]
    public void RefreshLayout()
    {
        if (useLayoutGroup && actualContainer != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(actualContainer.GetComponent<RectTransform>());
        }
    }
    
    // Inspector helper to show hierarchy info
    void OnValidate()
    {
        if (Application.isPlaying && actualContainer != null)
        {
            // Update layout group settings if they changed
            if (useLayoutGroup)
            {
                SetupLayoutGroup();
            }
        }
    }
}*/