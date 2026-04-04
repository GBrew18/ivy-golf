using System.Collections.Generic;
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
        // Hide the arc entirely when not charging.
        if (ballShooter != null && !ballShooter.IsCharging)
        {
            if (lineRenderer != null)
                lineRenderer.enabled = false;
            return;
        }

        if (lineRenderer != null)
            lineRenderer.enabled = true;

        DrawTrajectory();
    }

    private void DrawTrajectory()
    {
        if (lineRenderer == null)
            return;

        Vector3 startPosition = GetStartPosition();
        Vector3 initialVelocity = CalculateInitialVelocity();
        Vector3 gravity = Physics.gravity;

        // Projectile motion formula:
        // position = start + (velocity * t) + 0.5 * gravity * t^2
        // Stop adding points once the predicted Y drops below groundHeight.
        int maxPoints = Mathf.Max(2, sampleCount);
        List<Vector3> points = new List<Vector3>(maxPoints);

        for (int i = 0; i < maxPoints; i++)
        {
            float t = i * timeStep;
            Vector3 point = startPosition + initialVelocity * t + 0.5f * gravity * t * t;

            if (i > 0 && point.y < groundHeight)
                break;

            points.Add(point);
        }

        // Need at least 2 points for a visible line.
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
        timeStep = Mathf.Max(0.01f, timeStep);
    }
}
