using System;
using UnityEngine;

public class UsernameService : MonoBehaviour
{
    public static UsernameService Instance { get; private set; }

    // Fired when player confirms a username for the current round
    public static event Action<string> OnUsernameSet;

    private static string _roundUsername;
    public static bool HasUsername => !string.IsNullOrWhiteSpace(_roundUsername);
    public static string Username => _roundUsername;

    [Header("Suggestion")]
    [Tooltip("Prefix used when proposing a random name each round.")]
    public string defaultSuggestionPrefix = "Player_";

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Always clear at boot so the first round asks for a name
        _roundUsername = null;
    }

    public static void ClearUsername()
    {
        _roundUsername = null;
    }

    public static void SetUsername(string name)
    {
        _roundUsername = (name ?? string.Empty).Trim();
        OnUsernameSet?.Invoke(_roundUsername);
        Debug.Log($"[UsernameService] Round username set: {_roundUsername}");
    }

    public static string GetSuggestion()
    {
        string prefix = Instance != null ? Instance.defaultSuggestionPrefix : "Player_";
        return $"{prefix}{UnityEngine.Random.Range(1000, 9999)}";
    }
}