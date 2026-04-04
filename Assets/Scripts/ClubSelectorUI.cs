using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Wii Sports-style club selector — a clean horizontal pill at the bottom-left.
/// Shows the current club name in white (selected in yellow/gold).
/// Q = cycle left, E = cycle right (Aiming state only).
/// Fires OnClubChanged(ClubDefinition) when the selection changes.
/// </summary>
public class ClubSelectorUI : MonoBehaviour
{
    public event Action<ClubDefinition> OnClubChanged;

    private ClubDefinition[] _clubs;
    private int _index;

    private Text _centerText;

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

        // Wii Sports-style: clean horizontal pill at bottom-left
        // Dark semi-transparent background, club name in white/gold
        GameObject panel = new GameObject("ClubPanel");
        panel.transform.SetParent(canvas.transform, false);
        Image bg = panel.AddComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.60f);
        RectTransform panelRT = panel.GetComponent<RectTransform>();
        panelRT.anchorMin        = new Vector2(0f, 0f);
        panelRT.anchorMax        = new Vector2(0f, 0f);
        panelRT.pivot            = new Vector2(0f, 0f);
        panelRT.sizeDelta        = new Vector2(160f, 36f);
        panelRT.anchoredPosition = new Vector2(20f, 20f);

        // Club icon placeholder (small circle on the left)
        GameObject iconGO = new GameObject("ClubIcon");
        iconGO.transform.SetParent(panel.transform, false);
        Image iconImg = iconGO.AddComponent<Image>();
        iconImg.color = new Color(0.7f, 0.7f, 0.7f, 0.5f);
        RectTransform iconRT = iconGO.GetComponent<RectTransform>();
        iconRT.anchorMin        = new Vector2(0f, 0.5f);
        iconRT.anchorMax        = new Vector2(0f, 0.5f);
        iconRT.pivot            = new Vector2(0f, 0.5f);
        iconRT.sizeDelta        = new Vector2(22f, 22f);
        iconRT.anchoredPosition = new Vector2(7f, 0f);

        // Club name text — yellow/gold for the selected club
        GameObject nameGO = new GameObject("ClubName");
        nameGO.transform.SetParent(panel.transform, false);
        _centerText            = nameGO.AddComponent<Text>();
        _centerText.font       = GetBuiltinFont();
        _centerText.fontSize   = 15;
        _centerText.fontStyle  = FontStyle.Bold;
        _centerText.alignment  = TextAnchor.MiddleLeft;
        _centerText.color      = new Color(1f, 0.85f, 0.20f, 1f);  // gold/yellow
        RectTransform nameRT = nameGO.GetComponent<RectTransform>();
        nameRT.anchorMin        = new Vector2(0f, 0f);
        nameRT.anchorMax        = new Vector2(1f, 1f);
        nameRT.offsetMin        = new Vector2(36f, 0f);  // leave room for icon
        nameRT.offsetMax        = new Vector2(-6f, 0f);

        // Subtle Q/E hint in the corner
        GameObject hintGO = new GameObject("QEHint");
        hintGO.transform.SetParent(panel.transform, false);
        Text hintText = hintGO.AddComponent<Text>();
        hintText.text      = "Q / E";
        hintText.font      = GetBuiltinFont();
        hintText.fontSize  = 8;
        hintText.alignment = TextAnchor.MiddleRight;
        hintText.color     = new Color(0.5f, 0.5f, 0.5f, 1f);
        RectTransform hintRT = hintGO.GetComponent<RectTransform>();
        hintRT.anchorMin        = new Vector2(0f, 0f);
        hintRT.anchorMax        = new Vector2(1f, 0f);
        hintRT.pivot            = new Vector2(1f, 0f);
        hintRT.sizeDelta        = new Vector2(0f, 10f);
        hintRT.anchoredPosition = new Vector2(-4f, -1f);
    }

    private void Update()
    {
        // Only accept input while aiming
        if (GameStateManager.Instance?.CurrentState != GameStateManager.GameState.Aiming) return;

        if (Input.GetKeyDown(KeyCode.Q))
            CycleLeft();
        else if (Input.GetKeyDown(KeyCode.E))
            CycleRight();
    }

    private void CycleLeft()
    {
        _index = (_index - 1 + _clubs.Length) % _clubs.Length;
        RefreshLabels();
        var def = _clubs[_index];
        Debug.Log($"Club changed to: {def.clubName}");
        OnClubChanged?.Invoke(def);
    }

    private void CycleRight()
    {
        _index = (_index + 1) % _clubs.Length;
        RefreshLabels();
        var def = _clubs[_index];
        Debug.Log($"Club changed to: {def.clubName}");
        OnClubChanged?.Invoke(def);
    }

    private void RefreshLabels()
    {
        if (_centerText != null)
            _centerText.text = _clubs[_index].clubName;
    }

    private static Font GetBuiltinFont()
    {
        Font f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (f == null) f = Resources.GetBuiltinResource<Font>("Arial.ttf");
        return f;
    }
}
