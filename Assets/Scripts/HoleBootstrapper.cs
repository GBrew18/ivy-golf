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

        // ── Reposition ball to tee ────────────────────────────────────────────
        // NOTE: Bootstrap() runs before Start() (AfterSceneLoad order).
        // ResetShot.Start() would overwrite startPosition with transform.position,
        // so we set transform.position here first so Start() captures the tee pos.
        ResetShot resetShot = Object.FindFirstObjectByType<ResetShot>();
        if (resetShot != null)
        {
            resetShot.transform.position = builder.TeeBallPosition;
            resetShot.SetStartPosition(builder.TeeBallPosition);
            resetShot.ResetBallToStart();
        }

        // ── HoleScorecard ─────────────────────────────────────────────────────
        new GameObject("HoleScorecard").AddComponent<HoleScorecard>();

        // ── TeeBox flyover camera ─────────────────────────────────────────────
        new GameObject("TeeBoxCamera").AddComponent<TeeBoxCamera>();
    }
}
