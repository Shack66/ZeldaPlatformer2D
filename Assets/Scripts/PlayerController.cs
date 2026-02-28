using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(TouchingDirections))]
public class PlayerController : MonoBehaviour
{
    // MOVEMENT STATS
    // On-the-Floor Speed
    public float walkSpeed = 7f;
    public float runSpeed = 13f;

    // On-the-Air Speed
    public float airWalkSpeed = 7f;
    public float airRunSpeed = 10f;
    private bool _isAirRunning; // Tracks if the jump started while running to preserve momentum

    // JUMP IMPULSES (Different heights and speeds based on current state)
    public float runJumpImpulse = 18f;
    public float walkJumpImpulse = 15f;
    public float idleJumpImpulse = 14f;

    // GAME FEEL: Jump Cut (for short hops) and Gravity (for heavy falls)
    private float jumpCutMultiplier = 0.5f;
    public float fallGravityMultiplier = 1.3f;
    private float defaultGravityScale;

    // GAME FEEL: Coyote Time (Grace period to jump after falling off an edge)
    public float coyoteTime = 0.1f;
    private float coyoteTimeCounter;

    // GAME FEEL: Momentum (Extra push in the air to make jumps wider), also called "Boost"
    public float airMomentumMultiplier = 1.2f;

    // GAME FEEL: JumpBuffer (Remembers jump input right before landing)
    public float jumpBufferTime = 0.15f;
    private float jumpBufferCounter;

    // GAME FEEL: Smoothing (How fast the character reaches the target speed)
    public float lerpSpeed = 10f;

    // Tracks previous frame state to detect exact landing moments
    private bool _wasGrounded;

    private float _airYVelocity;
    private float _airXVelocity;

    Vector2 moveInput;
    TouchingDirections touchingDirections;
    Rigidbody2D rb;
    Animator animator;

    // Calculates the exact speed the player should have right now
    public float CurrentMoveSpeed
    {
        get
        {
            // 1. Base case: Stilled or Blocked by a Wall
            if (!IsMoving || touchingDirections.IsOnWall)
            {
                return 0;
            }

            // 2. Air Case: If running, apply the momentum multiplier for a longer jump
            if (!touchingDirections.IsGrounded)
            {
                return _isAirRunning ? airRunSpeed * airMomentumMultiplier : airWalkSpeed;
            }

            // 3. Floor Case: Normal running or walking
            return IsRunning ? runSpeed : walkSpeed;
        }
    }

    // Calculates how much force to apply to the jump based on horizontal speed
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
        get { return _isMoving; } // When it's asked if it's moving (the public property is going to return the private variable whenever "IsMoving get" is called)
        private set
        {
            _isMoving = value; // Update the value
            // Automatically updates the Animator whenever the value changes. 
            // No need to call this manually everywhere.
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

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        touchingDirections = GetComponent<TouchingDirections>();

        defaultGravityScale = rb.gravityScale;
}

