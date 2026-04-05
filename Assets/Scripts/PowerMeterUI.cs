using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Vertical power-meter HUD on the LEFT side of the screen.
/// Slides in from the left when charging, slides out when done.
/// Three color zones (green → yellow → red, bottom to top), white sweet-spot
/// bracket at 70%, and a horizontal needle that rises with power.
/// Built entirely in code — no scene setup required.
/// Call <see cref="Init"/> after adding this component (or let
/// <see cref="PowerMeterBootstrapper"/> do it automatically).
/// </summary>
public class PowerMeterUI : MonoBehaviour
{
    // ── Zone thresholds (fraction of bar height) ──────────────────────────────
    private const float ZoneMidStart  = 0.60f;   // green → yellow
    private const float ZoneHighStart = 0.80f;   // yellow → red
    private const float SweetSpot     = 0.70f;   // bracket position

    // ── Layout constants ──────────────────────────────────────────────────────
    private readonly Vector2 _barSize      = new Vector2(28f, 180f);
    private const float      LabelHeight   = 20f;
    private const float      LeftMargin    = 20f;
    private const float      SlideSpeed    = 10f;
    private const float      NeedleLerp    = 14f;

    // ── Runtime state ─────────────────────────────────────────────────────────
    private BallShooter _shooter;

    private RectTransform _container;
    private RectTransform _needleRect;
    private Text          _pctText;
    private Image         _flashOverlay;

    private float _shownX, _hiddenX;
    private float _targetX;
    private float _needleY;
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
        _targetX = _hiddenX;
        _container.anchoredPosition = new Vector2(_hiddenX, 0f);

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

        // ── Slide animation (X axis) ───────────────────────────────────────
        Vector2 pos = _container.anchoredPosition;
        pos.x = Mathf.Lerp(pos.x, _targetX, Time.deltaTime * SlideSpeed);
        _container.anchoredPosition = pos;

        if (_shooter == null) return;

        float t = _shooter.maxForce > 0f
            ? Mathf.Clamp01(_shooter.CurrentForce / _shooter.maxForce)
            : 0f;

        // ── Needle lerp (Y axis, bottom → top) ────────────────────────────
        float targetY = t * _barSize.y;
        _needleY = Mathf.Lerp(_needleY, targetY, Time.deltaTime * NeedleLerp);
        _needleRect.anchoredPosition = new Vector2(0f, _needleY);

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
        _targetX = show ? _shownX : _hiddenX;
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
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 20;
        canvasGO.AddComponent<CanvasScaler>();

        // Container dimensions:
        //   top label | 4px | bar | 4px | bottom pct text
        float containerW = _barSize.x + 8f;
        float containerH = LabelHeight + 4f + _barSize.y + 4f + LabelHeight;

        // Slide positions: pivot at container center, anchor at screen left-middle.
        // _shownX places the container's left edge exactly at LeftMargin.
        _shownX  = LeftMargin + containerW * 0.5f;
        _hiddenX = -(containerW * 0.5f + 20f);

        // Container — anchor left-middle of screen, pivot at center
        GameObject containerGO = new GameObject("PowerMeter_Container");
        containerGO.transform.SetParent(canvasGO.transform, false);
        _container = containerGO.GetComponent<RectTransform>();
        _container.anchorMin        = new Vector2(0f, 0.5f);
        _container.anchorMax        = new Vector2(0f, 0.5f);
        _container.pivot            = new Vector2(0.5f, 0.5f);
        _container.sizeDelta        = new Vector2(containerW, containerH);
        _container.anchoredPosition = new Vector2(_hiddenX, 0f);

        // Dark background bar — centered in container, above the pct label
        // In container space (pivot = center): bar center is at y = (LabelHeight + 4) / 2 - (LabelHeight + 4) / 2 = 0 when labels are equal,
        // but labels are equal so bar IS at y=0 (container center).
        // (LabelHeight above bar) == (LabelHeight below bar) → bar is perfectly centered.
        GameObject bgGO  = new GameObject("Bar_BG");
        bgGO.transform.SetParent(containerGO.transform, false);
        Image bgImg = bgGO.AddComponent<Image>();
        bgImg.color = new Color(0.06f, 0.06f, 0.06f, 0.90f);
        RectTransform bgRect = bgGO.GetComponent<RectTransform>();
        bgRect.anchorMin        = new Vector2(0.5f, 0.5f);
        bgRect.anchorMax        = new Vector2(0.5f, 0.5f);
        bgRect.pivot            = new Vector2(0.5f, 0.5f);
        bgRect.sizeDelta        = new Vector2(_barSize.x, _barSize.y);
        bgRect.anchoredPosition = new Vector2(0f, 0f);

        // Color zones (children of bgGO, stacked bottom → top)
        AddZone(bgGO, "Zone_Green",  0f,            ZoneMidStart,   new Color(0.15f, 0.75f, 0.15f, 0.9f));
        AddZone(bgGO, "Zone_Yellow", ZoneMidStart,  ZoneHighStart,  new Color(0.90f, 0.78f, 0.08f, 0.9f));
        AddZone(bgGO, "Zone_Red",    ZoneHighStart, 1f,             new Color(0.90f, 0.18f, 0.10f, 0.9f));

        // Horizontal tick marks at the two zone boundaries
        AddMarkerTick(bgGO, "Tick_60", ZoneMidStart);
        AddMarkerTick(bgGO, "Tick_80", ZoneHighStart);

        // Flash overlay (full bar)
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

        // Sweet-spot bracket
        AddSweetSpotBracket(bgGO);

