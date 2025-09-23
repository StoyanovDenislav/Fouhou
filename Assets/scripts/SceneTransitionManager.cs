using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneTransitionManager : MonoBehaviour
{
    [Header("Transition")]
    public float fadeOutDuration = 0.6f;
    public float fadeInDuration = 0.6f;
    public bool fadeInOnFirstScene = true;
    public bool debugLogs = false;

    [Header("Loading")]
    [Tooltip("Use async loading and only activate the scene when the screen is already black.")]
    public bool useAsyncLoading = true;

    [Tooltip("Extra hold on black before activating the new scene (in seconds, realtime).")]
    public float holdBlackBeforeActivate = 0.05f;

    [Tooltip("Extra hold on black after the new scene is loaded, before starting the fade-in (in seconds, realtime).")]
    public float postLoadBlackHold = 0.0f;

    private static SceneTransitionManager instance;
    private static GameObject overlayGO;
    private static Canvas overlayCanvas;
    private static Image fadeImage;

    // State
    private static bool isTransitioning = false;
    public static bool IsTransitioning => isTransitioning;

    private static bool shouldFadeInOnLoad = false;
    private static bool didInitialFade = false;

    // Events
    public static event Action OnTransitionStarted;
    public static event Action OnTransitionFinished;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            if (debugLogs) Debug.Log("[STM] Duplicate destroyed");
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        EnsureOverlay();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Start()
    {
        // Do a one-time fade-in for the first loaded scene (optional)
        if (!didInitialFade && fadeInOnFirstScene)
        {
            didInitialFade = true;
            // Ensure black start to fade from
            overlayCanvas.enabled = true;
            fadeImage.color = Color.black;
            StartCoroutine(FadeInCoroutine());
        }
    }

    void OnDestroy()
    {
        if (instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (debugLogs) Debug.Log($"[STM] Scene loaded: {scene.name}, shouldFadeInOnLoad={shouldFadeInOnLoad}");
        if (shouldFadeInOnLoad)
        {
            StartCoroutine(FadeInAfterHold());
            shouldFadeInOnLoad = false;
        }
    }

    private IEnumerator FadeInAfterHold()
    {
        if (postLoadBlackHold > 0f)
            yield return new WaitForSecondsRealtime(postLoadBlackHold);

        yield return FadeInCoroutine();
    }

    private static void EnsureOverlay()
    {
        if (overlayGO != null && overlayCanvas != null && fadeImage != null) return;

        overlayGO = GameObject.Find("SceneTransitionOverlay");
        if (overlayGO == null)
        {
            overlayGO = new GameObject("SceneTransitionOverlay");
            DontDestroyOnLoad(overlayGO);

            overlayCanvas = overlayGO.AddComponent<Canvas>();
            overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            overlayCanvas.sortingOrder = 1000;
            overlayCanvas.enabled = false;

            var scaler = overlayGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            overlayGO.AddComponent<GraphicRaycaster>();

            var fadeObj = new GameObject("FadeImage");
            fadeObj.transform.SetParent(overlayGO.transform, false);
            fadeImage = fadeObj.AddComponent<Image>();
            fadeImage.color = Color.clear;
            fadeImage.raycastTarget = true; // block clicks during transition

            var rt = fadeObj.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;
            rt.anchoredPosition = Vector2.zero;
        }
        else
        {
            overlayCanvas = overlayGO.GetComponent<Canvas>();
            fadeImage = overlayGO.GetComponentInChildren<Image>();
        }
    }

    // Public API: absolute index with wrap option
    public static void LoadSceneIndex(int index, bool wrap = true, bool useTransition = true)
    {
        if (instance == null)
        {
            Debug.LogWarning("[STM] No manager found, loading instantly.");
            SceneManager.LoadScene(NormalizeIndex(index, wrap));
            return;
        }
        if (isTransitioning) return;

        int target = NormalizeIndex(index, wrap);
        if (useTransition)
        {
            instance.StartCoroutine(instance.FadeOutThenLoadIndex(target));
        }
        else
        {
            SceneManager.LoadScene(target);
        }
    }

    // Public API: relative move (e.g., -1 previous, +1 next)
    public static void LoadSceneRelative(int delta, bool wrap = true, bool useTransition = true)
    {
        int current = SceneManager.GetActiveScene().buildIndex;
        LoadSceneIndex(current + delta, wrap, useTransition);
    }

    // Helpers
    public static void RestartCurrent(bool useTransition = true)
    {
        LoadSceneIndex(SceneManager.GetActiveScene().buildIndex, wrap: false, useTransition: useTransition);
    }

    public static void LoadFirst(bool useTransition = true) => LoadSceneIndex(0, wrap: false, useTransition: useTransition);
    public static void LoadLast(bool useTransition = true) => LoadSceneIndex(SceneManager.sceneCountInBuildSettings - 1, wrap: false, useTransition: useTransition);
    public static int SceneCount => SceneManager.sceneCountInBuildSettings;

    private static int NormalizeIndex(int index, bool wrap)
    {
        int count = SceneManager.sceneCountInBuildSettings;
        if (count <= 0)
        {
            Debug.LogError("[STM] No scenes in Build Settings.");
            return 0;
        }
        if (wrap)
        {
            // Proper modulo that handles negatives
            int m = ((index % count) + count) % count;
            return m;
        }
        // Clamp
        return Mathf.Clamp(index, 0, count - 1);
    }

    // Coroutines
    private IEnumerator FadeOutThenLoadIndex(int buildIndex)
    {
        isTransitioning = true;
        OnTransitionStarted?.Invoke();

        EnsureOverlay();

        if (debugLogs) Debug.Log($"[STM] FadeOut -> Load index {buildIndex}");
        yield return StartCoroutine(FadeOutCoroutine());

        // Keep the screen black and overlay enabled throughout load/activation
        shouldFadeInOnLoad = true;

        if (useAsyncLoading)
        {
            // Start loading but do not activate yet (stay black)
            AsyncOperation op = SceneManager.LoadSceneAsync(buildIndex);
            op.allowSceneActivation = false;

            // Optional pre-activation black hold
            if (holdBlackBeforeActivate > 0f)
                yield return new WaitForSecondsRealtime(holdBlackBeforeActivate);

            // Wait until the scene is loaded to 90%
            while (op.progress < 0.9f)
            {
                yield return null; // unscaled vs scaled doesn't matter here
            }

            // Another tiny hold (sometimes helps avoid a 'pop' depending on content)
            if (holdBlackBeforeActivate > 0f)
                yield return new WaitForSecondsRealtime(holdBlackBeforeActivate);

            // Now activate the scene. We remain black and will fade in in OnSceneLoaded.
            op.allowSceneActivation = true;
        }
        else
        {
            // Fallback to sync load (still safe because we're black here)
            SceneManager.LoadScene(buildIndex);
        }
    }

    private IEnumerator FadeOutCoroutine()
    {
        EnsureOverlay();

        overlayCanvas.enabled = true;
        fadeImage.raycastTarget = true;

        Color start = Color.clear;
        Color end = Color.black;

        float t = 0f;
        while (t < fadeOutDuration)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / fadeOutDuration);
            fadeImage.color = Color.Lerp(start, end, p);
            yield return null;
        }
        fadeImage.color = end;
    }

    private IEnumerator FadeInCoroutine()
    {
        EnsureOverlay();

        overlayCanvas.enabled = true;
        fadeImage.raycastTarget = true;

        Color start = Color.black;
        Color end = Color.clear;

        float t = 0f;
        while (t < fadeInDuration)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / fadeInDuration);
            fadeImage.color = Color.Lerp(start, end, p);
            yield return null;
        }

        fadeImage.color = end;
        fadeImage.raycastTarget = false;
        overlayCanvas.enabled = false;

        isTransitioning = false;
        OnTransitionFinished?.Invoke();

        if (debugLogs) Debug.Log("[STM] Transition finished");
    }
}