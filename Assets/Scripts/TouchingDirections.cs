using UnityEngine;

public class TouchingDirections : MonoBehaviour
{
    // which layers are considered "floor"?
    public ContactFilter2D castFilter;

    // Very short distance for a scan: is the Floor just below Link's feet?
    public float groundDistance = 0.05f;

    CapsuleCollider2D touchingCol; // check if Link is touching the floor, wall or ceiling
    Animator animator;

    RaycastHit2D[] groundHits = new RaycastHit2D[5]; // For the scan results

    [SerializeField]
    private bool _isGrounded;

    public bool IsGrounded { get {
            return _isGrounded;
        } private set { 
            _isGrounded = value;
            animator.SetBool(AnimationStrings.isGrounded, value);
        } }

    private void Awake()
    {
        touchingCol = GetComponent<CapsuleCollider2D>();
        animator = GetComponent<Animator>();
    }

    private void FixedUpdate()
    {
        // "True" = Link touches the ground, and "False" = Link is in the air
        IsGrounded = touchingCol.Cast(Vector2.down, castFilter, groundHits, groundDistance) > 0;

    }
}
