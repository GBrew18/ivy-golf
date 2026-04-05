using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Instantiates a complete driving-range setup at runtime — no scene wiring needed.
///
/// Runs in any scene that is NOT "Hole1". Creates a RangeBuilder, builds the range
/// terrain, and spawns a golf ball with BallShooter + BallPhysicsController so that
/// GameBootstrapper's 60-frame retry can find it and attach the club/UI systems.
/// </summary>
public static class RangeBootstrapper
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        // Only run outside the Hole1 scene — that scene is owned by HoleBootstrapper.
        if (SceneManager.GetActiveScene().name == "Hole1") return;

        // Don't double-build if a RangeBuilder was placed manually in the scene.
        if (Object.FindFirstObjectByType<RangeBuilder>() != null) return;

        // Skip if a ball already exists (e.g. BallPhysicsBootstrapper already ran).
        if (Object.FindFirstObjectByType<BallShooter>() != null) return;

        // ── RangeBuilder ──────────────────────────────────────────────────────
        GameObject rangeGO = new GameObject("RangeBuilder");
        RangeBuilder rangeBuilder = rangeGO.AddComponent<RangeBuilder>();
        // buildOnStart is true by default — disable it so we call BuildRange() now,
        // before Start() runs, guaranteeing the range exists this frame.
        typeof(RangeBuilder)
            .GetField("buildOnStart",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance)
            ?.SetValue(rangeBuilder, false);
        rangeBuilder.BuildRange();

        // ── Golf ball at tee ──────────────────────────────────────────────────
        // RangeBuilder default tee mat: size.y = 0.1 → surface at Y = 0.1.
        // Add 0.022 so the ball sits cleanly on top rather than clipping in.
        Vector3 teePosition = new Vector3(0f, 0.1f, 0f);

        GameObject ballGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        ballGO.name = "GolfBall";
        ballGO.transform.position = teePosition + Vector3.up * 0.022f;
        ballGO.transform.localScale = Vector3.one * 0.043f;
        ballGO.GetComponent<Renderer>().material.color = Color.white;

        // Physics
        var rb = ballGO.AddComponent<Rigidbody>();
        rb.mass = 0.046f;
        rb.linearDamping = 0.05f;
        rb.angularDamping = 0.05f;
        var col = ballGO.GetComponent<SphereCollider>();
        col.material = CreateBallPhysicsMaterial();

        // BallShooter drives input + launch; BallPhysicsController handles spin/roll.
        // AimController must be on the ball so rotating it affects transform.forward.
        ballGO.AddComponent<BallShooter>();
        ballGO.AddComponent<BallPhysicsController>();
        ballGO.AddComponent<AimController>();
    }

    private static PhysicsMaterial CreateBallPhysicsMaterial()
    {
        return new PhysicsMaterial("BallBootstrapMat")
        {
            bounciness      = 0.02f,
            dynamicFriction = 0.8f,
            staticFriction  = 0.8f,
            bounceCombine   = PhysicsMaterialCombine.Minimum,
            frictionCombine = PhysicsMaterialCombine.Maximum,
        };
    }
}
