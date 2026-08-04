using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    public GameObject gameOverPanel;
    public GameObject gameVictoryPanel;

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
    }

    public void EnableVictory()
    {
        gameVictoryPanel.SetActive(true);
        // Stop the game
        Time.timeScale = 0;
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
