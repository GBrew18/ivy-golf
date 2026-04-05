using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Bottom-left pill that displays the currently equipped club name.
/// Slides in during Aiming and Charging, slides out otherwise.
/// Auto-instantiated by <see cref="ClubSelectorBootstrapper"/> — no scene
/// setup required.
/// </summary>
public class ClubSelectorUI : MonoBehaviour
{
    // ── Layout ────────────────────────────────────────────────────────────────
    private const float PillW        = 150f;
    private const float PillH        = 40f;
    private const float BottomMargin = 24f;
    private const float LeftMargin   = 24f;
    private const float SlideSpeed   = 10f;

    private static readonly Color GoldColor = new Color(1f, 0.82f, 0.10f, 1f);

    // ── Runtime state ─────────────────────────────────────────────────────────
    private RectTransform _pillRect;
    private Text          _clubText;

    private float _shownX, _hiddenX, _targetX;

    // Hardcoded to Driver for now — swap when a club-selection system exists.
    private const string ClubName = "DRIVER";

    // ── Unity lifecycle ───────────────────────────────────────────────────────
    private void Awake()
    {
        BuildUI();
    }

    private void Start()
    {
        // Start hidden; the Aiming state broadcast will slide it in.
        _targetX = _hiddenX;
        _pillRect.anchoredPosition = new Vector2(_hiddenX, BottomMargin);

        if (GameStateManager.Instance != null)
            GameStateManager.Instance.OnStateChanged += OnStateChanged;

        // If we boot mid-game into Aiming, show immediately.
        if (GameStateManager.Instance != null &&
            (GameStateManager.Instance.CurrentState == GameStateManager.GameState.Aiming ||
             GameStateManager.Instance.CurrentState == GameStateManager.GameState.Charging))
            _targetX = _shownX;
    }

    private void OnDestroy()
    {
        if (GameStateManager.Instance != null)
            GameStateManager.Instance.OnStateChanged -= OnStateChanged;
    }

    private void Update()
    {
        Vector2 pos = _pillRect.anchoredPosition;
        pos.x = Mathf.Lerp(pos.x, _targetX, Time.deltaTime * SlideSpeed);
        _pillRect.anchoredPosition = pos;
    }

    // ── State listener ────────────────────────────────────────────────────────
    private void OnStateChanged(GameStateManager.GameState state)
    {
        bool show = state == GameStateManager.GameState.Aiming ||
                    state == GameStateManager.GameState.Charging;
        _targetX = show ? _shownX : _hiddenX;
    }

    // ── UI construction ───────────────────────────────────────────────────────
    private void BuildUI()
    {
        _shownX  = LeftMargin + PillW * 0.5f;
        _hiddenX = -(PillW + 20f);

        // Canvas
        GameObject canvasGO = new GameObject("ClubSelectorCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 18;
        canvasGO.AddComponent<CanvasScaler>();

        // Dark pill background — anchored bottom-left, slides in on X.
        GameObject pillGO = new GameObject("ClubPill");
        pillGO.transform.SetParent(canvasGO.transform, false);
        Image bg = pillGO.AddComponent<Image>();
        bg.color = new Color(0.05f, 0.05f, 0.05f, 0.85f);

        _pillRect = pillGO.GetComponent<RectTransform>();
        _pillRect.anchorMin        = new Vector2(0f, 0f);
        _pillRect.anchorMax        = new Vector2(0f, 0f);
        _pillRect.pivot            = new Vector2(0.5f, 0f);
        _pillRect.sizeDelta        = new Vector2(PillW, PillH);
        _pillRect.anchoredPosition = new Vector2(_hiddenX, BottomMargin);

        // Gold accent bar along the top edge of the pill.
        GameObject goldGO = new GameObject("GoldAccent");
        goldGO.transform.SetParent(pillGO.transform, false);
        Image goldBar = goldGO.AddComponent<Image>();
        goldBar.color = GoldColor;
        RectTransform goldRect = goldGO.GetComponent<RectTransform>();
        goldRect.anchorMin        = new Vector2(0f, 1f);
        goldRect.anchorMax        = new Vector2(1f, 1f);
        goldRect.pivot            = new Vector2(0.5f, 1f);
        goldRect.sizeDelta        = new Vector2(0f, 3f);
        goldRect.anchoredPosition = Vector2.zero;

        // Small "CLUB" label (grey, small) above the club name.
        AddLabel(pillGO, "Label_CLUB", "CLUB", 9, FontStyle.Normal,
                 new Color(0.60f, 0.60f, 0.60f, 1f), TextAnchor.MiddleCenter,
                 new Vector2(0f, 0.58f), new Vector2(1f, 1f),
                 new Vector2(0f, 0f), new Vector2(0f, 0f));

        // Club name — large gold bold text.
        GameObject nameGO = new GameObject("ClubNameText");
        nameGO.transform.SetParent(pillGO.transform, false);
        _clubText           = nameGO.AddComponent<Text>();
        _clubText.text      = ClubName;
        _clubText.font      = GetBuiltinFont();
        _clubText.fontSize  = 17;
        _clubText.fontStyle = FontStyle.Bold;
        _clubText.alignment = TextAnchor.MiddleCenter;
        _clubText.color     = GoldColor;
        RectTransform nameRect = nameGO.GetComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0f, 0f);
        nameRect.anchorMax = new Vector2(1f, 0.62f);
        nameRect.offsetMin = new Vector2(4f, 2f);
        nameRect.offsetMax = new Vector2(-4f, 0f);
    }

    private static void AddLabel(GameObject parent, string goName, string text,
                                 int fontSize, FontStyle style, Color color,
                                 TextAnchor align,
                                 Vector2 anchorMin, Vector2 anchorMax,
                                 Vector2 offsetMin, Vector2 offsetMax)
    {
        GameObject go = new GameObject(goName);
        go.transform.SetParent(parent.transform, false);
        Text t = go.AddComponent<Text>();
        t.text      = text;
        t.font      = GetBuiltinFont();
        t.fontSize  = fontSize;
        t.fontStyle = style;
        t.alignment = align;
        t.color     = color;
        RectTransform r = go.GetComponent<RectTransform>();
        r.anchorMin = anchorMin;
        r.anchorMax = anchorMax;
        r.offsetMin = offsetMin;
        r.offsetMax = offsetMax;
    }

    private static Font GetBuiltinFont()
    {
        Font f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (f == null) f = Resources.GetBuiltinResource<Font>("Arial.ttf");
        return f;
    }
}

/// <summary>
/// Auto-instantiates <see cref="ClubSelectorUI"/> after each scene finishes loading.
/// </summary>
public static class ClubSelectorBootstrapper
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        // Only needed in scenes that contain a ball.
        if (Object.FindObjectOfType<BallShooter>() == null) return;

        GameObject go = new GameObject("ClubSelectorUI");
        go.AddComponent<ClubSelectorUI>();
        Object.DontDestroyOnLoad(go);
    }
}
