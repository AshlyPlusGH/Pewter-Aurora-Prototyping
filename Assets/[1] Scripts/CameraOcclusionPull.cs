using UnityEngine;

/// <summary>
/// Prototype — disabled by default.
/// When an object on the chosen layers intersects the line between the camera
/// pivot and the desired camera position, the camera is smoothly pulled forward
/// to just in front of that object. Attach to the same GameObject as PlayerCameraController.
/// </summary>
[DefaultExecutionOrder(100)]
public class CameraOcclusionPull : MonoBehaviour
{
    // ─── Settings ──────────────────────────────────────────────────────────────

    [Header("Occlusion Pull — Prototype (disabled by default)")]
    [Tooltip("Toggle this prototype on or off.")]
    public bool enablePull = false;

    [Tooltip("Clearance kept between the occluding surface and the camera.")]
    public float collisionMargin = 0.3f;

    [Tooltip("Radius of the sphere cast used for occlusion detection. " +
             "Larger values pull the camera in earlier, preventing near-clipping.")]
    public float castRadius = 0.15f;

    [Tooltip("Speed at which the camera distance lerps toward the target value.")]
    public float smoothSpeed = 15f;

    [Tooltip("Layers whose objects trigger camera pull-in.")]
    public LayerMask occlusionLayers;

    // ─── Private State ─────────────────────────────────────────────────────────

    private PlayerCameraController _cam;
    private float _currentDistance;

    // ─── Unity Lifecycle ───────────────────────────────────────────────────────

    private void Awake()
    {
        _cam = GetComponent<PlayerCameraController>();
        if (_cam == null)
            _cam = FindFirstObjectByType<PlayerCameraController>();

        // Default to the Default layer (layer 0) if nothing is set.
        if (occlusionLayers.value == 0)
            occlusionLayers = 1;

        if (_cam != null)
            _currentDistance = _cam.cameraDistance;
    }

    private void OnEnable()
    {
        // Snap to configured distance so there's no jarring jump when first enabled.
        if (_cam != null)
            _currentDistance = _cam.cameraDistance;
    }

    private void LateUpdate()
    {
        if (!enablePull || _cam == null || _cam.target == null || _cam.cameraTransform == null)
            return;

        Vector3    pivot      = _cam.target.position + Vector3.up * _cam.heightOffset;
        Quaternion rotation   = Quaternion.Euler(_cam.Pitch, _cam.Yaw, 0f);
        // rotation * Vector3.back = direction FROM pivot TO camera (behind the player).
        Vector3    desiredDir = rotation * Vector3.back;
        float      maxDist    = _cam.cameraDistance;

        // Find the nearest occluder along the desired camera ray.
        float targetDistance = maxDist;
        if (Physics.SphereCast(pivot, castRadius, desiredDir, out RaycastHit hit,
                maxDist, occlusionLayers, QueryTriggerInteraction.Ignore))
        {
            targetDistance = Mathf.Max(0f, hit.distance - collisionMargin);
        }

        // Smooth the transition.
        _currentDistance = Mathf.Lerp(_currentDistance, targetDistance, smoothSpeed * Time.deltaTime);

        // Override the camera position produced by PlayerCameraController this frame.
        // desiredDir already points away from pivot, so positive distance places camera behind player.
        _cam.cameraTransform.position = pivot + desiredDir * _currentDistance;
        _cam.cameraTransform.LookAt(pivot, Vector3.up);
    }
}
