using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Full-featured power-meter HUD. Built entirely in code — no scene setup required.
/// Features: three color zones, white sweet-spot bracket, lerping needle, live %
/// text, slide-in/out animation, and a red flash coroutine at max charge.
/// Call <see cref="Init"/> after adding this component (or let
/// <see cref="PowerMeterBootstrapper"/> do it automatically).
/// </summary>
public class PowerMeterUI : MonoBehaviour
{
    // ── Zone thresholds (fraction of bar width) ───────────────────────────────
    private const float ZoneMidStart  = 0.60f;   // green → yellow
    private const float ZoneHighStart = 0.80f;   // yellow → red
    private const float SweetSpot     = 0.70f;   // bracket position

    // ── Layout constants ──────────────────────────────────────────────────────
    private readonly Vector2 _barSize       = new Vector2(320f, 28f);
    private const float      LabelRowHeight = 20f;   // above the bar
    private const float      BottomMargin   = 80f;
    private const float      SlideSpeed     = 10f;
    private const float      NeedleLerp     = 14f;

    // ── Runtime state ─────────────────────────────────────────────────────────
    private BallShooter _shooter;

    private RectTransform _container;
    private RectTransform _needleRect;
    private Text          _pctText;
    private Image         _flashOverlay;

    private float _shownY, _hiddenY;
    private float _targetY;
    private float _needleX;           // lerped x in container space
    private Coroutine _flashRoutine;

    // ── Public init ──────────────────────────────────────────────────────────
    /// <summary>Wire up the BallShooter after instantiation.</summary>
    public void Init(BallShooter shooter) => _shooter = shooter;

    // ── Unity lifecycle ───────────────────────────────────────────────────────
    private void Awake()
    {
        BuildUI();
    }

    private void Start()
    {
        _targetY = _hiddenY;
        _container.anchoredPosition = new Vector2(0f, _hiddenY);

        if (GameStateManager.Instance != null)
            GameStateManager.Instance.OnStateChanged += OnStateChanged;
    }

    private void OnDestroy()
    {
        if (GameStateManager.Instance != null)
            GameStateManager.Instance.OnStateChanged -= OnStateChanged;
    }

    private void Update()
    {
        if (_container == null) return;

        // ── Slide animation ────────────────────────────────────────────────
        Vector2 pos = _container.anchoredPosition;
        pos.y = Mathf.Lerp(pos.y, _targetY, Time.deltaTime * SlideSpeed);
        _container.anchoredPosition = pos;

        if (_shooter == null) return;

        float t = _shooter.maxForce > 0f
            ? Mathf.Clamp01(_shooter.CurrentForce / _shooter.maxForce)
            : 0f;

        // ── Needle lerp ────────────────────────────────────────────────────
        float targetX = Mathf.Lerp(-_barSize.x * 0.5f, _barSize.x * 0.5f, t);
        _needleX = Mathf.Lerp(_needleX, targetX, Time.deltaTime * NeedleLerp);
        _needleRect.anchoredPosition = new Vector2(_needleX, -3f);

        // ── Percentage text ────────────────────────────────────────────────
        _pctText.text = $"{Mathf.RoundToInt(t * 100f)}%";

        // ── Max-charge flash ───────────────────────────────────────────────
        if (t >= 1f && _flashRoutine == null)
            _flashRoutine = StartCoroutine(FlashRed());
        else if (t < 1f && _flashRoutine != null)
            StopFlash();
    }

    // ── State listener ────────────────────────────────────────────────────────
    private void OnStateChanged(GameStateManager.GameState state)
    {
        bool show = state == GameStateManager.GameState.Charging;
        _targetY = show ? _shownY : _hiddenY;

        if (!show) StopFlash();
    }

    // ── Flash coroutine ───────────────────────────────────────────────────────
    private IEnumerator FlashRed()
    {
        while (true)
        {
            _flashOverlay.enabled = true;
            yield return new WaitForSeconds(0.12f);
            _flashOverlay.enabled = false;
            yield return new WaitForSeconds(0.12f);
        }
    }

    private void StopFlash()
    {
        if (_flashRoutine != null)
        {
            StopCoroutine(_flashRoutine);
            _flashRoutine = null;
        }
        if (_flashOverlay != null) _flashOverlay.enabled = false;
    }

