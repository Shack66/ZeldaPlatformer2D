using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(TouchingDirections), typeof(Animator))]
[RequireComponent(typeof(Damageable))]

public class DarkLinkAI : MonoBehaviour
{
    // References to components
    private Rigidbody2D rb;
    private Animator animator;
    private TouchingDirections touchingDirections;
    private Damageable damageable;

    // Target to track the Player (Link)
    [Header("Targeting")]
    [SerializeField]
    private Transform _targetPlayer;

    // Movement configuration (mirror Link's stats)
    [Header("Movement stats")]
    public float walkSpeed = 7f;
    public float runSpeed = 13f;
    public float lerpSpeed = 10f; // Smooths movement transitions

    // Jump Impulse
    private float walkJumpImpulse = 15f;

    // AI decision making
    [Header("Behavior Toggles")]
    [SerializeField]
    private bool _isAggressive = true; // Set false to make him try to stay away

    // Current intent (calculated)
    private Vector2 _moveIntent;
    private bool _isFacingRight = true;

    [Header("Combat Settings")]
    public float attackRange = 1.6f;
    public float attackCooldown = 1.5f;
    private float _lastAttackTime = 0f;

    [SerializeField]
    private bool _canMoveAI;

    // AI Logic
    [Header("AI Logic Variables")]
    private int attackCounter = 0; // Counts the combo hits
    private float lastMoveChangeTime; // To avoid frantically changing direction
    private bool isEvading = false; // Temporarily fleeing an attack

    [Header("Combat Logic")]
    private string _currentPhysicsTrigger; // Will store "attackTrigger" or "kickTrigger"
    private float _comboWindowTime = 0.5f; // Max waiting time between combo hits

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        touchingDirections = GetComponent<TouchingDirections>();
        damageable = GetComponent<Damageable>();

