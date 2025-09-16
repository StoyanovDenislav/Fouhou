using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class DialogueManager : MonoBehaviour
{
    public GameObject dialoguePanel;
    public TMP_Text speakerText;
    public TMP_Text dialogueText;
    public Image portrait;
    public GameObject dialogueBox;
    
    [Header("Typewriter Settings")]
    public float typeSpeed = 0.05f; // Time between each character

    private DialogueSequence currentSequence;
    private int currentLineIndex = 0;
    private bool active = false;
    private bool isTyping = false;
    private Coroutine typewriterCoroutine;

    // Store the original timeScale to restore it properly
    private float originalTimeScale = 1f;

    void Start()
    {
        speakerText.text = "";
        dialogueText.text = "";
        portrait.sprite = null;
        dialogueBox.SetActive(false);
       
        
        // Set the Image component to preserve aspect ratio
        if (portrait != null)
        {
            portrait.type = Image.Type.Simple;
            portrait.preserveAspect = true;
        }
    }

    void Update()
    {
        if (!active) return;

        // Use unscaled input so it works even when time is paused
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            if (isTyping)
            {
                // If currently typing, complete the line instantly
                CompleteLineInstantly();
            }
            else
            {
                // If not typing, move to next line
                NextLine();
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            EndDialogue();
        }
    }

    public void StartDialogue(DialogueSequence sequence)
    {
        currentSequence = sequence;
        currentLineIndex = 0;
        active = true;
        dialoguePanel.SetActive(true);
        dialogueBox.SetActive(true);

        // Store original timeScale and pause the game
        originalTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        ShowLine();
    }

    void ShowLine()
    {
        if (currentLineIndex >= currentSequence.lines.Length)
        {
            EndDialogue();
            return;
        }

        DialogueLine line = currentSequence.lines[currentLineIndex];
        speakerText.text = line.speakerName;

        // Set portrait
        if (portrait != null && line.portrait != null)
        {
            portrait.sprite = line.portrait;
            portrait.color = Color.white; // Make sure it's visible
        }
        else if (portrait != null)
        {
            portrait.sprite = null;
            portrait.color = Color.clear; // Hide when no sprite
        }

        // Start typewriter effect for dialogue text
        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
        }
        typewriterCoroutine = StartCoroutine(TypewriterEffect(line.text));
    }

    IEnumerator TypewriterEffect(string fullText)
    {
        isTyping = true;
        dialogueText.text = "";

        for (int i = 0; i <= fullText.Length; i++)
        {
            dialogueText.text = fullText.Substring(0, i);
            yield return new WaitForSecondsRealtime(typeSpeed); // Use realtime so it works when Time.timeScale = 0
        }

        isTyping = false;
    }

    void CompleteLineInstantly()
    {
        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
        }

        // Show the full text immediately
        if (currentLineIndex < currentSequence.lines.Length)
        {
            DialogueLine line = currentSequence.lines[currentLineIndex];
            dialogueText.text = line.text;
        }

        isTyping = false;
    }

    public void NextLine()
    {
        if (isTyping) return; // Prevent advancing while typing

        currentLineIndex++;
        ShowLine();
    }

    void EndDialogue()
    {
        // Stop any ongoing typewriter effect
        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
        }

        active = false;
        isTyping = false;
        dialoguePanel.SetActive(false);
        dialogueBox.SetActive(false);

        // Hide portrait when dialogue ends
        if (portrait != null)
        {
            portrait.sprite = null;
            portrait.color = Color.clear;
        }

        // Restore the original timeScale
        Time.timeScale = originalTimeScale;

        Debug.Log("Dialogue finished.");
    }

    public bool IsActive() => active;
}