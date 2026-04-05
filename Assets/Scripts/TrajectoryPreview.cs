using UnityEngine;

/// <summary>
/// Attach this to your AimPivot.
/// Uses a LineRenderer to preview where the golf ball will travel.
/// The arc is only visible while the player is charging a shot and
/// stops drawing at <see cref="groundHeight"/> to avoid clipping through terrain.
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class TrajectoryPreview : MonoBehaviour
{
    [Header("References")]
    [Tooltip("BallShooter used to read current charge force, loft, and charging state.")]
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

    [Header("Ground Clipping")]
    [Tooltip("World-space Y height below which trajectory points are not drawn (prevents clipping through ground).")]
    [SerializeField] private float groundHeight = 0f;

    [Header("Charge Color Gradient")]
    [Tooltip("Line color at minimum charge.")]
    [SerializeField] private Color colorLow  = Color.green;
    [Tooltip("Line color at 50% charge.")]
    [SerializeField] private Color colorMid  = Color.yellow;
    [Tooltip("Line color at maximum charge.")]
    [SerializeField] private Color colorHigh = Color.red;

    [Range(0f, 1f)]
    [Tooltip("Fallback loft used only if BallShooter is not assigned.")]
    [SerializeField] private float fallbackLoftFactor = 0.15f;

    // Pre-allocated position buffer — avoids per-frame heap allocations.
    private Vector3[] _trajectoryBuffer;

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

        _trajectoryBuffer = new Vector3[sampleCount];

        // Force a thin, faint arc regardless of LineRenderer defaults set in the scene.
        if (lineRenderer != null)
        {
            lineRenderer.startWidth        = 0.07f;
            lineRenderer.endWidth          = 0.02f;
            lineRenderer.useWorldSpace     = true;
            lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lineRenderer.receiveShadows    = false;
        }
    }

    private void Update()
    {
        // Hide the arc when not charging — guard covers both null shooter and not-charging.
        if (ballShooter == null || !ballShooter.IsCharging)
        {
            if (lineRenderer != null)
                lineRenderer.enabled = false;
            return;
        }

        if (lineRenderer != null)
            lineRenderer.enabled = true;

        UpdateLineColor();
        DrawTrajectory();
    }

    // Interpolates green → yellow → red (faint, alpha=0.55) as charge builds.
    private void UpdateLineColor()
    {
        if (lineRenderer == null || ballShooter == null) return;

        float t = ballShooter.maxForce > 0f
            ? ballShooter.CurrentForce / ballShooter.maxForce
            : 0f;

        Color c = t <= 0.5f
            ? Color.Lerp(colorLow,  colorMid,  t * 2f)
            : Color.Lerp(colorMid, colorHigh, (t - 0.5f) * 2f);

        // Keep the arc faint — override whatever alpha the serialized colors have.
        c.a = 0.55f;

        lineRenderer.startColor = c;
        lineRenderer.endColor   = new Color(c.r, c.g, c.b, 0.2f); // taper to near-transparent at tip
    }

    private void DrawTrajectory()
    {
        if (lineRenderer == null)
            return;

        // Resize buffer if sampleCount was changed at runtime.
        int maxPoints = Mathf.Max(2, sampleCount);
        if (_trajectoryBuffer == null || _trajectoryBuffer.Length != maxPoints)
            _trajectoryBuffer = new Vector3[maxPoints];

        Vector3 startPosition   = GetStartPosition();
        Vector3 initialVelocity = CalculateInitialVelocity();
        Vector3 gravity         = Physics.gravity;

        // Projectile motion: position = start + v*t + 0.5*g*t²
        // Stop when the predicted Y drops below groundHeight.
        int count = 0;
        for (int i = 0; i < maxPoints; i++)
        {
            float   t     = i * timeStep;
            Vector3 point = startPosition + initialVelocity * t + 0.5f * gravity * t * t;

            if (i > 0 && point.y < groundHeight)
                break;

            _trajectoryBuffer[count++] = point;
        }

        // Need at least 2 points for a visible line.
        if (count < 2)
        {
            lineRenderer.positionCount = 0;
            return;
        }

        lineRenderer.positionCount = count;
        lineRenderer.SetPositions(_trajectoryBuffer); // only first `count` positions are read
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
        // Read the live charge force from BallShooter each frame.
        float launchForce = ballShooter != null ? ballShooter.CurrentForce : 0f;
        float loft = ballShooter != null ? ballShooter.loftFactor : fallbackLoftFactor;

        // Match BallShooter launch logic:
        // impulse = forward * force + up * (force * loftFactor)
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
        timeStep    = Mathf.Max(0.01f, timeStep);
        // Reallocate buffer immediately so Inspector tweaks take effect in Edit Mode.
        _trajectoryBuffer = new Vector3[sampleCount];
    }
}
