using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Button))]
public class SimpleSceneIndexButton : MonoBehaviour
{
    public enum LoadMode
    {
        AbsoluteIndex,  // Jump to specific index
        RelativeOffset, // Move by delta (e.g., +1 next, -1 previous)
        RestartCurrent
    }

    [Header("Mode")]
    public LoadMode mode = LoadMode.AbsoluteIndex;

    [Header("Absolute (Mode: AbsoluteIndex)")]
    public int targetIndex = 0;     // e.g., 2 -> 0 supported by setting targetIndex = 0

    [Header("Relative (Mode: RelativeOffset)")]
    public int offset = +1;         // +1 next, -1 previous

    [Header("Options")]
    public bool wrap = true;        // allow going 2 -> 0 (wrap around)
    public bool useTransition = true;
    public bool disableButtonDuringTransition = true;

    [Header("Audio")]
    public AudioClip clickSound;

    private Button button;
    private AudioSource audioSource;

    void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && clickSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    void OnEnable()
    {
        SceneTransitionManager.OnTransitionFinished += Reenable;
    }

    void OnDisable()
    {
        SceneTransitionManager.OnTransitionFinished -= Reenable;
    }

    private void Reenable()
    {
        if (button != null) button.interactable = true;
    }

    private void OnClick()
    {
        if (SceneTransitionManager.IsTransitioning) return;

        if (clickSound != null && audioSource != null)
            audioSource.PlayOneShot(clickSound);

        if (disableButtonDuringTransition && button != null)
            button.interactable = false;

        switch (mode)
        {
            case LoadMode.AbsoluteIndex:
                SceneTransitionManager.LoadSceneIndex(targetIndex, wrap, useTransition);
                break;

            case LoadMode.RelativeOffset:
                SceneTransitionManager.LoadSceneRelative(offset, wrap, useTransition);
                break;

            case LoadMode.RestartCurrent:
                SceneTransitionManager.RestartCurrent(useTransition);
                break;
        }
    }
}