using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Compass-style wind readout in the top-right corner.
/// Shows a rotating arrow (direction the wind is blowing toward) and
/// a speed label in m/s. Built entirely in code — no scene setup required.
/// Created automatically by <see cref="WindBootstrapper"/>.
/// </summary>
public class WindIndicatorUI : MonoBehaviour
{
    private RectTransform _arrowPivot;   // the rect we rotate
    private Text          _speedText;
    private Text          _calmsText;    // "CALM" label shown at zero wind

    private const float PanelSize = 80f;

    private void Awake()
    {
        BuildUI();
    }

    private void Update()
    {
        if (WindSystem.Instance == null) return;

        Vector3 wind   = WindSystem.Instance.CurrentWind;
        float   speed  = wind.magnitude;
        bool    calm   = speed < 0.05f;

        _speedText.text  = calm ? string.Empty : $"{speed:F1} m/s";
        _calmsText.text  = calm ? "CALM" : string.Empty;

        // Rotate arrow so its tip points in the wind direction (XZ → screen).
        // Atan2(x, z) gives the angle from +Z, which maps to screen-up.
        float deg = calm ? 0f : Mathf.Atan2(wind.x, wind.z) * Mathf.Rad2Deg;
        _arrowPivot.localEulerAngles = new Vector3(0f, 0f, -deg);
    }

    private void BuildUI()
    {
        // ── Canvas ────────────────────────────────────────────────────────────
        GameObject canvasGO = new GameObject("WindCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 19;
        canvasGO.AddComponent<CanvasScaler>();

        // ── Panel (top-right) ─────────────────────────────────────────────────
        GameObject panelGO = new GameObject("WindPanel");
        panelGO.transform.SetParent(canvasGO.transform, false);
        Image panelBg = panelGO.AddComponent<Image>();
        panelBg.color = new Color(0f, 0f, 0f, 0.60f);
        RectTransform panelRect = panelGO.GetComponent<RectTransform>();
        panelRect.anchorMin        = new Vector2(1f, 1f);
        panelRect.anchorMax        = new Vector2(1f, 1f);
        panelRect.pivot            = new Vector2(1f, 1f);
        panelRect.sizeDelta        = new Vector2(PanelSize, PanelSize);
        panelRect.anchoredPosition = new Vector2(-10f, -10f);

        // ── "WIND" label ──────────────────────────────────────────────────────
        AddLabel(panelGO, "WIND", 11, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white,
                 new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f),
                 new Vector2(0f, 16f), new Vector2(0f, 0f));

        // ── Arrow pivot — sits in the middle of the panel ─────────────────────
        // We rotate this rect; children define the arrow shape.
        GameObject pivotGO = new GameObject("ArrowPivot");
        pivotGO.transform.SetParent(panelGO.transform, false);
        _arrowPivot = pivotGO.GetComponent<RectTransform>();
        _arrowPivot.anchorMin        = new Vector2(0.5f, 0.5f);
        _arrowPivot.anchorMax        = new Vector2(0.5f, 0.5f);
        _arrowPivot.pivot            = new Vector2(0.5f, 0.5f);
        _arrowPivot.sizeDelta        = new Vector2(6f, 34f);
        _arrowPivot.anchoredPosition = new Vector2(0f, -4f);

        // Arrow shaft
        Image shaft = pivotGO.AddComponent<Image>();
        shaft.color = new Color(0.85f, 0.85f, 0.85f, 1f);

        // Arrow head (tip at top of pivot — pointing in +Y direction, i.e. where wind blows)
        GameObject headGO = new GameObject("ArrowHead");
        headGO.transform.SetParent(pivotGO.transform, false);
        Image headImg = headGO.AddComponent<Image>();
        headImg.color = new Color(1f, 0.85f, 0.1f, 1f);   // yellow tip
        RectTransform headRect = headGO.GetComponent<RectTransform>();
        headRect.anchorMin        = new Vector2(0.5f, 1f);
        headRect.anchorMax        = new Vector2(0.5f, 1f);
        headRect.pivot            = new Vector2(0.5f, 0f);
        headRect.sizeDelta        = new Vector2(14f, 14f);
        headRect.anchoredPosition = Vector2.zero;

        // ── Speed text ────────────────────────────────────────────────────────
        GameObject speedGO = new GameObject("SpeedText");
        speedGO.transform.SetParent(panelGO.transform, false);
        _speedText           = speedGO.AddComponent<Text>();
        _speedText.font      = GetBuiltinFont();
        _speedText.fontSize  = 11;
        _speedText.alignment = TextAnchor.MiddleCenter;
        _speedText.color     = Color.white;
        _speedText.text      = string.Empty;
        RectTransform speedRect = speedGO.GetComponent<RectTransform>();
        speedRect.anchorMin        = new Vector2(0f, 0f);
        speedRect.anchorMax        = new Vector2(1f, 0f);
        speedRect.pivot            = new Vector2(0.5f, 0f);
        speedRect.sizeDelta        = new Vector2(0f, 16f);
        speedRect.anchoredPosition = new Vector2(0f, 2f);

        // ── "CALM" label (shown when wind is essentially zero) ────────────────
        GameObject calmGO = new GameObject("CalmText");
        calmGO.transform.SetParent(panelGO.transform, false);
        _calmsText           = calmGO.AddComponent<Text>();
        _calmsText.font      = GetBuiltinFont();
        _calmsText.fontSize  = 11;
        _calmsText.fontStyle = FontStyle.Italic;
        _calmsText.alignment = TextAnchor.MiddleCenter;
        _calmsText.color     = new Color(0.6f, 0.6f, 0.6f, 1f);
        _calmsText.text      = string.Empty;
        RectTransform calmRect = calmGO.GetComponent<RectTransform>();
        calmRect.anchorMin        = new Vector2(0f, 0f);
        calmRect.anchorMax        = new Vector2(1f, 0f);
        calmRect.pivot            = new Vector2(0.5f, 0f);
        calmRect.sizeDelta        = new Vector2(0f, 16f);
        calmRect.anchoredPosition = new Vector2(0f, 2f);
    }

    // Convenience helper for fully-stretched text rows (not used directly here,
    // kept for symmetry; specific rows are built inline above).
    private static void AddLabel(GameObject parent, string text, int fontSize,
                                 FontStyle style, TextAnchor align, Color color,
                                 Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
                                 Vector2 sizeDelta, Vector2 anchoredPos)
    {
        GameObject go = new GameObject("Label_" + text);
        go.transform.SetParent(parent.transform, false);
        Text t = go.AddComponent<Text>();
        t.text      = text;
        t.font      = GetBuiltinFont();
        t.fontSize  = fontSize;
        t.fontStyle = style;
        t.alignment = align;
        t.color     = color;
        RectTransform r = go.GetComponent<RectTransform>();
        r.anchorMin        = anchorMin;
        r.anchorMax        = anchorMax;
        r.pivot            = pivot;
        r.sizeDelta        = sizeDelta;
        r.anchoredPosition = anchoredPos;
    }

    private static Font GetBuiltinFont()
    {
        Font f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (f == null) f = Resources.GetBuiltinResource<Font>("Arial.ttf");
        return f;
    }
}
