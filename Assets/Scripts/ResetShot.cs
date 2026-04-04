using UnityEngine;

/// <summary>
/// Attach this script to the golf ball.
/// Press R during play to reset the ball to its starting tee position.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class ResetShot : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private KeyCode resetKey = KeyCode.R;

    private Rigidbody rb;

    // Saved starting transform values.
    private Vector3 startPosition;
    private Quaternion startRotation;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Store where the ball starts at the beginning of play.
        // RangeBuilder may override this via SetStartPosition() after Start() runs.
        startPosition = transform.position;
        startRotation = transform.rotation;
    }

    private void Update()
    {
        // If player presses R, reset the ball.
        if (Input.GetKeyDown(resetKey))
        {
            ResetBallToStart();
        }
    }

    /// <summary>
    /// Overrides the tee position used by <see cref="ResetBallToStart"/>.
    /// Call this after the tee is placed at runtime (e.g. from <c>RangeBuilder</c>).
    /// </summary>
    public void SetStartPosition(Vector3 pos)
    {
        startPosition = pos;
    }

    /// <summary>
    /// Resets ball position/rotation and clears physics movement.
    /// </summary>
    public void ResetBallToStart()
    {
        // Stop movement first.
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Move ball back to starting tee position.
        rb.position = startPosition;
        rb.rotation = startRotation;

        // Optional: put rigidbody to sleep so it stays still until acted on.
        rb.Sleep();

        GameStateManager.Instance?.SetState(GameStateManager.GameState.Aiming);
    }
}
