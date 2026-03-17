using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : MonoBehaviour
{
    public int attackDamage = 10;
    public Vector2 knockback = new Vector2(5, 2); // what the enemy receives
    public Vector2 shieldRecoil = new Vector2(15, 3); // what Link receives when he bounces

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if it can be hit
        Damageable target = collision.GetComponent<Damageable>();
        
        Animator animator = GetComponentInParent<Animator>();
        PlayerController player = GetComponentInParent<PlayerController>();
        Damageable linkDamageable = GetComponentInParent<Damageable>(); 

        if (target != null)
        {
            // If parent is facing the left by localscale, our knockback x flips its value to face the left as well
            Vector2 deliveredKnockback = transform.parent.localScale.x > 0 ? knockback : new Vector2(-knockback.x, knockback.y);

            // Hit the target
            Damageable.HitResult attackResult = target.Hit(attackDamage, deliveredKnockback, transform.parent.position);

            if (attackResult == Damageable.HitResult.Blocked)
            {
                animator.SetBool(AnimationStrings.isStunned, true); // Boolean's stun activated in Link

                // Applied knockback
                player.OnShieldBlock(transform.parent.localScale.x > 0 ? new Vector2(-shieldRecoil.x, shieldRecoil.y) : new Vector2(shieldRecoil.x, shieldRecoil.y));

                linkDamageable.LockVelocity = true; // Links' velocity is blocked
            }
            else if (attackResult == Damageable.HitResult.Damage) // Hit the target
            {
                Debug.Log(collision.name + " hit for " + attackDamage);
            }
        }
    }
}
