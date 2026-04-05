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

        // GolfClub visual
        GameObject clubGO  = new GameObject("GolfClub");
        GolfClub golfClub  = clubGO.AddComponent<GolfClub>();
        golfClub.Build(clubs[0]);
        Object.DontDestroyOnLoad(clubGO);

        // Swing animator
        GameObject animGO         = new GameObject("ClubSwingAnimator");
        ClubSwingAnimator animator = animGO.AddComponent<ClubSwingAnimator>();
        animator.Init(golfClub, shooter, clubs[0]);
        Object.DontDestroyOnLoad(animGO);

        // Club selector UI
        GameObject uiGO        = new GameObject("ClubSelectorUI");
        ClubSelectorUI selector = uiGO.AddComponent<ClubSelectorUI>();
        selector.Init(clubs);
        Object.DontDestroyOnLoad(uiGO);

        // Wire event: propagate club changes to shooter, physics controller, and animator
        selector.OnClubChanged += def =>
        {
            shooter.OnClubChanged(def);
            shooter.GetComponent<BallPhysicsController>()?.SetRollingDragMultiplier(def.rollingDragMultiplier);
            animator.OnClubChanged(def);
        };

        // Apply Driver defaults immediately
        shooter.OnClubChanged(clubs[0]);
        shooter.GetComponent<BallPhysicsController>()?.SetRollingDragMultiplier(clubs[0].rollingDragMultiplier);
    }
}
