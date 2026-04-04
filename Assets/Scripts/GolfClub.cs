using UnityEngine;

/// <summary>
/// Builds the club mesh from Unity primitives at runtime.
/// The GameObject's pivot is at the top of the grip (hands).
/// Call Build(def) to construct or swap to a different club.
/// Positioning and rotation are driven externally by ClubSwingAnimator.
/// </summary>
public class GolfClub : MonoBehaviour
{
    private ClubDefinition _def;

    public void Build(ClubDefinition def)
    {
        _def = def;

        // Remove old visual children (keep the pivot GO itself)
        for (int i = transform.childCount - 1; i >= 0; i--)
            Destroy(transform.GetChild(i).gameObject);

        BuildGrip();
        BuildShaft();
        BuildHead();
    }

    // Unity cylinder: default height = 2 units, radius = 0.5.
    // Scale x/z = diameter, scale y = half-length.

    private void BuildGrip()
    {
        // 0.07 radius → 0.14 diameter.  0.22 length → y scale = 0.11.
        // Center at half the grip length below pivot.
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        go.name = "Grip";
        go.transform.SetParent(transform, false);
        go.transform.localScale    = new Vector3(0.14f, 0.11f, 0.14f);
        go.transform.localPosition = new Vector3(0f, -0.11f, 0f);
        SetColor(go, _def.gripColor);
        DestroyImmediate(go.GetComponent<Collider>());
    }

    private void BuildShaft()
    {
        // 0.045 radius → 0.09 diameter.  length = shaftLength → y scale = shaftLength/2.
        // Top of shaft sits flush below grip bottom (y = -0.22).
        float halfLen = _def.shaftLength * 0.5f;
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        go.name = "Shaft";
        go.transform.SetParent(transform, false);
        go.transform.localScale    = new Vector3(0.09f, halfLen, 0.09f);
        go.transform.localPosition = new Vector3(0f, -0.22f - halfLen, 0f);
        SetColor(go, _def.shaftColor);
        DestroyImmediate(go.GetComponent<Collider>());
    }

    private void BuildHead()
    {
        // Cube default size = 1×1×1.  Scale directly to head dimensions.
        // Center sits one half-height below the shaft bottom.
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = "ClubHead";
        go.transform.SetParent(transform, false);
        go.transform.localScale    = new Vector3(_def.headWidth, _def.headHeight, _def.headDepth);
        go.transform.localPosition = new Vector3(
            0f,
            -0.22f - _def.shaftLength - _def.headHeight * 0.5f,
            0f);
        SetColor(go, _def.headColor);
        DestroyImmediate(go.GetComponent<Collider>());
    }

    private static void SetColor(GameObject go, Color color)
    {
        Renderer r = go.GetComponent<Renderer>();
        if (r == null) return;
        // Instance material so each club part has its own color
        r.material       = new Material(Shader.Find("Standard"));
        r.material.color = color;
    }
}
