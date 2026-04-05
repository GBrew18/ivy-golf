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

        // GolfClub visual
        GameObject clubGO  = new GameObject("GolfClub");
        GolfClub golfClub  = clubGO.AddComponent<GolfClub>();
        golfClub.Build(clubs[0]);
        Object.DontDestroyOnLoad(clubGO);

        // Apply Driver defaults immediately
        shooter.OnClubChanged(clubs[0]);
        shooter.GetComponent<BallPhysicsController>()?.SetRollingDragMultiplier(clubs[0].rollingDragMultiplier);
    }
}
