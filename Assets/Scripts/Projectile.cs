using UnityEngine;

public class Projectile : MonoBehaviour
{
    public int damage = 10;
    public Vector2 moveSpeed = new Vector2(3f, 0);
    public Vector2 knockback = new Vector2(0, 0);

    Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb.linearVelocity = new Vector2(moveSpeed.x * transform.localScale.x, moveSpeed.y);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if it can be hit
        Damageable target = collision.GetComponent<Damageable>();

        if (target != null)
        { 
            // If parent is facing the left by localscale, our knockback x flips its value to face the left as well
            Vector2 deliveredKnockback = transform.localScale.x > 0 ? knockback : new Vector2(-knockback.x, knockback.y);

            // Hit the target
            Damageable.HitResult attackResult = target.Hit(damage, deliveredKnockback, transform.position);

            if (attackResult == Damageable.HitResult.Damage) // Hit the target
            {
                Destroy(gameObject);
            }
            else if (attackResult == Damageable.HitResult.Blocked)
            {
                Destroy(gameObject);
            }
        }
    }
}
