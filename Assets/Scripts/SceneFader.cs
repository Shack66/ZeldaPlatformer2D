using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneFader : MonoBehaviour
{
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private float fadeDuration = 0.8f;

    private void Awake()
    {
        if (fadeCanvasGroup == null)
            fadeCanvasGroup = GetComponent<CanvasGroup>();
    }

    private void Start()
    {
        // When the scene begins, there's a fade from black to transparent
        StartCoroutine(FadeInRoutine());
    }

    public void FadeToScene(string sceneName)
    {
        StartCoroutine(FadeOutRoutine(sceneName));
    }

    private IEnumerator FadeInRoutine()
    {
        fadeCanvasGroup.blocksRaycasts = true;
        float timer = fadeDuration;

        while (timer > 0)
        {
            timer -= Time.unscaledDeltaTime; 
            fadeCanvasGroup.alpha = timer / fadeDuration;
            yield return null;
        }

        fadeCanvasGroup.alpha = 0f;
        fadeCanvasGroup.blocksRaycasts = false; // Allows click to the buttons from the menu
    }

    private IEnumerator FadeOutRoutine(string sceneName)
    {
        fadeCanvasGroup.blocksRaycasts = true; // Blocks clicks druing transition
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            fadeCanvasGroup.alpha = timer / fadeDuration;
            yield return null;
        }

        fadeCanvasGroup.alpha = 1f;
        SceneManager.LoadScene(sceneName);
    }
}