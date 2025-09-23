using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StageSpawner : MonoBehaviour
{
    public BulletPool bulletPool;
    public StageSequence[] stageSequences;
    
    [Header("Gating")]
    public bool requireUsernameEachRound = true;

    [Header("Stage Settings")]
    public float safeMargin = 5f;
    public float dialogueSafeDelay = 2f;

    [Header("🎵 Audio")]
    public AudioSource musicSource;
    public AudioSource sfxSource;
    public float masterMusicVolume = 1f;
    public float masterSFXVolume = 1f;

    [Header("🔫 Bullet Spawn Audio")]
    public AudioClip bulletSpawnSound;
    [Range(0f, 1f)]
    public float bulletSpawnVolume = 0.3f;
    public float minSpawnSoundInterval = 0.1f;
    public float maxSpawnSoundInterval = 0.5f;

    [Header("🎬 Transition Settings")]
    public bool waitForTransition = true;
    [Tooltip("Kept for backward compatibility; not used anymore.")]
    public float maxTransitionWaitTime = 5f;
    
    [Header("🎬 Script Execution")]
    public bool enablePreStageScripts = true;
    
    [Header("🎭 Post-Stage Scripts")]
    public bool enablePostStageScripts = true;

    // Pre-stage script fields
    private bool waitingForPreStageScripts = false;
    private bool preStageScriptsCompleted = false;
    private int currentPreStageScriptIndex = 0;
    private Coroutine currentScriptCoroutine = null;

    // Post-stage script fields
    private bool waitingForPostStageScripts = false;
    private bool postStageScriptsCompleted = false;
    private int currentPostStageScriptIndex = 0;
    private Coroutine currentPostStageScriptCoroutine = null;

    private float lastBulletTime = -1f;
    private bool stageClearTriggered = false;
    private bool stageEnded = false;
    private bool waitingForDialogue = false;
    private bool waitingForBulletsClear = false;
    private float bulletsClearTime = -1f;
    private DialogueSequence pendingDialogue = null;
    private bool waitingForGroupDelay = false;
    private float groupDelayTimer = 0f;
    private int lastProcessedDialogueGroup = -1;

    // Game completion tracking
    private bool gameCompleted = false;

    // Transition gate
    private bool stageInitialized = false;
    private bool gateActive = false;

    private DialogueManager dialogueManager;

    // Music transition variables
    private Coroutine musicTransitionCoroutine;
    private AudioClip currentMusicClip;
    private bool musicStartedForCurrentStage = false;

    // Bullet spawn audio variables
    private float lastSpawnSoundTime = -1f;
    private float nextSpawnSoundDelay = 0f;
    private bool anyPatternsActive = false;
    private int bulletsSpawnedThisFrame = 0;

    private class RunningPattern
    {
        public StageEntry entry;
        public float timer = 0f;
        public float fireTimer = 0f;
        public float delayTimer = 0f;
        public int groupIndex;
        public int patternIndex;
        public bool hasFiredThisFrame = false;
    }

    private List<RunningPattern> running = new List<RunningPattern>();
    private int currentStageIndex = 0;
    private int nextGroupIndex = 0;

    void OnEnable()
    {
        SceneTransitionManager.OnTransitionFinished += OnTransitionFinished;
        SceneTransitionManager.OnTransitionStarted += OnTransitionStarted;
    }

    void OnDisable()
    {
        SceneTransitionManager.OnTransitionFinished -= OnTransitionFinished;
        SceneTransitionManager.OnTransitionStarted -= OnTransitionStarted;
    }

    void Start()
    {
        dialogueManager = FindObjectOfType<DialogueManager>();

        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.playOnAwake = false;
            musicSource.spatialBlend = 0f;
        }

        if (sfxSource == null)
        {
            GameObject sfxGO = new GameObject("SFX_AudioSource");
            sfxGO.transform.SetParent(transform);
            sfxSource = sfxGO.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSource.spatialBlend = 0f;
        }

        if (waitForTransition && SceneTransitionManager.IsTransitioning)
        {
            gateActive = true;
        }
        else
        {
            InitializeStage();
        }
    }

    private void OnTransitionStarted()
    {
        if (waitForTransition)
            gateActive = true;
    }

    private void OnTransitionFinished()
    {
        if (!stageInitialized)
        {
            InitializeStage();
        }
        gateActive = false;
    }

    void Update()
    {
        if ((waitForTransition && SceneTransitionManager.IsTransitioning) || gateActive)
            return;
    
        if (requireUsernameEachRound && !UsernameService.HasUsername)
            return;

        if (stageEnded || gameCompleted) return;
        if (stageSequences == null || stageSequences.Length == 0) return;

        StageSequence currentSequence = stageSequences[currentStageIndex];

        // Handle pre-stage scripts execution
        if (enablePreStageScripts && !preStageScriptsCompleted && !waitingForPreStageScripts)
        {
            if (currentSequence.preStageScripts != null && currentSequence.preStageScripts.Length > 0)
            {
                ExecuteNextPreStageScript(currentSequence);
                return; // Don't continue with normal stage logic until scripts are done
            }
            else
            {
                preStageScriptsCompleted = true; // No scripts to execute
            }
        }

        // Wait for pre-stage scripts to complete before starting pattern groups
        if (enablePreStageScripts && !preStageScriptsCompleted)
            return;

        // Start music after pre-stage scripts complete
        if (!musicStartedForCurrentStage)
        {
            StartStageMusic(currentSequence);
            musicStartedForCurrentStage = true;
        }

        bulletsSpawnedThisFrame = 0;
        anyPatternsActive = false;

        // Handle dialogue finishing
        if (waitingForDialogue && (dialogueManager == null || !dialogueManager.IsActive()))
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

        // Stop all gameplay if dialogue is active (but keep music playing)
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
            rp.hasFiredThisFrame = false;

            if (rp.delayTimer > 0f)
            {
                rp.delayTimer -= dt;
                anyPatternsActive = true;
                continue;
            }

            anyPatternsActive = true;
            rp.fireTimer += dt;

            if (rp.fireTimer >= rp.entry.pattern.fireRate)
            {
                rp.entry.pattern.Fire(transform, bulletPool, Time.time);
                rp.fireTimer = 0f;
                lastBulletTime = Time.time;
                rp.hasFiredThisFrame = true;
                bulletsSpawnedThisFrame++;
            }

            if (rp.timer >= rp.entry.duration + rp.entry.startDelay)
            {
                running.RemoveAt(i);
                Debug.Log($"✅ Pattern {rp.patternIndex} from group {rp.groupIndex} finished");

                if (ScoreManager.Instance != null)
                {
                    ScoreManager.Instance.OnPatternCompleted(currentStageIndex, rp.groupIndex, rp.patternIndex);
                }
            }
        }

        HandleBulletSpawnAudio();

        // Check for dialogue after all patterns in current group finish
        if (running.Count == 0 && !waitingForDialogue && !waitingForBulletsClear && !waitingForGroupDelay)
        {
            CheckForDialogueInCurrentGroup(currentSequence);
        }

        // Trigger stage-clear countdown once all groups are processed
        if (!stageClearTriggered && nextGroupIndex >= currentSequence.patternGroups.Length && running.Count == 0 && !waitingForDialogue && !waitingForBulletsClear)
        {
            stageClearTriggered = true;
            lastBulletTime = Time.time;

            if (ScoreManager.Instance != null)
            {
                bool isLastStage = (currentStageIndex >= stageSequences.Length - 1);
                ScoreManager.Instance.OnAllGroupsCompleted(currentStageIndex, isLastStage);
            }
        }

        // Only end the stage after safeMargin has passed
        if (stageClearTriggered && Time.time >= lastBulletTime + safeMargin)
        {
            EndStage();
        }
    }

    private void InitializeStage()
    {
        if (stageInitialized) return;

        stageInitialized = true;
        Debug.Log("🚀 Initializing stage (post-transition if gated)");
        LoadStage(currentStageIndex);
    }

    private void HandleBulletSpawnAudio()
    {
        if (bulletSpawnSound == null || sfxSource == null) return;

        bool shouldPlaySpawnSound = anyPatternsActive && bulletsSpawnedThisFrame > 0;

        if (shouldPlaySpawnSound && Time.time >= lastSpawnSoundTime + nextSpawnSoundDelay)
        {
            int activePatterns = CountActiveFiringPatterns();
            float activityFactor = Mathf.Clamp01((float)activePatterns / 5f);
            float dynamicInterval = Mathf.Lerp(maxSpawnSoundInterval, minSpawnSoundInterval, activityFactor);
            dynamicInterval *= Random.Range(0.8f, 1.2f);

            PlayBulletSpawnSound();
            lastSpawnSoundTime = Time.time;
            nextSpawnSoundDelay = dynamicInterval;

            Debug.Log($"🔫 Bullet spawn sound played! Active patterns: {activePatterns}, Next delay: {dynamicInterval:F2}s");
        }
    }

    private int CountActiveFiringPatterns()
    {
        int count = 0;
        foreach (var pattern in running)
        {
            if (pattern.delayTimer <= 0f)
            {
                count++;
            }
        }
        return count;
    }

    private void PlayBulletSpawnSound()
    {
        if (bulletSpawnSound != null && sfxSource != null)
        {
            sfxSource.volume = bulletSpawnVolume * masterSFXVolume;
            sfxSource.pitch = Random.Range(0.9f, 1.1f);
            sfxSource.PlayOneShot(bulletSpawnSound);
        }
    }

    private void StartNextGroup(StageSequence currentSequence)
    {
        if (nextGroupIndex >= currentSequence.patternGroups.Length)
            return;

        StageSequence.PatternGroup currentGroup = currentSequence.patternGroups[nextGroupIndex];

        Debug.Log($"🎯 Starting group {nextGroupIndex} with {currentGroup.patterns.Length} patterns");

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnGroupStarted(currentStageIndex, nextGroupIndex);
        }

        if (currentGroup.groupDelay > 0f)
        {
            waitingForGroupDelay = true;
            groupDelayTimer = currentGroup.groupDelay;
            Debug.Log($"⏳ Waiting {currentGroup.groupDelay}s before starting group {nextGroupIndex}");
            return;
        }

        StartPatternsInCurrentGroup(currentSequence);
    }

    private void StartPatternsInCurrentGroup(StageSequence currentSequence)
    {
        StageSequence.PatternGroup currentGroup = currentSequence.patternGroups[nextGroupIndex];

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

        nextGroupIndex++;
    }

    private void CheckForDialogueInCurrentGroup(StageSequence currentSequence)
    {
        int finishedGroupIndex = nextGroupIndex - 1;

        if (finishedGroupIndex >= 0 &&
            finishedGroupIndex < currentSequence.patternGroups.Length &&
            finishedGroupIndex != lastProcessedDialogueGroup)
        {
            StageSequence.PatternGroup finishedGroup = currentSequence.patternGroups[finishedGroupIndex];

            if (ScoreManager.Instance != null)
            {
                bool isLastStage = (currentStageIndex >= stageSequences.Length - 1);
                bool isLastGroup = (finishedGroupIndex >= currentSequence.patternGroups.Length - 1);
                ScoreManager.Instance.OnGroupCompleted(currentStageIndex, finishedGroupIndex, isLastStage, isLastGroup);
            }

            if (finishedGroup.showDialogue && finishedGroup.dialogue != null)
            {
                pendingDialogue = finishedGroup.dialogue;
                waitingForBulletsClear = true;
                bulletsClearTime = Time.time;
                lastProcessedDialogueGroup = finishedGroupIndex;
                Debug.Log($"💬 Scheduling dialogue from group {finishedGroupIndex} after bullets clear");
            }
            else
            {
                lastProcessedDialogueGroup = finishedGroupIndex;
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
        if (!waitingForDialogue && dialogueManager != null)
        {
            dialogueManager.StartDialogue(dialogue);
            waitingForDialogue = true;
        }
    }

    private void LoadStage(int index)
    {
        if (stageSequences == null || stageSequences.Length == 0)
        {
            Debug.LogWarning("⚠️ No stage sequences assigned.");
            return;
        }

        if (index >= stageSequences.Length)
        {
            Debug.Log("🎉 All stages completed!");
            gameCompleted = true;

            if (musicSource != null && musicSource.isPlaying)
            {
                StartMusicTransition(null, 0f, 3f);
            }

            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.OnGameCompleted();
            }
            return;
        }

        currentStageIndex = Mathf.Clamp(index, 0, stageSequences.Length - 1);

        Debug.Log($"▶ Starting Stage {currentStageIndex + 1}/{stageSequences.Length}");
        nextGroupIndex = 0;
        running.Clear();
        stageClearTriggered = false;
        waitingForDialogue = false;
        waitingForBulletsClear = false;
        waitingForGroupDelay = false;
        lastProcessedDialogueGroup = -1;
        pendingDialogue = null;
        groupDelayTimer = 0f;
        musicStartedForCurrentStage = false;
        
        // Reset pre-stage script state
        waitingForPreStageScripts = false;
        preStageScriptsCompleted = false;
        currentPreStageScriptIndex = 0;
        if (currentScriptCoroutine != null)
        {
            StopCoroutine(currentScriptCoroutine);
            currentScriptCoroutine = null;
        }
        
        // Reset post-stage script state
        waitingForPostStageScripts = false;
        postStageScriptsCompleted = false;
        currentPostStageScriptIndex = 0;
        if (currentPostStageScriptCoroutine != null)
        {
            StopCoroutine(currentPostStageScriptCoroutine);
            currentPostStageScriptCoroutine = null;
        }

        lastSpawnSoundTime = -1f;
        nextSpawnSoundDelay = 0f;

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnStageStarted(currentStageIndex);
        }
    }

    private void StartStageMusic(StageSequence stageSequence)
    {
        if (stageSequence == null || stageSequence.stageMusic == null || musicSource == null)
        {
            Debug.Log("🔇 No music assigned for this stage");
            return;
        }

        if (currentMusicClip == stageSequence.stageMusic && musicSource.isPlaying)
        {
            Debug.Log("🎵 Same music already playing, keeping current track");
            return;
        }

        float fadeInTime = stageSequence.fadeInDuration;
        float fadeOutTime = stageSequence.fadeOutDuration;

        if (!musicSource.isPlaying || currentMusicClip == null)
        {
            fadeOutTime = 0f;
        }

        Debug.Log($"🎵 Starting stage music immediately: {stageSequence.stageMusic.name}");
        StartMusicTransition(stageSequence.stageMusic, stageSequence.musicVolume * masterMusicVolume, fadeInTime, fadeOutTime, stageSequence.loopMusic);
        currentMusicClip = stageSequence.stageMusic;
    }

    private void StartMusicTransition(AudioClip newClip, float targetVolume, float fadeInDuration, float fadeOutDuration = 0f, bool loop = true)
    {
        if (musicTransitionCoroutine != null)
        {
            StopCoroutine(musicTransitionCoroutine);
        }

        musicTransitionCoroutine = StartCoroutine(TransitionMusicCoroutine(newClip, targetVolume, fadeInDuration, fadeOutDuration, loop));
    }

    private IEnumerator TransitionMusicCoroutine(AudioClip newClip, float targetVolume, float fadeInDuration, float fadeOutDuration, bool loop)
    {
        float originalVolume = musicSource.volume;

        if (musicSource.isPlaying && fadeOutDuration > 0f)
        {
            float fadeOutTimer = 0f;
            while (fadeOutTimer < fadeOutDuration)
            {
                fadeOutTimer += Time.unscaledDeltaTime;
                float t = fadeOutTimer / fadeOutDuration;
                musicSource.volume = Mathf.Lerp(originalVolume, 0f, t);
                yield return null;
            }
            musicSource.Stop();
        }

        if (newClip != null)
        {
            musicSource.clip = newClip;
            musicSource.loop = loop;
            musicSource.volume = 0f;
            musicSource.Play();

            if (fadeInDuration > 0f)
            {
                float fadeInTimer = 0f;
                while (fadeInTimer < fadeInDuration)
                {
                    fadeInTimer += Time.unscaledDeltaTime;
                    float t = fadeInTimer / fadeInDuration;
                    musicSource.volume = Mathf.Lerp(0f, targetVolume, t);
                    yield return null;
                }
            }

            musicSource.volume = targetVolume;
            Debug.Log($"🎼 Music transition complete: {newClip.name}");
        }
        else
        {
            musicSource.volume = 0f;
            Debug.Log("🔇 Music faded out");
        }

        musicTransitionCoroutine = null;
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

        // Check if we should execute post-stage scripts
        if (enablePostStageScripts && currentStageIndex < stageSequences.Length)
        {
            StageSequence currentSequence = stageSequences[currentStageIndex];
            if (currentSequence.postStageScripts != null && currentSequence.postStageScripts.Length > 0)
            {
                Debug.Log("🎭 Starting post-stage scripts");
                StartCoroutine(ExecutePostStageScripts(currentSequence));
                return; // Don't proceed to next stage until post-scripts are done
            }
        }

        // No post-stage scripts or they're disabled, proceed directly
        ProceedToNextStageOrGameOver();
    }

    // Pre-stage script methods
    private void ExecuteNextPreStageScript(StageSequence stageSequence)
    {
        if (currentPreStageScriptIndex >= stageSequence.preStageScripts.Length)
        {
            preStageScriptsCompleted = true;
            Debug.Log("✅ All pre-stage scripts completed");
            return;
        }

        StageScript script = stageSequence.preStageScripts[currentPreStageScriptIndex];
        if (script == null)
        {
            Debug.LogWarning($"⚠️ Pre-stage script {currentPreStageScriptIndex} is null, skipping");
            currentPreStageScriptIndex++;
            return;
        }

        Debug.Log($"🎬 Executing pre-stage script {currentPreStageScriptIndex + 1}/{stageSequence.preStageScripts.Length}: {script.scriptName}");
        
        waitingForPreStageScripts = true;
        currentScriptCoroutine = StartCoroutine(ExecuteScriptWithTimeout(script));
    }

    private IEnumerator ExecuteScriptWithTimeout(StageScript script)
    {
        bool completed = false;
        bool timedOut = false;

        IEnumerator scriptCoroutine = script.Execute(this, currentStageIndex);
        
        if (scriptCoroutine != null)
        {
            Coroutine scriptExecution = StartCoroutine(scriptCoroutine);
            
            float timeoutTimer = 0f;
            while (!completed && !timedOut)
            {
                timeoutTimer += Time.deltaTime;
                
                if (timeoutTimer >= script.maxExecutionTime)
                {
                    timedOut = true;
                    StopCoroutine(scriptExecution);
                    script.OnInterrupted();
                    Debug.LogWarning($"⏰ Pre-stage script '{script.scriptName}' timed out after {script.maxExecutionTime}s");
                }
                else if (scriptExecution == null)
                {
                    completed = true;
                }
                
                yield return null;
            }
        }
        else
        {
            completed = true;
        }

        currentPreStageScriptIndex++;
        waitingForPreStageScripts = false;
        
        Debug.Log($"✅ Pre-stage script completed: {script.scriptName}");
    }

    // Post-stage script methods
    private IEnumerator ExecutePostStageScripts(StageSequence stageSequence)
    {
        waitingForPostStageScripts = true;
        postStageScriptsCompleted = false;
        currentPostStageScriptIndex = 0;

        Debug.Log($"🎭 Executing {stageSequence.postStageScripts.Length} post-stage scripts");

        while (currentPostStageScriptIndex < stageSequence.postStageScripts.Length)
        {
            StageScript script = stageSequence.postStageScripts[currentPostStageScriptIndex];
            
            if (script == null)
            {
                Debug.LogWarning($"⚠️ Post-stage script {currentPostStageScriptIndex} is null, skipping");
                currentPostStageScriptIndex++;
                continue;
            }

            Debug.Log($"🎭 Executing post-stage script {currentPostStageScriptIndex + 1}/{stageSequence.postStageScripts.Length}: {script.scriptName}");
            
            yield return ExecutePostStageScriptWithTimeout(script);
            currentPostStageScriptIndex++;
        }

        postStageScriptsCompleted = true;
        waitingForPostStageScripts = false;
        
        Debug.Log("✅ All post-stage scripts completed");
        
        ProceedToNextStageOrGameOver();
    }

    private IEnumerator ExecutePostStageScriptWithTimeout(StageScript script)
    {
        bool completed = false;
        bool timedOut = false;

        IEnumerator scriptCoroutine = script.Execute(this, currentStageIndex);
        
        if (scriptCoroutine != null)
        {
            currentPostStageScriptCoroutine = StartCoroutine(scriptCoroutine);
            
            float timeoutTimer = 0f;
            while (!completed && !timedOut)
            {
                timeoutTimer += Time.deltaTime;
                
                if (timeoutTimer >= script.maxExecutionTime)
                {
                    timedOut = true;
                    if (currentPostStageScriptCoroutine != null)
                    {
                        StopCoroutine(currentPostStageScriptCoroutine);
                    }
                    script.OnInterrupted();
                    Debug.LogWarning($"⏰ Post-stage script '{script.scriptName}' timed out after {script.maxExecutionTime}s");
                }
                else if (currentPostStageScriptCoroutine == null)
                {
                    completed = true;
                }
                
                yield return null;
            }
        }
        else
        {
            completed = true;
        }
        
        Debug.Log($"✅ Post-stage script completed: {script.scriptName}");
    }

    private void ProceedToNextStageOrGameOver()
    {
        stageEnded = false;
        
        // Check if this was the last stage
        if (currentStageIndex >= stageSequences.Length - 1)
        {
            Debug.Log("🎉 All stages completed! Game Over!");
            gameCompleted = true;
            
            // Call game over logic
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.OnGameCompleted();
            }
            
            OnGameCompleted();
        }
        else
        {
            Debug.Log("🎬 Moving to next stage");
            LoadStage(currentStageIndex + 1);
        }
    }

    private void OnGameCompleted()
    {
        Debug.Log("🏆 Game completed! Implement your game over screen/logic here");
        
        // Example implementations you might want:
        // - Show final score screen
        // - Save high score
        // - Return to main menu
        // - Show credits
        // - Enable restart option
        
        // For now, you could trigger a scene transition or show a game over UI
        // SceneTransitionManager.LoadScene("GameOverScene");
    }

    // Public control helpers
    public void JumpToStage(int index, bool wrap = false)
    {
        if (stageSequences == null || stageSequences.Length == 0) return;
        int target = NormalizeStageIndex(index, wrap);
        LoadStage(target);
    }

    public void NextStage(bool wrap = true) => JumpToStage(currentStageIndex + 1, wrap);
    public void PreviousStage(bool wrap = true) => JumpToStage(currentStageIndex - 1, wrap);
    public void RestartStage() => JumpToStage(currentStageIndex, wrap: false);

    private int NormalizeStageIndex(int index, bool wrap)
    {
        int count = stageSequences?.Length ?? 0;
        if (count <= 0) return 0;
        if (wrap)
        {
            int m = ((index % count) + count) % count;
            return m;
        }
        return Mathf.Clamp(index, 0, count - 1);
    }

    // Public getters for ScoreManager
    public int GetCurrentStageIndex() => currentStageIndex;
    public int GetTotalStages() => stageSequences?.Length ?? 0;
    public int GetCurrentGroupIndex() => nextGroupIndex - 1;
    public int GetTotalGroupsInCurrentStage() =>
        (stageSequences != null && currentStageIndex < stageSequences.Length)
            ? stageSequences[currentStageIndex].patternGroups.Length
            : 0;

    // Public methods for music control
    public void SetMasterMusicVolume(float volume)
    {
        masterMusicVolume = Mathf.Clamp01(volume);
        if (musicSource != null && musicSource.isPlaying)
        {
            StageSequence currentSequence = stageSequences[currentStageIndex];
            musicSource.volume = currentSequence.musicVolume * masterMusicVolume;
        }
    }

    public void SetMasterSFXVolume(float volume)
    {
        masterSFXVolume = Mathf.Clamp01(volume);
    }

    public void PauseMusic()
    {
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Pause();
        }
    }

    public void ResumeMusic()
    {
        if (musicSource != null && !musicSource.isPlaying && musicSource.clip != null)
        {
            musicSource.UnPause();
        }
    }

    public void ResetForNewRound()
    {
        stageInitialized = false;
        stageEnded = false;
        gameCompleted = false;
        stageClearTriggered = false;
        musicStartedForCurrentStage = false;

        currentStageIndex = 0;
        nextGroupIndex = 0;
        running.Clear();
        waitingForDialogue = false;
        waitingForBulletsClear = false;
        waitingForGroupDelay = false;
        pendingDialogue = null;
        lastProcessedDialogueGroup = -1;
        groupDelayTimer = 0f;
        
        // Reset pre-stage script state
        waitingForPreStageScripts = false;
        preStageScriptsCompleted = false;
        currentPreStageScriptIndex = 0;
        if (currentScriptCoroutine != null)
        {
            StopCoroutine(currentScriptCoroutine);
            currentScriptCoroutine = null;
        }
        
        // Reset post-stage script state
        waitingForPostStageScripts = false;
        postStageScriptsCompleted = false;
        currentPostStageScriptIndex = 0;
        if (currentPostStageScriptCoroutine != null)
        {
            StopCoroutine(currentPostStageScriptCoroutine);
            currentPostStageScriptCoroutine = null;
        }

        lastSpawnSoundTime = -1f;
        nextSpawnSoundDelay = 0f;
    }
}