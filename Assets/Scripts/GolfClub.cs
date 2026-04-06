using UnityEngine;
using UnityEngine.Rendering;

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
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        go.name = "Grip";
        go.transform.SetParent(transform, false);
        go.transform.localScale    = new Vector3(0.008f, 0.06f, 0.008f);
        go.transform.localPosition = new Vector3(0f, 0.07f, 0f);
        SetColor(go, _def.gripColor);
        DestroyImmediate(go.GetComponent<Collider>());
    }

    private void BuildShaft()
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        go.name = "Shaft";
        go.transform.SetParent(transform, false);
        go.transform.localScale    = new Vector3(0.005f, 0.38f, 0.005f);
        go.transform.localPosition = new Vector3(0f, -0.31f, 0f);
        SetColor(go, _def.shaftColor);
        DestroyImmediate(go.GetComponent<Collider>());
    }

    private void BuildHead()
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = "ClubHead";
        go.transform.SetParent(transform, false);
        go.transform.localScale    = new Vector3(0.04f, 0.018f, 0.022f);
        go.transform.localPosition = new Vector3(0f, -0.70f, 0f);
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

        // No shadows — prevents odd shadow artifacts from small primitives
        r.shadowCastingMode = ShadowCastingMode.Off;
        r.receiveShadows    = false;
    }
}
