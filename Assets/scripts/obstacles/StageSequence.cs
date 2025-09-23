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
    
    [Header("🎬 Pre-Stage Scripts")]
    [Tooltip("Scripts that execute before any pattern groups start (entrance animations, cutscenes, etc.)")]
    public StageScript[] preStageScripts;
    
    public PatternGroup[] patternGroups;
    
    [Header("🎭 Post-Stage Scripts")]
    [Tooltip("Scripts that execute after all pattern groups finish (exit animations, cleanup, etc.)")]
    public StageScript[] postStageScripts;
    
    [Header("🎵 Stage Music")]
    public AudioClip stageMusic;
    [Range(0f, 1f)]
    public float musicVolume = 0.7f;
    public bool loopMusic = true;
    
    [Header("🎼 Music Transition")]
    public float fadeInDuration = 2f;
    public float fadeOutDuration = 1.5f;
}