    // ── UI construction ───────────────────────────────────────────────────────
    private void BuildUI()
    {
        // Canvas
        GameObject canvasGO = new GameObject("PowerMeterCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode  = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 20;
        canvasGO.AddComponent<CanvasScaler>();

        // Container — the rect that slides in/out
        float containerH = _barSize.y + LabelRowHeight + 6f;
        _shownY  = BottomMargin;
        _hiddenY = -(containerH + 20f);

        GameObject containerGO = new GameObject("PowerMeter_Container");
        containerGO.transform.SetParent(canvasGO.transform, false);
        _container = containerGO.GetComponent<RectTransform>();
        _container.anchorMin        = new Vector2(0.5f, 0f);
        _container.anchorMax        = new Vector2(0.5f, 0f);
        _container.pivot            = new Vector2(0.5f, 0f);
        _container.sizeDelta        = new Vector2(_barSize.x, containerH);
        _container.anchoredPosition = new Vector2(0f, _hiddenY);

        // Dark background bar
        GameObject bgGO  = new GameObject("Bar_BG");
        bgGO.transform.SetParent(containerGO.transform, false);
        Image bgImg = bgGO.AddComponent<Image>();
        bgImg.color = new Color(0.06f, 0.06f, 0.06f, 0.90f);
        RectTransform bgRect = bgGO.GetComponent<RectTransform>();
        bgRect.anchorMin        = new Vector2(0f, 0f);
        bgRect.anchorMax        = new Vector2(1f, 0f);
        bgRect.pivot            = new Vector2(0.5f, 0f);
        bgRect.sizeDelta        = new Vector2(0f, _barSize.y);
        bgRect.anchoredPosition = Vector2.zero;

        // Color zones (children of bgGO so they fill by anchor fraction)
        AddZone(bgGO, "Zone_Green",  0f,            ZoneMidStart,   new Color(0.15f, 0.75f, 0.15f, 0.9f));
        AddZone(bgGO, "Zone_Yellow", ZoneMidStart,  ZoneHighStart,  new Color(0.90f, 0.78f, 0.08f, 0.9f));
        AddZone(bgGO, "Zone_Red",    ZoneHighStart, 1f,             new Color(0.90f, 0.18f, 0.10f, 0.9f));

        // Flash overlay (full-width, same height as bar, child of bgGO)
        GameObject flashGO = new GameObject("FlashOverlay");
        flashGO.transform.SetParent(bgGO.transform, false);
        _flashOverlay       = flashGO.AddComponent<Image>();
        _flashOverlay.color = new Color(1f, 0f, 0f, 0.45f);
        _flashOverlay.enabled = false;
        RectTransform flashRect = flashGO.GetComponent<RectTransform>();
        flashRect.anchorMin = Vector2.zero;
        flashRect.anchorMax = Vector2.one;
        flashRect.offsetMin = Vector2.zero;
        flashRect.offsetMax = Vector2.zero;

        // Sweet-spot bracket at SweetSpot fraction
        AddSweetSpotBracket(containerGO);

        // Needle — child of container, positioned by x each frame
        GameObject needleGO = new GameObject("Needle");
        needleGO.transform.SetParent(containerGO.transform, false);
        Image needleImg = needleGO.AddComponent<Image>();
        needleImg.color = Color.white;
        _needleRect = needleGO.GetComponent<RectTransform>();
        _needleRect.anchorMin = new Vector2(0.5f, 0f);
        _needleRect.anchorMax = new Vector2(0.5f, 0f);
        _needleRect.pivot     = new Vector2(0.5f, 0f);
        _needleRect.sizeDelta = new Vector2(3f, _barSize.y + 8f);
        // Start at left edge
        _needleRect.anchoredPosition = new Vector2(-_barSize.x * 0.5f, -3f);
        _needleX = -_barSize.x * 0.5f;

        // POWER label — top-left above the bar
        GameObject labelGO = new GameObject("Label_POWER");
        labelGO.transform.SetParent(containerGO.transform, false);
        Text label = labelGO.AddComponent<Text>();
        label.text      = "POWER";
        label.font      = GetBuiltinFont();
        label.fontSize  = 12;
        label.fontStyle = FontStyle.Bold;
        label.alignment = TextAnchor.MiddleLeft;
        label.color     = Color.white;
        RectTransform labelRect = labelGO.GetComponent<RectTransform>();
        labelRect.anchorMin        = new Vector2(0f, 0f);
        labelRect.anchorMax        = new Vector2(0f, 0f);
        labelRect.pivot            = new Vector2(0f, 0f);
        labelRect.sizeDelta        = new Vector2(70f, LabelRowHeight);
        labelRect.anchoredPosition = new Vector2(0f, _barSize.y + 2f);

        // Live percentage text — top-right above the bar
        GameObject pctGO = new GameObject("Label_Pct");
        pctGO.transform.SetParent(containerGO.transform, false);
        _pctText           = pctGO.AddComponent<Text>();
        _pctText.text      = "0%";
        _pctText.font      = GetBuiltinFont();
        _pctText.fontSize  = 12;
        _pctText.fontStyle = FontStyle.Bold;
        _pctText.alignment = TextAnchor.MiddleRight;
        _pctText.color     = Color.white;
        RectTransform pctRect = pctGO.GetComponent<RectTransform>();
        pctRect.anchorMin        = new Vector2(1f, 0f);
        pctRect.anchorMax        = new Vector2(1f, 0f);
        pctRect.pivot            = new Vector2(1f, 0f);
        pctRect.sizeDelta        = new Vector2(50f, LabelRowHeight);
        pctRect.anchoredPosition = new Vector2(0f, _barSize.y + 2f);
    }

    /// <summary>Add a horizontally-anchored color zone as a child of <paramref name="barParent"/>.</summary>
    private static void AddZone(GameObject barParent, string zoneName,
                                float startFrac, float endFrac, Color color)
    {
        GameObject go = new GameObject(zoneName);
        go.transform.SetParent(barParent.transform, false);
        go.AddComponent<Image>().color = color;
        RectTransform r = go.GetComponent<RectTransform>();
        r.anchorMin = new Vector2(startFrac, 0f);
        r.anchorMax = new Vector2(endFrac,   1f);
        r.offsetMin = Vector2.zero;
        r.offsetMax = Vector2.zero;
    }

    /// <summary>
    /// Builds a small white "[ ]" bracket at the sweet-spot position.
    /// The bracket straddles the bar top and bottom to make it clearly visible.
    /// </summary>
    private void AddSweetSpotBracket(GameObject container)
    {
        // Center of bracket in container space (container pivot = (0.5, 0))
        float cx    = (SweetSpot - 0.5f) * _barSize.x;   // x offset from center
        float bHalf = 8f;    // half-width of bracket
        float barTop = _barSize.y;
        float extra  = 5f;   // how far the verticals extend beyond bar edges

        // Top horizontal bar
        AddBracketRect(container, "Bracket_TopBar",  cx, barTop + extra,  bHalf * 2f + 2f, 2f);
        // Bottom horizontal bar
        AddBracketRect(container, "Bracket_BotBar",  cx, -extra,          bHalf * 2f + 2f, 2f);
        // Left vertical
        AddBracketRect(container, "Bracket_LeftBar",  cx - bHalf, barTop * 0.5f, 2f, barTop + extra * 2f);
        // Right vertical
        AddBracketRect(container, "Bracket_RightBar", cx + bHalf, barTop * 0.5f, 2f, barTop + extra * 2f);
    }

    /// <summary>Creates a white rectangle anchored to (0.5, 0) in <paramref name="parent"/>.</summary>
    private static void AddBracketRect(GameObject parent, string name,
                                       float anchoredX, float anchoredY,
                                       float w, float h)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        go.AddComponent<Image>().color = Color.white;
        RectTransform r = go.GetComponent<RectTransform>();
        r.anchorMin        = new Vector2(0.5f, 0f);
        r.anchorMax        = new Vector2(0.5f, 0f);
        r.pivot            = new Vector2(0.5f, 0.5f);
        r.sizeDelta        = new Vector2(w, h);
        r.anchoredPosition = new Vector2(anchoredX, anchoredY);
    }

    private static Font GetBuiltinFont()
    {
        Font f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (f == null) f = Resources.GetBuiltinResource<Font>("Arial.ttf");
        return f;
    }
}
