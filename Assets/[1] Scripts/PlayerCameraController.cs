using UnityEngine;

/// <summary>
/// Orbits the scene camera around a target transform's midpoint using mouse input.
/// Exposes HorizontalForward and HorizontalRight for PlayerController to use.
/// All references are resolved automatically at runtime — no Inspector wiring required.
/// </summary>
public class PlayerCameraController : MonoBehaviour
{
    // ─── References ────────────────────────────────────────────────────────────

    [Header("References")]
    [Tooltip("The transform the camera orbits. Auto-resolved to the tagged 'Player' if left empty.")]
    public Transform target;

    [Tooltip("The camera transform that is repositioned each LateUpdate. Auto-resolved to Camera.main if left empty.")]
    public Transform cameraTransform;

    // ─── Orbit ─────────────────────────────────────────────────────────────────

    [Header("Orbit")]
    [Tooltip("Mouse sensitivity for horizontal and vertical rotation.")]
    public float mouseSensitivity = 2.5f;

    [Tooltip("Invert the vertical mouse axis.")]
    public bool invertY = false;

    [Tooltip("Minimum vertical pitch (degrees — negative = look up).")]
    public float minPitch = -30f;

    [Tooltip("Maximum vertical pitch (degrees — positive = look down).")]
    public float maxPitch = 70f;

    [Tooltip("Vertical offset from the target pivot to the orbit centre (character midpoint).")]
    public float heightOffset = 1f;

    [Tooltip("Distance the camera is pulled back from the pivot point.")]
    public float cameraDistance = 5f;

    // ─── Inertia ───────────────────────────────────────────────────────────────

    [Header("Inertia")]
    [Tooltip("How quickly the camera rotation velocity tracks mouse input at 60 fps. " +
             "1 = instant (no inertia); lower values add a subtle trailing deceleration. " +
             "A value around 0.2–0.3 gives a small but perceptible feel.")]
    [Range(0.01f, 1f)]
    public float inertiaSmoothing = 0.25f;

    [Tooltip("How quickly the camera pivot position tracks the player's position at 60 fps. " +
             "1 = instant snap (no positional lag); lower values add a soft drag on player movement. " +
             "A value around 0.1–0.2 gives a subtle but perceptible trailing feel.")]
    [Range(0.01f, 1f)]
    public float movementInertia = 0.15f;

    // ─── Private State ─────────────────────────────────────────────────────────

    private float   _yaw;
    private float   _pitch;
    private Vector2 _rotationVelocity;
    private Vector3 _smoothedPivot;

    // ─── Public Accessors ──────────────────────────────────────────────────────

    /// <summary>Camera forward projected onto the XZ plane and normalised.</summary>
    public Vector3 HorizontalForward { get; private set; } = Vector3.forward;

    /// <summary>Camera right projected onto the XZ plane and normalised.</summary>
    public Vector3 HorizontalRight { get; private set; } = Vector3.right;

    /// <summary>Current horizontal orbit angle in degrees.</summary>
    public float Yaw => _yaw;

    /// <summary>Current vertical orbit angle in degrees.</summary>
    public float Pitch => _pitch;

    // ─── Unity Lifecycle ───────────────────────────────────────────────────────

    private void Awake()
    {
        // Auto-resolve target from Player tag.
        if (target == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
                target = player.transform;
        }

        // Fallback: find a PlayerController in the scene.
        if (target == null)
        {
            PlayerController pc = FindFirstObjectByType<PlayerController>();
            if (pc != null)
                target = pc.transform;
        }

        if (target == null)
            Debug.LogError("[PlayerCameraController] No target found. Tag your Player GameObject as 'Player' or add a PlayerController.", this);

        // Auto-resolve camera to Camera.main.
        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        if (cameraTransform == null)
            Debug.LogError("[PlayerCameraController] No camera found. Ensure a Camera tagged 'MainCamera' exists in the scene.", this);
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;

        // Initialise yaw from the target's current rotation so the camera
        // does not snap on the first frame.
        if (target != null)
        {
            _yaw = target.eulerAngles.y;
            _smoothedPivot = target.position + Vector3.up * heightOffset;
        }
    }

    private void LateUpdate()
    {
        ReadMouseInput();
        PositionCamera();
        UpdateHorizontalAxes();
    }

    // ─── Input ─────────────────────────────────────────────────────────────────

    private void ReadMouseInput()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity * (invertY ? 1f : -1f);

        // Frame-rate-independent exponential lerp: velocity decays toward raw input each frame.
        // At 60 fps this equals exactly inertiaSmoothing per frame.
        float t = 1f - Mathf.Pow(1f - inertiaSmoothing, Time.deltaTime * 60f);
        _rotationVelocity = Vector2.Lerp(_rotationVelocity, new Vector2(mouseX, mouseY), t);

        _yaw  += _rotationVelocity.x;
        _pitch = Mathf.Clamp(_pitch + _rotationVelocity.y, minPitch, maxPitch);
    }

    // ─── Camera Positioning ────────────────────────────────────────────────────

    private void PositionCamera()
    {
        if (target == null || cameraTransform == null)
            return;

        Vector3 targetPivot = target.position + Vector3.up * heightOffset;

        // Frame-rate-independent exponential lerp toward the target pivot.
        // movementInertia of 1 = instant tracking; lower values add a soft lag.
        float t = 1f - Mathf.Pow(1f - movementInertia, Time.deltaTime * 60f);
        _smoothedPivot = Vector3.Lerp(_smoothedPivot, targetPivot, t);

        Quaternion rotation = Quaternion.Euler(_pitch, _yaw, 0f);
        Vector3 offset      = rotation * new Vector3(0f, 0f, -cameraDistance);

        cameraTransform.position = _smoothedPivot + offset;
        cameraTransform.LookAt(_smoothedPivot, Vector3.up);
    }

    // ─── Horizontal Axes ───────────────────────────────────────────────────────

    private void UpdateHorizontalAxes()
    {
        if (cameraTransform == null)
            return;

        Vector3 forward = cameraTransform.forward;
        forward.y = 0f;

        if (forward.sqrMagnitude < 0.001f)
        {
            // Camera pointing straight up or down — keep previous axes.
            return;
        }

        HorizontalForward = forward.normalized;
        HorizontalRight   = Vector3.Cross(Vector3.up, HorizontalForward).normalized;
    }

    // ─── Gizmos ────────────────────────────────────────────────────────────────

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (target == null)
            return;

        // Show both the true target pivot and the smoothed (lagging) pivot.
        Vector3 targetPivot = target.position + Vector3.up * heightOffset;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(targetPivot, 0.1f);

        if (Application.isPlaying)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(_smoothedPivot, 0.08f);
            Gizmos.DrawLine(targetPivot, _smoothedPivot);
        }

        if (cameraTransform != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(Application.isPlaying ? _smoothedPivot : targetPivot, cameraTransform.position);
        }
    }
#endif
}
