using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject firstSelectedButton;

    [Header("Input Action Reference")]
    [SerializeField] private InputActionReference pauseAction;

    [Header("Player Input Reference")]
    [SerializeField] private PlayerInput playerInput;

    private bool isPaused = false;

    private void OnEnable()
    {
        if (pauseAction != null)
        {
            pauseAction.action.Enable();
            pauseAction.action.performed += OnPausePerformed;
        }
    }

    private void OnDisable()
    {
        if (pauseAction != null)
        {
            pauseAction.action.performed -= OnPausePerformed;
            pauseAction.action.Disable();
        }

        AudioListener.pause = false;
    }

    private void OnPausePerformed(InputAction.CallbackContext context)
    {
        TogglePause();
    }

    public void TogglePause()
    {
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        pausePanel.SetActive(true);
        Time.timeScale = 0f; // Pause game time (physics, timers, etc.)
        AudioListener.pause = true;

        // Deactivates Player's Action Map so that Link ignores the keyboard/controller
        if (playerInput != null)
        {
            playerInput.SwitchCurrentActionMap("UI");
        }

        // Resume button will be the first selected for keyboard/controller
        if (EventSystem.current != null && firstSelectedButton != null)
        {
            EventSystem.current.SetSelectedGameObject(firstSelectedButton);
        }
    }

    public void ResumeGame()
    {
        isPaused = false;
        pausePanel.SetActive(false);
        Time.timeScale = 1f; // Resumes the time normally
        AudioListener.pause = false;

        // Gives back control to the player (Link)
        if (playerInput != null)
        {
            playerInput.SwitchCurrentActionMap("Player");
        }
    }

    public void LoadMainMenu(string mainMenuSceneName)
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}