using UnityEngine;

/// <summary>
/// Procedurally builds a par-4 golf hole (~250 units, slightly uphill) from primitive
/// meshes with distinct materials. No scene or prefab editing required.
///
/// Hole 1 layout:
///   - Elevated tee box at (0, 2, 0) — ball starts at Y≈2.15
///   - Straight fairway: 18 units wide, 250 units long, rising from Y=0 to Y=6
///   - Rough strips either side of fairway
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

    // ── Surface colors (Wii Sports-inspired realistic palette) ───────────────
    private static readonly Color FairwayColor = new Color(0.25f, 0.50f, 0.18f, 1f); // rich medium grass
    private static readonly Color RoughColor   = new Color(0.20f, 0.38f, 0.15f, 1f); // darker rough
    private static readonly Color GreenColor   = new Color(0.18f, 0.48f, 0.18f, 1f); // bright putting green
    private static readonly Color TeeColor     = new Color(0.22f, 0.45f, 0.18f, 1f); // tee box
    private static readonly Color GroundColor  = new Color(0.30f, 0.55f, 0.25f, 1f); // surrounding grass
    private static readonly Color FlagRed      = new Color(0.90f, 0.15f, 0.15f, 1f);
    private static readonly Color PoleWhite    = new Color(0.92f, 0.92f, 0.92f, 1f);
    private static readonly Color CupBlack     = new Color(0.05f, 0.05f, 0.05f, 1f);

    // ── Geometry constants ────────────────────────────────────────────────────
    private const float FairwayLength = 250f;
    private const float FairwayWidth  = 18f;
    private const float GreenY        = 6f;
    private const float GreenRadius   = 16f;
    private const float TeeElevation  = 2f;
    private const float RoughWidth    = 18f;
    private const float PinZ          = 248f;

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
        GameObject go = Prim("Ground", PrimitiveType.Cube,
            new Vector3(0f, -0.5f, FairwayLength * 0.5f),
            new Vector3(120f, 1f, FairwayLength + 60f),
            root, GroundColor);
        SafeTag(go, "Ground");
    }

    // ── Tee box ───────────────────────────────────────────────────────────────
    private void BuildTeeBox(Transform root)
    {
        TeeBallPosition = transform.TransformPoint(new Vector3(0f, TeeElevation + 0.15f, 0f));

        GameObject go = Prim("TeeBox", PrimitiveType.Cube,
            new Vector3(0f, TeeElevation, 0f),
            new Vector3(5f, 0.15f, 5f),
            root, TeeColor);
        SafeTag(go, "Ground");
    }

    // ── Fairway ───────────────────────────────────────────────────────────────
    private void BuildFairway(Transform root)
    {
        const int segments = 5;
        float segLen = FairwayLength / segments;

        for (int i = 0; i < segments; i++)
        {
            float t0   = (float)i / segments;
            float t1   = (float)(i + 1) / segments;
            float yMid = Mathf.Lerp(0f, GreenY, (t0 + t1) * 0.5f);
            float zMid = segLen * i + segLen * 0.5f;

            float yStart = Mathf.Lerp(0f, GreenY, t0);
            float yEnd   = Mathf.Lerp(0f, GreenY, t1);
            float angle  = Mathf.Atan2(yEnd - yStart, segLen) * Mathf.Rad2Deg;

            GameObject seg = new GameObject($"Fairway_Seg{i}");
            seg.transform.SetParent(root, false);
            seg.transform.localPosition = new Vector3(0f, yMid, zMid);
            seg.transform.localRotation = Quaternion.Euler(-angle, 0f, 0f);

            GameObject mesh = Prim($"Fairway_Seg{i}_Mesh", PrimitiveType.Cube,
                Vector3.zero,
                new Vector3(FairwayWidth, 0.12f, segLen + 0.2f),
                seg.transform, FairwayColor);
            SafeTag(mesh, "Fairway");
        }
    }

    // ── Rough ─────────────────────────────────────────────────────────────────
    private void BuildRough(Transform root)
    {
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

        GameObject go = Prim("Green", PrimitiveType.Cylinder,
            new Vector3(0f, GreenY, PinZ),
            new Vector3(GreenRadius * 2f, 0.06f, GreenRadius * 2f),
            root, GreenColor);
        SafeTag(go, "Green");
    }

    // ── Flagstick ─────────────────────────────────────────────────────────────
    private void BuildFlagstick(Transform root)
    {
        GameObject pole = Prim("Flagpole", PrimitiveType.Cylinder,
            new Vector3(0f, GreenY + 1.56f, PinZ),
            new Vector3(0.08f, 1.5f, 0.08f),
            root, PoleWhite);
        Object.Destroy(pole.GetComponent<Collider>());

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
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", color);
        else if (m.HasProperty("_Color")) m.SetColor("_Color", color);
    }

    private static void SafeTag(GameObject go, string tag)
    {
        // Tags must be pre-registered in Unity's Tag Manager; assigning an
        // unregistered tag throws at runtime. Gameplay does not depend on tags.
    }
}
