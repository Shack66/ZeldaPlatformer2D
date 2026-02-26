using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(TouchingDirections))]
public class PlayerController : MonoBehaviour
{
    // On-the-Floor Speed
    public float walkSpeed = 7f;
    public float runSpeed = 13f;

    // On-the-Air Speed
    public float airWalkSpeed = 7f;
    public float airRunSpeed = 8f;
    private bool _isAirRunning;

    // Jump impulse
    public float runJumpImpulse = 18f;
    public float walkJumpImpulse = 15f;
    public float idleJumpImpulse = 14f;

    private float jumpCutMultiplier = 0.5f;
    public float fallGravityMultiplier = 1.3f;
    private float defaultGravityScale;

    // Coyote Time
    public float coyoteTime = 0.2f;
    private float coyoteTimeCounter;

    public float airMomentumMultiplier = 1.2f; // Boost variable

    // JumpBuffer
    public float jumpBufferTime = 0.15f;
    private float jumpBufferCounter;

    // Smoothing
    public float lerpSpeed = 10f;

    Vector2 moveInput;
    TouchingDirections touchingDirections;

    public float CurrentMoveSpeed
    {
        get
        {
            // 1. Base case: Stilled or Blocked 
            if (!IsMoving || touchingDirections.IsOnWall) return 0;

            // 2. Air Case
            if (!touchingDirections.IsGrounded)
            {
                return _isAirRunning ? airRunSpeed * airMomentumMultiplier : airWalkSpeed;
            }

            // 3. Floor Case 
            return IsRunning ? runSpeed : walkSpeed;
        }
    }

    public float CurrentJumpImpulse
    {
        get
        {
            if (IsMoving && IsRunning)
            {
                return runJumpImpulse;
            }
            else if (IsMoving && !IsRunning)
            {
                return walkJumpImpulse;
            }
            else
            {
                return idleJumpImpulse;
            }
        }
    }


    [SerializeField]
    private bool _isMoving = false;

    public bool IsMoving
    {
        get { return _isMoving; } // When someone asks if it's moving (the public property is going to return the private variable whenever "IsMoving get" is called)
        private set
        {
            _isMoving = value; // Update the value
            animator.SetBool(AnimationStrings.isMoving, value); // Here the animator is warned (whenever "isMoving" is set, like "OnMove", it's also going to be setting the parameter on the animator)
        }
    }

    [SerializeField]
    private bool _isRunning = false;

    public bool IsRunning
    {
        get
        {
            return _isRunning;
        }
        set
        {
            _isRunning = value;
            animator.SetBool(AnimationStrings.isRunning, value);
        }
    }

    public bool _isFacingRight = true;

    public bool IsFacingRight
    {
        get { return _isFacingRight; }
        private set
        {
            // Flip only if value is new
            if (_isFacingRight != value)
            {
                // Flip the local scale to make the player face the opposite direction
                transform.localScale *= new Vector2(-1, 1);
            }

            _isFacingRight = value;
        }
    }

    Rigidbody2D rb;
    Animator animator;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        touchingDirections = GetComponent<TouchingDirections>();

        defaultGravityScale = rb.gravityScale;
}

    private void FixedUpdate()
    {
        // 1. Horizontal Speed Management 
        // Calculate where we want to get
        float targetVelocity = moveInput.x * CurrentMoveSpeed;

        // Smoothing current velocity toward target velocity
        float lerpSpeed = touchingDirections.IsGrounded ? 8f : 5f; // More grip on the ground, more inertia in the air
        float newXVelocity = Mathf.Lerp(rb.linearVelocity.x, targetVelocity, Time.fixedDeltaTime * lerpSpeed);

        rb.linearVelocity = new Vector2(newXVelocity, rb.linearVelocity.y);
        
        // 2. Animation and Counters
        animator.SetFloat(AnimationStrings.yVelocity, rb.linearVelocity.y);

        jumpBufferCounter -= Time.fixedDeltaTime; // Rest the time to the counters at the beginning of the frame

        if (touchingDirections.IsGrounded)
        {
            coyoteTimeCounter = coyoteTime;
            _isAirRunning = false;
        }
        else
        {
            coyoteTimeCounter -= Time.fixedDeltaTime;
        }

        // 3.Jump Logic (Jump Buffer + Coyote Time)
        if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f)
        {
            // Execute Jump
            animator.SetTrigger(AnimationStrings.jump);
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, CurrentJumpImpulse);
            _isAirRunning = IsRunning;

            // Reset the counters so that the jump is not repeated
            jumpBufferCounter = 0f;
            coyoteTimeCounter = 0f;
        }

        // 4. Ceiling Colision
        if (touchingDirections.IsOnCeiling && rb.linearVelocity.y > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -2f);
        }

        if (rb.linearVelocity.y < 0)
        {
            rb.gravityScale = defaultGravityScale * fallGravityMultiplier;
        }
        else
        {
            rb.gravityScale = defaultGravityScale;
        }


    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();

        IsMoving = moveInput != Vector2.zero;

        SetFacingDirection(moveInput);
    }

    private void SetFacingDirection(Vector2 moveInput)
    {
        if (moveInput.x < 0 && IsFacingRight)
        {
            // Face the left
            IsFacingRight = false;
        }
        else if (moveInput.x > 0 && !IsFacingRight)
        {
            // Face the right
            IsFacingRight = true;
        }

    }

    public void OnRun(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            IsRunning = true;
        }
        else if (context.canceled)
        {
            IsRunning = false;
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        // TODO: Check if alive as well
        if (context.started && coyoteTimeCounter > 0f)
        {
            jumpBufferCounter = jumpBufferTime; // The player wants to jump
        }
        else if (context.canceled)
        {
            if (rb.linearVelocity.y > 0)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
            }
        }
    }
}