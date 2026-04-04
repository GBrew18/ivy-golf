using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Wii Sports-style vertical power meter.
/// Positioned on the LEFT side of the screen, vertically centered.
/// A yellow/gold bar fills bottom-to-top as charge builds.
/// Two white dot markers at 50% and 80%.
/// A small diamond indicator moves up with current power level.
/// Slides in from the left when charging, hides when not.
/// </summary>
public class PowerMeterUI : MonoBehaviour
{
    // ── Layout constants ──────────────────────────────────────────────────────
    private const float BarWidth      = 28f;
    private const float BarHeight     = 220f;
    private const float LeftMargin    = 20f;
    private const float DiamondSize   = 12f;
    private const float SlideSpeed    = 10f;
    private const float FillLerp      = 12f;

    // ── Runtime state ─────────────────────────────────────────────────────────
    private BallShooter _shooter;

    private RectTransform _container;
    private Image         _fillImage;
    private RectTransform _diamondRect;

    private float _shownX, _hiddenX;
    private float _targetX;
    private float _currentFill;
    private Coroutine _flashRoutine;
    private Image     _flashOverlay;

    // ── Public init ──────────────────────────────────────────────────────────
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

        // Slide in/out from the left
        Vector2 pos = _container.anchoredPosition;
        pos.x = Mathf.Lerp(pos.x, _targetX, Time.deltaTime * SlideSpeed);
        _container.anchoredPosition = pos;

        if (_shooter == null) return;

        float t = _shooter.maxForce > 0f
            ? Mathf.Clamp01(_shooter.CurrentForce / _shooter.maxForce)
            : 0f;

        // Smoothly lerp fill amount
        _currentFill = Mathf.Lerp(_currentFill, t, Time.deltaTime * FillLerp);
        if (_fillImage != null)
            _fillImage.fillAmount = _currentFill;

        // Diamond moves up the bar: bottom of bar → top of bar
        if (_diamondRect != null)
        {
            float barBottomY = -(BarHeight * 0.5f);
            float barTopY    =   BarHeight * 0.5f;
            float diamondY   = Mathf.Lerp(barBottomY, barTopY, _currentFill);
            _diamondRect.anchoredPosition = new Vector2(BarWidth * 0.5f + DiamondSize * 0.5f + 2f, diamondY);
        }

        // Flash at max charge
        if (t >= 1f && _flashRoutine == null)
            _flashRoutine = StartCoroutine(FlashMax());
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

