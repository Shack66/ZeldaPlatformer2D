using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    public float walkSpeed = 5f;
    public float runSpeed = 10.3f;
    Vector2 moveInput;

    public float CurrentMoveSpeed
    {
        get
        {
            if (IsMoving)
            {
                if (IsRunning)
                {
                    return runSpeed;
                }
                else
                {
                    return walkSpeed;
                }
            }
            else
            {
                // Idle speed is 0
                return 0;
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

    public bool _isFacingLeft = true;

    public bool IsFacingLeft { get { return _isFacingLeft; } private set {
            // Flip only if value is new
            if (_isFacingLeft != value)
            {
                // Flip the local scale to make the player face the opposite direction
                transform.localScale *= new Vector2 (-1, 1);
            }

            _isFacingLeft = value;
        } }

    Rigidbody2D rb;
    Animator animator;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = rb.GetComponent<Animator>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(moveInput.x * CurrentMoveSpeed, rb.linearVelocity.y);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();

        IsMoving = moveInput != Vector2.zero;

        SetFacingDirection(moveInput);
    }

    private void SetFacingDirection(Vector2 moveInput)
    {
        if (moveInput.x < 0 && IsFacingLeft)
        {
            // Face the left
            IsFacingLeft = false;
        }
        else if (moveInput.x > 0 && !IsFacingLeft)
        {
            // Face the right
            IsFacingLeft = true; 
        }

        }

    public void onRun(InputAction.CallbackContext context)
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
}
