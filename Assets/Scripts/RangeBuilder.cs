using UnityEngine;

/// <summary>
/// Builds a simple driving range from Unity primitive objects.
/// Attach this to an empty GameObject, then either:
/// 1) Enable Build On Start, or
/// 2) Call BuildRange() from your own script/UI.
/// </summary>
[DisallowMultipleComponent]
public class RangeBuilder : MonoBehaviour
{
    [Header("Build Options")]
    [Tooltip("If true, BuildRange() is called automatically in Start().")]
    [SerializeField] private bool buildOnStart = true;
    [Tooltip("If true, previously generated range objects are removed before rebuilding.")]
    [SerializeField] private bool clearPreviousBuild = true;
    [SerializeField] private string generatedRootName = "GeneratedRange";
    [Tooltip("Remove colliders from generated primitives if you do not need physics interaction.")]
    [SerializeField] private bool removePrimitiveColliders = false;

    [Header("Ball Reference")]
    [Tooltip("Optional: assign the golf ball to sync its ResetShot tee position after the range is built.")]
    [SerializeField] private ResetShot ballResetShot;

    [Header("Tee Mat")]
    [SerializeField] private Vector3 teeSize = new Vector3(2f, 0.1f, 3f);
    [Tooltip("Local offset from this GameObject. Keep Z near 0 so the tee is near origin.")]
    [SerializeField] private Vector3 teeOffset = Vector3.zero;
    [SerializeField] private Material teeMaterial;
    [SerializeField] private Color teeColor = new Color(0.15f, 0.5f, 0.15f, 1f);

    [Header("Range Strip / Fairway")]
    [Min(1f)][SerializeField] private float rangeLength = 120f;
    [Min(1f)][SerializeField] private float rangeWidth = 30f;
    [Min(0.01f)][SerializeField] private float rangeThickness = 0.1f;
    [Min(0f)][SerializeField] private float gapFromTee = 1f;
    [SerializeField] private Material fairwayMaterial;
    [SerializeField] private Color fairwayColor = new Color(0.25f, 0.65f, 0.25f, 1f);

    [Header("Targets")]
    [Tooltip("Distances in meters from this GameObject along +Z (forward).")]
    [SerializeField] private float[] targetDistances = { 25f, 50f, 100f };
    [Min(0.1f)][SerializeField] private float targetDiameter = 2.5f;
    [Min(0.05f)][SerializeField] private float targetHeight = 0.2f;
    [SerializeField] private Material targetMaterial;
    [SerializeField] private Color targetColor = new Color(1f, 0.85f, 0.2f, 1f);

    private void Start()
    {
        if (buildOnStart)
        {
            BuildRange();
        }
    }

    /// <summary>
    /// Public build method so this can be triggered by gameplay/UI scripts.
    /// </summary>
    [ContextMenu("Build Range")]
    public void BuildRange()
    {
        Transform root = GetOrCreateGeneratedRoot();

        if (clearPreviousBuild)
        {
            ClearChildren(root);
        }

        // Tee mat (cube): placed near origin.
        Vector3 teeCenter = new Vector3(
            teeOffset.x,
            teeOffset.y + (teeSize.y * 0.5f),
            teeOffset.z
        );

        CreatePrimitive(
            "TeeMat",
            PrimitiveType.Cube,
            teeCenter,
            teeSize,
            root,
            teeMaterial,
            teeColor
        );

        // Notify the ball's ResetShot of the actual tee surface position so
        // pressing R always returns to the correct spot even when the range
        // is built at runtime rather than placed in the Editor.
        if (ballResetShot != null)
        {
            // Top surface of the tee mat in world space.
            Vector3 teeWorldPos = transform.TransformPoint(
                new Vector3(teeCenter.x, teeCenter.y + teeSize.y * 0.5f, teeCenter.z)
            );
            ballResetShot.SetStartPosition(teeWorldPos);
        }

        // Fairway strip (cube): starts in front of tee and extends forward (+Z).
        float teeFrontZ = teeCenter.z + (teeSize.z * 0.5f);
        float fairwayCenterZ = teeFrontZ + gapFromTee + (rangeLength * 0.5f);

        Vector3 fairwayCenter = new Vector3(
            teeCenter.x,
            rangeThickness * 0.5f,
            fairwayCenterZ
        );

        CreatePrimitive(
            "Fairway",
            PrimitiveType.Cube,
            fairwayCenter,
            new Vector3(rangeWidth, rangeThickness, rangeLength),
            root,
            fairwayMaterial,
            fairwayColor
        );

        // Targets (cylinders): one marker per distance in targetDistances.
        // Unity cylinder default size is diameter 1, height 2.
        // So scale Y is half desired height.
        if (targetDistances != null)
        {
            for (int i = 0; i < targetDistances.Length; i++)
            {
                float distance = Mathf.Max(0f, targetDistances[i]);

                Vector3 targetCenter = new Vector3(
                    teeCenter.x,
                    targetHeight * 0.5f,
                    distance
                );

                float cylinderYScale = targetHeight * 0.5f;

                GameObject target = CreatePrimitive(
                    $"Target_{distance:0.#}m",
                    PrimitiveType.Cylinder,
                    targetCenter,
                    new Vector3(targetDiameter, cylinderYScale, targetDiameter),
                    root,
                    targetMaterial,
                    targetColor
                );

                // Attach TargetZone and record metadata so hit events carry context.
                TargetZone zone = target.AddComponent<TargetZone>();
                zone.targetIndex = i;
                zone.distanceFromTee = distance;

                target.name = $"Target_{distance:0.#}m";
            }
        }
    }

