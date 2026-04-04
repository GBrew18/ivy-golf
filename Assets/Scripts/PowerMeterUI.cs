using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Creates a power-meter bar at runtime — no scene setup required.
/// Add this component to any GameObject and assign <see cref="ballShooter"/>.
/// The bar appears at the bottom-center of the screen only while charging
/// and its fill color mirrors the trajectory arc gradient (green → yellow → red).
/// </summary>
public class PowerMeterUI : MonoBehaviour
{
    [Header("References")]
    [Tooltip("BallShooter whose CurrentForce and IsCharging drive the bar.")]
    [SerializeField] private BallShooter ballShooter;

    [Header("Layout")]
    [Tooltip("Width and height of the power bar in screen pixels.")]
    [SerializeField] private Vector2 barSize = new Vector2(300f, 28f);
    [Tooltip("Pixels above the bottom edge of the screen.")]
    [SerializeField] private float bottomMargin = 80f;

    [Header("Colors")]
    [SerializeField] private Color backgroundColor = new Color(0.08f, 0.08f, 0.08f, 0.85f);
    [SerializeField] private Color fillColorLow  = Color.green;
    [SerializeField] private Color fillColorMid  = Color.yellow;
    [SerializeField] private Color fillColorHigh = Color.red;
    [SerializeField] private Color labelColor    = Color.white;

    // Runtime UI references.
    private GameObject _canvasRoot;
    private RectTransform _fillRect;
    private Image _fillImage;

    private void Awake()
    {
        BuildUI();
    }

    private void Update()
    {
        if (_canvasRoot == null || ballShooter == null) return;

        bool charging = ballShooter.IsCharging;
        _canvasRoot.SetActive(charging);

        if (!charging) return;

        float t = ballShooter.maxForce > 0f
            ? ballShooter.CurrentForce / ballShooter.maxForce
            : 0f;

        // Stretch fill rect proportionally by setting its right anchor.
        _fillRect.anchorMax = new Vector2(t, 1f);

        // Mirror trajectory arc colors: green → yellow → red.
        _fillImage.color = t <= 0.5f
            ? Color.Lerp(fillColorLow,  fillColorMid,  t * 2f)
            : Color.Lerp(fillColorMid, fillColorHigh, (t - 0.5f) * 2f);
    }

    private void BuildUI()
    {
        // ── Canvas ──────────────────────────────────────────────────────────
        _canvasRoot = new GameObject("PowerMeterCanvas");
        Canvas canvas = _canvasRoot.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 20;
        _canvasRoot.AddComponent<CanvasScaler>();
        _canvasRoot.SetActive(false);

        // ── Background ──────────────────────────────────────────────────────
        GameObject bgGO = new GameObject("PowerMeter_BG");
        bgGO.transform.SetParent(_canvasRoot.transform, false);
        Image bgImage = bgGO.AddComponent<Image>();
        bgImage.color = backgroundColor;

        RectTransform bgRect = bgGO.GetComponent<RectTransform>();
        bgRect.anchorMin        = new Vector2(0.5f, 0f);
        bgRect.anchorMax        = new Vector2(0.5f, 0f);
        bgRect.pivot            = new Vector2(0.5f, 0f);
        bgRect.anchoredPosition = new Vector2(0f, bottomMargin);
        bgRect.sizeDelta        = barSize;

        // ── Fill (width driven by anchorMax.x = power fraction) ─────────────
        GameObject fillGO = new GameObject("PowerMeter_Fill");
        fillGO.transform.SetParent(bgGO.transform, false);
        _fillImage = fillGO.AddComponent<Image>();
        _fillImage.color = fillColorLow;

        _fillRect            = fillGO.GetComponent<RectTransform>();
        _fillRect.anchorMin  = Vector2.zero;
        _fillRect.anchorMax  = Vector2.zero;   // starts empty; set each frame
        _fillRect.pivot      = new Vector2(0f, 0.5f);
        _fillRect.offsetMin  = Vector2.zero;
        _fillRect.offsetMax  = Vector2.zero;

        // ── Label ────────────────────────────────────────────────────────────
        GameObject labelGO = new GameObject("PowerMeter_Label");
        labelGO.transform.SetParent(bgGO.transform, false);
        Text label = labelGO.AddComponent<Text>();
        label.text      = "POWER";
        label.font      = GetBuiltinFont();
        label.fontSize  = 13;
        label.fontStyle = FontStyle.Bold;
        label.alignment = TextAnchor.MiddleCenter;
        label.color     = labelColor;

        RectTransform labelRect = labelGO.GetComponent<RectTransform>();
        labelRect.anchorMin        = Vector2.zero;
        labelRect.anchorMax        = Vector2.one;
        labelRect.offsetMin        = Vector2.zero;
        labelRect.offsetMax        = Vector2.zero;
    }

    private static Font GetBuiltinFont()
    {
        Font f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (f == null) f = Resources.GetBuiltinResource<Font>("Arial.ttf");
        return f;
    }
}
