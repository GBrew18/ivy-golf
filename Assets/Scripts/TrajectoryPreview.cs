using UnityEngine;

/// <summary>
/// Attach this to your AimPivot.
/// Uses a LineRenderer to preview where the golf ball will travel.
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class TrajectoryPreview : MonoBehaviour
{
    [Header("References")]
    [Tooltip("BallShooter used to match launch settings (max force + loft).")]
    [SerializeField] private BallShooter ballShooter;

    [Tooltip("Golf ball Rigidbody. Used for start position and mass.")]
    [SerializeField] private Rigidbody ballRigidbody;

    [Tooltip("LineRenderer used to draw the preview arc.")]
    [SerializeField] private LineRenderer lineRenderer;

    [Header("Preview Sampling")]
    [Min(2)]
    [Tooltip("How many points are used to draw the line.")]
    [SerializeField] private int sampleCount = 30;

    [Min(0.01f)]
    [Tooltip("Time between each sample point (seconds).")]
    [SerializeField] private float timeStep = 0.08f;

    [Header("Shot Preview")]
    [Min(0f)]
    [Tooltip("Preview shot force (same units as BallShooter force).")]
    [SerializeField] private float previewForce = 10f;

    [Range(0f, 1f)]
    [Tooltip("Fallback loft used only if BallShooter is not assigned.")]
    [SerializeField] private float fallbackLoftFactor = 0.15f;

    private void Reset()
    {
        // Auto-fill LineRenderer when script is first added.
        lineRenderer = GetComponent<LineRenderer>();
    }

    private void Awake()
    {
        if (lineRenderer == null)
            lineRenderer = GetComponent<LineRenderer>();

        // Helpful auto-link if BallShooter is assigned but Rigidbody is not.
        if (ballRigidbody == null && ballShooter != null)
            ballRigidbody = ballShooter.GetComponent<Rigidbody>();
    }

    private void Update()
    {
        DrawTrajectory();
    }

    private void DrawTrajectory()
    {
        if (lineRenderer == null)
            return;

        int points = Mathf.Max(2, sampleCount);
        lineRenderer.positionCount = points;

        Vector3 startPosition = GetStartPosition();
        Vector3 initialVelocity = CalculateInitialVelocity();
        Vector3 gravity = Physics.gravity;

        // Projectile motion formula:
        // position = start + (velocity * t) + 0.5 * gravity * t^2
        for (int i = 0; i < points; i++)
        {
            float t = i * timeStep;
            Vector3 point = startPosition + initialVelocity * t + 0.5f * gravity * t * t;
            lineRenderer.SetPosition(i, point);
        }
    }

    private Vector3 GetStartPosition()
    {
        if (ballRigidbody != null)
            return ballRigidbody.position;

        if (ballShooter != null)
            return ballShooter.transform.position;

        return transform.position;
    }

    private Vector3 CalculateInitialVelocity()
    {
        float maxForce = ballShooter != null ? ballShooter.maxForce : Mathf.Infinity;
        float launchForce = Mathf.Clamp(previewForce, 0f, maxForce);

        // Match BallShooter launch logic:
        // impulse = forward * force + up * (force * loftFactor)
        float loft = ballShooter != null ? ballShooter.loftFactor : fallbackLoftFactor;
        Vector3 launchImpulse = (transform.forward * launchForce) + (Vector3.up * (launchForce * loft));

        // ForceMode.Impulse changes velocity by impulse / mass.
        float mass = 1f;
        if (ballRigidbody != null)
        {
            mass = Mathf.Max(0.0001f, ballRigidbody.mass);
        }
        else if (ballShooter != null)
        {
            Rigidbody shooterRb = ballShooter.GetComponent<Rigidbody>();
            if (shooterRb != null)
                mass = Mathf.Max(0.0001f, shooterRb.mass);
        }

        return launchImpulse / mass;
    }

    private void OnValidate()
    {
        sampleCount = Mathf.Max(2, sampleCount);
        timeStep = Mathf.Max(0.01f, timeStep);
        previewForce = Mathf.Max(0f, previewForce);
    }
}
