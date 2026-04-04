using UnityEngine;

/// <summary>
/// Instantiates all Hole 1 gameplay objects at runtime — no scene wiring needed.
/// Skips execution in the DrivingRange scene (detected by the presence of RangeBuilder).
/// </summary>
public static class HoleBootstrapper
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        // Don't run in the driving range scene.
        if (Object.FindObjectOfType<RangeBuilder>() != null) return;

        // Don't double-build if HoleBuilder already exists (e.g. placed manually).
        if (Object.FindObjectOfType<HoleBuilder>() != null) return;

        // ── HoleBuilder ───────────────────────────────────────────────────────
        GameObject holeGO = new GameObject("Hole1Builder");
        HoleBuilder builder = holeGO.AddComponent<HoleBuilder>();
        builder.buildOnStart = false; // we call BuildHole() right now
        builder.BuildHole();

        // ── Reposition ball to tee ────────────────────────────────────────────
        ResetShot resetShot = Object.FindObjectOfType<ResetShot>();
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
}
