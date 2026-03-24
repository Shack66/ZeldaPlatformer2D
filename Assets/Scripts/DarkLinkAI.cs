using System;
using UnityEngine;
using UnityEngine.Splines;

[RequireComponent(typeof(Rigidbody2D), typeof(TouchingDirections), typeof(Animator))]
[RequireComponent(typeof(Damageable))]

public class DarkLinkAI : MonoBehaviour
{
    // References to components
    private Rigidbody2D rb;
    private Animator animator;
    private TouchingDirections touchingDirections;
    private Damageable damageable;

    private Animator _playerAnimator;

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
    public float attackCooldown = 0.8f;
    private float _lastAttackTime = 0f;

    public float maxRetreatDistance = 8f; // Maximum distance Dark Link will retreat before stopping

    [SerializeField]
    private bool _canMoveAI;

    // AI Logic
    [Header("AI Logic Variables")]
    private int attackCounter = 0; // Counts the combo hits
    private float lastMoveChangeTime; // To avoid frantically changing direction
    private bool isEvading = false; // Temporarily fleeing an attack

    [Header("Combat Logic")]
    private string _currentPhysicsTrigger; // Will store "attackTrigger" or "kickTrigger"
    private float _comboWindowTime = 1.0f; // Max waiting time between combo hits

    [Header("Spam Direction")]
    private float _playerSpamTimer = 0f;
    private float _spamThreshold = 1.2f; // Continuous attack time to consider it spam

    private float _evadeTimer = 0f;
    private float _patienceTimer;

    // Active/Inactive System
    [Header("Aggro System")]
    public float aggroRadius = 12f; // Distance for Dark Link to awaken
    private bool _isActive = false; // Is Dark Link fighting now?


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        touchingDirections = GetComponent<TouchingDirections>();
        damageable = GetComponent<Damageable>();

        _patienceTimer = UnityEngine.Random.Range(1f, 3f);

