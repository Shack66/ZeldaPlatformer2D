using UnityEngine;
using UnityEngine.Events;
using static Darknut;

public class Damageable : MonoBehaviour
{
    public UnityEvent<int, Vector2> damageableHit;
    public UnityEvent<GameObject, Vector2>characterDeath;

    Animator animator;

    [SerializeField]
    private int _maxHealth = 100;

    public int MaxHealth
    {
        get
        {
            return _maxHealth;
        }
        set
        {
            _maxHealth = value;
        }
    }

    [SerializeField]
    private int _health = 100;

    public int Health
    {
        get
        {
            return _health;
        }
        set
        {
            _health = value;

            // If health drops below 0, character is no longer alive
            if (_health <= 0)
            {
                IsAlive = false;
                characterDeath?.Invoke(gameObject, transform.position);
            }
        }
    }

    [SerializeField]
    private bool _isAlive = true;

    [SerializeField]
    public bool isInvincible = false;

    private float timeSinceHit = 0;
    public float invincibilityTime = 0.5f;

    [SerializeField]
    public bool canBlock; // Only the Darknut can use his shield
    public bool isFacingRight = true;

    WalkableDirection WalkDirection;

    public enum HitResult { Damage, Blocked, Missed }

    public bool isStunned;

    public bool IsAlive
    {
        get
        {
            return _isAlive;
        }
        set
        {
            _isAlive = value;
            animator.SetBool(AnimationStrings.isAlive, value);
        }
    }

    // The velocity should not be changed while this is true, but needs to be respected by other physics component like the player controller
    public bool LockVelocity
    {
        get
        {
            return animator.GetBool(AnimationStrings.lockVelocity);
        }
        set
        {
            animator.SetBool(AnimationStrings.lockVelocity, value);
        }
    }

    public bool IsStunned
    {
        get
        {
            return animator.GetBool(AnimationStrings.isStunned);
        }
    }

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (isInvincible)
        {
            if (timeSinceHit > invincibilityTime)
            {
                // Remove invincibility
                isInvincible = false;
                timeSinceHit = 0;
                invincibilityTime = 0.25f; // reset to default
            }

            timeSinceHit += Time.deltaTime;
        }
    }

    public HitResult Hit(int damage, Vector2 knockback, Vector2 attackerPos)
    {
        if (IsAlive && !isInvincible)
        {
            float distanceX = attackerPos.x - transform.position.x;
            bool attackerIsOnRight = distanceX > 0;

            // 1. Calculate the height difference
            // Link's position - Darknut's base position
            float relativeHeight = attackerPos.y - transform.position.y;

            // 2. Define a threshold
            // If Link is more than 0.25 units higher, the Darknut's shield cannot cover the hit
            float shieldHeightLimit = 0.25f;
            bool attackerIsTooHigh = relativeHeight > shieldHeightLimit;

            bool isBlockingPosition = (isFacingRight && attackerIsOnRight) || (!isFacingRight && !attackerIsOnRight);

            if (canBlock && isBlockingPosition && !attackerIsTooHigh)
            {
                // If the Darknut's looking at the same direction that Link is, then his shield gets activated
                OnShield();
                return HitResult.Blocked;
            }

            // but if Link's behind the Darknut, then the Darknut takes the hit
            // The character cannot block, so it takes the hit

            Health -= damage;
            isInvincible = true;

            // Notify other suscribed components that the damageable was hit to handle the knockback and such
            animator.SetTrigger(AnimationStrings.hitTrigger);
            LockVelocity = true;
            damageableHit?.Invoke(damage, knockback);
            CharacterEvents.characterDamaged.Invoke(gameObject, damage);

            return HitResult.Damage;

        }
        // Unable to be hit
        return HitResult.Missed;
    }

    public void OnShield()
    {
        animator.SetTrigger(AnimationStrings.shield);
        isInvincible = true;
        invincibilityTime = 0.25f;
    }

    public void StartLongInvincibility(float duration)
    {
        isInvincible = true;
        invincibilityTime = duration;
        timeSinceHit = 0;
    }

    public void Heal(int healthRestore)
    {
        if (IsAlive)
        {
            int maxHeal = Mathf.Max(MaxHealth - Health, 0);
            int actualHeal = Mathf.Min(maxHeal, healthRestore);
            Health += actualHeal;
            CharacterEvents.characterHealed(gameObject, actualHeal);
        }
    }

    public void KillInstantly()
    {
        IsAlive = false;

        // Stop the movement so Link doesn't fall infinitely
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.simulated = false;
        }

    }
}
