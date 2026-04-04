using UnityEngine;

/// <summary>
/// ScriptableObject that holds all tunable parameters for Wii Sports-style ball physics.
/// Create one via Assets > Create > Golf > Ball Physics Profile, or let
/// BallPhysicsBootstrapper auto-create an instance at runtime.
/// </summary>
[CreateAssetMenu(fileName = "BallPhysicsProfile", menuName = "Golf/Ball Physics Profile")]
public class BallPhysicsProfile : ScriptableObject
{
    [Header("Bounce & Friction")]
    [Tooltip("Almost no bounce — ball thunks into the ground and sticks.")]
    public float bounciness = 0.05f;
    public float dynamicFriction = 0.8f;
    public float staticFriction = 0.9f;

    [Header("Landing")]
    [Tooltip("Multiply velocity by this on first ground contact (kills the bounce).")]
    [Range(0f, 1f)]
    public float landingVelocityDamping = 0.35f;

    [Header("Rolling Drag")]
    [Tooltip("High linear drag once ball is rolling — stops quickly.")]
    public float rollingLinearDrag = 4.0f;
    [Tooltip("High angular drag once ball is rolling — no spin-out.")]
    public float rollingAngularDrag = 8.0f;

    [Header("In-Flight Drag")]
    [Tooltip("Low drag while airborne — clean arc.")]
    public float inFlightLinearDrag = 0.05f;
    public float inFlightAngularDrag = 0.1f;

    [Header("Settle")]
    [Tooltip("When speed drops below this, snap velocity to zero and call it settled.")]
    public float settleSpeedThreshold = 0.3f;
}
