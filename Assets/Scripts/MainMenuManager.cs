using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject aboutPanel;

    [Header("First Selected Buttons (Controller/Keyboard)")]
    [SerializeField] private GameObject mainFirstButton;  // Start Button
    [SerializeField] private GameObject aboutFirstButton; // Back Button in AboutPanel

    [Header("Scene Loading")]
    [SerializeField] private string gameplaySceneName = "GameplayScene";

    [SerializeField] private SceneFader sceneFader;

    [Header("Audio")]
    [SerializeField] private AudioSource sfxAudioSource;
    [SerializeField] private AudioClip backButtonSfx;

    [Header("Credits Scroll")]
    [SerializeField] private ScrollRect creditsScrollRect;
    [SerializeField] private float gamepadScrollSpeed = 1.5f;

    private void Start()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;

        ShowMainMenu();
    }

    private void Update()
    {
        // Only hear the input if the About panel is opened
        if (aboutPanel != null && aboutPanel.activeSelf)
        {
            // If there's no button selected 
            if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == null)
            {
                bool hasNavInput = false;

                // and the player moves the stick 
                if (Gamepad.current != null && Gamepad.current.leftStick.ReadValue().magnitude > 0.2f)
                    hasNavInput = true;

                // or presses the arrows/WASD,
                if (Keyboard.current != null && (Keyboard.current.wKey.wasPressedThisFrame ||
                                                 Keyboard.current.sKey.wasPressedThisFrame ||
                                                 Keyboard.current.upArrowKey.wasPressedThisFrame ||
                                                 Keyboard.current.downArrowKey.wasPressedThisFrame))
                    hasNavInput = true;

                if (hasNavInput)
                {
                    SetFocus(aboutFirstButton); // Focus the BackButton
                }
            }

            bool cancelKeyPressed = Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame; // Esc key
            bool cancelButtonPressed = Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame; // Button South or B button

            if (cancelKeyPressed || cancelButtonPressed)
            {
                CloseAbout();
            }
        }
    }

    public void PlayGame()
    {
        AudioListener.pause = false;
        if (sceneFader != null)
            sceneFader.FadeToScene(gameplaySceneName);
        else
            SceneManager.LoadScene(gameplaySceneName);
    }

    public void OpenAbout()
    {
        mainMenuPanel.SetActive(false);
        aboutPanel.SetActive(true);

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    public void CloseAbout()
    {
        // Play the close sfx
        if (sfxAudioSource != null && backButtonSfx != null)
        {
            sfxAudioSource.PlayOneShot(backButtonSfx);
        }

        aboutPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
        SetFocus(mainFirstButton);
    }

    public void ExitGame()
    {
        StartCoroutine(ExitGameRoutine());
    }

    private IEnumerator ExitGameRoutine()
    {
        if (sceneFader != null)
        {
            // Dispara el fundido a negro (puedes reutilizar la función pasando un string vacío)
            sceneFader.FadeToScene("");
            yield return new WaitForSecondsRealtime(0.8f); // 0.8s to let fade and exit sfx play
        }
        else
        {
            yield return new WaitForSecondsRealtime(0.25f);
        }

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    private void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        if (aboutPanel != null) aboutPanel.SetActive(false);
        SetFocus(mainFirstButton);
    }

    private void SetFocus(GameObject targetButton)
    {
        if (EventSystem.current != null && targetButton != null)
        {
            EventSystem.current.SetSelectedGameObject(targetButton);
        }
    }
}