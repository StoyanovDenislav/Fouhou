using UnityEngine;

[CreateAssetMenu(menuName = "BulletHell/StageSequence")]
public class StageSequence : ScriptableObject
{
    [System.Serializable]
    public class PatternGroup
    {
        [Header("Patterns")]
        public StageEntry[] patterns;
        
        [Header("Dialogue (optional)")]
        public bool showDialogue = false;
        public DialogueSequence dialogue;
        
        [Header("Timing")]
        public float groupDelay = 0f; // Wait before starting this group
    }
    
    public PatternGroup[] patternGroups;
}