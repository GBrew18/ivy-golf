using System.Collections;
using UnityEngine;

/// <summary>
/// Master scene bootstrapper. Retries finding BallShooter for up to 60 frames so
/// that HoleBootstrapper / RangeBuilder (which may take several frames) can finish
/// spawning the ball before we try to wire up the club and UI systems.
/// </summary>
public static class GameBootstrapper
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        GameObject runner = new GameObject("_GameBootstrapRunner");
        Object.DontDestroyOnLoad(runner);
        runner.AddComponent<GameBootstrapRunner>();
    }
}

public class GameBootstrapRunner : MonoBehaviour
{
    private IEnumerator Start()
    {
        yield return StartCoroutine(Init());
        Destroy(gameObject);
    }

    private IEnumerator Init()
    {
        BallShooter shooter = null;
        for (int i = 0; i < 60; i++)
        {
            yield return null;
            shooter = Object.FindFirstObjectByType<BallShooter>();
            if (shooter != null) break;
        }

        if (shooter == null)
        {
            Debug.LogWarning("[GameBootstrapper] No BallShooter found after 60 frames.");
            yield break;
        }

        SetupClub(shooter);
        SetupPowerMeter(shooter);
        SetupClubSelector(shooter);
    }

    // ── Club visual + defaults ────────────────────────────────────────────────

    private void SetupClub(BallShooter shooter)
    {
        // Skip if ClubBootstrapper already ran (GolfClub will already exist).
        if (Object.FindFirstObjectByType<GolfClub>() != null) return;

        ClubDefinition[] clubs = ClubBag.GetFullBag();
        GameObject clubGO = new GameObject("GolfClub");
        GolfClub golfClub = clubGO.AddComponent<GolfClub>();
        golfClub.Build(clubs[0]);
        DontDestroyOnLoad(clubGO);

        shooter.OnClubChanged(clubs[0]);
        shooter.GetComponent<BallPhysicsController>()
               ?.SetRollingDragMultiplier(clubs[0].rollingDragMultiplier);
    }

    // ── Power-meter HUD ───────────────────────────────────────────────────────

    private void SetupPowerMeter(BallShooter shooter)
    {
        // Skip if PowerMeterBootstrapper already ran.
        if (Object.FindFirstObjectByType<PowerMeterUI>() != null) return;

        GameObject pmGO = new GameObject("PowerMeter");
        PowerMeterUI ui = pmGO.AddComponent<PowerMeterUI>();
        ui.Init(shooter);
        DontDestroyOnLoad(pmGO);
    }

    // ── Club selector UI ──────────────────────────────────────────────────────

    private void SetupClubSelector(BallShooter shooter)
    {
        // ClubSelectorUI and its bootstrapper are self-contained; no extra wiring needed.
    }
}
