using UnityEngine;

/// <summary>
/// Rigidbody-based character controller with 8-directional momentum movement,
/// jump with landing cooldown, and wall kick mechanic.
/// Requires a Rigidbody on the same GameObject. CapsuleCollider may live on a child.
/// All references are resolved automatically at runtime — no Inspector wiring required.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    // ─── References ────────────────────────────────────────────────────────────

    [Header("References")]
    [Tooltip("The visual root child that rotates to face the camera forward. Auto-resolved to the first child if left empty.")]
    public Transform visualRoot;

    [Tooltip("The camera controller. Auto-resolved via FindFirstObjectByType if left empty.")]
    public PlayerCameraController cameraController;

    // ─── Movement ──────────────────────────────────────────────────────────────

    [Header("Movement")]
    [Tooltip("Soft horizontal speed cap on the ground. Standard WASD will not push beyond this; " +
             "external forces (wall kicks, explosions, etc.) can freely exceed it.")]
    public float moveSpeed = 7f;

    [Tooltip("Constant acceleration applied in the input direction while grounded. " +
             "Must exceed moveSpeed × groundDrag (" + "≈56 at defaults) for the cap to be the limiter " +
             "rather than drag. Higher values reach the cap faster.")]
    public float groundAcceleration = 80f;

    [Tooltip("Rigidbody drag when the player is grounded.")]
    public float groundDrag = 8f;

    [Tooltip("Rigidbody drag while airborne.")]
    public float airDrag = 0.5f;

    [Tooltip("Force applied in the input direction while airborne.")]
    public float acceleration = 60f;

    [Tooltip("Scales the additive steering force while airborne for input that aligns with or is " +
             "perpendicular to the current velocity. Does not affect the opposing braking factor.")]
    [Range(0f, 1f)]
    public float airControlFactor = 0.4f;

    [Tooltip("Fraction of airControlFactor applied when input directly opposes current horizontal velocity. " +
             "0 = cannot reverse momentum at all, 1 = same as aligned steering. " +
             "Interpolates continuously — perpendicular input is always at full airControlFactor.")]
    [Range(0f, 1f)]
    public float airOpposingFactor = 0.1f;

    [Tooltip("Speed at which the visual root slerps to face the camera forward direction.")]
    public float bodyRotationSpeed = 12f;

    // ─── Jump ──────────────────────────────────────────────────────────────────

    [Header("Jump")]
    [Tooltip("Upward impulse strength applied when jumping.")]
    public float jumpForce = 10f;

    [Tooltip("Seconds after landing before the player may jump again.")]
    public float jumpCooldown = 0.15f;

    // ─── Wall Kick ─────────────────────────────────────────────────────────────

    [Header("Wall Kick")]
    [Tooltip("Lateral force applied away from the wall on a wall kick.")]
    public float wallKickLateralForce = 6f;

    [Tooltip("Upward force applied on a wall kick.")]
    public float wallKickUpForce = 5f;

    [Tooltip("Horizontal reach of the wall detection sphere cast.")]
    public float wallCheckDistance = 0.65f;

    [Tooltip("Seconds the wall kick ability is locked out after use (resets fully on landing).")]
    public float wallKickCooldown = 0.2f;

    // ─── Gravity ───────────────────────────────────────────────────────────────

    [Header("Gravity")]
    [Tooltip("How quickly the gravity multiplier increases per second while airborne. " +
             "Resets to 1 on landing, jumping, wall kicking, or any call to ResetGravityRamp().")]
    public float gravityAccelRate = 0.4f;

    [Tooltip("Maximum gravity multiplier the airborne ramp can reach.")]
    public float maxGravityMultiplier = 3f;

    // ─── Ground Detection ──────────────────────────────────────────────────────

    [Header("Ground Detection")]
    [Tooltip("Radius of the foot sphere overlap used to detect the ground.")]
    public float groundCheckRadius = 0.25f;

    [Tooltip("Vertical offset from the GameObject pivot to the foot check centre.")]
    public float groundCheckOffset = 0.05f;

    [Tooltip("Layers considered as ground or wall surfaces. Defaults to Everything if not set.")]
    public LayerMask groundLayer = ~0;

    // ─── Private State ─────────────────────────────────────────────────────────

    private Rigidbody _rb;
    private CapsuleCollider _col;

    private bool _isGrounded;
    private bool _wasGrounded = true; // Prevents a false landing event on the first frame.

    private bool _canJump = true;
    private float _jumpCooldownTimer;

    private bool _canWallKick = false;
    private float _wallKickCooldownTimer;

    private float _gravityMultiplier = 1f;

    /// <summary>
    /// True from the moment of a wall kick until the player lands.
    /// While active, movement force toward the kicked wall's normal is suppressed,
    /// preventing the player from steering back into the surface mid-air.
    /// </summary>
    private bool _wallKickActive;
    private Vector3 _wallKickNormal;

    private Vector2 _moveInput;
    private bool _jumpPressed;

    // Cardinal directions used for the wall fan-cast.
    private static readonly Vector3[] WallCheckDirections =
    {
        Vector3.forward,
        Vector3.back,
        Vector3.left,
        Vector3.right,
    };

    // ─── Unity Lifecycle ───────────────────────────────────────────────────────

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();

        // Collider may live on the root or on a child (e.g. Body).
        _col = GetComponent<CapsuleCollider>();
        if (_col == null)
            _col = GetComponentInChildren<CapsuleCollider>();

        if (_col == null)
            Debug.LogError("[PlayerController] No CapsuleCollider found on this GameObject or its children.", this);

        // Auto-resolve visual root to first child if not assigned.
        if (visualRoot == null && transform.childCount > 0)
            visualRoot = transform.GetChild(0);

        // Auto-resolve camera controller.
        if (cameraController == null)
            cameraController = FindFirstObjectByType<PlayerCameraController>();

        if (cameraController == null)
            Debug.LogError("[PlayerController] No PlayerCameraController found in the scene.", this);

        // Guard against groundLayer being left at Nothing (mask = 0) by the serializer.
        if (groundLayer.value == 0)
            groundLayer = ~0;

        _rb.freezeRotation = true;
        _rb.interpolation = RigidbodyInterpolation.Interpolate;
        _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        _rb.useGravity = false; // Gravity is applied manually to support the airborne ramp-up.

        // Assign a zero-friction physics material so the player doesn't slow-slide
        // along walls due to collider friction. Ground deceleration is handled by linearDamping.
        PhysicsMaterial frictionlessMat = new PhysicsMaterial("Player_Frictionless")
        {
            dynamicFriction = 0f,
            staticFriction  = 0f,
            frictionCombine = PhysicsMaterialCombine.Minimum,
        };
        _col.material = frictionlessMat;
    }

    private void Update()
    {
        GatherInput();
        TickCooldowns();
    }

    private void FixedUpdate()
    {
        CheckGround();
        HandleLanding();
        ApplyDrag();
        ApplyGravity();
        ApplyMovement();
        HandleJump();
        HandleWallKick();
        RotateVisualRoot();
    }

    // ─── Input ─────────────────────────────────────────────────────────────────

    private void GatherInput()
    {
        _moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        // Latch jump press so it is never missed between Update and FixedUpdate.
        if (Input.GetButtonDown("Jump"))
            _jumpPressed = true;
    }

    // ─── Cooldown Timers ───────────────────────────────────────────────────────

    private void TickCooldowns()
    {
        if (!_canJump)
        {
            _jumpCooldownTimer -= Time.deltaTime;
            // Only restore jump once the cooldown has elapsed AND the player is on the ground.
            if (_jumpCooldownTimer <= 0f && _isGrounded)
                _canJump = true;
        }

        if (!_canWallKick)
        {
            _wallKickCooldownTimer -= Time.deltaTime;
            if (_wallKickCooldownTimer <= 0f)
                _canWallKick = true;
        }
    }

    // ─── Ground Check ──────────────────────────────────────────────────────────

    private void CheckGround()
    {
        _wasGrounded = _isGrounded;

        // Foot position accounts for the collider potentially being on a child.
        Vector3 colBottom  = _col.transform.position + _col.center + Vector3.down * (_col.height * 0.5f);
        Vector3 footCentre = colBottom + Vector3.up * groundCheckOffset;

        _isGrounded = Physics.CheckSphere(footCentre, groundCheckRadius, groundLayer, QueryTriggerInteraction.Ignore);
    }

    // ─── Landing ───────────────────────────────────────────────────────────────

    private void HandleLanding()
    {
        if (_isGrounded && !_wasGrounded)
        {
            // Start jump cooldown on landing.
            _canJump = false;
            _jumpCooldownTimer = jumpCooldown;

            // Reset wall kick on landing.
            _canWallKick = true;
            _wallKickCooldownTimer = 0f;

            // Clear the post-kick movement suppression.
            _wallKickActive = false;

            ResetGravityRamp();
        }
    }

    // ─── Drag ──────────────────────────────────────────────────────────────────

    private void ApplyDrag()
    {
        _rb.linearDamping = _isGrounded ? groundDrag : airDrag;
    }

    // ─── Gravity ───────────────────────────────────────────────────────────────

    private void ApplyGravity()
    {
        if (!_isGrounded)
            _gravityMultiplier = Mathf.Min(_gravityMultiplier + gravityAccelRate * Time.fixedDeltaTime, maxGravityMultiplier);

        _rb.AddForce(Physics.gravity * _gravityMultiplier, ForceMode.Acceleration);
    }

    /// <summary>
    /// Resets the airborne gravity ramp back to 1×. Call this from any action that
    /// imparts upward or sustained mid-air momentum — jumps, wall kicks, jetpack boosts, etc.
    /// </summary>
    public void ResetGravityRamp()
    {
        _gravityMultiplier = 1f;
    }

    // ─── Movement ──────────────────────────────────────────────────────────────

    private void ApplyMovement()
    {
        if (_moveInput.sqrMagnitude < 0.01f || cameraController == null)
            return;

        // Project input onto the camera's horizontal plane.
        Vector3 camForward = cameraController.HorizontalForward;
        Vector3 camRight   = cameraController.HorizontalRight;

        Vector3 targetDirection   = (camForward * _moveInput.y + camRight * _moveInput.x).normalized;
        Vector3 currentHorizontal = new Vector3(_rb.linearVelocity.x, 0f, _rb.linearVelocity.z);

        if (_isGrounded)
        {
            // Additive force toward the input direction, capped at moveSpeed.
            // Forces from outside this system (kicks, explosions) can freely exceed the cap —
            // WASD simply won't add further speed in that direction until below it again.
            float groundSpeedAlongInput = Vector3.Dot(currentHorizontal, targetDirection);
            if (groundSpeedAlongInput < moveSpeed)
                _rb.AddForce(targetDirection * groundAcceleration, ForceMode.Acceleration);
            return;
        }

        // ── Airborne: momentum-preserving additive steering ─────────────────────
        // Applies a force in the input direction rather than correcting toward a target
        // velocity, so existing momentum (wall kicks, explosions, etc.) is not aggressively
        // counteracted. The scale is reduced when input opposes current velocity.

        float controlScale = airControlFactor;

        float currentSpeed = currentHorizontal.magnitude;
        if (currentSpeed > 0.1f)
        {
            float dot = Vector3.Dot(targetDirection, currentHorizontal.normalized);
            if (dot < 0f)
            {
                // Remap dot from [-1, 0] → scale from (airControlFactor * airOpposingFactor) to airControlFactor.
                // Fully opposing = minimum scale; perpendicular = full airControlFactor.
                float t = 1f + dot; // 0 at dot = -1, 1 at dot = 0
                controlScale = Mathf.Lerp(airControlFactor * airOpposingFactor, airControlFactor, t);
            }
        }

        // Hard-suppress any force back toward the kicked wall while airborne.
        if (_wallKickActive && Vector3.Dot(targetDirection, -_wallKickNormal) > 0f)
            controlScale = 0f;

        // Do not add speed beyond moveSpeed in the input direction.
        // This caps steering/additive acceleration without damping launch momentum.
        float speedAlongInput = Vector3.Dot(currentHorizontal, targetDirection);
        if (speedAlongInput >= moveSpeed)
            return;

        _rb.AddForce(targetDirection * acceleration * controlScale, ForceMode.Acceleration);
    }

    // ─── Jump ──────────────────────────────────────────────────────────────────

    private void HandleJump()
    {
        if (!_jumpPressed)
            return;

        if (_isGrounded && _canJump)
        {
            // Zero downward velocity for a consistent jump height.
            _rb.linearVelocity = new Vector3(_rb.linearVelocity.x, 0f, _rb.linearVelocity.z);
            _rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

            _canJump = false;
            _jumpCooldownTimer = 0.2f;
            _jumpPressed = false;

            ResetGravityRamp();
        }
        // If standard jump did not fire, leave _jumpPressed=true for HandleWallKick.
    }

    // ─── Wall Kick ─────────────────────────────────────────────────────────────

    private void HandleWallKick()
    {
        if (!_jumpPressed)
            return;

        _jumpPressed = false;

        if (_isGrounded || !_canWallKick)
            return;

        if (!TryDetectWall(out Vector3 wallNormal))
            return;

        // Zero vertical momentum for a consistent kick height.
        // Also strip any velocity component going into the wall so the kick
        // is never fighting the player's current approach momentum.
        Vector3 vel       = _rb.linearVelocity;
        float   intoWall  = Mathf.Min(0f, Vector3.Dot(vel, wallNormal)); // negative = moving into wall
        vel              -= wallNormal * intoWall;                        // remove that component
        vel.y             = 0f;
        _rb.linearVelocity = vel;

        Vector3 kickForce = wallNormal * wallKickLateralForce + Vector3.up * wallKickUpForce;
        _rb.AddForce(kickForce, ForceMode.Impulse);

        _canWallKick          = false;
        _wallKickCooldownTimer = wallKickCooldown;

        // Block movement back toward this wall for the entire remaining airborne duration.
        _wallKickActive = true;
        _wallKickNormal = wallNormal;

        ResetGravityRamp();
    }

    /// <summary>
    /// Fans a sphere cast in four cardinal directions at hip height.
    /// Returns true and the averaged wall normal if a wall is found within range.
    /// </summary>
    private bool TryDetectWall(out Vector3 wallNormal)
    {
        wallNormal = Vector3.zero;
        int hitCount = 0;

        float hipHeight   = _col.height * 0.25f;
        Vector3 hipCentre = transform.position + Vector3.up * hipHeight;

        foreach (Vector3 dir in WallCheckDirections)
        {
            if (Physics.SphereCast(hipCentre, groundCheckRadius, dir, out RaycastHit hit,
                    wallCheckDistance, groundLayer, QueryTriggerInteraction.Ignore))
            {
                wallNormal += hit.normal;
                hitCount++;
            }
        }

        if (hitCount == 0)
            return false;

        wallNormal = (wallNormal / hitCount).normalized;

        // Reject surfaces that are mostly horizontal (floors, not walls).
        return Vector3.Dot(wallNormal, Vector3.up) < 0.5f;
    }

    // ─── Visual Root Rotation ──────────────────────────────────────────────────

    private void RotateVisualRoot()
    {
        if (visualRoot == null || cameraController == null)
            return;

        Vector3 forward = cameraController.HorizontalForward;
        if (forward.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(forward, Vector3.up);
        visualRoot.rotation = Quaternion.Slerp(
            visualRoot.rotation, targetRotation, bodyRotationSpeed * Time.fixedDeltaTime);
    }

    // ─── Gizmos ────────────────────────────────────────────────────────────────

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        CapsuleCollider col = GetComponent<CapsuleCollider>() ?? GetComponentInChildren<CapsuleCollider>();
        if (col == null)
            return;

        // Ground check sphere.
        Gizmos.color = Color.green;
        Vector3 colBottom  = col.transform.position + col.center + Vector3.down * (col.height * 0.5f);
        Vector3 footCentre = colBottom + Vector3.up * groundCheckOffset;
        Gizmos.DrawWireSphere(footCentre, groundCheckRadius);

        // Wall check rays.
        Gizmos.color = Color.cyan;
        float hipHeight   = col.height * 0.25f;
        Vector3 hipCentre = transform.position + Vector3.up * hipHeight;
        foreach (Vector3 dir in WallCheckDirections)
            Gizmos.DrawRay(hipCentre, dir * wallCheckDistance);
    }
#endif
}
