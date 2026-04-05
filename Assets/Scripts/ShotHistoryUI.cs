using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays the result of the most recent shot — distance in metres and
/// whether a target was hit — in a small panel that slides in from the right
/// after the ball lands and auto-hides after 4 seconds.
///
/// Auto-instantiates via <see cref="ShotHistoryBootstrapper"/> — no scene
/// or Inspector setup required.
/// </summary>
public class ShotHistoryUI : MonoBehaviour
{
    // ── Layout ────────────────────────────────────────────────────────────────
    private const float PanelW     = 180f;
    private const float PanelH     = 60f;
    private const float SlideSpeed = 9f;
    private const float HideDelay  = 4f;

    // ── Runtime state ─────────────────────────────────────────────────────────
    private BallShooter   _shooter;
    private RectTransform _panelRect;
    private Text          _mainText;
    private Text          _subText;

    private float _shownX, _hiddenX, _targetX;
    private bool  _targetHitThisShot;
    private Vector3 _shotOrigin;
    private Coroutine _hideRoutine;

    // ── Public init ───────────────────────────────────────────────────────────
    public void Init(BallShooter shooter) => _shooter = shooter;

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    private void Awake()
    {
        BuildUI();
    }

    private void Start()
    {
        TargetZone.OnTargetHit += OnTargetHit;

        if (GameStateManager.Instance != null)
            GameStateManager.Instance.OnStateChanged += OnStateChanged;
    }

    private void OnDestroy()
    {
        TargetZone.OnTargetHit -= OnTargetHit;

        if (GameStateManager.Instance != null)
            GameStateManager.Instance.OnStateChanged -= OnStateChanged;
    }

    private void Update()
    {
        Vector2 pos = _panelRect.anchoredPosition;
        pos.x = Mathf.Lerp(pos.x, _targetX, Time.deltaTime * SlideSpeed);
        _panelRect.anchoredPosition = pos;
    }

    // ── Event handlers ────────────────────────────────────────────────────────
    private void OnTargetHit(TargetZone zone) => _targetHitThisShot = true;

    private void OnStateChanged(GameStateManager.GameState state)
    {
        if (state == GameStateManager.GameState.InFlight)
        {
            _targetHitThisShot = false;
            _shotOrigin = _shooter != null ? _shooter.transform.position : Vector3.zero;
        }
        else if (state == GameStateManager.GameState.Landed)
        {
            RecordAndShow();
        }
    }

    // ── Logic ─────────────────────────────────────────────────────────────────
    private void RecordAndShow()
    {
        float dist = _shooter != null
            ? Vector3.Distance(_shotOrigin, _shooter.transform.position)
            : 0f;

        _mainText.text  = $"{dist:F0} m";
        _mainText.color = _targetHitThisShot ? new Color(0.2f, 0.95f, 0.3f) : Color.white;
        _subText.text   = _targetHitThisShot ? "Target hit!" : "No target";
        _subText.color  = _targetHitThisShot ? new Color(0.2f, 0.95f, 0.3f) : new Color(0.6f, 0.6f, 0.6f);

        // Slide in, then schedule auto-hide
        _targetX = _shownX;
        if (_hideRoutine != null) StopCoroutine(_hideRoutine);
        _hideRoutine = StartCoroutine(AutoHide());
    }

    private IEnumerator AutoHide()
    {
        yield return new WaitForSeconds(HideDelay);
        _targetX = _hiddenX;
        _hideRoutine = null;
    }

    // ── UI construction ───────────────────────────────────────────────────────
    private void BuildUI()
    {
        // Panel starts off-screen to the right; slides in after a shot lands.
        _shownX  = -10f;          // right edge 10 px from screen edge
        _hiddenX = PanelW + 20f;  // fully off-screen

        // Canvas
        GameObject canvasGO = new GameObject("ShotHistoryCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 18;
        canvasGO.AddComponent<CanvasScaler>();

        // Panel
        GameObject panelGO = new GameObject("ShotHistoryPanel");
        panelGO.transform.SetParent(canvasGO.transform, false);
        Image panelBg = panelGO.AddComponent<Image>();
        panelBg.color = new Color(0f, 0f, 0f, 0.75f);
        _panelRect = panelGO.GetComponent<RectTransform>();
        _panelRect.anchorMin        = new Vector2(1f, 0.5f);
        _panelRect.anchorMax        = new Vector2(1f, 0.5f);
        _panelRect.pivot            = new Vector2(1f, 0.5f);
        _panelRect.sizeDelta        = new Vector2(PanelW, PanelH);
        _panelRect.anchoredPosition = new Vector2(_hiddenX, 0f);
        _targetX = _hiddenX;

        // Distance (large text, centred)
        GameObject mainGO = new GameObject("ShotDist");
        mainGO.transform.SetParent(panelGO.transform, false);
        _mainText           = mainGO.AddComponent<Text>();
        _mainText.font      = GetBuiltinFont();
        _mainText.fontSize  = 22;
        _mainText.fontStyle = FontStyle.Bold;
        _mainText.alignment = TextAnchor.MiddleCenter;
        _mainText.color     = Color.white;
        _mainText.text      = string.Empty;
        RectTransform mainRect = mainGO.GetComponent<RectTransform>();
        mainRect.anchorMin        = new Vector2(0f, 0.5f);
        mainRect.anchorMax        = new Vector2(1f, 1f);
        mainRect.offsetMin        = Vector2.zero;
        mainRect.offsetMax        = Vector2.zero;

        // Hit/miss label (small text below)
        GameObject subGO = new GameObject("ShotResult");
        subGO.transform.SetParent(panelGO.transform, false);
        _subText           = subGO.AddComponent<Text>();
        _subText.font      = GetBuiltinFont();
        _subText.fontSize  = 12;
        _subText.alignment = TextAnchor.MiddleCenter;
        _subText.color     = new Color(0.6f, 0.6f, 0.6f);
        _subText.text      = string.Empty;
        RectTransform subRect = subGO.GetComponent<RectTransform>();
        subRect.anchorMin  = new Vector2(0f, 0f);
        subRect.anchorMax  = new Vector2(1f, 0.5f);
        subRect.offsetMin  = Vector2.zero;
        subRect.offsetMax  = Vector2.zero;
    }

    private static Font GetBuiltinFont()
    {
        Font f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (f == null) f = Resources.GetBuiltinResource<Font>("Arial.ttf");
        return f;
    }
}

/// <summary>Creates <see cref="ShotHistoryUI"/> at runtime after scene load.</summary>
public static class ShotHistoryBootstrapper
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        BallShooter shooter = Object.FindFirstObjectByType<BallShooter>();
        if (shooter == null) return;

        GameObject go = new GameObject("ShotHistoryUI");
        ShotHistoryUI ui = go.AddComponent<ShotHistoryUI>();
        ui.Init(shooter);
        Object.DontDestroyOnLoad(go);
    }
}
