using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(TouchingDirections), typeof(Damageable))]
public class Darknut : MonoBehaviour
{

    public float walkSpeed = 1f;
    public float walkStopRate = 0.002f;
    public DetectionZone attackZone;
    public DetectionZone cliffDetectionZone;

    [Header("Movement & Direction Settings")]
    public WalkableDirection startDirection = WalkableDirection.Left; 

    [Header("Flip Settings")]
    public float flipCooldown = 0.5f; // Cooldown to avoid infinite turns on walls
    private float timeSinceLastFlip = 0f;

    Rigidbody2D rb;
    TouchingDirections touchingDirections;
    Animator animator;
    Damageable damageable;

    public enum WalkableDirection { Left, Right }

    private WalkableDirection _walkDirection = WalkableDirection.Left;
    private Vector2 walkDirectionVector = Vector2.right;

    public WalkableDirection WalkDirection
    { 
        get { return _walkDirection; } 
        set {
            if (value == WalkableDirection.Right)
            {
                walkDirectionVector = Vector2.right;
                transform.localScale = new Vector2(Mathf.Abs(transform.localScale.x), transform.localScale.y);
            }
            else if (value == WalkableDirection.Left)
            {
                walkDirectionVector = Vector2.left;
                transform.localScale = new Vector2(-Mathf.Abs(transform.localScale.x), transform.localScale.y);
            }
            _walkDirection = value; 
        }
    }

    public bool _hasTarget = false;
    
    public bool HasTarget { 
        get { return _hasTarget; } 
        private set
        {
            _hasTarget = value;
            animator.SetBool(AnimationStrings.hasTarget, value);
        }
    }

    public bool CanMove
    {
        get
        {
            return animator.GetBool(AnimationStrings.canMove);
        }
    }

    public float AttackCooldown 
    {
        get
        {
            return animator.GetFloat(AnimationStrings.attackCooldown);
        }
        private set
        {
            animator.SetFloat(AnimationStrings.attackCooldown, Mathf.Max(value, 0));
        }
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        touchingDirections = GetComponent<TouchingDirections>();
        animator = GetComponent<Animator>();
        damageable = GetComponent<Damageable>();

        WalkDirection = startDirection;
        damageable.isFacingRight = (startDirection == WalkableDirection.Right);

        // Start timer on flipCooldown to allow turning if colliding with a wall
        timeSinceLastFlip = flipCooldown;
    }

    // Update is called once per frame
    void Update()
    {
        HasTarget = attackZone.detectedColliders.Count > 0;

        if (AttackCooldown > 0)
        {
            AttackCooldown -= Time.deltaTime;
        }

        // Increase cooldown timer
        timeSinceLastFlip += Time.deltaTime;
    }

    private void FixedUpdate()
    {
        if (!damageable.LockVelocity)
        {
            if (CanMove && touchingDirections.IsGrounded)
            {
                rb.linearVelocity = new Vector2(walkSpeed * walkDirectionVector.x, rb.linearVelocity.y);
            }
            else
            {
                rb.linearVelocity = new Vector2(Mathf.Lerp(rb.linearVelocity.x, 0, walkStopRate), rb.linearVelocity.y);
            }
        }

        if (touchingDirections.IsGrounded && touchingDirections.IsOnWall && CanMove && !damageable.LockVelocity && timeSinceLastFlip >= flipCooldown)
        {
            FlipDirection();
        }
    }

    private void FlipDirection()
    {
        if (WalkDirection == WalkableDirection.Right)
        {
            WalkDirection = WalkableDirection.Left;
            damageable.isFacingRight = false;
        }
        else if (WalkDirection == WalkableDirection.Left)
        {
            WalkDirection = WalkableDirection.Right;
            damageable.isFacingRight = true;
        }
        else
        {
            Debug.LogError("Current walkable direction is not set to legal values of right or left");
        }

        // Reset the timer every time the darknut turns
        timeSinceLastFlip = 0f;
    }

    public void OnHit(int damage, Vector2 knockback)
    {
        rb.linearVelocity = new Vector2(knockback.x, rb.linearVelocity.y + knockback.y);

        // Reset timer to avoid turns after knockback
        timeSinceLastFlip = 0f;
    }

    public void OnCliffDetected()
    {
        if (touchingDirections.IsGrounded && CanMove && !damageable.LockVelocity && timeSinceLastFlip >= flipCooldown)
        {
            FlipDirection();
        }
    }

    public void CheckForNextAttack()
    {
        animator.SetBool(AnimationStrings.shouldContinueAttack, HasTarget);
    }
}
