using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(AudioSource))]
public class UIButtonSFX : MonoBehaviour, IPointerEnterHandler, ISelectHandler, IPointerClickHandler, ISubmitHandler
{
    [Header("Audio Clips")]
    [SerializeField] private AudioClip hoverSound;
    [SerializeField] private AudioClip clickSound;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.ignoreListenerPause = true;
    }

    // When the mouse passes over (hover)
    public void OnPointerEnter(PointerEventData eventData)
    {
        // Setting the priority to the mouse or keyboard/controller 
        if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject != gameObject)
        {
            EventSystem.current.SetSelectedGameObject(gameObject);
        }
    }

    // When the keyboard or controller selects a button (hover)
    public void OnSelect(BaseEventData eventData)
    {
        PlayHover(hoverSound);
    }

    // For clicks
    public void OnPointerClick(PointerEventData eventData)
    {
        PlayClick(clickSound);
    }

    // For A button or Enter on keyboard 
    public void OnSubmit(BaseEventData eventData)
    {
        PlayClick(clickSound);
    }

    private void PlayHover(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            // PlayOneShot allows sounds to be played even if the game is paused (Time.timeScale = 0)
            audioSource.PlayOneShot(clip);
        }
    }

    private void PlayClick(AudioClip clip)
    {
        if (clip == null)
        {
            return;
        }

        // Temporary AudioSource that survives to the scene change
        GameObject tempAudioGO = new GameObject("ButtonSFX_Click");
        AudioSource tempAudio = tempAudioGO.AddComponent<AudioSource>();

        tempAudio.clip = clip;
        tempAudio.volume = audioSource.volume;
        tempAudio.pitch = audioSource.pitch;
        audioSource.ignoreListenerPause = true; // Allows the click to sound during the pause
        tempAudio.Play();

        DontDestroyOnLoad(tempAudioGO);
        Destroy(tempAudioGO, clip.length); // It destroys when the sound finishes
    }
}