    private void FixedUpdate()
    {
        // 1. HORIZONTAL SPEED MANAGEMENT (Lerp) 
        // Calculate the theoretical target speed
        float targetVelocity = moveInput.x * CurrentMoveSpeed;

        // Smoothing current velocity toward target velocity
        float currentLerpSpeed = 8f;

        // Check if Link is trying to reverse direction (Directional Snap)
        bool isChangingDirection = (moveInput.x > 0 && rb.linearVelocity.x < 0) || (moveInput.x < 0 && rb.linearVelocity.x > 0);

        if (touchingDirections.IsGrounded)
        {
            if (isChangingDirection)
            {
                // Instant snap when switching directions
                currentLerpSpeed = 25f;
            }
            else if (moveInput.x == 0)
            {
                float speedMagnitude = Mathf.Abs(rb.linearVelocity.x);

                if (speedMagnitude < 5f) // if Link is Walking
                {
                    currentLerpSpeed = 20f; // Instantly stops
                }
                else // if Link is in Run
                {
                    currentLerpSpeed = 2f;  // He slides a little
                }
            }
            else
            {
                currentLerpSpeed = 12f; // Standard ground traction
            }
        }
        else
        {
            currentLerpSpeed = 3f; // Air momentum
        }

        // Apply horizontal force smoothly
        float newXVelocity = Mathf.Lerp(rb.linearVelocity.x, targetVelocity, Time.fixedDeltaTime * currentLerpSpeed);
        rb.linearVelocity = new Vector2(newXVelocity, rb.linearVelocity.y);

        // 2. ANIMATION AND COUNTERS
        // isMoving is updating based on input intent, not physical speed
        animator.SetBool("isMoving", moveInput.x != 0);

        // Tell the animator how fast Link is falling/rising
        animator.SetFloat(AnimationStrings.yVelocity, rb.linearVelocity.y);

        // Rest the time to the counters at the beginning of the frame
        jumpBufferCounter -= Time.fixedDeltaTime; 

        if (touchingDirections.IsGrounded)
        {
            // Refill Coyote Time when on the floor
            coyoteTimeCounter = coyoteTime;
            _isAirRunning = false; // Reset the air momentum flag
        }
        else
        {
            // Tick down Coyote Time when falling
            coyoteTimeCounter -= Time.fixedDeltaTime;
        }

        // 3.JUMP LOGIC (Jump Buffer + Coyote Time)
        if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f)
        {
            float finalJumpImpulse = CurrentJumpImpulse;

            if (!touchingDirections.IsGrounded)
            {
                finalJumpImpulse *= 0.75f; // The jump is reduced by 25% if the jump is a "coyote" jump
            }

            // Execute Jump
            animator.SetTrigger(AnimationStrings.jump);
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, finalJumpImpulse);

            // Save if Link was running when the jump started for the momentum boost
            _isAirRunning = IsRunning;

            // Reset the counters to prevent double jumping
            jumpBufferCounter = 0f;
            coyoteTimeCounter = 0f;
        }

        // 4. CEILING COLLISION (Anti-Stick)
        // If hitting a ceiling (or a platform that is above Link) while moving up, push down slightly to kill upward momentum
        if (touchingDirections.IsOnCeiling && rb.linearVelocity.y > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -2f);
        }

        // 5. LANDING DETECTION (Smart Landing)
        // Check if we just touched the ground this frame
        if (!_wasGrounded && touchingDirections.IsGrounded)
        {
            // The Landing Animation is only only activated if the fall is really high.
            if (_airYVelocity < -25f)
            {
                animator.SetTrigger("landingTrigger");
            }
        }

        // 6. DYNAMIC GRAVITY
        // Make the character fall faster than they rise for a dynamic/agile feel
        if (rb.linearVelocity.y < 0)
        {
            rb.gravityScale = defaultGravityScale * fallGravityMultiplier;
        }
        else
        {
            rb.gravityScale = defaultGravityScale;
        }

        // 7. STATE TRACKING
        // Keep track of vertical speed while in the air
        if (!touchingDirections.IsGrounded)
        {
            _airYVelocity = rb.linearVelocity.y;
            _airXVelocity = rb.linearVelocity.x;
        }
        else
        {
            _airYVelocity = 0;
            _airXVelocity = 0;
        }

        // Save current state for next frame's comparison
        _wasGrounded = touchingDirections.IsGrounded;

    }

    // Triggered by Left Stick or WASD/Arrows
    public void OnMove(InputAction.CallbackContext context)
    {
        // Read the direction (-1 to 1)
        moveInput = context.ReadValue<Vector2>();

        // If input is not exactly (0,0), we are moving
        IsMoving = moveInput != Vector2.zero;

        SetFacingDirection(moveInput);
    }

    // Extracted logic to keep OnMove clean
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

    // Triggered by the Run button (Shift or Button West)
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

    // Triggered by the Jump button (Space or Button South)
    public void OnJump(InputAction.CallbackContext context)
    {
        // TODO: Check if alive as well
        if (context.started)
        {
            // The player wants to jump, so the desire to jump is saved.
            // FixedUpdate will execute it when it's physically safe.
            jumpBufferCounter = jumpBufferTime; 
        }
        else if (context.canceled)
        {
            // Variable Jump Height: If the button is released while still moving UP, instantly cut the upward speed in half.
            if (rb.linearVelocity.y > 0)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
            }
        }
    }
}