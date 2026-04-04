using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Bottom-left carousel showing the current club and its neighbours.
/// Q = cycle left, E = cycle right (Aiming state only).
/// Fires OnClubChanged(ClubDefinition) when the selection changes.
/// </summary>
public class ClubSelectorUI : MonoBehaviour
{
    public event Action<ClubDefinition> OnClubChanged;

    private ClubDefinition[] _clubs;
    private int _index;

    private RectTransform _strip;
    private Text          _leftText;
    private Text          _centerText;
    private Text          _rightText;

    private float _stripCurrentX;
    private float _stripTargetX;
    private const float SlideSpeed = 14f;

    public ClubDefinition CurrentClub => _clubs[_index];

    public void Init(ClubDefinition[] clubs)
    {
        _clubs = clubs;
        _index = 0;
        BuildUI();
        RefreshLabels();
    }

    private void BuildUI()
    {
        // Canvas
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 18;
        gameObject.AddComponent<CanvasScaler>();
        gameObject.AddComponent<GraphicRaycaster>();

        // Dark background panel — bottom-left, above the power meter
        GameObject panel = new GameObject("ClubPanel");
        panel.transform.SetParent(canvas.transform, false);
        Image bg = panel.AddComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.55f);
        RectTransform panelRT = panel.GetComponent<RectTransform>();
        panelRT.anchorMin        = new Vector2(0f, 0f);
        panelRT.anchorMax        = new Vector2(0f, 0f);
        panelRT.pivot            = new Vector2(0f, 0f);
        panelRT.sizeDelta        = new Vector2(280f, 50f);
        panelRT.anchoredPosition = new Vector2(10f, 160f);

        // Strip: slides for the transition animation
        GameObject stripGO = new GameObject("Strip");
        stripGO.transform.SetParent(panel.transform, false);
        _strip = stripGO.AddComponent<RectTransform>();
        _strip.anchorMin        = new Vector2(0.5f, 0.5f);
        _strip.anchorMax        = new Vector2(0.5f, 0.5f);
        _strip.pivot            = new Vector2(0.5f, 0.5f);
        _strip.sizeDelta        = new Vector2(280f, 50f);
        _strip.anchoredPosition = Vector2.zero;

        _leftText   = MakeLabel(stripGO.transform, "Left",   new Vector2(-90f, 0f), 12, false);
        _centerText = MakeLabel(stripGO.transform, "Center", new Vector2(  0f, 0f), 17, true);
        _rightText  = MakeLabel(stripGO.transform, "Right",  new Vector2( 90f, 0f), 12, false);

        // Key hint below the panel
        GameObject hint = new GameObject("Hint");
        hint.transform.SetParent(panel.transform, false);
        Text hintText = hint.AddComponent<Text>();
        hintText.text      = "Q  /  E";
        hintText.font      = GetBuiltinFont();
        hintText.fontSize  = 10;
        hintText.alignment = TextAnchor.MiddleCenter;
        hintText.color     = new Color(0.6f, 0.6f, 0.6f, 1f);
        RectTransform hintRT = hint.GetComponent<RectTransform>();
        hintRT.anchorMin        = new Vector2(0f, 0f);
        hintRT.anchorMax        = new Vector2(1f, 0f);
        hintRT.pivot            = new Vector2(0.5f, 1f);
        hintRT.sizeDelta        = new Vector2(0f, 14f);
        hintRT.anchoredPosition = new Vector2(0f, -2f);
    }

    private Text MakeLabel(Transform parent, string objName, Vector2 pos, int size, bool bold)
    {
        GameObject go = new GameObject(objName);
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta        = new Vector2(90f, 44f);
        Text t = go.AddComponent<Text>();
        t.font      = GetBuiltinFont();
        t.fontSize  = size;
        t.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
        t.alignment = TextAnchor.MiddleCenter;
        t.color     = bold ? Color.white : new Color(0.72f, 0.72f, 0.72f);
        return t;
    }

    private void Update()
    {
        // Smooth strip slide back to center
        _stripCurrentX = Mathf.Lerp(_stripCurrentX, _stripTargetX, SlideSpeed * Time.deltaTime);
        if (_strip != null)
            _strip.anchoredPosition = new Vector2(_stripCurrentX, 0f);

        // Only accept input while aiming
        if (GameStateManager.Instance?.CurrentState != GameStateManager.GameState.Aiming) return;

        if (Input.GetKeyDown(KeyCode.Q))
            CycleLeft();
        else if (Input.GetKeyDown(KeyCode.E))
            CycleRight();
    }

    private void CycleLeft()
    {
        _index          = (_index - 1 + _clubs.Length) % _clubs.Length;
        _stripCurrentX  = -90f;   // snap offset, then slide to 0
        _stripTargetX   = 0f;
        RefreshLabels();
        OnClubChanged?.Invoke(_clubs[_index]);
    }

    private void CycleRight()
    {
        _index          = (_index + 1) % _clubs.Length;
        _stripCurrentX  = 90f;    // snap offset, then slide to 0
        _stripTargetX   = 0f;
        RefreshLabels();
        OnClubChanged?.Invoke(_clubs[_index]);
    }

    private void RefreshLabels()
    {
        int left  = (_index - 1 + _clubs.Length) % _clubs.Length;
        int right = (_index + 1) % _clubs.Length;
        _leftText.text   = _clubs[left].clubName;
        _centerText.text = _clubs[_index].clubName;
        _rightText.text  = _clubs[right].clubName;
    }

    private static Font GetBuiltinFont()
    {
        Font f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (f == null) f = Resources.GetBuiltinResource<Font>("Arial.ttf");
        return f;
    }
}
