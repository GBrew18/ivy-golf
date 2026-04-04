using UnityEngine;

/// <summary>
/// Singleton that generates a new random wind vector each time the player
/// begins charging a shot, then applies a continuous lateral force to the
/// ball while it is in flight.
///
/// Auto-instantiates via <see cref="WindBootstrapper"/> — no scene wiring needed.
/// </summary>
public class WindSystem : MonoBehaviour
{
    public static WindSystem Instance { get; private set; }

    [Header("Strength range (m/s equivalent force)")]
    public float minStrength = 0f;
    public float maxStrength = 6f;

    /// <summary>Current wind force vector (XZ plane only).</summary>
    public Vector3 CurrentWind { get; private set; }

    private Rigidbody _ballRb;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        GenerateWind();
    }

    private void Start()
    {
        if (GameStateManager.Instance != null)
            GameStateManager.Instance.OnStateChanged += OnStateChanged;

        CacheBall();
    }

    private void OnDestroy()
    {
        if (GameStateManager.Instance != null)
            GameStateManager.Instance.OnStateChanged -= OnStateChanged;
    }

    private void OnStateChanged(GameStateManager.GameState state)
    {
        if (state == GameStateManager.GameState.Charging)
        {
            GenerateWind();
            CacheBall();
        }
        else if (state == GameStateManager.GameState.Landed)
        {
            _ballRb = null;
        }
    }

    private void FixedUpdate()
    {
        if (GameStateManager.Instance?.CurrentState != GameStateManager.GameState.InFlight) return;
        if (_ballRb == null) return;

        _ballRb.AddForce(CurrentWind, ForceMode.Force);
    }

    private void GenerateWind()
    {
        float angle    = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float strength = Random.Range(minStrength, maxStrength);
        CurrentWind = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * strength;
    }

    private void CacheBall()
    {
        BallShooter shooter = FindObjectOfType<BallShooter>();
        _ballRb = shooter != null ? shooter.GetComponent<Rigidbody>() : null;
    }
}

/// <summary>
/// Creates <see cref="WindSystem"/> and <see cref="WindIndicatorUI"/> at runtime.
/// </summary>
public static class WindBootstrapper
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        // WindSystem
        GameObject sysGO = new GameObject("WindSystem");
        sysGO.AddComponent<WindSystem>();
        Object.DontDestroyOnLoad(sysGO);

        // WindIndicatorUI
        GameObject uiGO = new GameObject("WindIndicatorUI");
        uiGO.AddComponent<WindIndicatorUI>();
        Object.DontDestroyOnLoad(uiGO);
    }
}
