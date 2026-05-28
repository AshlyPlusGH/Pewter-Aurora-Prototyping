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

    // ─── Private State ─────────────────────────────────────────────────────────

    private float _yaw;
    private float _pitch;

    // ─── Public Accessors ──────────────────────────────────────────────────────

    /// <summary>Camera forward projected onto the XZ plane and normalised.</summary>
    public Vector3 HorizontalForward { get; private set; } = Vector3.forward;

    /// <summary>Camera right projected onto the XZ plane and normalised.</summary>
    public Vector3 HorizontalRight { get; private set; } = Vector3.right;

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
            _yaw = target.eulerAngles.y;
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

        _yaw  += mouseX;
        _pitch = Mathf.Clamp(_pitch + mouseY, minPitch, maxPitch);
    }

    // ─── Camera Positioning ────────────────────────────────────────────────────

    private void PositionCamera()
    {
        if (target == null || cameraTransform == null)
            return;

        Vector3 pivotWorld = target.position + Vector3.up * heightOffset;

        Quaternion rotation = Quaternion.Euler(_pitch, _yaw, 0f);
        Vector3 offset      = rotation * new Vector3(0f, 0f, -cameraDistance);

        cameraTransform.position = pivotWorld + offset;
        cameraTransform.LookAt(pivotWorld, Vector3.up);
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

        Gizmos.color = Color.yellow;
        Vector3 pivot = target.position + Vector3.up * heightOffset;
        Gizmos.DrawWireSphere(pivot, 0.1f);

        if (cameraTransform != null)
            Gizmos.DrawLine(pivot, cameraTransform.position);
    }
#endif
}
