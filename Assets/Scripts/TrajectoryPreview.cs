using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Wii Sports-style faint trajectory arc preview.
/// Attach to any GameObject in the scene. The arc uses the AimController's forward
/// direction (not world forward) and is only visible during the Charging state.
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class TrajectoryPreview : MonoBehaviour
{
    [Header("References")]
    [Tooltip("BallShooter used to read current charge force and charging state.")]
    [SerializeField] private BallShooter ballShooter;

    [Tooltip("Golf ball Rigidbody. Used for start position and mass.")]
    [SerializeField] private Rigidbody ballRigidbody;

    [Tooltip("LineRenderer used to draw the preview arc.")]
    [SerializeField] private LineRenderer lineRenderer;

    [Header("Preview Sampling")]
    [Min(2)][Max(18)]
    [Tooltip("How many points are used to draw the line (max 18).")]
    [SerializeField] private int sampleCount = 18;

    [Min(0.01f)]
    [Tooltip("Time between each sample point (seconds).")]
    [SerializeField] private float timeStep = 0.08f;

    [Header("Ground Clipping")]
    [Tooltip("World-space Y height below which trajectory points are not drawn.")]
    [SerializeField] private float groundHeight = 0f;

    [Range(0f, 1f)]
    [Tooltip("Fallback loft used only if BallShooter is not assigned.")]
    [SerializeField] private float fallbackLoftFactor = 0.15f;

    private AimController _aimController;

    private void Reset()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    private void Awake()
    {
        if (lineRenderer == null)
            lineRenderer = GetComponent<LineRenderer>();

        if (ballRigidbody == null && ballShooter != null)
            ballRigidbody = ballShooter.GetComponent<Rigidbody>();

        // Cache aim controller so we use the correct forward direction
        _aimController = FindFirstObjectByType<AimController>();

        SetupLineRenderer();
    }

    private void SetupLineRenderer()
    {
        if (lineRenderer == null) return;

        // Sprites/Default supports transparency — required for the faint arc
        Material mat = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.material = mat;

        // Very thin, tapering line — Wii Sports style
        lineRenderer.startWidth = 0.012f;
        lineRenderer.endWidth   = 0f;

        // No shadows from a UI-hint line
        lineRenderer.shadowCastingMode = ShadowCastingMode.Off;
        lineRenderer.receiveShadows    = false;
        lineRenderer.useWorldSpace     = true;

        // Gradient: semi-transparent white fading to fully transparent
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(Color.white, 0f),
                new GradientColorKey(Color.white, 1f)
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(0.25f, 0f),
                new GradientAlphaKey(0f,    1f)
            }
        );
        lineRenderer.colorGradient = grad;
        lineRenderer.enabled = false;
    }

    private void Update()
    {
        // Arc only visible while charging — completely hidden otherwise
        if (ballShooter == null || !ballShooter.IsCharging)
        {
            if (lineRenderer != null) lineRenderer.enabled = false;
            return;
        }

        if (lineRenderer != null) lineRenderer.enabled = true;
        DrawTrajectory();
    }

    private void DrawTrajectory()
    {
        if (lineRenderer == null) return;

        Vector3 startPos        = GetStartPosition();
        Vector3 initialVelocity = CalculateInitialVelocity();
        Vector3 gravity         = Physics.gravity;

        int maxPoints = Mathf.Clamp(sampleCount, 2, 18);
        List<Vector3> points = new List<Vector3>(maxPoints);

        for (int i = 0; i < maxPoints; i++)
        {
            float   t  = i * timeStep;
            Vector3 pt = startPos + initialVelocity * t + 0.5f * gravity * t * t;

            if (i > 0 && pt.y < groundHeight) break;

            points.Add(pt);
        }

        if (points.Count < 2)
        {
            lineRenderer.positionCount = 0;
            return;
        }

        lineRenderer.positionCount = points.Count;
        lineRenderer.SetPositions(points.ToArray());
    }

    private Vector3 GetStartPosition()
    {
        if (ballRigidbody != null) return ballRigidbody.position;
        if (ballShooter   != null) return ballShooter.transform.position;
        return transform.position;
    }

    private Vector3 CalculateInitialVelocity()
    {
        float launchForce = ballShooter != null ? ballShooter.CurrentForce : 0f;

        // IMPORTANT: use AimController's forward, not world/self forward.
        // This fixes the "arc shoots sideways" bug.
        Vector3 forward = (_aimController != null)
            ? _aimController.transform.forward
            : transform.forward;

        Vector3 impulse;
        float angleOverride = ballShooter != null ? ballShooter.LaunchAngleOverride : 0f;

        if (angleOverride > 0f)
        {
            // Club-specific launch angle (degrees → radians)
            float rad = angleOverride * Mathf.Deg2Rad;
            impulse = (forward * Mathf.Cos(rad) + Vector3.up * Mathf.Sin(rad)) * launchForce;
        }
        else
        {
            // Fallback: loftFactor-based launch (matches BallShooter.Shoot)
            float loft = ballShooter != null ? ballShooter.loftFactor : fallbackLoftFactor;
            impulse = forward * launchForce + Vector3.up * (launchForce * loft);
        }

        float mass = 1f;
        if (ballRigidbody != null)
            mass = Mathf.Max(0.0001f, ballRigidbody.mass);

        // ForceMode.Impulse: velocity change = impulse / mass
        return impulse / mass;
    }

    private void OnValidate()
    {
        sampleCount = Mathf.Clamp(sampleCount, 2, 18);
        timeStep    = Mathf.Max(0.01f, timeStep);
    }
}
