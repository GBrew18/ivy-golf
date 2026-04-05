using UnityEngine;

/// <summary>
/// Wires up the full club system at runtime — no scene setup needed.
/// Runs after every scene load that contains a BallShooter.
/// </summary>
public static class ClubBootstrapper
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        BallShooter shooter = Object.FindFirstObjectByType<BallShooter>();
        if (shooter == null) return;

        ClubDefinition[] clubs = ClubBag.GetFullBag();

        // GolfClub visual — attach to shooter so it follows the aim pivot.
        if (shooter.GetComponent<GolfClub>() == null)
        {
            GolfClub golfClub = shooter.gameObject.AddComponent<GolfClub>();
            golfClub.Build(clubs[0]);
        }

        // Apply Driver defaults immediately.
        shooter.OnClubChanged(clubs[0]);
        shooter.GetComponent<BallPhysicsController>()?.SetRollingDragMultiplier(clubs[0].rollingDragMultiplier);
    }
}
