using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthHeart : MonoBehaviour
{
    public int healthRestore = 15;

    [SerializeField] 
    private LayerMask groundLayer;

    private SpriteRenderer spriteRenderer;
    private bool isGrounded = false;
    private Rigidbody2D rb;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        StartCoroutine(BlinkRoutine());
        Destroy(gameObject, 10f); // Destroy the heart after 10 seconds
    }

    private IEnumerator BlinkRoutine()
    {
        yield return new WaitForSeconds(7f);

        while (true)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return new WaitForSeconds(0.1f);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) // Check if the object that collided has the "Player" Tag
        { 
            Damageable damageable = collision.GetComponent<Damageable>();
        
            if (damageable)
            {
                damageable.Heal(healthRestore);
                Destroy(gameObject); // Destroy the heart after healing Player (Link)
            }
        }
    }

    private void FixedUpdate()
    {
        if (isGrounded) return; // If the heart landed, then there's nothing left to do

        // For the heart landing
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 0.24f, groundLayer);

        if (hit.collider != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Static;
            isGrounded = true;
        }
    }
}
