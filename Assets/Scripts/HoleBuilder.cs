using UnityEngine;

/// <summary>
/// Procedurally builds a par-4 golf hole (~250 units, slightly uphill) from primitive
/// meshes with distinct materials. No scene or prefab editing required.
///
/// Hole 1 layout:
///   - Elevated tee box at (0, 2, 0) — ball starts at Y≈2.15
///   - Straight fairway: 18 units wide, 250 units long, rising from Y=0 to Y=6
///   - Rough strips either side of fairway (brownish-green)
///   - Flat circular green (radius≈16) centered at (0, 6, 248)
///   - Cup trigger + flagstick at pin position
///   - Large surrounding ground plane
/// </summary>
[DisallowMultipleComponent]
public class HoleBuilder : MonoBehaviour
{
    [Header("Hole Identity")]
    public int holeNumber = 1;
    public int par = 4;

    [Header("Build Options")]
    public bool buildOnStart = true;
    [SerializeField] private string rootName = "GeneratedHole1";

    // ── Surface colors ────────────────────────────────────────────────────────
    private static readonly Color FairwayColor = new Color(0.25f, 0.65f, 0.25f, 1f);
    private static readonly Color RoughColor   = new Color(0.28f, 0.42f, 0.18f, 1f); // brownish-green
    private static readonly Color GreenColor   = new Color(0.12f, 0.78f, 0.22f, 1f);
    private static readonly Color TeeColor     = new Color(0.22f, 0.60f, 0.22f, 1f);
    private static readonly Color GroundColor  = new Color(0.38f, 0.52f, 0.28f, 1f);
    private static readonly Color FlagRed      = new Color(0.90f, 0.15f, 0.15f, 1f);
    private static readonly Color PoleWhite    = new Color(0.92f, 0.92f, 0.92f, 1f);
    private static readonly Color CupBlack     = new Color(0.05f, 0.05f, 0.05f, 1f);

    // ── Geometry constants ────────────────────────────────────────────────────
    private const float FairwayLength = 250f;
    private const float FairwayWidth  = 18f;
    private const float GreenY        = 6f;          // green elevation
    private const float GreenRadius   = 16f;         // cylinder half-diameter in scale
    private const float TeeElevation  = 2f;          // tee box surface Y
    private const float RoughWidth    = 18f;
    private const float PinZ          = 248f;        // pin along fairway

    // ── Runtime-accessible positions ─────────────────────────────────────────
    /// <summary>World-space position where the ball should be placed on the tee.</summary>
    public Vector3 TeeBallPosition { get; private set; }

    /// <summary>World-space position of the cup / pin.</summary>
    public Vector3 CupPosition { get; private set; }

    // ─────────────────────────────────────────────────────────────────────────

    private void Start()
    {
        if (buildOnStart)
            BuildHole();
    }

    [ContextMenu("Build Hole")]
    public void BuildHole()
    {
        Transform root = GetOrCreateRoot();
        ClearChildren(root);

        BuildGround(root);
        BuildTeeBox(root);
        BuildFairway(root);
        BuildRough(root);
        BuildGreen(root);
        BuildFlagstick(root);
        BuildCup(root);
    }

    // ── Ground ────────────────────────────────────────────────────────────────
    private void BuildGround(Transform root)
    {
        // Large flat plane beneath everything — slightly angled to follow the slope.
        // We fake the uphill feel with a gentle slope on the fairway itself rather
        // than tilting the entire ground, which would complicate ball physics.
        GameObject go = Prim("Ground", PrimitiveType.Cube,
            new Vector3(0f, -0.5f, FairwayLength * 0.5f),
            new Vector3(120f, 1f, FairwayLength + 60f),
            root, GroundColor);
        SafeTag(go, "Ground");
    }

    // ── Tee box ───────────────────────────────────────────────────────────────
    private void BuildTeeBox(Transform root)
    {
        // Elevated platform — the ball sits on top at Y = TeeElevation + 0.15.
        TeeBallPosition = transform.TransformPoint(new Vector3(0f, TeeElevation + 0.15f, 0f));

        GameObject go = Prim("TeeBox", PrimitiveType.Cube,
            new Vector3(0f, TeeElevation, 0f),
            new Vector3(5f, 0.15f, 5f),
            root, TeeColor);
        SafeTag(go, "Ground");
    }

    // ── Fairway ───────────────────────────────────────────────────────────────
    // The fairway rises from Y≈0 at the tee to Y=GreenY at the green.
    // We split it into 5 segments, each slightly higher, to approximate the uphill slope
    // without a continuous mesh (which primitive cubes can't do directly).
    private void BuildFairway(Transform root)
    {
        const int segments = 5;
        float segLen = FairwayLength / segments;

        for (int i = 0; i < segments; i++)
        {
            float t0 = (float)i / segments;
            float t1 = (float)(i + 1) / segments;
            float yMid = Mathf.Lerp(0f, GreenY, (t0 + t1) * 0.5f);
            float zMid = segLen * i + segLen * 0.5f;

            // Tilt each segment to match the slope between its start and end Y.
            float yStart = Mathf.Lerp(0f, GreenY, t0);
            float yEnd   = Mathf.Lerp(0f, GreenY, t1);
            float angle  = Mathf.Atan2(yEnd - yStart, segLen) * Mathf.Rad2Deg;

            GameObject seg = new GameObject($"Fairway_Seg{i}");
            seg.transform.SetParent(root, false);
            seg.transform.localPosition = new Vector3(0f, yMid, zMid);
            seg.transform.localRotation = Quaternion.Euler(-angle, 0f, 0f);

            GameObject mesh = Prim($"Fairway_Seg{i}_Mesh", PrimitiveType.Cube,
                Vector3.zero,
                new Vector3(FairwayWidth, 0.12f, segLen + 0.2f), // small overlap to avoid gaps
                seg.transform, FairwayColor);
            SafeTag(mesh, "Fairway");
        }
    }

