using UnityEngine;
using System.Collections.Generic;

public class StageSpawner : MonoBehaviour
{
    public BulletPool bulletPool;
    public StageSequence[] stageSequences;

    [Header("Stage Settings")]
    public float safeMargin = 5f; // ⏳ seconds to wait after last bullet
    public float dialogueSafeDelay = 2f; // ⏳ seconds to wait after bullets clear before showing dialogue

    private float lastBulletTime = -1f;
    private bool stageClearTriggered = false;
    private bool stageEnded = false;
    private bool waitingForDialogue = false;
    private bool waitingForBulletsClear = false;
    private float bulletsClearTime = -1f;
    private DialogueSequence pendingDialogue = null;
    private bool waitingForGroupDelay = false;
    private float groupDelayTimer = 0f;
    private int lastProcessedDialogueGroup = -1; // Track which group's dialogue we've processed

    private DialogueManager dialogueManager;

    private class RunningPattern
    {
        public StageEntry entry;
        public float timer = 0f;
        public float fireTimer = 0f;
        public float delayTimer = 0f;
        public int groupIndex;
        public int patternIndex;
    }

    private List<RunningPattern> running = new List<RunningPattern>();
    private int currentStageIndex = 0;
    private int nextGroupIndex = 0;

    void Start()
    {
        dialogueManager = FindObjectOfType<DialogueManager>();
        LoadStage(0);
    }

    void Update()
    {
        if (stageEnded) return;
        if (stageSequences == null || stageSequences.Length == 0) return;

        StageSequence currentSequence = stageSequences[currentStageIndex];

        // Handle dialogue finishing
        if (waitingForDialogue && !dialogueManager.IsActive())
        {
            waitingForDialogue = false;
            Debug.Log("🎬 Dialogue finished, continuing to next group");
        }

        // Handle group delay
        if (waitingForGroupDelay)
        {
            groupDelayTimer -= Time.deltaTime;
            if (groupDelayTimer <= 0f)
            {
                waitingForGroupDelay = false;
                Debug.Log("⏰ Group delay finished, starting patterns");
                // Now actually start the patterns after delay
                StartPatternsInCurrentGroup(currentSequence);
            }
            return;
        }

        // Check if we're waiting for bullets to clear before showing dialogue
        if (waitingForBulletsClear)
        {
            if (AreBulletsCleared() && Time.time >= bulletsClearTime + dialogueSafeDelay)
            {
                if (pendingDialogue != null)
                {
                    StartDialogue(pendingDialogue);
                    pendingDialogue = null;
                }
                waitingForBulletsClear = false;
            }
            return;
        }

        // ⏸ Stop all gameplay if dialogue is active
        if (dialogueManager != null && dialogueManager.IsActive())
        {
            return;
        }

        float dt = Time.deltaTime;

        // Start next group when no patterns are running and we're not waiting
        if (running.Count == 0 && !waitingForDialogue && !waitingForBulletsClear && !waitingForGroupDelay && nextGroupIndex < currentSequence.patternGroups.Length)
        {
            StartNextGroup(currentSequence);
        }

        // Update running patterns
        for (int i = running.Count - 1; i >= 0; i--)
        {
            RunningPattern rp = running[i];
            rp.timer += dt;

            if (rp.delayTimer > 0f)
            {
                rp.delayTimer -= dt;
                continue;
            }

            rp.fireTimer += dt;

            if (rp.fireTimer >= rp.entry.pattern.fireRate)
            {
                rp.entry.pattern.Fire(transform, bulletPool, Time.time);
                rp.fireTimer = 0f;
                lastBulletTime = Time.time;
            }

            // Check if pattern finished
            if (rp.timer >= rp.entry.duration + rp.entry.startDelay)
            {
                running.RemoveAt(i);
                Debug.Log($"✅ Pattern {rp.patternIndex} from group {rp.groupIndex} finished");
            }
        }

        // Check for dialogue after all patterns in current group finish
        if (running.Count == 0 && !waitingForDialogue && !waitingForBulletsClear && !waitingForGroupDelay)
        {
            CheckForDialogueInCurrentGroup(currentSequence);
        }

        // ✅ Trigger stage-clear countdown once all groups are processed
        if (!stageClearTriggered && nextGroupIndex >= currentSequence.patternGroups.Length && running.Count == 0 && !waitingForDialogue && !waitingForBulletsClear)
        {
            stageClearTriggered = true;
            lastBulletTime = Time.time;
        }

        // ✅ Only end the stage after safeMargin has passed
        if (stageClearTriggered && Time.time >= lastBulletTime + safeMargin)
        {
            EndStage();
        }
    }

