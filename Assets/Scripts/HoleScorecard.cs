using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Tracks strokes for the current hole and shows a result panel when the ball
/// is holed out. Built entirely in code — no scene setup required.
/// </summary>
public class HoleScorecard : MonoBehaviour
{
    public static HoleScorecard Instance { get; private set; }

    /// <summary>Number of shots fired this hole (increments on each InFlight transition).</summary>
    public int StrokeCount { get; private set; }

    private HoleBuilder _holeBuilder;
    private CanvasGroup _panelGroup;
    private Text _scoreNameText;
    private Text _detailText;
    private bool _resultShown;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        _holeBuilder = Object.FindObjectOfType<HoleBuilder>();
        BuildUI();

        if (GameStateManager.Instance != null)
            GameStateManager.Instance.OnStateChanged += OnStateChanged;

        CupDetector.OnBallHoledOut += OnBallHoledOut;
    }

    private void OnDestroy()
    {
        if (GameStateManager.Instance != null)
            GameStateManager.Instance.OnStateChanged -= OnStateChanged;

        CupDetector.OnBallHoledOut -= OnBallHoledOut;
    }

    private void OnStateChanged(GameStateManager.GameState state)
    {
        if (state == GameStateManager.GameState.InFlight)
            StrokeCount++;
    }

    private void OnBallHoledOut()
    {
        if (_resultShown) return;
        _resultShown = true;
        ShowResult();
    }

    private void ShowResult()
    {
        int par    = _holeBuilder != null ? _holeBuilder.par    : 4;
        int hole   = _holeBuilder != null ? _holeBuilder.holeNumber : 1;
        int diff   = StrokeCount - par;

        string scoreName = ScoreName(StrokeCount, diff);
        string detail    = $"Hole {hole}  |  Par {par}  |  {StrokeCount} Stroke{(StrokeCount == 1 ? "" : "s")}";

        _scoreNameText.text = scoreName;
        _detailText.text    = detail;

        StartCoroutine(FadeIn());
    }

    private static string ScoreName(int strokes, int diff)
    {
        if (strokes == 1)   return "HOLE IN ONE!";
        if (diff <= -3)     return "ALBATROSS!";
        if (diff == -2)     return "EAGLE!";
        if (diff == -1)     return "BIRDIE!";
        if (diff == 0)      return "PAR";
        if (diff == 1)      return "BOGEY";
        if (diff == 2)      return "DOUBLE BOGEY";
        return $"+{diff}";
    }

    private IEnumerator FadeIn()
    {
        _panelGroup.gameObject.SetActive(true);
        _panelGroup.alpha = 0f;

        float elapsed = 0f;
        while (elapsed < 0.8f)
        {
            elapsed += Time.deltaTime;
            _panelGroup.alpha = Mathf.Clamp01(elapsed / 0.8f);
            yield return null;
        }
        _panelGroup.alpha = 1f;
    }

    // ── UI Construction ───────────────────────────────────────────────────────
    private void BuildUI()
    {
        // Canvas
        GameObject canvasGO = new GameObject("ScorecardCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode  = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 30;
        canvasGO.AddComponent<CanvasScaler>();

        // Semi-transparent panel — centered, 480×300
        GameObject panelGO = new GameObject("ResultPanel");
        panelGO.transform.SetParent(canvasGO.transform, false);
        _panelGroup = panelGO.AddComponent<CanvasGroup>();

        Image bg = panelGO.AddComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.82f);

        RectTransform panelRect = panelGO.GetComponent<RectTransform>();
        panelRect.anchorMin        = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax        = new Vector2(0.5f, 0.5f);
        panelRect.pivot            = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta        = new Vector2(480f, 300f);
        panelRect.anchoredPosition = Vector2.zero;

        Font font = GetBuiltinFont();

        // Score name (large, centered)
        _scoreNameText = AddText(panelGO, "ScoreNameText",
            font, 62, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter,
            new Vector2(0f, 60f), new Vector2(460f, 90f));

        // Detail line (hole / par / strokes)
        _detailText = AddText(panelGO, "DetailText",
            font, 24, FontStyle.Normal, new Color(0.85f, 0.85f, 0.85f, 1f), TextAnchor.MiddleCenter,
            new Vector2(0f, -10f), new Vector2(460f, 40f));

        // "Next Hole" placeholder button
        GameObject btnGO = new GameObject("NextHoleButton");
        btnGO.transform.SetParent(panelGO.transform, false);
        Image btnBg = btnGO.AddComponent<Image>();
        btnBg.color = new Color(0.18f, 0.65f, 0.28f, 1f);
        RectTransform btnRect = btnGO.GetComponent<RectTransform>();
        btnRect.anchorMin        = new Vector2(0.5f, 0f);
        btnRect.anchorMax        = new Vector2(0.5f, 0f);
        btnRect.pivot            = new Vector2(0.5f, 0f);
        btnRect.sizeDelta        = new Vector2(200f, 50f);
        btnRect.anchoredPosition = new Vector2(0f, 20f);

        AddText(btnGO, "NextHoleLabel",
            font, 22, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter,
            Vector2.zero, new Vector2(200f, 50f));
        btnGO.GetComponentInChildren<Text>().text = "Next Hole";

        panelGO.SetActive(false); // hidden until holed out
    }

    private static Text AddText(GameObject parent, string name,
        Font font, int size, FontStyle style, Color color,
        TextAnchor alignment, Vector2 anchoredPos, Vector2 sizeDelta)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        Text t = go.AddComponent<Text>();
        t.font      = font;
        t.fontSize  = size;
        t.fontStyle = style;
        t.color     = color;
        t.alignment = alignment;

        RectTransform r = go.GetComponent<RectTransform>();
        r.anchorMin        = new Vector2(0.5f, 0.5f);
        r.anchorMax        = new Vector2(0.5f, 0.5f);
        r.pivot            = new Vector2(0.5f, 0.5f);
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
