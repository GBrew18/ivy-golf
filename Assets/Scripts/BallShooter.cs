using UnityEngine;

// Ensures this GameObject always has a Rigidbody component.
[RequireComponent(typeof(Rigidbody))]
public class BallShooter : MonoBehaviour
{
    [Header("Shot Settings")]
    [Tooltip("Impulse force applied in the ball's forward direction when Space is pressed.")]
    [SerializeField] private float shotForce = 8f;

    private Rigidbody ballRigidbody;
    private bool shotQueued;

    private void Awake()
    {
        // Cache the Rigidbody reference once for performance and clarity.
        ballRigidbody = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        // Read player input in Update so key presses are not missed between physics ticks.
        if (Input.GetKeyDown(KeyCode.Space))
        {
            shotQueued = true;
        }
    }

    private void FixedUpdate()
    {
        // Apply physics forces in FixedUpdate for consistent Rigidbody behavior.
        if (!shotQueued)
        {
            return;
        }

        shotQueued = false;
        ballRigidbody.AddForce(transform.forward * shotForce, ForceMode.Impulse);
    }

    private void OnValidate()
    {
        // Prevent negative force values from the Inspector.
        if (shotForce < 0f)
        {
            shotForce = 0f;
        }
    }
}