using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BallShooter : MonoBehaviour
{
    [Header("Power")]
    // maxForce=18: full-power Driver shot feels powerful but not rocket-like.
    // Matches Wii Sports Golf: satisfying arc, moderate speed, good hang time.
    public float maxForce = 18f;
    public float chargeSpeed = 15f;

    [Header("Launch Arc")]
    [Tooltip("Upward impulse as a fraction of forward impulse. " +
             "0.75 ≈ 37-degree launch — Wii Sports-style arc with good hang time.")]
    [Range(0f, 1f)]
    public float loftFactor = 0.75f;

    private Rigidbody rb;
    private float currentForce = 0f;
    private bool isCharging = false;

    // Club system
    private float _baseMaxForce = -1f;
    private float _launchAngleOverride = 0f;

    /// <summary>Current charge force accumulated while holding the shot button.</summary>
    public float CurrentForce => currentForce;

    /// <summary>True while the player is holding the shot button to charge power.</summary>
    public bool IsCharging => isCharging;

    /// <summary>Launch angle in degrees set by the active club. 0 falls back to loftFactor.</summary>
    public float LaunchAngleOverride => _launchAngleOverride;

    /// <summary>Called by ClubBootstrapper / ClubSelectorUI when the club changes.</summary>
    public void OnClubChanged(ClubDefinition def)
    {
        if (_baseMaxForce < 0f) _baseMaxForce = maxForce;
        maxForce = _baseMaxForce * def.maxForceMultiplier;
        _launchAngleOverride = def.launchAngleDegrees;
    }

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
        Vector3 impulse;

        if (_launchAngleOverride > 0f)
        {
            // Rotate forward vector upward by the club's launch angle.
            Vector3 launchDir = Quaternion.AngleAxis(_launchAngleOverride, transform.right) * transform.forward;
            impulse = launchDir * currentForce;
        }
        else
        {
            // Fallback: original loftFactor-based arc.
            impulse = transform.forward * currentForce + Vector3.up * (currentForce * loftFactor);
        }

        rb.AddForce(impulse, ForceMode.Impulse);
    }
}