        // Needle — horizontal white bar that rises from bottom (0%) to top (100%).
        // Child of bgGO; anchor at bottom-center so anchoredPosition.y maps directly to % height.
        GameObject needleGO = new GameObject("Needle");
        needleGO.transform.SetParent(bgGO.transform, false);
        Image needleImg = needleGO.AddComponent<Image>();
        needleImg.color = Color.white;
        _needleRect = needleGO.GetComponent<RectTransform>();
        _needleRect.anchorMin        = new Vector2(0.5f, 0f);
        _needleRect.anchorMax        = new Vector2(0.5f, 0f);
        _needleRect.pivot            = new Vector2(0.5f, 0.5f);
        _needleRect.sizeDelta        = new Vector2(_barSize.x + 8f, 3f);
        _needleRect.anchoredPosition = new Vector2(0f, 0f);
        _needleY = 0f;

        // "POWER" label — above bar
        GameObject labelGO = new GameObject("Label_POWER");
        labelGO.transform.SetParent(containerGO.transform, false);
        Text label = labelGO.AddComponent<Text>();
        label.text      = "POWER";
        label.font      = GetBuiltinFont();
        label.fontSize  = 11;
        label.fontStyle = FontStyle.Bold;
        label.alignment = TextAnchor.MiddleCenter;
        label.color     = Color.white;
        RectTransform labelRect = labelGO.GetComponent<RectTransform>();
        labelRect.anchorMin        = new Vector2(0.5f, 0.5f);
        labelRect.anchorMax        = new Vector2(0.5f, 0.5f);
        labelRect.pivot            = new Vector2(0.5f, 0.5f);
        labelRect.sizeDelta        = new Vector2(containerW, LabelHeight);
        // Center of POWER label = bar top + 4px gap + half label height
        labelRect.anchoredPosition = new Vector2(0f, _barSize.y * 0.5f + 4f + LabelHeight * 0.5f);

        // Live percentage text — below bar
        GameObject pctGO = new GameObject("Label_Pct");
        pctGO.transform.SetParent(containerGO.transform, false);
        _pctText           = pctGO.AddComponent<Text>();
        _pctText.text      = "0%";
        _pctText.font      = GetBuiltinFont();
        _pctText.fontSize  = 11;
        _pctText.fontStyle = FontStyle.Bold;
        _pctText.alignment = TextAnchor.MiddleCenter;
        _pctText.color     = Color.white;
        RectTransform pctRect = pctGO.GetComponent<RectTransform>();
        pctRect.anchorMin        = new Vector2(0.5f, 0.5f);
        pctRect.anchorMax        = new Vector2(0.5f, 0.5f);
        pctRect.pivot            = new Vector2(0.5f, 0.5f);
        pctRect.sizeDelta        = new Vector2(containerW, LabelHeight);
        // Center of pct label = bar bottom - 4px gap - half label height
        pctRect.anchoredPosition = new Vector2(0f, -(_barSize.y * 0.5f + 4f + LabelHeight * 0.5f));
    }

    /// <summary>
    /// Adds a vertical color zone as a child of <paramref name="barParent"/>.
    /// startFrac/endFrac are 0–1 fractions of bar height (bottom to top).
    /// </summary>
    private static void AddZone(GameObject barParent, string zoneName,
                                float startFrac, float endFrac, Color color)
    {
        GameObject go = new GameObject(zoneName);
        go.transform.SetParent(barParent.transform, false);
        go.AddComponent<Image>().color = color;
        RectTransform r = go.GetComponent<RectTransform>();
        r.anchorMin = new Vector2(0f, startFrac);
        r.anchorMax = new Vector2(1f, endFrac);
        r.offsetMin = Vector2.zero;
        r.offsetMax = Vector2.zero;
    }

    /// <summary>
    /// Adds a 3 px tall horizontal tick at <paramref name="frac"/> (0–1) up the bar.
    /// </summary>
    private static void AddMarkerTick(GameObject barParent, string name, float frac)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(barParent.transform, false);
        go.AddComponent<Image>().color = new Color(1f, 1f, 1f, 0.80f);
        RectTransform r = go.GetComponent<RectTransform>();
        r.anchorMin = new Vector2(0f, frac);
        r.anchorMax = new Vector2(1f, frac);
        r.pivot     = new Vector2(0.5f, 0.5f);
        r.offsetMin = new Vector2( 3f, -1.5f);
        r.offsetMax = new Vector2(-3f,  1.5f);
    }

    /// <summary>
    /// Builds a small white bracket around the sweet-spot position on the bar.
    /// The bracket is a horizontal rectangle at SweetSpot fraction.
    /// </summary>
    private void AddSweetSpotBracket(GameObject barParent)
    {
        // y position of sweet spot relative to bgGO bottom-center anchor
        float sweetY = SweetSpot * _barSize.y;
        float bHalf  = 8f;   // half-height of bracket span
        float extra  = 4f;   // how far horizontals extend beyond bar width

        // Top horizontal bar of bracket
        AddBracketRect(barParent, "Bracket_TopBar",   0f,                sweetY + bHalf, _barSize.x + extra * 2f, 2f);
        // Bottom horizontal bar
        AddBracketRect(barParent, "Bracket_BotBar",   0f,                sweetY - bHalf, _barSize.x + extra * 2f, 2f);
        // Left vertical
        AddBracketRect(barParent, "Bracket_LeftBar",  -(_barSize.x * 0.5f + extra), sweetY, 2f, bHalf * 2f + 2f);
        // Right vertical
        AddBracketRect(barParent, "Bracket_RightBar",   _barSize.x * 0.5f + extra,  sweetY, 2f, bHalf * 2f + 2f);
    }

    /// <summary>Creates a white rectangle using bgGO's bottom-center anchor space.</summary>
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
