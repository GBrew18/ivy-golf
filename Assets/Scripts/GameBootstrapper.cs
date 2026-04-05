using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Single-entry-point bootstrapper. Fires in every scene after load.
/// Creates: GolfClub visual, PowerMeter UI, ClubSelector UI.
/// Replaces ClubBootstrapper + PowerMeterBootstrapper.
/// </summary>
public class GameBootstrapper : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        var go = new GameObject("GameBootstrapper");
        DontDestroyOnLoad(go);
        go.AddComponent<GameBootstrapper>().StartCoroutine(go.GetComponent<GameBootstrapper>().Init());
    }

    IEnumerator Init()
    {
        // Wait two frames so all other bootstrappers have run
        yield return null;
        yield return null;

        var shooter = Object.FindFirstObjectByType<BallShooter>();
        if (shooter == null)
        {
            Debug.LogWarning("[GameBootstrapper] No BallShooter found — skipping club/UI setup.");
            yield break;
        }

        SetupClub(shooter);
        SetupPowerMeter(shooter);
        SetupClubSelector(shooter);
    }

    // ── Club Visual ──────────────────────────────────────────────────────────

    void SetupClub(BallShooter shooter)
    {
        // Remove any pre-existing club GO parented to the shooter
        var existing = shooter.transform.Find("GolfClub");
        if (existing != null) Destroy(existing.gameObject);

        var clubRoot = new GameObject("GolfClub");
        clubRoot.transform.SetParent(shooter.transform, false);
        clubRoot.transform.localPosition = new Vector3(0.35f, -0.05f, 0f);

        // Grip
        var grip = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        grip.name = "Grip";
        Destroy(grip.GetComponent<Collider>());
        grip.transform.SetParent(clubRoot.transform, false);
        grip.transform.localPosition = new Vector3(0f, 0.11f, 0f);
        grip.transform.localScale = new Vector3(0.04f, 0.11f, 0.04f);
        grip.GetComponent<Renderer>().material.color = new Color(0.12f, 0.08f, 0.06f);

        // Shaft
        var shaft = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        shaft.name = "Shaft";
        Destroy(shaft.GetComponent<Collider>());
        shaft.transform.SetParent(clubRoot.transform, false);
        shaft.transform.localPosition = new Vector3(0f, -0.28f, 0f);
        shaft.transform.localScale = new Vector3(0.025f, 0.28f, 0.025f);
        shaft.GetComponent<Renderer>().material.color = new Color(0.8f, 0.8f, 0.82f);

        // Head
        var head = GameObject.CreatePrimitive(PrimitiveType.Cube);
        head.name = "Head";
        Destroy(head.GetComponent<Collider>());
        head.transform.SetParent(clubRoot.transform, false);
        head.transform.localPosition = new Vector3(0f, -0.62f, 0f);
        head.transform.localScale = new Vector3(0.14f, 0.07f, 0.06f);
        head.GetComponent<Renderer>().material.color = new Color(0.2f, 0.2f, 0.2f);

        Debug.Log("[GameBootstrapper] Club built and attached to shooter.");
    }

    // ── Power Meter ───────────────────────────────────────────────────────────

    GameObject _meterFill;
    BallShooter _shooter;

    void SetupPowerMeter(BallShooter shooter)
    {
        _shooter = shooter;

        // Destroy any existing power meter canvas
        var old = GameObject.Find("PowerMeterCanvas");
        if (old != null) Destroy(old);

        var canvasGO = new GameObject("PowerMeterCanvas");
        DontDestroyOnLoad(canvasGO);
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasGO.AddComponent<GraphicRaycaster>();

        // Background panel — left side, vertically centred
        var bgGO = new GameObject("BG");
        bgGO.transform.SetParent(canvasGO.transform, false);
        var bgRect = bgGO.AddComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0f, 0.5f);
        bgRect.anchorMax = new Vector2(0f, 0.5f);
        bgRect.pivot     = new Vector2(0f, 0.5f);
        bgRect.anchoredPosition = new Vector2(20f, 0f);
        bgRect.sizeDelta = new Vector2(30f, 200f);
        var bgImg = bgGO.AddComponent<Image>();
        bgImg.color = new Color(0.05f, 0.05f, 0.05f, 0.75f);

        // Coloured fill — green bottom, yellow mid, red top
        // We'll use a simple gradient via a child image that we scale
        var fillGO = new GameObject("Fill");
        fillGO.transform.SetParent(bgGO.transform, false);
        var fillRect = fillGO.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = new Vector2(1f, 0f);
        fillRect.pivot     = new Vector2(0.5f, 0f);
        fillRect.anchoredPosition = Vector2.zero;
        fillRect.sizeDelta = new Vector2(0f, 0f); // width matches parent, height set in Update
        var fillImg = fillGO.AddComponent<Image>();
        fillImg.color = new Color(1f, 0.82f, 0.1f); // gold
        _meterFill = fillGO;

        // Zone ticks at 50% and 80%
        CreateTick(bgGO.transform, 0.5f);
        CreateTick(bgGO.transform, 0.8f);

        // Label
        var labelGO = new GameObject("Label");
        labelGO.transform.SetParent(canvasGO.transform, false);
        var labelRect = labelGO.AddComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0f, 0.5f);
        labelRect.anchorMax = new Vector2(0f, 0.5f);
        labelRect.pivot     = new Vector2(0f, 0.5f);
        labelRect.anchoredPosition = new Vector2(20f, -115f);
        labelRect.sizeDelta = new Vector2(60f, 20f);
        var labelText = labelGO.AddComponent<Text>();
        labelText.text = "PWR";
        labelText.fontSize = 11;
        labelText.color = Color.white;
        labelText.alignment = TextAnchor.MiddleCenter;
        labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        canvasGO.SetActive(false); // hidden until charging
        _meterCanvas = canvasGO;

        // Listen for state changes
        if (GameStateManager.Instance != null)
            GameStateManager.Instance.OnStateChanged += OnStateChanged;
    }

    void CreateTick(Transform parent, float t)
    {
        var tick = new GameObject("Tick");
        tick.transform.SetParent(parent, false);
        var r = tick.AddComponent<RectTransform>();
        r.anchorMin = new Vector2(0f, t);
        r.anchorMax = new Vector2(1f, t);
        r.pivot     = new Vector2(0.5f, 0.5f);
        r.anchoredPosition = Vector2.zero;
        r.sizeDelta = new Vector2(0f, 2f);
        var img = tick.AddComponent<Image>();
        img.color = Color.white;
    }

    GameObject _meterCanvas;

    void OnStateChanged(GameStateManager.GameState state)
    {
        if (_meterCanvas != null)
            _meterCanvas.SetActive(state == GameStateManager.GameState.Charging);
    }

    void Update()
    {
        if (_meterFill == null || _shooter == null) return;
        if (_meterCanvas == null || !_meterCanvas.activeSelf) return;

        float t = Mathf.Clamp01(_shooter.CurrentForce / 18f); // 18 = maxForce
        var rect = _meterFill.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(0f, 200f * t);

        // Color: green → yellow → red
        Color c;
        if (t < 0.5f)      c = Color.Lerp(new Color(0.2f, 0.85f, 0.2f), new Color(1f, 0.85f, 0.1f), t * 2f);
        else               c = Color.Lerp(new Color(1f, 0.85f, 0.1f),   new Color(0.9f, 0.15f, 0.1f), (t - 0.5f) * 2f);
        _meterFill.GetComponent<Image>().color = c;
    }

    // ── Club Selector ─────────────────────────────────────────────────────────

    void SetupClubSelector(BallShooter shooter)
    {
        var old = GameObject.Find("ClubSelectorCanvas");
        if (old != null) Destroy(old);

        var canvasGO = new GameObject("ClubSelectorCanvas");
        DontDestroyOnLoad(canvasGO);
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

        var panelGO = new GameObject("Panel");
        panelGO.transform.SetParent(canvasGO.transform, false);
        var panelRect = panelGO.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0f, 0f);
        panelRect.anchorMax = new Vector2(0f, 0f);
        panelRect.pivot     = new Vector2(0f, 0f);
        panelRect.anchoredPosition = new Vector2(20f, 20f);
        panelRect.sizeDelta = new Vector2(140f, 36f);
        var panelImg = panelGO.AddComponent<Image>();
        panelImg.color = new Color(0.05f, 0.05f, 0.05f, 0.75f);

        var textGO = new GameObject("ClubName");
        textGO.transform.SetParent(panelGO.transform, false);
        var textRect = textGO.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(8f, 0f);
        textRect.offsetMax = new Vector2(-8f, 0f);
        _clubLabel = textGO.AddComponent<Text>();
        _clubLabel.text = "Driver  [Q/E]";
        _clubLabel.fontSize = 13;
        _clubLabel.color = new Color(1f, 0.85f, 0.2f);
        _clubLabel.alignment = TextAnchor.MiddleLeft;
        _clubLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        _clubs = ClubBag.GetFullBag();
        _clubIndex = 0;
        UpdateClubLabel();
    }

    Text _clubLabel;
    ClubDefinition[] _clubs;
    int _clubIndex;

    void UpdateClubLabel()
    {
        if (_clubLabel == null || _clubs == null) return;
        _clubLabel.text = _clubs[_clubIndex].clubName + "  [Q/E]";
        if (_shooter != null) _shooter.OnClubChanged(_clubs[_clubIndex]);
    }

    void LateUpdate()
    {
        if (_clubs == null) return;
        if (Input.GetKeyDown(KeyCode.Q))
        {
            _clubIndex = (_clubIndex - 1 + _clubs.Length) % _clubs.Length;
            UpdateClubLabel();
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            _clubIndex = (_clubIndex + 1) % _clubs.Length;
            UpdateClubLabel();
        }
    }
}
