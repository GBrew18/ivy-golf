using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BallShooter : MonoBehaviour
{
    [Header("Power")]
    // maxForce=25 + loftFactor=0.75 → ~37° launch angle, ~95 units at full power on flat ground.
    // Matches Wii Sports Golf: nice high arc, good hang time, lands satisfyingly.
    public float maxForce = 25f;
    public float chargeSpeed = 15f;

    [Header("Launch Arc")]
    [Tooltip("Upward impulse as a fraction of forward impulse. " +
             "0.75 ≈ 37-degree launch — Wii Sports-style arc with good hang time.")]
    [Range(0f, 1f)]
    public float loftFactor = 0.75f;

    private Rigidbody rb;
    private float currentForce = 0f;
    private bool isCharging = false;

    /// <summary>Current charge force accumulated while holding the shot button.</summary>
    public float CurrentForce => currentForce;

    /// <summary>True while the player is holding the shot button to charge power.</summary>
    public bool IsCharging => isCharging;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Start charging when space is pressed — blocked during flight so the
        // player cannot fire a second shot before the ball has landed.
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (GameStateManager.Instance != null &&
                GameStateManager.Instance.CurrentState == GameStateManager.GameState.InFlight)
                return;

            isCharging = true;
            currentForce = 0f;
            GameStateManager.Instance?.SetState(GameStateManager.GameState.Charging);
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
            GameStateManager.Instance?.SetState(GameStateManager.GameState.InFlight);
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
