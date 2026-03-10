using UnityEngine;

public class SpriteFlash : MonoBehaviour
{
    private SpriteRenderer sr;
    private Damageable damageable;
    private Color originalColor;
    public Color flashColor = Color.white;
    public float flashSpeed = 8f;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        damageable = GetComponent<Damageable>();
        originalColor = sr.color;
    }

    void Update()
    {
        if (damageable.isInvincible)
        {
            float alpha = Mathf.PingPong(Time.time * flashSpeed, 1f);
            sr.color = Color.Lerp(originalColor, flashColor, alpha);
        }
        else
        {
            sr.color = originalColor;
        }
    }
}