        // Try to automatically find the Player (Link) if not assigned in the inspector
        if (_targetPlayer == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                _targetPlayer = player.transform;
            }
        }
        else
        {
            _playerAnimator = _targetPlayer.GetComponent<Animator>();
        }
    }

    private void FixedUpdate()
    {
        _currentPhysicsTrigger = touchingDirections.IsGrounded ? AnimationStrings.attackTrigger : AnimationStrings.kickTrigger;

        if (touchingDirections.IsGrounded && damageable.LockVelocity && damageable.IsAlive)
        {
            animator.SetTrigger(AnimationStrings.getUp);
        }

        // Only make decisions if alive and not currently reacting to a hit
        if (!damageable.IsAlive || damageable.LockVelocity)
        {
            SyncAnimatorStates(); // Sync hit and death animations
            return;
        }

        _canMoveAI = animator.GetBool(AnimationStrings.canMove);

        // When evading, rest the time left to evade
        if (_evadeTimer > 0)
        {
            _evadeTimer -= Time.fixedDeltaTime;
        }

        // Where is the player (Link) relative to Dark Link?
        if (_targetPlayer != null)
        {
            // Calculate real distance between Link and Dark Link
            float distanceToPlayer = Vector2.Distance(transform.position, _targetPlayer.position);

            // If Link is in the zone
            if (!_isActive && distanceToPlayer <= aggroRadius)
            {
                _isActive = true; // Dark Link awakens
            }

            // Execute action according to his state
            if (_isActive)
            {
                CalculateMovementIntent();
            }
            else
            {
                _moveIntent = Vector2.zero;
            }

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

        // For Spam
        // Check if Link is attacking
        bool playerIsAttacking = IsPlayerAttacking();
        if (playerIsAttacking)
        {
            _playerSpamTimer += Time.fixedDeltaTime;
        }
        else
        {
            _playerSpamTimer = MathF.Max(0, _playerSpamTimer - Time.fixedDeltaTime);
        }

        bool isLinkSpamming = _playerSpamTimer > _spamThreshold;

        if (!_canMoveAI)
        {
            _moveIntent = Vector2.zero;
            return; // Not to walk nor change direction while shooting
        }

        // Decide the close attack trigger (Sword or Kick)
        string triggerAttack = touchingDirections.IsGrounded ? AnimationStrings.attackTrigger : AnimationStrings.kickTrigger;

        if (_isAggressive)
        {
            // For Evading
            _evadeTimer = Mathf.Max(0, _evadeTimer - Time.fixedDeltaTime);

            // If there's a close or spam attack, Dark Link evades it
            isEvading = (playerIsAttacking && distanceToPlayer < 2.5f) || isLinkSpamming || _evadeTimer > 0;

            // For Movement
            // If Dark Link is between Link and a wall
            if (touchingDirections.IsOnWall && distanceToPlayer <= 3f && touchingDirections.IsGrounded)
            {
                // He jumps towards Link to pass him
                float escapeDirection = (transform.position.x < _targetPlayer.position.x) ? 1f : -1f;

                _moveIntent = new Vector2(escapeDirection, 0);

                ResetCombo();

                animator.SetTrigger(AnimationStrings.jumpTrigger);
                rb.linearVelocity = new Vector2(escapeDirection * 8f, walkJumpImpulse);
                animator.SetTrigger(AnimationStrings.kickTrigger);

                _lastAttackTime = Time.time;

                // Reset timers so that he doesn't stay trapped in the wait logic
                _patienceTimer = UnityEngine.Random.Range(1f, 3f);
                _evadeTimer = 0;

                UpdateFacingDirection(escapeDirection > 0);
                return;
            }
            else if (isEvading)
            {
                // Check if Dark Link stepped back too much
                if (distanceToPlayer < maxRetreatDistance)
                {
                    _moveIntent = new Vector2(-horizontalDirection, 0); // If Dark Link is evading, then he gets away from Link
                }
                else
                {
                    _moveIntent = Vector2.zero; // He stands and waits/shoots.
                }
            }
            else if (_patienceTimer <= 0)
            {
                _moveIntent = new Vector2(horizontalDirection, 0); // If Link doesn't move, then Dark Link moves toward him to attack

                if (touchingDirections.IsGrounded && UnityEngine.Random.value < 0.02f)
                {
                    animator.SetTrigger(AnimationStrings.jumpTrigger);
                    rb.linearVelocity = new Vector2(horizontalDirection * 5f, walkJumpImpulse); // front impulse
                    animator.SetTrigger(AnimationStrings.kickTrigger);
                    _patienceTimer = UnityEngine.Random.Range(1f, 3f);
                }
            }
            // Move towards Player (Link)
            else if (distanceToPlayer > 5.0f)
            {
                _moveIntent = new Vector2(horizontalDirection, 0); // If Dark Link is far from Link, he goes after Link
            }
            else if (distanceToPlayer < 1.3f)
            {
                _moveIntent = new Vector2(-horizontalDirection * 0.5f, 0); // If Dark Link is too close to Link, he slowly steps back 
            }
            // Waiting for Link to move
            else
            {
                _moveIntent = Vector2.zero; // If Dark Link is very close, he stops moving to prepare for the next action
                _patienceTimer -= Time.fixedDeltaTime;
            }

            // For Jumps
            if (touchingDirections.IsGrounded && _canMoveAI)
            {
                // Check if Link is doing the aerial kick
                bool playerDoingAerialKick = _playerAnimator != null && _playerAnimator.GetCurrentAnimatorStateInfo(0).IsName("Link_AerialAttack_Kick");

                // Counterattack
                if (playerDoingAerialKick && distanceToPlayer < 4f)
                {
                    animator.SetTrigger(AnimationStrings.jumpTrigger);
                    animator.SetTrigger(AnimationStrings.kickTrigger);

                    // Dark Link is launched aggressively towards Link to win the air crash
                    rb.linearVelocity = new Vector2(horizontalDirection * 8f, walkJumpImpulse * 1.1f);
                    UpdateFacingDirection(horizontalDirection > 0);
                    _moveIntent = Vector2.zero;
                }
                // Evading backwards
                else if (isEvading && playerIsAttacking && distanceToPlayer < 3f)
                {
                    animator.SetTrigger(AnimationStrings.jumpTrigger);

                    // Long fast backward jump 
                    rb.linearVelocity = new Vector2(-horizontalDirection * 7f, walkJumpImpulse * 0.8f);
                    _moveIntent = Vector2.zero;
                }
                else
                {
                    bool playerIsHigher = _targetPlayer.position.y - transform.position.y > 2f;
                    bool randomJumpChance = distanceToPlayer < 3f && UnityEngine.Random.value < 0.01f;

                    // Jumps if Link's higher, or with a 1% chance in each frame is Link is close
                    if (playerIsHigher || randomJumpChance)
                    {
                        animator.SetTrigger(AnimationStrings.jumpTrigger);
                        rb.linearVelocity = new Vector2(rb.linearVelocity.x, walkJumpImpulse);

                        // If jumping randomly and Link is close, Dark Link kicks
                        if (randomJumpChance && distanceToPlayer > 2f)
                        {
                            animator.SetTrigger(AnimationStrings.kickTrigger);
                        }
                    }
                }
            }

            // For Attacks
            bool isAttacking = animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack");

            if (isAttacking || distanceToPlayer < 1.5f)
            {
                // If Dark Link is attacking or Link is too close, 
                // Dark Link looks at Link
                UpdateFacingDirection(rawXOffset > 0); 
            }
            else if (_moveIntent.x != 0)
            {
                // If Dark Link is moving (and not attacking),
                // Dark Link looks in the direction he's walking
                UpdateFacingDirection(_moveIntent.x > 0);  
            }
            HandleAttackLogic(distanceToPlayer, isLinkSpamming, horizontalDirection == 1f);

        }
        else
        {
            // Move away
            _moveIntent = new Vector2(-horizontalDirection, 0); // Dark Link runs in the opposite direction
        }

        // Update Facing Direction
        bool isRecentlyAttacking = Time.time < _lastAttackTime + 0.4f;

        if (isRecentlyAttacking)
        {
            // Lock Dark Link's gaze towards Link if he has just attacked/shooted
            UpdateFacingDirection(horizontalDirection > 0);

            // If Link was spamming, lock the movement to 0 so Dark Link doesn't run backwards while shooting 
            if (isLinkSpamming)
            {
                _moveIntent = Vector2.zero;
            }
        }
        else if (_moveIntent.x != 0 && !animator.GetCurrentAnimatorStateInfo(0).IsName("Link_Bow"))
        {
            UpdateFacingDirection(_moveIntent.x > 0);
        }
    }

    private int _maxComboSteps = 0; // How many hits did he make this time?

    private void HandleAttackLogic(float distanceToPlayer, bool isLinkSpamming, bool horizontalDirection)
    {
        // If Link attacks and Dark Link is close
        if (IsPlayerAttacking() &&  distanceToPlayer < 2.5f)
        {
            if (UnityEngine.Random.value < 0.4f)
            {
                UpdateFacingDirection(horizontalDirection);
                ExecuteComboStep(horizontalDirection); // Dark Link swordattacks
                return;
            }
        }
        // If player (Link) is spamming and Dark Link is at a safe distance, then shoots
        if (isLinkSpamming && distanceToPlayer > 3.5f && Time.time > _lastAttackTime + (attackCooldown * 0.5f))
        {
            UpdateFacingDirection(horizontalDirection);
            _moveIntent = Vector2.zero; // Dark Link only stops when shooting
            animator.SetTrigger(AnimationStrings.rangedAttackTrigger);
            _lastAttackTime = Time.time; // Save when the attack occured
            return; // Not to chain in further attacks
        }
        
        // Only attack if the actual time is greater than the time of the last attack + the cooldown, and if Dark Link it's not in the middle of a combo
        if (Time.time >= _lastAttackTime + attackCooldown && attackCounter == 0)
        {
            // SwordAttack/kickAttack if he's close
            if (distanceToPlayer < 1.5f)
            {
                _maxComboSteps = UnityEngine.Random.Range(1, 4);
                ExecuteComboStep(horizontalDirection);
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
            bool linkIsInRange = distanceToPlayer <= 2.2f;
            float timeSinceLastHit = Time.time - _lastAttackTime;
            bool insideComboWindow = timeSinceLastHit > 0.2f && timeSinceLastHit < _comboWindowTime;

            // If Link is close, the min time has passed, and the max time (_comboWindowTime) hasn't
            if (linkIsInRange && insideComboWindow)
            {
                ExecuteComboStep(horizontalDirection); // Dark Link does the combo
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

    private void ExecuteComboStep(bool horizontalDirection)
    {
        animator.SetInteger(AnimationStrings.comboStep, attackCounter); // Tell the animator what hit number is
        animator.SetTrigger(_currentPhysicsTrigger);
        attackCounter++;
        _lastAttackTime = Time.time; // Save when the attack occured

        // Front impulse: Dark Link is launched a little towards Link when attacking
        float boostForce = 4f;
        float direction = _isFacingRight ? 1f : -1f;
        rb.linearVelocity = new Vector2(direction * boostForce, rb.linearVelocity.y);

        UpdateFacingDirection(horizontalDirection);
        _patienceTimer = UnityEngine.Random.Range(1f, 3f);
    }

    private void ResetCombo()
    {
        attackCounter = 0;
        _maxComboSteps = 0;
        animator.SetInteger(AnimationStrings.comboStep, 0);
        animator.ResetTrigger(AnimationStrings.attackTrigger);
        animator.ResetTrigger(AnimationStrings.kickTrigger);

        _evadeTimer = 2.0f; // When Dark Link finishes a combo, he has 2 seconds of fleeing
    }

    private bool IsPlayerAttacking()
    {
        if (_playerAnimator == null) return false;

        AnimatorStateInfo state = _playerAnimator.GetCurrentAnimatorStateInfo(0);

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

        // Horizontal move forced if Dark Link is on floor
        if (touchingDirections.IsGrounded)
        {
            // Smoothly reach the target speed (simulates Link's movement feel)
            float currentXVelocity = Mathf.Lerp(rb.linearVelocity.x, targetXSpeed, Time.fixedDeltaTime * lerpSpeed);

            // Apply new velocity
            rb.linearVelocity = new Vector2(currentXVelocity, rb.linearVelocity.y);
        }
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
        if (_targetPlayer == null)
        {
            return false;
        }

        float dist = Mathf.Abs(_targetPlayer.position.x - transform.position.x);

        // Run if far away to close the gap fast, if fleeing, or if Link's getting close
        return dist > 4.5f || isEvading || !_isAggressive;
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
