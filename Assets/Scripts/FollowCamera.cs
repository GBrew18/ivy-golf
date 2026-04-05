using UnityEngine;

/// <summary>
/// Attach this to the Main Camera.
/// Follows a golf ball with smooth SmoothDamp movement and keeps it in view.
/// During flight the follow offset direction lerps toward the ball's travel
/// direction so the camera naturally trails behind the shot.
/// </summary>
public class FollowCamera : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("Drag the golf ball Transform here.")]
    [SerializeField] private Transform ballTarget;

    [Tooltip("Optional: drag the ball Rigidbody here for velocity-aware trailing. Auto-found if left empty.")]
    [SerializeField] private Rigidbody ballRigidbody;

    [Header("Follow Settings")]
    [Tooltip("Camera position relative to the ball (X = side, Y = height, Z = behind/in front).")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 6f, -10f);

    [Tooltip("Approximate time (seconds) for the camera to reach the target position.")]
    [Min(0.01f)]
    [SerializeField] private float smoothTime = 0.5f;

    [Header("Velocity Trailing")]
    [Tooltip("How strongly the follow offset rotates toward the ball's travel direction during flight (0 = fixed offset, 1 = full trail).")]
    [SerializeField] [Range(0f, 1f)] private float velocityInfluence = 0.7f;

    [Tooltip("Ball speed (m/s) above which velocity-based trailing is active.")]
    [Min(0f)]
    [SerializeField] private float flightSpeedThreshold = 1f;

    private Vector3 _positionVelocity = Vector3.zero;

    private void Start()
    {
        // Optional helper: if no target is assigned, try to find one tagged "GolfBall".
        if (ballTarget == null)
        {
            GameObject found = GameObject.FindWithTag("GolfBall");
            if (found != null)
            {
                ballTarget = found.transform;
            }
        }

        if (ballRigidbody == null && ballTarget != null)
            ballRigidbody = ballTarget.GetComponent<Rigidbody>();
    }

    private void LateUpdate()
    {
        // LateUpdate is best for cameras so it follows after physics/movement updates.
        if (ballTarget == null) return;

        // 1) Compute the effective follow offset, rotating it toward travel direction in flight.
        Vector3 effectiveOffset = ComputeEffectiveOffset();

        // 2) SmoothDamp toward the desired follow position for buttery-smooth framing.
        Vector3 desiredPosition = ballTarget.position + effectiveOffset;
        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref _positionVelocity,
            smoothTime
        );

        // 3) Keep the ball in view by rotating to look at it.
        Vector3 lookDirection = ballTarget.position - transform.position;
        if (lookDirection.sqrMagnitude > 0.0001f)
        {
            transform.rotation = Quaternion.LookRotation(lookDirection, Vector3.up);
        }
    }

    /// <summary>
    /// Returns the world-space offset to add to the ball position.
    /// When the ball is moving faster than <see cref="flightSpeedThreshold"/>,
    /// the offset direction lerps toward the opposite of the velocity direction
    /// so the camera trails naturally behind the shot.
    /// </summary>
    private Vector3 ComputeEffectiveOffset()
    {
        if (ballRigidbody == null || velocityInfluence <= 0f)
            return offset;

        Vector3 velocity = ballRigidbody.linearVelocity;
        float speed = velocity.magnitude;

        if (speed < flightSpeedThreshold)
            return offset;

        // Trail direction: opposite of travel, flattened to horizontal + preserve height.
        Vector3 travelDir = velocity / speed;
        Vector3 trailDir = new Vector3(-travelDir.x, 0f, -travelDir.z).normalized;

        float offsetHorizontalMag = new Vector3(offset.x, 0f, offset.z).magnitude;

        // Lerp between the inspector offset direction and the trail direction.
        Vector3 baseHorizontal = new Vector3(offset.x, 0f, offset.z);
        Vector3 trailHorizontal = trailDir * offsetHorizontalMag;

        Vector3 blendedHorizontal = Vector3.Lerp(baseHorizontal, trailHorizontal, velocityInfluence);

        return new Vector3(blendedHorizontal.x, offset.y, blendedHorizontal.z);
    }
}
