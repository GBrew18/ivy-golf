using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Singleton that tracks shots taken and targets hit this session.
/// Listens to <see cref="TargetZone.OnTargetHit"/> and
/// <see cref="GameStateManager.OnStateChanged"/> — no polling required.
/// Creates its own HUD text at runtime; no scene setup needed.
/// </summary>
public class ScoreManager : MonoBehaviour
{
    /// <summary>The single shared instance of <see cref="ScoreManager"/>.</summary>
    public static ScoreManager Instance { get; private set; }

    /// <summary>Total shots fired this session (increments each time state → InFlight).</summary>
    public int TotalShots { get; private set; }

    /// <summary>Total target hits recorded this session.</summary>
    public int TotalHits { get; private set; }

    private Text _hudText;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        BuildUI();
    }

    private void Start()
    {
        TargetZone.OnTargetHit += HandleTargetHit;

        if (GameStateManager.Instance != null)
            GameStateManager.Instance.OnStateChanged += HandleStateChanged;

        RefreshHUD();
    }

    private void OnDestroy()
    {
        TargetZone.OnTargetHit -= HandleTargetHit;

        if (GameStateManager.Instance != null)
            GameStateManager.Instance.OnStateChanged -= HandleStateChanged;
    }

    private void HandleStateChanged(GameStateManager.GameState state)
    {
        if (state != GameStateManager.GameState.InFlight) return;
        TotalShots++;
        RefreshHUD();
    }

    private void HandleTargetHit(TargetZone zone)
    {
        TotalHits++;
        RefreshHUD();
    }

    private void RefreshHUD()
    {
        if (_hudText != null)
            _hudText.text = $"Shots: {TotalShots}   Hits: {TotalHits}";
    }

    private void BuildUI()
    {
        // ── Canvas ──────────────────────────────────────────────────────────
        GameObject canvasGO = new GameObject("ScoreCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode  = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 20;
        canvasGO.AddComponent<CanvasScaler>();

        // ── Score text — anchored top-left ───────────────────────────────────
        GameObject textGO = new GameObject("ScoreHUD");
        textGO.transform.SetParent(canvasGO.transform, false);
        _hudText = textGO.AddComponent<Text>();
        _hudText.font      = GetBuiltinFont();
        _hudText.fontSize  = 22;
        _hudText.fontStyle = FontStyle.Bold;
        _hudText.color     = Color.white;
        _hudText.alignment = TextAnchor.UpperLeft;

        RectTransform rect = textGO.GetComponent<RectTransform>();
        rect.anchorMin        = new Vector2(0f, 1f);
        rect.anchorMax        = new Vector2(0f, 1f);
        rect.pivot            = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(20f, -20f);
        rect.sizeDelta        = new Vector2(320f, 60f);
    }

    private static Font GetBuiltinFont()
    {
        Font f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (f == null) f = Resources.GetBuiltinResource<Font>("Arial.ttf");
        return f;
    }
}
