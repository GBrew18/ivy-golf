using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Attach to the golf ball alongside its <see cref="Rigidbody"/>.
/// Records the ball's world position when a shot is fired (state → InFlight),
/// then monitors velocity. Once the ball has been nearly stationary for
/// <see cref="restDuration"/> seconds it transitions the game to
/// <see cref="GameStateManager.GameState.Landed"/> and displays the shot
/// distance on a center-screen label.
/// The display resets automatically when the next shot is charged.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class DistanceTracker : MonoBehaviour
{
    [Header("Landing Detection")]
    [Tooltip("Speed (m/s) below which the ball is considered at rest.")]
    [Min(0f)]
    [SerializeField] private float restSpeedThreshold = 0.15f;

    [Tooltip("Seconds the ball must remain below the rest threshold to count as landed.")]
    [Min(0.1f)]
    [SerializeField] private float restDuration = 0.6f;

    // Runtime state.
    private Rigidbody _rb;
    private Vector3   _shotOrigin;
    private float     _restTimer;
    private bool      _tracking;

    // UI references.
    private GameObject _canvasRoot;
    private Text       _distanceText;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        BuildUI();
    }

    private void Start()
    {
        if (GameStateManager.Instance != null)
            GameStateManager.Instance.OnStateChanged += HandleStateChanged;
    }

    private void OnDestroy()
    {
        if (GameStateManager.Instance != null)
            GameStateManager.Instance.OnStateChanged -= HandleStateChanged;
    }

    private void HandleStateChanged(GameStateManager.GameState state)
    {
        switch (state)
        {
            case GameStateManager.GameState.InFlight:
                // Record tee position at the moment the ball is launched.
                _shotOrigin = transform.position;
                _restTimer  = 0f;
                _tracking   = true;
                _canvasRoot.SetActive(false);
                break;

            case GameStateManager.GameState.Aiming:
                // Reset was pressed — hide distance and stop tracking.
                _tracking = false;
                _canvasRoot.SetActive(false);
                break;
        }
    }

    private void Update()
    {
        if (!_tracking) return;

        float speed = _rb.linearVelocity.magnitude;

        if (speed < restSpeedThreshold)
        {
            _restTimer += Time.deltaTime;
            if (_restTimer >= restDuration)
                OnBallLanded();
        }
        else
        {
            _restTimer = 0f;
        }
    }

    private void OnBallLanded()
    {
        _tracking = false;

        float distance = Vector3.Distance(transform.position, _shotOrigin);
        Debug.Log($"[DistanceTracker] Ball landed. Shot distance: {distance:0.0} m");

        ShowDistance(distance);
        GameStateManager.Instance?.SetState(GameStateManager.GameState.Landed);
    }

    private void ShowDistance(float distance)
    {
        _distanceText.text = $"{distance:0.0} m";
        _canvasRoot.SetActive(true);
    }

    private void BuildUI()
    {
        // ── Canvas ──────────────────────────────────────────────────────────
        _canvasRoot = new GameObject("DistanceCanvas");
        Canvas canvas = _canvasRoot.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 20;
        _canvasRoot.AddComponent<CanvasScaler>();
        _canvasRoot.SetActive(false);

        // ── Distance label — screen centre, offset above middle ──────────────
        GameObject textGO = new GameObject("DistanceLabel");
        textGO.transform.SetParent(_canvasRoot.transform, false);
        _distanceText = textGO.AddComponent<Text>();
        _distanceText.font      = GetBuiltinFont();
        _distanceText.fontSize  = 42;
        _distanceText.fontStyle = FontStyle.Bold;
        _distanceText.color     = Color.white;
        _distanceText.alignment = TextAnchor.MiddleCenter;

        RectTransform rect = textGO.GetComponent<RectTransform>();
        rect.anchorMin        = new Vector2(0.5f, 0.5f);
        rect.anchorMax        = new Vector2(0.5f, 0.5f);
        rect.pivot            = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0f, 120f);
        rect.sizeDelta        = new Vector2(400f, 70f);
    }

    private static Font GetBuiltinFont()
    {
        Font f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (f == null) f = Resources.GetBuiltinResource<Font>("Arial.ttf");
        return f;
    }
}
