using UnityEngine;
using System.Collections;

public abstract class StageScript : ScriptableObject
{
    [Header("Script Settings")]
    public string scriptName = "Unnamed Script";
    [TextArea(2, 4)]
    public string description = "Description of what this script does";
    
    [Header("Execution")]
    public bool blockPatternGroups = true; // Should pattern groups wait for this script to finish?
    public float maxExecutionTime = 10f; // Safety timeout
    
    /// <summary>
    /// Execute the stage script. Return a coroutine if the script needs time to complete.
    /// Return null for instant execution.
    /// </summary>
    /// <param name="stageSpawner">Reference to the stage spawner</param>
    /// <param name="stageIndex">Current stage index</param>
    /// <returns>Coroutine if script needs time, null if instant</returns>
    public abstract IEnumerator Execute(StageSpawner stageSpawner, int stageIndex);
    
    /// <summary>
    /// Called when the script is interrupted or times out
    /// </summary>
    public virtual void OnInterrupted()
    {
        Debug.LogWarning($"[StageScript] {scriptName} was interrupted or timed out");
    }
}