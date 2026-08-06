using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class IntroManager : MonoBehaviour
{
    [Header("Video Settings")]
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private string nextSceneName = "MainMenuScene";

    [Header("Scene Transition")]
    [SerializeField] private SceneFader sceneFader;

    private bool isTransitioning = false;

    private void Awake()
    {
        if (videoPlayer == null)
        {
            videoPlayer = GetComponent<VideoPlayer>();
        }
    }

    private void Start()
    {
        if (videoPlayer != null)
        {
            // For detecting when the video ends 
            videoPlayer.loopPointReached += OnVideoEnd;
        }
    }

    private void Update()
    {
        if (isTransitioning)
        {
            return;
        }

        // Check Mouse, Keyboard or Controller (A/B/Start)
        bool keyboardPressed = Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame;
        bool mousePressed = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        bool gamepadPressed = Gamepad.current != null && (
            Gamepad.current.buttonSouth.wasPressedThisFrame ||
            Gamepad.current.buttonEast.wasPressedThisFrame ||
            Gamepad.current.startButton.wasPressedThisFrame
        );

        if (keyboardPressed || mousePressed || gamepadPressed)
        {
            LoadNextScene();
        }
    }

    private void OnVideoEnd(VideoPlayer vp)
    {
        LoadNextScene();
    }

    private void LoadNextScene()
    {
        if (isTransitioning) return;
        isTransitioning = true;

        if (sceneFader != null)
        {
            sceneFader.FadeToScene(nextSceneName);
        }
        else
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }

    private void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoEnd;
        }
    }
}