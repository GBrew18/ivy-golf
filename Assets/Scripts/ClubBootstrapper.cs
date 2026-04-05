using UnityEngine;

/// <summary>
/// Wires up the GolfClub visual and applies initial club defaults to the shooter
/// at runtime — no scene setup needed.
/// Runs after every scene load that contains a BallShooter.
/// (ClubSwingAnimator and ClubSelectorUI each have their own bootstrappers.)
/// </summary>
public static class ClubBootstrapper
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        BallShooter shooter = Object.FindFirstObjectByType<BallShooter>();
        if (shooter == null) return;

        ClubDefinition[] clubs = ClubBag.GetFullBag();

        // GolfClub visual — parented to the shooter so it moves/rotates with the ball
        // and ClubSwingAnimator can animate it via the parent transform.
        GameObject clubGO  = new GameObject("GolfClub");
        clubGO.transform.SetParent(shooter.transform, false);
        GolfClub golfClub  = clubGO.AddComponent<GolfClub>();
        golfClub.Build(clubs[0]);

        // Apply Driver defaults immediately
        shooter.OnClubChanged(clubs[0]);
        shooter.GetComponent<BallPhysicsController>()?.SetRollingDragMultiplier(clubs[0].rollingDragMultiplier);
    }
}
