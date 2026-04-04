using System.Collections;
using UnityEngine;

/// <summary>
/// Attach to the golf ball. Applies Wii Sports-style landing physics:
/// almost no bounce, high rolling drag, fast settle.
/// Automatically wired by BallPhysicsBootstrapper — no scene setup needed.
/// </summary>
[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class BallPhysicsController : MonoBehaviour
{
    [SerializeField] private BallPhysicsProfile profile;

    private Rigidbody _rb;
    private Collider _col;
    private bool _hasLanded;
    private float _rollingDragMultiplier = 1f;

    /// <summary>Set by ClubSelectorUI to scale rolling drag per club.</summary>
    public void SetRollingDragMultiplier(float multiplier)
    {
        _rollingDragMultiplier = multiplier;
        // Re-apply immediately if the ball is currently rolling.
        if (GameStateManager.Instance?.CurrentState == GameStateManager.GameState.Landed && _hasLanded)
            ApplyRollingDrag();
    }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _col = GetComponent<Collider>();

        if (profile == null)
            profile = ScriptableObject.CreateInstance<BallPhysicsProfile>();

        // Apply ball physics material immediately — bounceCombine.Minimum means
        // the ball's near-zero bounciness wins against any ground surface.
        ApplyBallPhysicsMaterial();
        ApplyInFlightDrag();
    }

    private void Start()
    {
        if (GameStateManager.Instance != null)
            GameStateManager.Instance.OnStateChanged += OnStateChanged;

        // Delay ground-material application one frame so RangeBuilder/HoleBuilder
        // Start() methods have run and tagged colliders exist.
        StartCoroutine(ApplyGroundMaterialNextFrame());
    }

    private void OnDestroy()
    {
        if (GameStateManager.Instance != null)
            GameStateManager.Instance.OnStateChanged -= OnStateChanged;
    }

    private void OnStateChanged(GameStateManager.GameState state)
    {
        if (state == GameStateManager.GameState.InFlight)
        {
            _hasLanded = false;
            ApplyInFlightDrag();
        }
        else if (state == GameStateManager.GameState.Aiming)
        {
            _hasLanded = false;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (_hasLanded) return;
        if (GameStateManager.Instance?.CurrentState != GameStateManager.GameState.InFlight) return;

        _hasLanded = true;

        // Kill the bounce — this is the core Wii Sports landing feel.
        _rb.linearVelocity *= profile.landingVelocityDamping;
        _rb.angularVelocity *= profile.landingVelocityDamping;

        ApplyRollingDrag();
        GameStateManager.Instance.SetState(GameStateManager.GameState.Landed);
    }

    private void FixedUpdate()
    {
        if (GameStateManager.Instance?.CurrentState != GameStateManager.GameState.Landed) return;
        if (!_hasLanded) return;

        // Snap to stopped once rolling speed is negligible.
        if (_rb.linearVelocity.magnitude < profile.settleSpeedThreshold &&
            _rb.angularVelocity.magnitude < profile.settleSpeedThreshold)
        {
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
        }
    }

    private void ApplyBallPhysicsMaterial()
    {
        PhysicsMaterial ballMat = new PhysicsMaterial("BallPhysics")
        {
            bounciness      = profile.bounciness,
            dynamicFriction = profile.dynamicFriction,
            staticFriction  = profile.staticFriction,
            // Minimum combine: ball's near-zero bounciness wins over any surface.
            bounceCombine   = PhysicsMaterialCombine.Minimum,
            // Maximum combine: uses higher friction value for sticky landing.
            frictionCombine = PhysicsMaterialCombine.Maximum
        };
        _col.material = ballMat;
    }

    private IEnumerator ApplyGroundMaterialNextFrame()
    {
        yield return null;
        ApplyGroundMaterial();
    }

    private void ApplyGroundMaterial()
    {
        PhysicsMaterial groundMat = new PhysicsMaterial("GroundPhysics")
        {
            bounciness      = profile.bounciness,
            dynamicFriction = profile.dynamicFriction,
            staticFriction  = profile.staticFriction,
            bounceCombine   = PhysicsMaterialCombine.Minimum,
            frictionCombine = PhysicsMaterialCombine.Maximum
        };

        string[] groundTags = { "Ground", "Fairway", "Rough", "Green", "Bunker" };
        foreach (string tag in groundTags)
        {
            try
            {
                GameObject[] tagged = GameObject.FindGameObjectsWithTag(tag);
                foreach (var go in tagged)
                {
                    Collider col = go.GetComponent<Collider>();
                    if (col != null) col.material = groundMat;
                }
            }
            catch (UnityException)
            {
                // Tag doesn't exist in the project — skip silently.
            }
        }
    }

    private void ApplyInFlightDrag()
    {
        _rb.linearDamping  = profile.inFlightLinearDrag;
        _rb.angularDamping = profile.inFlightAngularDrag;
    }

    private void ApplyRollingDrag()
    {
        _rb.linearDamping  = profile.rollingLinearDrag * _rollingDragMultiplier;
        _rb.angularDamping = profile.rollingAngularDrag;
    }
}