    // ── Rough ─────────────────────────────────────────────────────────────────
    private void BuildRough(Transform root)
    {
        // Left and right rough follow the same slope; use flat slabs for simplicity.
        float roughYMid = GreenY * 0.5f;

        GameObject roughL = Prim("Rough_Left", PrimitiveType.Cube,
            new Vector3(-(FairwayWidth * 0.5f + RoughWidth * 0.5f), roughYMid, FairwayLength * 0.5f),
            new Vector3(RoughWidth, 0.08f, FairwayLength + 40f),
            root, RoughColor);
        SafeTag(roughL, "Rough");

        GameObject roughR = Prim("Rough_Right", PrimitiveType.Cube,
            new Vector3( (FairwayWidth * 0.5f + RoughWidth * 0.5f), roughYMid, FairwayLength * 0.5f),
            new Vector3(RoughWidth, 0.08f, FairwayLength + 40f),
            root, RoughColor);
        SafeTag(roughR, "Rough");
    }

    // ── Green ─────────────────────────────────────────────────────────────────
    private void BuildGreen(Transform root)
    {
        CupPosition = transform.TransformPoint(new Vector3(0f, GreenY + 0.12f, PinZ));

        // Flat cylinder: Unity default diameter=1, height=2 → scale (diameter, halfHeight, diameter)
        GameObject go = Prim("Green", PrimitiveType.Cylinder,
            new Vector3(0f, GreenY, PinZ),
            new Vector3(GreenRadius * 2f, 0.06f, GreenRadius * 2f),
            root, GreenColor);
        SafeTag(go, "Green");
    }

    // ── Flagstick ─────────────────────────────────────────────────────────────
    private void BuildFlagstick(Transform root)
    {
        // Pole: 3 units tall (Unity cylinder height=2 at localScale.y=1, so 1.5 → height=3)
        GameObject pole = Prim("Flagpole", PrimitiveType.Cylinder,
            new Vector3(0f, GreenY + 1.56f, PinZ),
            new Vector3(0.08f, 1.5f, 0.08f),
            root, PoleWhite);
        Object.Destroy(pole.GetComponent<Collider>()); // non-collidable

        // Flag quad — small rectangle at the top of the pole
        GameObject flag = GameObject.CreatePrimitive(PrimitiveType.Quad);
        flag.name = "Flag";
        flag.transform.SetParent(root, false);
        flag.transform.localPosition = new Vector3(0.6f, GreenY + 3.1f, PinZ);
        flag.transform.localScale    = new Vector3(1.1f, 0.65f, 1f);
        ApplyColor(flag.GetComponent<Renderer>(), FlagRed);
        Object.Destroy(flag.GetComponent<Collider>());
    }

    // ── Cup trigger ───────────────────────────────────────────────────────────
    private void BuildCup(Transform root)
    {
        // Small cylinder slightly sunk into the green — isTrigger so CupDetector fires.
        // Radius ~0.75 world units gives the ball a fair chance to sink in.
        GameObject cup = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cup.name = "Cup";
        cup.transform.SetParent(root, false);
        cup.transform.localPosition = new Vector3(0f, GreenY - 0.05f, PinZ);
        cup.transform.localScale    = new Vector3(1.5f, 0.15f, 1.5f);
        ApplyColor(cup.GetComponent<Renderer>(), CupBlack);

        Collider col = cup.GetComponent<Collider>();
        if (col != null) col.isTrigger = true;

        cup.AddComponent<CupDetector>();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    private Transform GetOrCreateRoot()
    {
        Transform existing = transform.Find(rootName);
        if (existing != null) return existing;
        GameObject go = new GameObject(rootName);
        go.transform.SetParent(transform, false);
        return go.transform;
    }

    private static void ClearChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
            Object.Destroy(parent.GetChild(i).gameObject);
    }

    private static GameObject Prim(string name, PrimitiveType type,
        Vector3 localPos, Vector3 localScale, Transform parent, Color color)
    {
        GameObject go = GameObject.CreatePrimitive(type);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale    = localScale;
        ApplyColor(go.GetComponent<Renderer>(), color);
        return go;
    }

    private static void ApplyColor(Renderer r, Color color)
    {
        if (r == null) return;
        Material m = r.material;
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", color); // URP / HDRP
        else if (m.HasProperty("_Color")) m.SetColor("_Color", color);    // Built-in
    }

    /// <summary>Tags a GameObject, silently skipping if the tag isn't registered.</summary>
    private static void SafeTag(GameObject go, string tag)
    {
        try { go.tag = tag; }
        catch (UnityException) { /* tag not in project — harmless */ }
    }
}
