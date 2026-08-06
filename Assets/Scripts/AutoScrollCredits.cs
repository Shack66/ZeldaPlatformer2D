using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class AutoScrollCredits : MonoBehaviour
{
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private float scrollSpeed = 0.05f;
    private bool isUserInteracting = false;

    private void OnEnable()
    {
        // Restart every time it opens
        if (scrollRect != null)
            scrollRect.verticalNormalizedPosition = 1f;
    }

    private void Update()
    {
        // If the user isn't doing anything,
        if (!isUserInteracting && scrollRect != null && scrollRect.verticalNormalizedPosition > 0)
        {
            scrollRect.verticalNormalizedPosition -= scrollSpeed * Time.deltaTime; //  it scrolls automatically
        }
    }
}