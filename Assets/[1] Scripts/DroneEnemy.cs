using UnityEngine;

/// <summary>
/// Rigidbody-driven drone enemy. Wanders freely, strafes the player on sight,
/// occasionally pauses to take a "shot" (slow-down stand-in), and periodically
/// telegraphs a red flash before committing to a kamikaze charge whose turning
/// degrades as it accelerates — giving the player a window to dodge.
/// Physics handles wall collisions. Height varies via Perlin noise and surface
/// avoidance raycasts keep the drone clear of nearby geometry.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class DroneEnemy : MonoBehaviour
{
    // ─── References ────────────────────────────────────────────────────────────

    [Header("References")]
    [Tooltip("Auto-resolved via PlayerController if left empty.")]
    public Transform playerTarget;

    // ─── Detection ─────────────────────────────────────────────────────────────

    [Header("Detection")]
    [Tooltip("Maximum distance at which the drone can detect the player.")]
    public float detectionRange = 22f;

    [Tooltip("Layers that block line of sight AND used for height/avoidance raycasts " +
             "(e.g. Default geometry). Should NOT include the player's layer.")]
    public LayerMask sightBlockingLayers;

    // ─── Movement ──────────────────────────────────────────────────────────────

    [Header("Movement")]
    [Tooltip("Top speed used in Wander, Strafe, and Investigate states.")]
    public float moveSpeed = 5f;

    [Tooltip("Rate of velocity change (m/s²) toward a drive target.")]
    public float moveAcceleration = 25f;

    [Tooltip("Rotation smoothing speed (Slerp factor per second).")]
    public float rotationSpeed = 6f;

    [Tooltip("XZ arrival radius — height is excluded from arrival checks.")]
    public float arrivalThreshold = 0.8f;

    // ─── Height & surface avoidance ────────────────────────────────────────────

    [Header("Height & Surface Avoidance")]
    [Tooltip("Minimum clearance the drone tries to maintain above any surface below it.")]
    public float minGroundClearance = 1.5f;

    [Tooltip("How much the drone's target height oscillates above/below its reference Y.")]
    public float heightVariationAmplitude = 1.2f;

    [Tooltip("Time-scale of the Perlin noise driving height variation. Smaller = slower.")]
    public float heightVariationSpeed = 0.22f;

    [Tooltip("Distance at which the drone starts steering away from nearby surfaces.")]
    public float surfaceAvoidanceRange = 2.5f;

    [Tooltip("Maximum velocity contribution (m/s) from surface avoidance at point-blank range.")]
    public float surfaceAvoidanceStrength = 9f;

    // ─── Wander ────────────────────────────────────────────────────────────────

    [Header("Wander")]
    [Tooltip("Radius of the wander area around the spawn position.")]
    public float wanderRadius = 10f;

    [Tooltip("How long the drone dwells at each waypoint before picking the next.")]
    public float wanderDwellTime = 2.5f;

    // ─── Strafe ────────────────────────────────────────────────────────────────

    [Header("Strafe")]
    [Tooltip("XZ orbit radius while strafing around the player.")]
    public float strafeRadius = 6f;

    [Tooltip("Orbit speed in degrees per second.")]
    public float strafeAngularSpeed = 50f;

    [Tooltip("Y offset above the player used as the height reference while strafing.")]
    public float strafeHoverOffset = 1.8f;

    [Tooltip("Seconds between random direction reversals.")]
    public float strafeDirectionInterval = 2.5f;

    // ─── Shooting stand-in ─────────────────────────────────────────────────────

    [Header("Shooting (Stand-In)")]
    [Tooltip("How long the drone slows to a near-stop while 'shooting'.")]
    public float shootDuration = 0.9f;

    [Tooltip("Average interval between shots while strafing. Randomised ±30% each cycle.")]
    public float shootInterval = 5f;

    // ─── Kamikaze ──────────────────────────────────────────────────────────────

    [Header("Kamikaze")]
    [Tooltip("Probability per second of triggering a kamikaze once the cooldown has expired.")]
    public float kamikazeProbabilityPerSecond = 0.12f;

    [Tooltip("Minimum strafing time between kamikaze attempts.")]
    public float kamikazeCooldown = 10f;

    [Tooltip("Seconds the drone flashes red before charging.")]
    public float telegraphDuration = 1.5f;

    [Tooltip("Red flash frequency in Hz during the telegraph.")]
    public float telegraphFlashRate = 6f;

    [Tooltip("Top speed the drone reaches during the charge.")]
    public float kamikazeMaxSpeed = 18f;

    [Tooltip("Acceleration applied toward the player during the charge.")]
    public float kamikazeAcceleration = 14f;

    [Tooltip("Turning rate (deg/s) at low speed — should feel somewhat steerable.")]
    public float kamikazeTurnRateMax = 160f;

    [Tooltip("Turning rate (deg/s) at full speed — keep low so the player can sidestep.")]
    public float kamikazeTurnRateMin = 18f;

    [Tooltip("Maximum duration of a single charge before the drone gives up.")]
    public float kamikazeDuration = 4f;

    // ─── Kick & Stun ───────────────────────────────────────────────────────────

    [Header("Kick & Stun")]
    [Tooltip("Impulse magnitude applied to the drone when the player wall-kicks off it.")]
    public float kickForce = 10f;

    [Tooltip("How long the drone remains stunned after being wall-kicked.")]
    public float stunDuration = 2.5f;

    // ─── Visuals ───────────────────────────────────────────────────────────────

    [Header("Visuals")]
    public Color normalColor   = Color.white;
    public Color kamikazeColor = Color.red;
    public Color stunColor     = Color.yellow;

    // ─── Debug ─────────────────────────────────────────────────────────────────

    [Header("Debug")]
    [SerializeField] private DroneState _debugState;
    [SerializeField] private bool       _debugHasLos;

    // ─── State machine ─────────────────────────────────────────────────────────

    private enum DroneState
    {
        Wandering,
        Strafing,
        Shooting,
        Telegraphing,
        Kamikazeing,
        Investigating,
        Stunned
    }

    private DroneState _state = DroneState.Wandering;

    // ─── Components ────────────────────────────────────────────────────────────

    private Rigidbody             _rb;
    private Renderer              _rend;
    private MaterialPropertyBlock _mpb;

    // ─── Wander ────────────────────────────────────────────────────────────────

    private Vector3 _spawnPosition;
    private Vector3 _wanderTarget;
    private float   _wanderDwellTimer;

    // ─── Strafe / shoot ────────────────────────────────────────────────────────

    private float _strafeAngle;
    private int   _strafeDirection = 1;
    private float _strafeDirectionTimer;
    private float _shootTimer;
    private float _kamikazeCooldownTimer;

    // ─── Shared state timer ────────────────────────────────────────────────────

    private float   _stateTimer;
    private Vector3 _lastKnownPlayerPos;

    // ─── Height variation ──────────────────────────────────────────────────────

    private float _perlinOffset;

    // ─── Avoidance directions (cached, no per-frame allocation) ────────────────

    private static readonly Vector3[] AvoidanceDirs =
    {
        Vector3.down, Vector3.up,
        Vector3.right, Vector3.left,
        Vector3.forward, Vector3.back
    };

    // ──────────────────────────────────────────────────────────────────────────
    // Lifecycle
    // ──────────────────────────────────────────────────────────────────────────

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.useGravity             = false;
        _rb.freezeRotation         = true;
        _rb.linearDamping          = 1f;
        _rb.angularDamping         = 10f;
        _rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        _rend = GetComponentInChildren<Renderer>();
        _mpb  = new MaterialPropertyBlock();
        SetColor(normalColor);

        _spawnPosition = transform.position;
        _perlinOffset  = Random.Range(0f, 100f);  // unique noise slice per instance
        PickWanderTarget();
    }

    private void Start()
    {
        if (playerTarget == null)
        {
            var pc = FindFirstObjectByType<PlayerController>();
            if (pc != null)
                playerTarget = pc.transform;
            else
                Debug.LogWarning("[DroneEnemy] No PlayerController found in scene.", this);
        }

        _shootTimer            = shootInterval * Random.Range(0.5f, 1.5f);
        _kamikazeCooldownTimer = kamikazeCooldown;
    }

    private void FixedUpdate()
    {
        if (playerTarget == null) return;

        bool los     = HasLineOfSight();
        _debugHasLos = los;
        _debugState  = _state;

        switch (_state)
        {
            case DroneState.Wandering:     FixedWander(los);      break;
            case DroneState.Strafing:      FixedStrafe(los);      break;
            case DroneState.Shooting:      FixedShooting();       break;
            case DroneState.Telegraphing:  FixedTelegraph(los);   break;
            case DroneState.Kamikazeing:   FixedKamikaze();       break;
            case DroneState.Investigating: FixedInvestigate(los); break;
            case DroneState.Stunned:       FixedStunned();        break;
        }
    }

    private void Update()
    {
        UpdateVisuals();
    }

    // ──────────────────────────────────────────────────────────────────────────
    // State — Wandering
    // ──────────────────────────────────────────────────────────────────────────

    private void FixedWander(bool los)
    {
        if (los) { EnterStrafe(); return; }

        float   targetY = ComputeTargetY(_spawnPosition.y);
        Vector3 target  = new Vector3(_wanderTarget.x, targetY, _wanderTarget.z);
        DriveToward(target, moveSpeed);

        _wanderDwellTimer -= Time.fixedDeltaTime;
        if (XZDistance(transform.position, _wanderTarget) < arrivalThreshold
            || _wanderDwellTimer <= 0f)
            PickWanderTarget();
    }

    // ──────────────────────────────────────────────────────────────────────────
    // State — Strafing
    // ──────────────────────────────────────────────────────────────────────────

    private void FixedStrafe(bool los)
    {
        if (!los)
        {
            _lastKnownPlayerPos = playerTarget.position + Vector3.up * strafeHoverOffset;
            _state = DroneState.Investigating;
            return;
        }

        // Shoot check
        _shootTimer -= Time.fixedDeltaTime;
        if (_shootTimer <= 0f)
        {
            _stateTimer = shootDuration;
            _state      = DroneState.Shooting;
            return;
        }

        // Kamikaze check
        _kamikazeCooldownTimer -= Time.fixedDeltaTime;
        if (_kamikazeCooldownTimer <= 0f
            && Random.value < kamikazeProbabilityPerSecond * Time.fixedDeltaTime)
        {
            _stateTimer = telegraphDuration;
            _state      = DroneState.Telegraphing;
            return;
        }

        // Orbit
        _strafeDirectionTimer -= Time.fixedDeltaTime;
        if (_strafeDirectionTimer <= 0f)
        {
            _strafeDirectionTimer = strafeDirectionInterval + Random.Range(-0.5f, 0.5f);
            if (Random.value < 0.3f)
                _strafeDirection = -_strafeDirection;
        }

        _strafeAngle += _strafeDirection * strafeAngularSpeed * Time.fixedDeltaTime;

        float   rad         = _strafeAngle * Mathf.Deg2Rad;
        Vector3 orbitOffset = new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad)) * strafeRadius;
        float   targetY     = ComputeTargetY(playerTarget.position.y + strafeHoverOffset);
        Vector3 desired     = new Vector3(
            playerTarget.position.x + orbitOffset.x,
            targetY,
            playerTarget.position.z + orbitOffset.z);

        DriveToward(desired, moveSpeed);
        FacePoint(playerTarget.position);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // State — Shooting (stand-in)
    // ──────────────────────────────────────────────────────────────────────────

    private void FixedShooting()
    {
        // Hover in place with height correction and surface avoidance
        ApplyHoverVelocity(transform.position.y);
        FacePoint(playerTarget.position);

        _stateTimer -= Time.fixedDeltaTime;
        if (_stateTimer <= 0f)
        {
            _shootTimer = shootInterval * Random.Range(0.7f, 1.3f);
            EnterStrafe();
        }
    }

    // ──────────────────────────────────────────────────────────────────────────
    // State — Telegraphing (pre-kamikaze flash)
    // ──────────────────────────────────────────────────────────────────────────

    private void FixedTelegraph(bool los)
    {
        // Hover in place — height + avoidance still active so it doesn't sink
        ApplyHoverVelocity(transform.position.y);
        if (los) FacePoint(playerTarget.position);

        _stateTimer -= Time.fixedDeltaTime;
        if (_stateTimer <= 0f)
        {
            _kamikazeCooldownTimer = kamikazeCooldown;
            _stateTimer            = kamikazeDuration;
            _state                 = DroneState.Kamikazeing;
        }
    }

    // ──────────────────────────────────────────────────────────────────────────
    // State — Kamikazeing
    // ──────────────────────────────────────────────────────────────────────────

    private void FixedKamikaze()
    {
        _stateTimer -= Time.fixedDeltaTime;
        if (_stateTimer <= 0f) { EnterStrafe(); return; }

        Vector3 toPlayer = playerTarget.position - transform.position;
        float   dist     = toPlayer.magnitude;

        if (dist < 1.2f) { EnterStrafe(); return; }

        Vector3 desiredDir   = toPlayer.normalized;
        float   currentSpeed = _rb.linearVelocity.magnitude;
        Vector3 currentDir   = currentSpeed > 0.5f
            ? _rb.linearVelocity.normalized
            : transform.forward;

        float speedFraction = Mathf.Clamp01(currentSpeed / kamikazeMaxSpeed);
        float maxTurnRad    = Mathf.Lerp(kamikazeTurnRateMax, kamikazeTurnRateMin, speedFraction)
                              * Mathf.Deg2Rad * Time.fixedDeltaTime;

        Vector3 steerDir = Vector3.RotateTowards(currentDir, desiredDir, maxTurnRad, 0f);
        float   newSpeed = Mathf.Min(currentSpeed + kamikazeAcceleration * Time.fixedDeltaTime,
                                     kamikazeMaxSpeed);

        _rb.linearVelocity = steerDir * newSpeed;

        Vector3 flatSteer = new Vector3(steerDir.x, 0f, steerDir.z);
        if (flatSteer.sqrMagnitude > 0.001f)
            FaceDirection(flatSteer);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // State — Investigating
    // ──────────────────────────────────────────────────────────────────────────

    private void FixedInvestigate(bool los)
    {
        if (los) { EnterStrafe(); return; }

        float   targetY = ComputeTargetY(_lastKnownPlayerPos.y);
        Vector3 target  = new Vector3(_lastKnownPlayerPos.x, targetY, _lastKnownPlayerPos.z);
        DriveToward(target, moveSpeed);

        if (XZDistance(transform.position, _lastKnownPlayerPos) < arrivalThreshold)
        {
            _state = DroneState.Wandering;
            PickWanderTarget();
        }
    }

    // ──────────────────────────────────────────────────────────────────────────
    // State — Stunned
    // ──────────────────────────────────────────────────────────────────────────

    private void FixedStunned()
    {
        // Don't steer — let the kick impulse carry the drone naturally.
        // Use AddForce so we nudge without overriding the Rigidbody velocity.
        float   dy         = ComputeTargetY(transform.position.y) - transform.position.y;
        Vector3 correction = ComputeAvoidanceVelocity();
        correction.y      += Mathf.Clamp(dy * 5f, -3f, 3f);
        _rb.AddForce(correction, ForceMode.Acceleration);

        _stateTimer -= Time.fixedDeltaTime;
        if (_stateTimer <= 0f)
            RecoverFromStun();
    }

    private void RecoverFromStun()
    {
        if (HasLineOfSight())
            EnterStrafe();
        else
        {
            _state = DroneState.Wandering;
            PickWanderTarget();
        }
    }

    /// <summary>
    /// Called by PlayerController when the player wall-kicks off this drone.
    /// Bleeds the current velocity, applies an impulse in kickDirection, and
    /// enters the Stunned state for stunDuration seconds.
    /// </summary>
    public void ReceiveKick(Vector3 kickDirection)
    {
        // Bleed existing velocity so the kick impulse reads cleanly
        _rb.linearVelocity *= 0.2f;
        _rb.AddForce(kickDirection.normalized * kickForce, ForceMode.Impulse);
        _stateTimer = stunDuration;
        _state      = DroneState.Stunned;
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Collision — kamikaze contact
    // ──────────────────────────────────────────────────────────────────────────

    private void OnCollisionEnter(Collision collision)
    {
        if (_state != DroneState.Kamikazeing) return;
        _rb.linearVelocity *= 0.15f;
        EnterStrafe();
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Visuals
    // ──────────────────────────────────────────────────────────────────────────

    private void UpdateVisuals()
    {
        switch (_state)
        {
            case DroneState.Telegraphing:
                float t = Mathf.Abs(Mathf.Sin(Time.time * telegraphFlashRate * Mathf.PI));
                SetColor(Color.Lerp(normalColor, kamikazeColor, t));
                break;
            case DroneState.Kamikazeing:
                SetColor(kamikazeColor);
                break;
            case DroneState.Stunned:
                SetColor(stunColor);
                break;
            default:
                SetColor(normalColor);
                break;
        }
    }

    /// <summary>Applies a colour via MaterialPropertyBlock — no material allocation.</summary>
    private void SetColor(Color color)
    {
        if (_rend == null) return;
        _mpb.SetColor("_BaseColor", color);
        _rend.SetPropertyBlock(_mpb);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Height & avoidance
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Computes a target Y that oscillates around referenceY via Perlin noise,
    /// then clamps it so the drone never dips below minGroundClearance above the
    /// nearest surface directly below it.
    /// </summary>
    private float ComputeTargetY(float referenceY)
    {
        float noise     = Mathf.PerlinNoise(Time.fixedTime * heightVariationSpeed + _perlinOffset,
                                            _perlinOffset * 0.31f);
        float variation = (noise * 2f - 1f) * heightVariationAmplitude;
        float targetY   = referenceY + variation;

        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit,
                            100f, sightBlockingLayers))
            targetY = Mathf.Max(targetY, hit.point.y + minGroundClearance);

        return targetY;
    }

    /// <summary>
    /// Casts a ray in each of 6 cardinal directions and accumulates a repulsion
    /// velocity away from any surface found within surfaceAvoidanceRange.
    /// Uses quadratic falloff so repulsion is gentle at range but firm up close.
    /// </summary>
    private Vector3 ComputeAvoidanceVelocity()
    {
        Vector3 avoidance = Vector3.zero;
        foreach (Vector3 dir in AvoidanceDirs)
        {
            if (Physics.Raycast(transform.position, dir, out RaycastHit hit,
                                surfaceAvoidanceRange, sightBlockingLayers))
            {
                float t = 1f - hit.distance / surfaceAvoidanceRange;
                avoidance += hit.normal * (t * t) * surfaceAvoidanceStrength;
            }
        }
        return avoidance;
    }

    /// <summary>
    /// Drives the Rigidbody toward a world-space destination at up to targetSpeed.
    /// Surface avoidance is blended into the target velocity so the drone steers
    /// clear of nearby geometry without fighting its movement.
    /// Speed is reduced proportionally as the drone closes the last few metres.
    /// </summary>
    private void DriveToward(Vector3 destination, float targetSpeed)
    {
        Vector3 delta = destination - transform.position;
        float   dist  = delta.magnitude;
        if (dist < 0.01f) return;

        float   speed     = Mathf.Min(dist * 2f, targetSpeed);
        Vector3 targetVel = delta.normalized * speed + ComputeAvoidanceVelocity();

        _rb.linearVelocity = Vector3.MoveTowards(
            _rb.linearVelocity, targetVel, moveAcceleration * Time.fixedDeltaTime);

        Vector3 flatDir = new Vector3(delta.x, 0f, delta.z);
        if (flatDir.sqrMagnitude > 0.01f)
            FaceDirection(flatDir);
    }

    /// <summary>
    /// Holds the drone near its current XZ position while keeping it at the
    /// Perlin-varied target height and away from surfaces.
    /// Used by Shooting and Telegraphing states.
    /// </summary>
    private void ApplyHoverVelocity(float referenceY)
    {
        float   targetY  = ComputeTargetY(referenceY);
        float   dy       = targetY - transform.position.y;

        Vector3 targetVel = ComputeAvoidanceVelocity();
        targetVel.y += Mathf.Clamp(dy * 4f, -moveSpeed * 0.4f, moveSpeed * 0.4f);

        _rb.linearVelocity = Vector3.Lerp(_rb.linearVelocity, targetVel, 8f * Time.fixedDeltaTime);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Helpers
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>Returns the flat XZ distance between two positions, ignoring height.</summary>
    private static float XZDistance(Vector3 a, Vector3 b)
        => new Vector2(a.x - b.x, a.z - b.z).magnitude;

    private bool HasLineOfSight()
    {
        Vector3 toPlayer = playerTarget.position - transform.position;
        float   dist     = toPlayer.magnitude;
        if (dist > detectionRange) return false;
        return !Physics.Raycast(transform.position, toPlayer / dist, dist - 0.1f, sightBlockingLayers);
    }

    private void EnterStrafe()
    {
        Vector3 offset        = transform.position - playerTarget.position;
        _strafeAngle          = Mathf.Atan2(offset.z, offset.x) * Mathf.Rad2Deg;
        _strafeDirection      = Random.value > 0.5f ? 1 : -1;
        _strafeDirectionTimer = strafeDirectionInterval;
        _state                = DroneState.Strafing;
    }

    private void FacePoint(Vector3 point)
    {
        Vector3 dir = point - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.001f)
            FaceDirection(dir);
    }

    private void FaceDirection(Vector3 flatDir)
    {
        if (flatDir.sqrMagnitude < 0.001f) return;
        Quaternion desired = Quaternion.LookRotation(flatDir);
        transform.rotation = Quaternion.Slerp(transform.rotation, desired,
                                              rotationSpeed * Time.fixedDeltaTime);
    }

    private void PickWanderTarget()
    {
        Vector2 rand      = Random.insideUnitCircle * wanderRadius;
        _wanderTarget     = _spawnPosition + new Vector3(rand.x, 0f, rand.y);
        _wanderDwellTimer = wanderDwellTime + Random.Range(-0.5f, 0.5f);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Scene gizmos
    // ──────────────────────────────────────────────────────────────────────────

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Vector3 origin = Application.isPlaying ? _spawnPosition : transform.position;
        Gizmos.color   = new Color(0f, 1f, 1f, 0.4f);
        Gizmos.DrawWireSphere(origin, wanderRadius);

        Gizmos.color = new Color(1f, 0.5f, 0f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, strafeRadius);

        Gizmos.color = new Color(1f, 0f, 1f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, surfaceAvoidanceRange);

        if (!Application.isPlaying) return;

        if (_state == DroneState.Wandering)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, _wanderTarget);
            Gizmos.DrawSphere(_wanderTarget, 0.2f);
        }

        if (_state == DroneState.Investigating)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, _lastKnownPlayerPos);
            Gizmos.DrawSphere(_lastKnownPlayerPos, 0.3f);
        }
    }
}