    private Transform GetOrCreateGeneratedRoot()
    {
        Transform existing = transform.Find(generatedRootName);
        if (existing != null)
        {
            return existing;
        }

        GameObject root = new GameObject(generatedRootName);
        root.transform.SetParent(transform, false);
        return root.transform;
    }

    private void ClearChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            DestroySafely(parent.GetChild(i).gameObject);
        }
    }

    private GameObject CreatePrimitive(
        string objectName,
        PrimitiveType type,
        Vector3 localPosition,
        Vector3 localScale,
        Transform parent,
        Material material,
        Color color
    )
    {
        GameObject go = GameObject.CreatePrimitive(type);
        go.name = objectName;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPosition;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = localScale;

        Renderer renderer = go.GetComponent<Renderer>();
        ApplyMaterialOrColor(renderer, material, color);

        if (removePrimitiveColliders)
        {
            Collider col = go.GetComponent<Collider>();
            if (col != null)
            {
                DestroySafely(col);
            }
        }

        return go;
    }

    private static void ApplyMaterialOrColor(Renderer renderer, Material material, Color color)
    {
        if (renderer == null) return;

        if (material != null)
        {
            renderer.sharedMaterial = material;
            return;
        }

        // Use an instance so changing color here does not modify shared assets.
        Material runtimeMat = renderer.material;

        if (runtimeMat.HasProperty("_BaseColor"))
        {
            runtimeMat.SetColor("_BaseColor", color); // URP/HDRP
        }
        else if (runtimeMat.HasProperty("_Color"))
        {
            runtimeMat.SetColor("_Color", color); // Built-in pipeline
        }
    }

    private static void DestroySafely(Object obj)
    {
        if (obj == null) return;

        if (Application.isPlaying)
        {
            Object.Destroy(obj);
        }
        else
        {
            Object.DestroyImmediate(obj);
        }
    }

    private void OnValidate()
    {
        // Keep values sensible for beginners and avoid accidental negatives.
        teeSize.x = Mathf.Max(0.1f, teeSize.x);
        teeSize.y = Mathf.Max(0.01f, teeSize.y);
        teeSize.z = Mathf.Max(0.1f, teeSize.z);

        rangeLength = Mathf.Max(1f, rangeLength);
        rangeWidth = Mathf.Max(1f, rangeWidth);
        rangeThickness = Mathf.Max(0.01f, rangeThickness);
        gapFromTee = Mathf.Max(0f, gapFromTee);

        targetDiameter = Mathf.Max(0.1f, targetDiameter);
        targetHeight = Mathf.Max(0.05f, targetHeight);

        if (targetDistances == null || targetDistances.Length == 0)
        {
            targetDistances = new float[] { 25f, 50f, 100f };
        }
    }
}
