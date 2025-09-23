using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UsernamePromptController : MonoBehaviour
{
    [Header("UI References")]
    public GameObject panel;        // Fullscreen blocking panel
    public TMP_InputField inputField;   
    public Button confirmButton;
    public TMP_Text errorText;          // Optional

    [Header("Validation")]
    public int minLength = 3;
    public int maxLength = 16;
    public bool restrictChars = true;

    private bool wePausedTime;

    void Awake()
    {
        if (panel != null) panel.SetActive(false);
    }

    void OnEnable()
    {
        if (confirmButton != null)
            confirmButton.onClick.AddListener(Confirm);
    }

    void OnDisable()
    {
        if (confirmButton != null)
            confirmButton.onClick.RemoveListener(Confirm);
    }

    void Update()
    {
        if (panel != null && panel.activeInHierarchy)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                Confirm();
            }
        }
    }

    public void ShowPrompt()
    {
        if (panel != null) panel.SetActive(true);

        wePausedTime = Time.timeScale != 0f;
        Time.timeScale = 0f;

        if (inputField != null)
        {
            inputField.text = UsernameService.GetSuggestion();
            FocusInput();
        }

        if (errorText != null) errorText.text = string.Empty;
    }

    public void HidePrompt()
    {
        if (panel != null) panel.SetActive(false);

        if (wePausedTime)
            Time.timeScale = 1f;
    }

    private void Confirm()
    {
        if (inputField == null) return;

        string name = (inputField.text ?? string.Empty).Trim();

        if (name.Length < minLength)
        {
            SetError($"Name must be at least {minLength} characters.");
            return;
        }
        if (name.Length > maxLength)
        {
            SetError($"Name must be at most {maxLength} characters.");
            return;
        }
        if (restrictChars && !IsValidName(name))
        {
            SetError("Only letters, numbers, and underscore are allowed.");
            return;
        }

        UsernameService.SetUsername(name);
        HidePrompt();
    }

    private void SetError(string msg)
    {
        if (errorText != null) errorText.text = msg;
        Debug.LogWarning($"[UsernamePrompt] {msg}");
    }

    private bool IsValidName(string s)
    {
        for (int i = 0; i < s.Length; i++)
        {
            char c = s[i];
            if (!(char.IsLetterOrDigit(c) || c == '_'))
                return false;
        }
        return true;
    }

    private void FocusInput()
    {
        if (inputField == null) return;

        var es = EventSystem.current;
        if (es != null) es.SetSelectedGameObject(inputField.gameObject);
        inputField.Select();
        inputField.ActivateInputField();
    }
}