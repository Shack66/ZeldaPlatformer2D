using UnityEngine;
using UnityEngine.Audio;
public class PlayOneShotBehaviour : StateMachineBehaviour
{
    [Header("Audio Settings")]
    public AudioClip soundToPlay;
    public AudioMixerGroup audioMixerGroup;
    [Range(0f, 1f)] public float volume = 1f;

    [Header("Timing")]
    public bool playOnEnter = true;
    public bool playOnExit = false;
    public bool playAfterDelay = false;
    public float playDelay = 0.25f;

    [Header("Filter")]
    public string requiredTag = ""; // Filter so that the sfx only sounds in certain characters

    public float timeSinceEntered = 0f;
    private bool hasDelayedSoundPlayed = false;

    // Cache to reuse Mixer
    private static AudioMixerGroup cachedMixerGroup;

    private void PlayAudio(Animator animator, AudioClip clip, float vol)
    {
        if (clip == null)
        {
            return;
        }

        GameObject tempGO = new GameObject("TempAnimSFX_" + clip.name);
        tempGO.transform.position = animator.transform.position;

        AudioSource aSource = tempGO.AddComponent<AudioSource>();
        aSource.clip = clip;
        aSource.volume = vol;
        aSource.spatialBlend = 0f;

        if (audioMixerGroup != null)
        {
            aSource.outputAudioMixerGroup = audioMixerGroup;
        }

        aSource.Play();
        Destroy(tempGO, clip.length);
    }

    private bool ShouldPlay(Animator animator)
    {
        // If there's no tag, it always sounds
        // If there's a tag, it should coincide with the object's tag
        return string.IsNullOrEmpty(requiredTag) || animator.gameObject.CompareTag(requiredTag);
    }

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (playOnEnter && ShouldPlay(animator))
        {
            AudioSource.PlayClipAtPoint(soundToPlay, animator.gameObject.transform.position, volume);
        }
        timeSinceEntered = 0f;
        hasDelayedSoundPlayed = false;
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (playAfterDelay && !hasDelayedSoundPlayed && ShouldPlay(animator))
        {
            timeSinceEntered += Time.deltaTime;

            if (timeSinceEntered > playDelay)
            {
                AudioSource.PlayClipAtPoint(soundToPlay, animator.gameObject.transform.position, volume);
                hasDelayedSoundPlayed = true;
            }
        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (playOnExit && ShouldPlay(animator))
        {
            AudioSource.PlayClipAtPoint(soundToPlay, animator.gameObject.transform.position, volume);
        }
    }
}