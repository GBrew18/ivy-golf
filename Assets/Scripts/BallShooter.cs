using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BallShooter : MonoBehaviour
{
    [Header("Power")]
    public float maxForce = 20f;
    public float chargeSpeed = 15f;

    [Header("Launch Arc")]
    [Tooltip("How much upward impulse is added relative to the charged forward force.")]
    [Range(0f, 1f)]
    public float loftFactor = 0.15f;

    private Rigidbody rb;
    private float currentForce = 0f;
    private bool isCharging = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Start charging when space is pressed
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isCharging = true;
            currentForce = 0f;
        }

        // Increase power while space is held
        if (Input.GetKey(KeyCode.Space) && isCharging)
        {
            currentForce += chargeSpeed * Time.deltaTime;
            currentForce = Mathf.Clamp(currentForce, 0f, maxForce);
        }

        // Release shot when space is released
        if (Input.GetKeyUp(KeyCode.Space) && isCharging)
        {
            Shoot();
            isCharging = false;
        }
    }

    void Shoot()
    {
        // Forward impulse still uses the charged power.
        Vector3 forwardImpulse = transform.forward * currentForce;

        // Add a small upward impulse for a golf-like arc.
        Vector3 upwardImpulse = Vector3.up * (currentForce * loftFactor);

        // Combine both so the ball launches forward and upward.
        rb.AddForce(forwardImpulse + upwardImpulse, ForceMode.Impulse);
    }
}
