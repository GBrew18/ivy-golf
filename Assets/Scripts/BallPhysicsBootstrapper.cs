using UnityEngine;

/// <summary>
/// Creates a BallPhysicsProfile at runtime and attaches BallPhysicsController
/// to the ball automatically — no scene wiring required.
/// </summary>
public static class BallPhysicsBootstrapper
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        BallShooter shooter = Object.FindObjectOfType<BallShooter>();
        if (shooter == null) return;

        // Skip if already attached (e.g. placed manually in the scene).
        if (shooter.GetComponent<BallPhysicsController>() != null) return;

        // Create the profile with Wii Sports defaults.
        BallPhysicsProfile profile = ScriptableObject.CreateInstance<BallPhysicsProfile>();

        BallPhysicsController controller = shooter.gameObject.AddComponent<BallPhysicsController>();

        // Inject the profile via the serialized field using reflection so the
        // bootstrapper works without making the field public.
        var field = typeof(BallPhysicsController)
            .GetField("profile",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance);
        field?.SetValue(controller, profile);
    }
}
