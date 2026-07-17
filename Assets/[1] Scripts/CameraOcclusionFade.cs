using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Prototype — disabled by default.
/// Objects on the chosen layers between the camera pivot and the camera position
/// are switched to alpha-blend transparent at occlusionAlpha, then restored when
/// they stop occluding. Attach to the same GameObject as PlayerCameraController.
/// </summary>
[DefaultExecutionOrder(100)]
public class CameraOcclusionFade : MonoBehaviour
{
    // ─── Settings ──────────────────────────────────────────────────────────────

    [Header("Occlusion Fade — Prototype (disabled by default)")]
    [Tooltip("Toggle this prototype on or off.")]
    public bool enableFade = false;

    [Tooltip("Alpha applied to occluding objects. 0 = invisible, 1 = fully opaque.")]
    [Range(0f, 1f)]
    public float occlusionAlpha = 0.3f;

    [Tooltip("Layers whose objects are faded when they occlude the player.")]
    public LayerMask occlusionLayers;

    // ─── Private State ─────────────────────────────────────────────────────────

    private PlayerCameraController _cam;

    /// <summary>
    /// For each faded renderer, stores (originalSharedMaterials, fadedCopies) so both
    /// can be referenced cleanly on restore without relying on per-instance material state.
    /// </summary>
    private readonly Dictionary<Renderer, (Material[] originals, Material[] copies)> _faded = new();

    // ─── Unity Lifecycle ───────────────────────────────────────────────────────

    private void Awake()
    {
        _cam = GetComponent<PlayerCameraController>();
        if (_cam == null)
            _cam = FindFirstObjectByType<PlayerCameraController>();

        if (occlusionLayers.value == 0)
            occlusionLayers = 1; // Default layer
    }

    private void LateUpdate()
    {
        if (!enableFade || _cam == null || _cam.target == null || _cam.cameraTransform == null)
        {
            RestoreAll();
            return;
        }

        UpdateOccluders();
    }

    private void OnDisable() => RestoreAll();
    private void OnDestroy() => RestoreAll();

    // ─── Occlusion Logic ───────────────────────────────────────────────────────

    private void UpdateOccluders()
    {
        Vector3 pivot  = _cam.target.position + Vector3.up * _cam.heightOffset;
        Vector3 camPos = _cam.cameraTransform.position;
        Vector3 delta  = camPos - pivot;
        float   dist   = delta.magnitude;

        if (dist < 0.001f)
            return;

        RaycastHit[] hits = Physics.RaycastAll(
            pivot, delta / dist, dist, occlusionLayers, QueryTriggerInteraction.Ignore);

        var occluding = new HashSet<Renderer>();
        foreach (RaycastHit hit in hits)
        {
            if (hit.transform.IsChildOf(_cam.target))
                continue;

            Renderer rend = hit.collider.GetComponent<Renderer>();
            if (rend == null || !rend)
                continue;

            occluding.Add(rend);
            if (!_faded.ContainsKey(rend))
                FadeRenderer(rend);
        }

        // Restore anything that is no longer in the way.
        var toRestore = new List<Renderer>();
        foreach (Renderer r in _faded.Keys)
            if (!occluding.Contains(r) || !r)
                toRestore.Add(r);

        foreach (Renderer r in toRestore)
            RestoreRenderer(r);
    }

    // ─── Material Manipulation ─────────────────────────────────────────────────

    private void FadeRenderer(Renderer rend)
    {
        Material[] originals = rend.sharedMaterials;
        var copies = new Material[originals.Length];
        for (int i = 0; i < originals.Length; i++)
        {
            copies[i] = new Material(originals[i]);
            SetUrpAlphaBlend(copies[i], occlusionAlpha);
        }
        _faded[rend] = (originals, copies);
        // Assign directly as sharedMaterials — copies are new so nothing else references them.
        rend.sharedMaterials = copies;
    }

    private void RestoreRenderer(Renderer rend)
    {
        if (!_faded.TryGetValue(rend, out (Material[] originals, Material[] copies) entry))
            return;

        if (rend)
            rend.sharedMaterials = entry.originals;

        foreach (Material m in entry.copies)
            if (m) Destroy(m);

        _faded.Remove(rend);
    }

    private void RestoreAll()
    {
        foreach (Renderer r in new List<Renderer>(_faded.Keys))
            RestoreRenderer(r);
    }

    /// <summary>
    /// Applies all URP Lit properties required for alpha-blend transparent rendering.
    /// Sourced from BaseShaderGUI.SetupMaterialBlendModeInternal in the URP package.
    /// </summary>
    private static void SetUrpAlphaBlend(Material mat, float alpha)
    {
        // Surface type → Transparent, blend mode → Alpha
        mat.SetFloat("_Surface",  1f);
        mat.SetFloat("_Blend",    0f);

        // Blend factors for Alpha mode (SrcAlpha / OneMinusSrcAlpha for RGB, One / OneMinusSrcAlpha for A)
        mat.SetFloat("_SrcBlend",      (float)BlendMode.SrcAlpha);
        mat.SetFloat("_DstBlend",      (float)BlendMode.OneMinusSrcAlpha);
        mat.SetFloat("_SrcBlendAlpha", (float)BlendMode.One);
        mat.SetFloat("_DstBlendAlpha", (float)BlendMode.OneMinusSrcAlpha);

        mat.SetFloat("_ZWrite",    0f);
        mat.SetFloat("_AlphaClip", 0f);

        mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.DisableKeyword("_ALPHAMODULATE_ON");

        mat.SetOverrideTag("RenderType", "Transparent");
        mat.SetShaderPassEnabled("DepthOnly", false);
        mat.renderQueue = (int)RenderQueue.Transparent;

        // Apply the alpha value
        if (mat.HasProperty("_BaseColor"))
        {
            Color c = mat.GetColor("_BaseColor");
            c.a = alpha;
            mat.SetColor("_BaseColor", c);
        }
        else
        {
            Color c = mat.color;
            c.a = alpha;
            mat.color = c;
        }
    }
}
