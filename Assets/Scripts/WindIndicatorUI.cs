using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Wii Sports-style wind/distance readout — teal rectangular panel in the top-right corner.
/// Shows a distance-to-pin placeholder ("-- yds") and wind speed + direction below it.
/// Built entirely in code — no scene setup required.
/// Created automatically by WindBootstrapper.
/// </summary>
public class WindIndicatorUI : MonoBehaviour
{
    private RectTransform _arrowPivot;
    private Text          _distanceText;
    private Text          _speedText;

    // Teal panel matching Wii Sports Golf color language
    private static readonly Color PanelColor = new Color(0.10f, 0.65f, 0.75f, 0.85f);

    private void Awake()
    {
        BuildUI();
    }

    private void Update()
    {
        if (WindSystem.Instance == null) return;

        Vector3 wind  = WindSystem.Instance.CurrentWind;
        float   speed = wind.magnitude;
        bool    calm  = speed < 0.05f;

        // Wind speed — convert m/s to mph (×2.237) for Wii Sports feel
        float mph = speed * 2.237f;
        _speedText.text = calm ? "CALM" : $"{mph:F1} mph";

        // Arrow rotates to show wind direction
        float deg = calm ? 0f : Mathf.Atan2(wind.x, wind.z) * Mathf.Rad2Deg;
        if (_arrowPivot != null)
            _arrowPivot.localEulerAngles = new Vector3(0f, 0f, -deg);

        // Distance text — placeholder until CupDetector/TeeDistance is hooked up
        // Hook up: _distanceText.text = $"{Mathf.RoundToInt(distanceYds)} yds";
        if (_distanceText != null && _distanceText.text == string.Empty)
            _distanceText.text = "-- yds";
    }

    private void BuildUI()
    {
        // ── Canvas ────────────────────────────────────────────────────────────
        GameObject canvasGO = new GameObject("WindCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 19;
        canvasGO.AddComponent<CanvasScaler>();

        // ── Panel (top-right, teal) ───────────────────────────────────────────
        GameObject panelGO = new GameObject("WindPanel");
        panelGO.transform.SetParent(canvasGO.transform, false);
        Image panelBg = panelGO.AddComponent<Image>();
        panelBg.color = PanelColor;
        RectTransform panelRect = panelGO.GetComponent<RectTransform>();
        panelRect.anchorMin        = new Vector2(1f, 1f);
        panelRect.anchorMax        = new Vector2(1f, 1f);
        panelRect.pivot            = new Vector2(1f, 1f);
        panelRect.sizeDelta        = new Vector2(130f, 90f);
        panelRect.anchoredPosition = new Vector2(-20f, -20f);

        // ── Distance row (top of panel) ───────────────────────────────────────
        _distanceText = MakeText(panelGO, "DistanceText",
            new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, 28f), new Vector2(0f, 0f),
            "-- yds", 16, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);

        // ── Divider line ──────────────────────────────────────────────────────
        GameObject divGO = new GameObject("Divider");
        divGO.transform.SetParent(panelGO.transform, false);
        Image divImg = divGO.AddComponent<Image>();
        divImg.color = new Color(1f, 1f, 1f, 0.30f);
        RectTransform divRect = divGO.GetComponent<RectTransform>();
        divRect.anchorMin        = new Vector2(0.05f, 1f);
        divRect.anchorMax        = new Vector2(0.95f, 1f);
        divRect.pivot            = new Vector2(0.5f, 1f);
        divRect.sizeDelta        = new Vector2(0f, 1f);
        divRect.anchoredPosition = new Vector2(0f, -28f);

        // ── Arrow pivot (center of bottom half) ──────────────────────────────
        GameObject pivotGO = new GameObject("ArrowPivot");
        pivotGO.transform.SetParent(panelGO.transform, false);
        _arrowPivot = pivotGO.GetComponent<RectTransform>();
        _arrowPivot.anchorMin        = new Vector2(0.25f, 0f);
        _arrowPivot.anchorMax        = new Vector2(0.25f, 0f);
        _arrowPivot.pivot            = new Vector2(0.5f, 0.5f);
        _arrowPivot.sizeDelta        = new Vector2(6f, 34f);
        _arrowPivot.anchoredPosition = new Vector2(0f, 32f);

        // Arrow shaft
        Image shaft = pivotGO.AddComponent<Image>();
        shaft.color = new Color(1f, 1f, 1f, 0.90f);

        // Arrowhead (yellow tip pointing up = wind direction)
        GameObject headGO = new GameObject("ArrowHead");
        headGO.transform.SetParent(pivotGO.transform, false);
        Image headImg = headGO.AddComponent<Image>();
        headImg.color = new Color(1f, 0.95f, 0.3f, 1f);
        RectTransform headRect = headGO.GetComponent<RectTransform>();
        headRect.anchorMin        = new Vector2(0.5f, 1f);
        headRect.anchorMax        = new Vector2(0.5f, 1f);
        headRect.pivot            = new Vector2(0.5f, 0f);
        headRect.sizeDelta        = new Vector2(12f, 12f);
        headRect.anchoredPosition = Vector2.zero;

        // ── Speed text (right side of bottom half) ────────────────────────────
        _speedText = MakeText(panelGO, "SpeedText",
            new Vector2(0.45f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, 56f), new Vector2(0f, -6f),
            string.Empty, 12, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
    }

    private static Text MakeText(GameObject parent, string objName,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
        Vector2 sizeDelta, Vector2 anchoredPos,
        string text, int fontSize, FontStyle style,
        TextAnchor align, Color color)
    {
        GameObject go = new GameObject(objName);
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
        return t;
    }

    private static Font GetBuiltinFont()
    {
        Font f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (f == null) f = Resources.GetBuiltinResource<Font>("Arial.ttf");
        return f;
    }
}
