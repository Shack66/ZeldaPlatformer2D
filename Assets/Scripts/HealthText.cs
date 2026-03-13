using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthText : MonoBehaviour
{
    // Pixels per second
    public Vector3 moveSpeed = new Vector3(0, 75, 0);
    public float timeToFade = 1f;

    RectTransform textTransform;
    TextMeshProUGUI textMeshPro;

    private float timeElapsed = 0f;
    private Color startColor;

    private void Awake()
    {
        textTransform = GetComponent<RectTransform>();
        textMeshPro = GetComponent<TextMeshProUGUI>();
        startColor = textMeshPro.color;
    }

    private void Update()
    {
        textTransform.position += moveSpeed * Time.deltaTime;

        timeElapsed += Time.deltaTime;

        float fadeAlpha = startColor.a * (1 - (timeElapsed / timeToFade));
        textMeshPro.color = new Color(startColor.r, startColor.g, startColor.b, fadeAlpha);

        if (timeElapsed > timeToFade)
        {
            Destroy(gameObject);
        }

    }
}
