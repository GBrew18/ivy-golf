using System.Collections;
using UnityEngine;

/// <summary>
/// Auto-wires <see cref="PowerMeterUI"/> into every scene that contains a
/// <see cref="BallShooter"/>.
/// Uses a one-frame delayed coroutine to ensure all other scene objects have
/// finished their Awake/Start before we search for BallShooter.
/// </summary>
public static class PowerMeterBootstrapper
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        // Spawn a tiny MonoBehaviour runner so we can yield one frame
        GameObject runner = new GameObject("_PowerMeterBootstrapRunner");
        runner.AddComponent<PowerMeterBootstrapRunner>();
    }
}

/// <summary>
/// Helper MonoBehaviour that waits one frame, then instantiates PowerMeterUI.
/// Destroys itself when done.
/// </summary>
public class PowerMeterBootstrapRunner : MonoBehaviour
{
    private IEnumerator Start()
    {
        // Wait one frame — ensures BallShooter has fully awakened
        yield return null;

        BallShooter shooter = FindObjectOfType<BallShooter>();
        if (shooter == null)
        {
            Destroy(gameObject);
            yield break;
        }

        // Check canvas render mode explicitly
        GameObject pmGO  = new GameObject("PowerMeter");
        PowerMeterUI ui  = pmGO.AddComponent<PowerMeterUI>();
        ui.Init(shooter);
        DontDestroyOnLoad(pmGO);

        Destroy(gameObject);
    }
}
