using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

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

    private void Start()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;

        ShowMainMenu();
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
        SetFocus(aboutFirstButton);
    }

    public void CloseAbout()
    {
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