        // Try to automatically find the Player (Link) if not assigned in the inspector
        if (_targetPlayer == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                _targetPlayer = player.transform;
            }
        }
    }

    private void FixedUpdate()
    {
        _currentPhysicsTrigger = touchingDirections.IsGrounded ? AnimationStrings.attackTrigger : AnimationStrings.kickTrigger;

        // Only make decisions if alive and not currently reacting to a hit
        if (!damageable.IsAlive || damageable.LockVelocity)
        {
            SyncAnimatorStates(); // Sync hit and death animations
            return;
        }

        _canMoveAI = animator.GetBool(AnimationStrings.canMove);

        // Where is the player (Link) relative to Dark Link?
        if (_targetPlayer != null)
        {
            CalculateMovementIntent();
        }
        else
        {
            // Stand still if player is missing
            _moveIntent = Vector2.zero;
        }

        // Apply forces smoothe over time (lerp)
        ApplyPhysicsMovement();

        // Sync AI intent with the base state machine logic
        SyncAnimatorStates();
    }

    private void CalculateMovementIntent()
    {
        // Get the horizontal offset. Using Abs for vulnerability deadzone logic later.
        float rawXOffset = _targetPlayer.position.x - transform.position.x;
        float distanceToPlayer = Mathf.Abs(rawXOffset);

        // Basic behavior toggle (Hunting or Fleeing)
        float horizontalDirection = rawXOffset > 0 ? 1f : -1f;

        // Check if Link is attacking
        bool playerIsAttacking = animator.GetBool(AnimationStrings.attackTrigger);

        // Decide the close attack trigger (Sword or Kick)
        string triggerAttack = touchingDirections.IsGrounded ? AnimationStrings.attackTrigger : AnimationStrings.kickTrigger;

        if (_isAggressive)
        {
            // For Evading
            if (playerIsAttacking && distanceToPlayer < 2.0f)
            {
                isEvading = true;
            }
            else
            {
                isEvading = false;
            }

            // For Movement
            if (isEvading)
            {
                _moveIntent = new Vector2(-horizontalDirection, 0); // If Dark Link is evading, then he gets away from Link
            }
            // Move towards Player (Link)
            else if (distanceToPlayer > 1.5f)
            {
                _moveIntent = new Vector2(horizontalDirection, 0); // If Dark Link is far from Link, he goes after Link
            }
            else
            {
                _moveIntent = Vector2.zero; // If Dark Link is very close, he stops moving to prepare for attacks
            }

            // For jumps
            if (touchingDirections.IsGrounded && _canMoveAI)
            {
                bool randomJumpChance = distanceToPlayer < 3f && UnityEngine.Random.value < 0.01f;

                // Jumps if Link attacks, if he's higher, or with a 1% chance in each frame is Link is close
                if (playerIsAttacking && distanceToPlayer < 2.5f || _targetPlayer.position.y - transform.position.y > 2f || randomJumpChance)
                {
                    animator.SetTrigger(AnimationStrings.jumpTrigger);
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, walkJumpImpulse);

                    // Jumps backwars to dodge, so he receives an extra impulse
                    if (isEvading)
                    {
                        rb.linearVelocity = new Vector2(-horizontalDirection * 5f, rb.linearVelocity.y);
                    }
                }
            }

            // For Attacks

            HandleAttackLogic(distanceToPlayer);

        }
        else
        {
            // Move away
            _moveIntent = new Vector2(-horizontalDirection, 0); // Dark Link runs in the opposite direction
        }

        // Update Facing Direction
        if (_moveIntent.x != 0)
        {
            UpdateFacingDirection(_moveIntent.x > 0);
        }
    }

    private int _maxComboSteps = 0; // How many hits did he make this time?

    private void HandleAttackLogic(float distanceToPlayer)
    {
        // Only attack if the actual time is greater than the time of the last attack + the cooldown, and if Dark Link it's not in the middle of a combo
        if (Time.time >= _lastAttackTime + attackCooldown && attackCounter == 0)
        {
            // SwordAttack/kickAttack if he's close
            if (distanceToPlayer < 1.5f)
            {
                _maxComboSteps = UnityEngine.Random.Range(1, 4);
                ExecuteComboStep();
            }
            // If he's far and on the floor, then he uses the bow
            else if (distanceToPlayer > 5.0f && distanceToPlayer < 10f && touchingDirections.IsGrounded)
            {
                animator.SetTrigger(AnimationStrings.rangedAttackTrigger);
                _lastAttackTime = Time.time; // Save when the attack occured
                _moveIntent = Vector2.zero; // Dark Link only stops when shooting
            }
        }
        else if (attackCounter > 0 && attackCounter < _maxComboSteps)
        {
            bool linkIsInRange = distanceToPlayer <= 2f;

            float timeSinceLastHit = Time.time - _lastAttackTime;
            bool insideComboWindow = timeSinceLastHit > 0.2f && timeSinceLastHit < _comboWindowTime;

            // If Link is close, the min time has passed, and the max time (_comboWindowTime) hasn't
            if (linkIsInRange && insideComboWindow)
            {
                ExecuteComboStep(); // Dark Link does the combo
            }
            else if (timeSinceLastHit >= _comboWindowTime || !linkIsInRange)
            {
                ResetCombo();
                isEvading = true;
            }
        }
        // Reset if the combo ended successfully
        else if (attackCounter >= _maxComboSteps && _maxComboSteps != 0)
        {
            ResetCombo();
        }
    }

    private void ExecuteComboStep()
    {
        animator.SetTrigger(_currentPhysicsTrigger);
        attackCounter++;
        _lastAttackTime = Time.time; // Save when the attack occured
    }

    private void ResetCombo()
    {
        attackCounter = 0;
        _maxComboSteps = 0;
        animator.ResetTrigger(AnimationStrings.attackTrigger);
        animator.ResetTrigger(AnimationStrings.kickTrigger);
    }

    private bool IsPlayerAttacking()
    {
        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);

        return state.IsName("Link_NeutralAttack_1") ||
               state.IsName("Link_NeutralAttack_2") ||
               state.IsName("Link_NeutralAttack_3") ||
               state.IsName("Link_AerialAttack_Kick");
    }

    private void ApplyPhysicsMovement()
    {
        // Calculate the target velocity based on intent
        float targetXSpeed = _moveIntent.x * (IsRunningIntent() ? runSpeed : walkSpeed);

        // If Dark Link cannot move, then the (target) velocity is 0
        // If he can move, then the target velocity calculated before is maintained
        targetXSpeed = _canMoveAI ? targetXSpeed : 0;

        // Smoothly reach the target speed (simulates Link's movement feel)
        float currentXVelocity = Mathf.Lerp(rb.linearVelocity.x, targetXSpeed, Time.fixedDeltaTime * lerpSpeed);

        // Apply new velocity
        rb.linearVelocity = new Vector2(currentXVelocity, rb.linearVelocity.y);
    }

    private void SyncAnimatorStates()
    {
        // Tells the base state machine logic when the AI wants to do 
        animator.SetBool(AnimationStrings.isMoving, _moveIntent.x != 0);
        animator.SetBool(AnimationStrings.isRunning, IsRunningIntent());
        
        animator.SetBool(AnimationStrings.isGrounded, touchingDirections.IsGrounded);

        animator.SetFloat(AnimationStrings.xVelocity, Mathf.Abs(rb.linearVelocity.x));

        // If Dark Link is on the floor, then we force the Animator to see a 0
        float yForAnimator = touchingDirections.IsGrounded ? 0 : rb.linearVelocity.y;

        // Sync floor velocity for blend trees
        animator.SetFloat(AnimationStrings.xVelocity, Mathf.Abs(rb.linearVelocity.x));
        animator.SetFloat(AnimationStrings.yVelocity, yForAnimator);
    }

    private void UpdateFacingDirection(bool nowFacingRigt)
    {
        // Only flip is necessary
        if (_isFacingRight != nowFacingRigt)
        {
            _isFacingRight = nowFacingRigt;
            
            // Flip the physical scale
            transform.localScale = new Vector3(nowFacingRigt ? 6 : -6, 6, 6);

            // Inform Damageable of the change (needed for directional shield block logic)
            damageable.isFacingRight = nowFacingRigt;
        }
    }

    // Temporary logic to decide when the AI runs vs walks
    private bool IsRunningIntent()
    {
        if (_targetPlayer == null) return false;
        // Run if far away to close the gap fast, or if fleeing
        return Mathf.Abs(_targetPlayer.position.x - transform.position.x) > 5f || !_isAggressive;
    }

    public void OnHitAI(int damage, Vector2 knockback)
    {
        if (damageable.IsAlive)
        {
            // Apply physical force
            // Note: AI usually needs explicit velocity manipulation over AddForce to prevent stuck states if LockVelocity logic is weak.
            rb.linearVelocity = new Vector2(knockback.x, rb.linearVelocity.y + knockback.y);

            // Trigger Hit Reactions in the Base State Machine 
            if (knockback.y > 5f || !touchingDirections.IsGrounded)
            {
                animator.SetTrigger(AnimationStrings.strongHit);
            }
            else
            {
                animator.SetTrigger(AnimationStrings.hitTrigger);
            }
        }
        else
        {
            // AI stops dead upon death
            rb.linearVelocity = Vector2.zero;
        }
    }
}
