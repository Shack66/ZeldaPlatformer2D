using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class GameOverManager : MonoBehaviour
{
    public GameObject gameOverPanel;
    public GameObject gameVictoryPanel;

    [Header("First Selected Buttons for Gamepad")]
    [SerializeField] private GameObject gameOverFirstButton;
    [SerializeField] private GameObject victoryFirstButton;

    public static Vector3 lastCheckpointPosition;
    public static bool hasCheckpoint = false;

    // Automatic reset for testing 
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void ResetStaticsOnPlayMode()
    {
        hasCheckpoint = false;
    }

    public static void ResetCheckpoint()
    {
        hasCheckpoint = false;
    }

    public void EnableGameOver()
    {
        gameOverPanel.SetActive(true);
        // Stop the game
        Time.timeScale = 0;

        // Set Restart button when playing with Controller
        SetSelectedButton(gameOverFirstButton);
    }

    public void EnableVictory()
    {
        gameVictoryPanel.SetActive(true);
        // Stop the game
        Time.timeScale = 0;

        // Set Replay button when playing with Controller
        SetSelectedButton(victoryFirstButton);
    }

    public void SetSelectedButton(GameObject button)
    {
        if (EventSystem.current != null && button != null)
        {
            // Clears the actual selection and assign the new selection
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(button);
        }
    }

    // For "Restart" button in Game Over UI
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void VictoryReplay()
    {
        Time.timeScale = 1f;
        ResetCheckpoint(); // Clears the checkpoint to return to the very beginning of the level
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ExitToMainMenu()
    {
        Time.timeScale = 1;
        ResetCheckpoint();
        SceneManager.LoadScene("MainMenuScene");
    }
}
