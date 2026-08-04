using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    [Header("Audio Channels")]
    public AudioSource introSource;
    public AudioSource loopSource;

    [Header("Victory Clips")]
    public AudioClip victoryIntroClip;
    public AudioClip victoryLoopClip;

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

    private void PlayTrack ()
    {
        introSource.Stop();
        loopSource.Stop();
            
        introSource.Play();
        loopSource.PlayScheduled(AudioSettings.dspTime + introSource.clip.length);
    }
}
