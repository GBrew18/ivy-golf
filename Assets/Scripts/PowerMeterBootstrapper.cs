using UnityEngine;

/// <summary>
/// Auto-wires <see cref="PowerMeterUI"/> into every scene that contains a
/// <see cref="BallShooter"/> — no Inspector setup required.
/// The method runs once after the first scene finishes loading.
/// </summary>
public static class PowerMeterBootstrapper
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        BallShooter shooter = Object.FindObjectOfType<BallShooter>();
        if (shooter == null) return;   // no ball in this scene — skip

        GameObject go = new GameObject("PowerMeter");
        PowerMeterUI ui = go.AddComponent<PowerMeterUI>();
        ui.Init(shooter);
        Object.DontDestroyOnLoad(go);
    }
}