    private void StartNextGroup(StageSequence currentSequence)
    {
        if (nextGroupIndex >= currentSequence.patternGroups.Length)
            return;

        StageSequence.PatternGroup currentGroup = currentSequence.patternGroups[nextGroupIndex];
        
        Debug.Log($"🎯 Starting group {nextGroupIndex} with {currentGroup.patterns.Length} patterns");

        // Apply group delay if needed
        if (currentGroup.groupDelay > 0f)
        {
            waitingForGroupDelay = true;
            groupDelayTimer = currentGroup.groupDelay;
            Debug.Log($"⏳ Waiting {currentGroup.groupDelay}s before starting group {nextGroupIndex}");
            // DON'T increment nextGroupIndex yet - we'll do it after the delay
            return;
        }

        // No delay, start patterns immediately
        StartPatternsInCurrentGroup(currentSequence);
    }

    private void StartPatternsInCurrentGroup(StageSequence currentSequence)
    {
        StageSequence.PatternGroup currentGroup = currentSequence.patternGroups[nextGroupIndex];
        
        // Start all patterns in the group
        for (int i = 0; i < currentGroup.patterns.Length; i++)
        {
            StageEntry pattern = currentGroup.patterns[i];
            
            var rp = new RunningPattern 
            { 
                entry = pattern, 
                delayTimer = pattern.startDelay,
                groupIndex = nextGroupIndex,
                patternIndex = i
            };
            running.Add(rp);
            Debug.Log($"🔥 Starting pattern {i} from group {nextGroupIndex} (delay: {pattern.startDelay}s)");
        }

        nextGroupIndex++; // Move to next group after starting patterns
    }

    private void CheckForDialogueInCurrentGroup(StageSequence currentSequence)
    {
        // Check if the group that just finished has dialogue
        int finishedGroupIndex = nextGroupIndex - 1;
        
        // Only process dialogue if we haven't already processed it for this group
        if (finishedGroupIndex >= 0 && 
            finishedGroupIndex < currentSequence.patternGroups.Length && 
            finishedGroupIndex != lastProcessedDialogueGroup)
        {
            StageSequence.PatternGroup finishedGroup = currentSequence.patternGroups[finishedGroupIndex];
            
            if (finishedGroup.showDialogue && finishedGroup.dialogue != null)
            {
                pendingDialogue = finishedGroup.dialogue;
                waitingForBulletsClear = true;
                bulletsClearTime = Time.time;
                lastProcessedDialogueGroup = finishedGroupIndex; // Mark this group as processed
                Debug.Log($"💬 Scheduling dialogue from group {finishedGroupIndex} after bullets clear");
            }
            else
            {
                lastProcessedDialogueGroup = finishedGroupIndex; // Mark as processed even if no dialogue
                Debug.Log($"🚫 No dialogue in group {finishedGroupIndex}, continuing to next group");
            }
        }
    }

    private bool AreBulletsCleared()
    {
        var activeBullets = bulletPool.GetActive();
        return activeBullets.Count == 0;
    }

    private void StartDialogue(DialogueSequence dialogue)
    {
        if (!waitingForDialogue)
        {
            dialogueManager.StartDialogue(dialogue);
            waitingForDialogue = true;
        }
    }

    private void LoadStage(int index)
    {
        if (index >= stageSequences.Length)
        {
            Debug.Log("🎉 All stages completed!");
            stageEnded = true;
            return;
        }

        Debug.Log($"▶ Starting Stage {index + 1}/{stageSequences.Length}");
        currentStageIndex = index;
        nextGroupIndex = 0;
        running.Clear();
        stageClearTriggered = false;
        waitingForDialogue = false;
        waitingForBulletsClear = false;
        waitingForGroupDelay = false;
        lastProcessedDialogueGroup = -1; // Reset dialogue tracking
        pendingDialogue = null;
        groupDelayTimer = 0f;
    }

    public void EndStage()
    {
        stageEnded = true;

        var activeBullets = new List<GameObject>(bulletPool.GetActive());
        foreach (var bullet in activeBullets)
        {
            var obstacle = bullet.GetComponent<Obstacle>();
            if (obstacle != null)
            {
                obstacle.Speed = 0;
                obstacle.FreezeAndConvert();
            }
        }

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.EndStage();
            Debug.Log("Stage ended! Final Score: " + ScoreManager.Instance.GetScore());
        }

        stageEnded = false;
        LoadStage(currentStageIndex + 1);
    }
}