    // ── Flash at max power ────────────────────────────────────────────────────
    private IEnumerator FlashMax()
    {
        while (true)
        {
            if (_flashOverlay != null) _flashOverlay.enabled = true;
            yield return new WaitForSeconds(0.10f);
            if (_flashOverlay != null) _flashOverlay.enabled = false;
            yield return new WaitForSeconds(0.10f);
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

        // Container — anchored to left-center, slides in/out horizontally
        // Extra width for the diamond indicator sticking out to the right
        float containerW = BarWidth + 28f;   // bar + diamond clearance
        float containerH = BarHeight + 30f;  // bar + label space

        // shown: left edge 20px from screen left
        // hidden: fully off screen left
        _shownX  =  LeftMargin;
        _hiddenX = -(containerW + LeftMargin + 10f);

        GameObject containerGO = new GameObject("PowerMeter_Container");
        containerGO.transform.SetParent(canvasGO.transform, false);
        _container = containerGO.GetComponent<RectTransform>();
        _container.anchorMin        = new Vector2(0f, 0.5f);
        _container.anchorMax        = new Vector2(0f, 0.5f);
        _container.pivot            = new Vector2(0f, 0.5f);
        _container.sizeDelta        = new Vector2(containerW, containerH);
        _container.anchoredPosition = new Vector2(_hiddenX, 0f);

        // ── Outer border (slightly larger dark rect behind bar) ───────────────
        GameObject borderGO = new GameObject("Border");
        borderGO.transform.SetParent(containerGO.transform, false);
        Image borderImg = borderGO.AddComponent<Image>();
        borderImg.color = new Color(0.05f, 0.05f, 0.05f, 0.95f);
        RectTransform borderRect = borderGO.GetComponent<RectTransform>();
        borderRect.anchorMin        = new Vector2(0f, 0.5f);
        borderRect.anchorMax        = new Vector2(0f, 0.5f);
        borderRect.pivot            = new Vector2(0f, 0.5f);
        borderRect.sizeDelta        = new Vector2(BarWidth + 4f, BarHeight + 4f);
        borderRect.anchoredPosition = new Vector2(0f, 0f);

        // ── Bar background ────────────────────────────────────────────────────
        GameObject bgGO = new GameObject("BarBG");
        bgGO.transform.SetParent(containerGO.transform, false);
        Image bgImg = bgGO.AddComponent<Image>();
        bgImg.color = new Color(0.10f, 0.10f, 0.10f, 0.95f);
        RectTransform bgRect = bgGO.GetComponent<RectTransform>();
        bgRect.anchorMin        = new Vector2(0f, 0.5f);
        bgRect.anchorMax        = new Vector2(0f, 0.5f);
        bgRect.pivot            = new Vector2(0f, 0.5f);
        bgRect.sizeDelta        = new Vector2(BarWidth, BarHeight);
        bgRect.anchoredPosition = new Vector2(2f, 0f);  // 2px inset from border left

        // ── Yellow fill (fills bottom-to-top) ─────────────────────────────────
        GameObject fillGO = new GameObject("Fill");
        fillGO.transform.SetParent(bgGO.transform, false);
        _fillImage            = fillGO.AddComponent<Image>();
        _fillImage.color      = new Color(1f, 0.85f, 0.10f, 1f);  // yellow/gold
        _fillImage.type       = Image.Type.Filled;
        _fillImage.fillMethod = Image.FillMethod.Vertical;
        _fillImage.fillOrigin = (int)Image.OriginVertical.Bottom;
        _fillImage.fillAmount = 0f;
        RectTransform fillRect = fillGO.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        // ── Flash overlay (full bar, flashes at max charge) ───────────────────
        GameObject flashGO = new GameObject("FlashOverlay");
        flashGO.transform.SetParent(bgGO.transform, false);
        _flashOverlay       = flashGO.AddComponent<Image>();
        _flashOverlay.color = new Color(1f, 1f, 0.5f, 0.5f);
        _flashOverlay.enabled = false;
        RectTransform flashRect = flashGO.GetComponent<RectTransform>();
        flashRect.anchorMin = Vector2.zero;
        flashRect.anchorMax = Vector2.one;
        flashRect.offsetMin = Vector2.zero;
        flashRect.offsetMax = Vector2.zero;

        // ── Dot markers at 50% and 80% height ────────────────────────────────
        AddDotMarker(bgGO, 0.50f, "Marker50");
        AddDotMarker(bgGO, 0.80f, "Marker80");

        // ── Diamond indicator (outside bar, to the right) ─────────────────────
        // Positioned by Update each frame based on current fill
        GameObject diamondGO = new GameObject("Diamond");
        diamondGO.transform.SetParent(containerGO.transform, false);
        Image diamondImg = diamondGO.AddComponent<Image>();
        diamondImg.color = new Color(1f, 0.90f, 0.20f, 1f);  // yellow diamond
        _diamondRect = diamondGO.GetComponent<RectTransform>();
        _diamondRect.anchorMin        = new Vector2(0f, 0.5f);
        _diamondRect.anchorMax        = new Vector2(0f, 0.5f);
        _diamondRect.pivot            = new Vector2(0.5f, 0.5f);
        _diamondRect.sizeDelta        = new Vector2(DiamondSize, DiamondSize);
        // Rotate 45° to make a diamond/rhombus shape
        _diamondRect.localEulerAngles = new Vector3(0f, 0f, 45f);
        // Start at bottom of bar
        _diamondRect.anchoredPosition = new Vector2(BarWidth * 0.5f + DiamondSize * 0.5f + 4f,
                                                     -(BarHeight * 0.5f));

        // ── "POWER" label above bar ───────────────────────────────────────────
        GameObject labelGO = new GameObject("Label_Power");
        labelGO.transform.SetParent(containerGO.transform, false);
        Text labelText = labelGO.AddComponent<Text>();
        labelText.text      = "PWR";
        labelText.font      = GetBuiltinFont();
        labelText.fontSize  = 9;
        labelText.fontStyle = FontStyle.Bold;
        labelText.alignment = TextAnchor.MiddleCenter;
        labelText.color     = new Color(0.9f, 0.85f, 0.5f, 1f);  // warm gold
        RectTransform labelRect = labelGO.GetComponent<RectTransform>();
        labelRect.anchorMin        = new Vector2(0f, 0.5f);
        labelRect.anchorMax        = new Vector2(0f, 0.5f);
        labelRect.pivot            = new Vector2(0f, 0f);
        labelRect.sizeDelta        = new Vector2(BarWidth + 4f, 14f);
        labelRect.anchoredPosition = new Vector2(0f, BarHeight * 0.5f + 4f);
    }

    /// <summary>Adds a thin white horizontal marker line at <paramref name="fraction"/> height of the bar.</summary>
    private static void AddDotMarker(GameObject barParent, float fraction, string markerName)
    {
        GameObject go = new GameObject(markerName);
        go.transform.SetParent(barParent.transform, false);
        Image img = go.AddComponent<Image>();
        img.color = new Color(1f, 1f, 1f, 0.70f);
        RectTransform r = go.GetComponent<RectTransform>();
        // Horizontal line spanning the full width at the given height fraction
        r.anchorMin        = new Vector2(0f, fraction);
        r.anchorMax        = new Vector2(1f, fraction);
        r.pivot            = new Vector2(0.5f, 0.5f);
        r.sizeDelta        = new Vector2(0f, 2f);
        r.anchoredPosition = Vector2.zero;
    }

    private static Font GetBuiltinFont()
    {
        Font f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (f == null) f = Resources.GetBuiltinResource<Font>("Arial.ttf");
        return f;
    }
}
