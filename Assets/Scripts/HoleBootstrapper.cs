using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Instantiates all Hole 1 gameplay objects at runtime — no scene wiring needed.
///
/// IMPORTANT: Only activates in a Unity scene named "Hole1".
/// Create a Unity scene named "Hole1" and add it to Build Settings for this to activate.
/// The DrivingRange scene uses RangeBuilder instead, and HoleBootstrapper will not
/// interfere with it because of this scene name check.
/// </summary>
public static class HoleBootstrapper
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        // Only run in the Hole1 scene — prevents interference with DrivingRange or other scenes.
        // Create a Unity scene named "Hole1" and add it to Build Settings for this to activate.
        if (SceneManager.GetActiveScene().name != "Hole1") return;

        // Don't double-build if HoleBuilder already exists (e.g. placed manually).
        if (Object.FindFirstObjectByType<HoleBuilder>() != null) return;

        // ── HoleBuilder ───────────────────────────────────────────────────────
        GameObject holeGO = new GameObject("Hole1Builder");
        HoleBuilder builder = holeGO.AddComponent<HoleBuilder>();
        builder.buildOnStart = false; // call BuildHole() right now
        builder.BuildHole();

        // ── Golf ball at tee ──────────────────────────────────────────────────
        // TeeBallPosition = (0, 2.15, 0) — TeeElevation(2) + 0.15 surface offset.
        Vector3 teePosition = builder.TeeBallPosition;

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
        // AimController must be on the ball itself so rotating it affects
        // BallShooter.Shoot()'s transform.forward direction.
        ballGO.AddComponent<BallShooter>();
        ballGO.AddComponent<BallPhysicsController>();
        ballGO.AddComponent<AimController>();

        // ── Reposition ResetShot if one already exists in the scene ──────────
        ResetShot resetShot = Object.FindFirstObjectByType<ResetShot>();
        if (resetShot != null)
        {
            resetShot.SetStartPosition(builder.TeeBallPosition);
            resetShot.ResetBallToStart();
        }

        // ── HoleScorecard ─────────────────────────────────────────────────────
        new GameObject("HoleScorecard").AddComponent<HoleScorecard>();

        // ── TeeBox flyover camera ─────────────────────────────────────────────
        new GameObject("TeeBoxCamera").AddComponent<TeeBoxCamera>();
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
