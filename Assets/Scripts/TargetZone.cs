using System;
using UnityEngine;

/// <summary>
/// Attach to a target cylinder to detect when the golf ball lands on it.
/// <see cref="RangeBuilder"/> adds this component automatically to each
/// generated target and populates <see cref="targetIndex"/> and
/// <see cref="distanceFromTee"/>.
/// </summary>
public class TargetZone : MonoBehaviour
{
    /// <summary>Zero-based index of this target in the range layout.</summary>
    public int targetIndex;

    /// <summary>Distance from the tee to this target in meters.</summary>
    public float distanceFromTee;

    /// <summary>
    /// Fired when any collider enters this target zone.
    /// Passes the <see cref="TargetZone"/> that was hit.
    /// </summary>
    public static event Action<TargetZone> OnTargetHit;

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"[TargetZone] Target {targetIndex} hit at {distanceFromTee:0.#} m " +
                  $"by '{collision.gameObject.name}'.");

        OnTargetHit?.Invoke(this);
    }
}
