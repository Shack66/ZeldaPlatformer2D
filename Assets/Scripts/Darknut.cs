using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(TouchingDirections))]
public class Darknut : MonoBehaviour
{

    public float walkSpeed = 1f;
    public float walkStopRate = 0.002f;
    public DetectionZone attackZone;

    Rigidbody2D rb;
    TouchingDirections touchingDirections;
    Animator animator;

    public enum WalkableDirection { Left, Right }

    private WalkableDirection _walkDirection = WalkableDirection.Left;
    private Vector2 walkDirectionVector = Vector2.left;

    public WalkableDirection WalkDirection
    { 
        get { return _walkDirection; } 
        set {
            if (_walkDirection == WalkableDirection.Right)
            {
                walkDirectionVector = Vector2.right;
                transform.localScale = new Vector2(-Mathf.Abs(transform.localScale.x), transform.localScale.y);
            }
            else if (_walkDirection == WalkableDirection.Left)
            {
                walkDirectionVector = Vector2.left;
                transform.localScale = new Vector2(Mathf.Abs(transform.localScale.x), transform.localScale.y);
            }
            _walkDirection = value; }
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

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        touchingDirections = GetComponent<TouchingDirections>();
        animator = GetComponent<Animator>();

        if (transform.localScale.x > 0)
        {
            _walkDirection = WalkableDirection.Right;
            walkDirectionVector = Vector2.right;
        }
        else
        {
            _walkDirection = WalkableDirection.Left;
            walkDirectionVector = Vector2.left;
        }
    }

    // Update is called once per frame
    void Update()
    {
        HasTarget = attackZone.detectedColliders.Count > 0;
    }

    public bool CanMove
    {  get
        {
            return animator.GetBool(AnimationStrings.canMove);
        } 
    }

    private void FixedUpdate()
    {
        if (touchingDirections.IsGrounded && touchingDirections.IsOnWall)
        {
            FlipDirection();
        }

        if (CanMove)
        {
            rb.linearVelocity = new Vector2(walkSpeed * walkDirectionVector.x, rb.linearVelocity.y);
        }
        else
        {
            rb.linearVelocity = new Vector2(Mathf.Lerp(rb.linearVelocity.x, 0, walkStopRate), rb.linearVelocity.y);
        }
        
    }

    private void FlipDirection()
    {
        if (WalkDirection == WalkableDirection.Right)
        {
            WalkDirection = WalkableDirection.Left;
        }
        else if (WalkDirection == WalkableDirection.Left)
        {
            WalkDirection = WalkableDirection.Right;
        }
        else
        {
            Debug.LogError("Current walkable direction is not set to legal values of right or left");
        }
    }

    private void Start()
    {
        WalkDirection = WalkableDirection.Left;
    }

}
