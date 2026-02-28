using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Uses the collider to check directions to see if the object is currently on the ground, or touching the wall or the ceiling
public class TouchingDirections : MonoBehaviour
{
    public ContactFilter2D castFilter; // which layers are considered "floor"?
    public float groundDistance = 0.05f; // Very short distance for a scan: is the Floor just below Link's feet?
    public float wallDistance = 0.2f;
    public float ceilingDistance = 0.05f;

    CapsuleCollider2D touchingCol; // check if Link is touching the floor, wall or ceiling
    Animator animator;

    RaycastHit2D[] groundHits = new RaycastHit2D[5]; // For the scan results
    RaycastHit2D[] wallHits = new RaycastHit2D[5];
    RaycastHit2D[] ceilingHits = new RaycastHit2D[5];

    [SerializeField]
    private bool _isGrounded;

    public bool IsGrounded
    {
        get
        {
            return _isGrounded;
        }
        private set
        {
            if (_isGrounded != value)
            {
                _isGrounded = value;
                animator.SetBool(AnimationStrings.isGrounded, value);
            }
        }
    }

    [SerializeField]
    private bool _isOnWall;

    public bool IsOnWall
    {
        get
        {
            return _isOnWall;
        }
        private set
        {
            if (_isOnWall != value)
            {
                _isOnWall = value;
                animator.SetBool(AnimationStrings.isOnWall, value);
            }
        }
    }

    [SerializeField]
    private bool _isOnCeiling;
    private Vector2 wallCheckDirection => gameObject.transform.localScale.x > 0 ? Vector2.right : Vector2.left;

    public bool IsOnCeiling
    {
        get
        {
            return _isOnCeiling;
        }
        private set
        {
            if (_isOnCeiling != value)
            {
                _isOnCeiling = value;
                animator.SetBool(AnimationStrings.isOnCeiling, value);
            }
        }
    }

    private void Awake()
    {
        touchingCol = GetComponent<CapsuleCollider2D>();
        animator = GetComponent<Animator>();
    }

    private void FixedUpdate()
    {
        // "True" = Link touches the ground, and "False" = Link is in the air
        IsGrounded = touchingCol.Cast(Vector2.down, castFilter, groundHits, groundDistance) > 0;
        IsOnWall = touchingCol.Cast(wallCheckDirection, castFilter, wallHits, wallDistance) > 0;
        IsOnCeiling = touchingCol.Cast(Vector2.up, castFilter, ceilingHits, ceilingDistance) > 0;
    }
}
