using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    public GameObject gameOverPanel;
    public GameObject gameVictoryPanel;

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

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // TODO: Setup the Main Menu
    public void ExitToMainMenu()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("MainMenuScene");
    }
}
