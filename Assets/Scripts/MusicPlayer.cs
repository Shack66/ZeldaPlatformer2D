using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    [Header("Audio Channels")]
    public AudioSource introSource;
    public AudioSource loopSource;

    [Header("Victory Clips")]
    public AudioClip victoryIntroClip;
    public AudioClip victoryLoopClip;

    [Header("Game Over Clips")]
    [SerializeField] private AudioClip gameOverClip;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        PlayTrack();
    }

    // Changes music to victory theme
    public void PlayVictory()
    {
        if (victoryIntroClip != null) introSource.clip = victoryIntroClip;
        if (victoryLoopClip != null) loopSource.clip = victoryLoopClip;

        PlayTrack();
    }

    public void PlayGameOver()
    {
        // Stops the music level
        introSource.Stop();
        loopSource.Stop();

        // Plays the Game Over theme (no loop)
        introSource.clip = gameOverClip;
        introSource.loop = false;
        introSource.Play();
    }

    private void PlayTrack ()
    {
        introSource.Stop();
        loopSource.Stop();
            
        introSource.Play();
        loopSource.PlayScheduled(AudioSettings.dspTime + introSource.clip.length);
    }
}
