using UnityEngine;

// Orchestrates the "each round must enter username" flow.
public class RoundController : MonoBehaviour
{
    [Header("References")]
    public UsernamePromptController usernamePrompt;
    public StageSpawner stageSpawner;   // assign your existing StageSpawner

    void OnEnable()
    {
        UsernameService.OnUsernameSet += OnUsernameConfirmed;
    }

    void OnDisable()
    {
        UsernameService.OnUsernameSet -= OnUsernameConfirmed;
    }

    void Start()
    {
        // Start the very first round by prompting for a username
        StartNewRound();
    }

    public void StartNewRound()
    {
        // Clear the previous round's username, force prompt again
        UsernameService.ClearUsername();

        // Reset StageSpawner so it starts from stage 0 after name entry
        if (stageSpawner != null)
        {
            stageSpawner.ResetForNewRound();
        }

        // Show prompt (pauses time until confirmed)
        if (usernamePrompt != null)
        {
            usernamePrompt.ShowPrompt();
        }
        else
        {
            Debug.LogWarning("[RoundController] UsernamePromptController not assigned.");
        }
    }

    private void OnUsernameConfirmed(string name)
    {
        Debug.Log($"[RoundController] Username confirmed for round: {name}");
        // StageSpawner will begin automatically once it detects a username (see small gate below)
    }

    // Optionally call this when the game ends to immediately start a new round
    public void OnGameEnded_StartNextRound()
    {
        StartNewRound();
    }
}