using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
public class PauseMenu : MonoBehaviour
{

    [SerializeField] private GameObject pauseMenu;
    private float originalTimeScale;
    private bool isPaused = false;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        Time.timeScale = 1f;
        
        if(pauseMenu != null)
            pauseMenu.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            switch (isPaused)
            {
                case true:
                    Resume();
                    break;
                case false:
                    Pause();
                    break;
            }
        }
       

    }

    public void Pause()
    {
        isPaused = true;
        originalTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        pauseMenu.SetActive(true);
        Debug.Log(originalTimeScale);
    }

    public void Resume()
    {
        Debug.Log(originalTimeScale);
        pauseMenu.SetActive(false);
        isPaused = false;
        Time.timeScale = originalTimeScale;
    }

    public void Exit()
    { 
        Debug.Log(originalTimeScale);
        Resume();
        Debug.Log("🏠 Going to main menu...");
        SceneManager.LoadScene("MainMenu");
        Destroy(gameObject);
        
    }

    public void Quit()
    {
        Application.Quit();
    }
}
