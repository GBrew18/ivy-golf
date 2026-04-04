using System;
using UnityEngine;

/// <summary>
/// Trigger zone placed at the cup/hole in the green.
/// Fires <see cref="OnBallHoledOut"/> when the ball rolls in slowly enough.
/// Added automatically by <see cref="HoleBuilder"/> to the cup GameObject.
/// </summary>
public class CupDetector : MonoBehaviour
{
    /// <summary>Fired when the ball enters the cup at a valid speed.</summary>
    public static event Action OnBallHoledOut;

    [Tooltip("Ball must be moving slower than this (m/s) to count as holed out.")]
    public float maxEntrySpeed = 2.0f;

    private bool _holed;

    private void OnTriggerEnter(Collider other)
    {
        if (_holed) return;

        // Must be the golf ball (identified by BallShooter component).
        if (other.GetComponent<BallShooter>() == null) return;

        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb == null) return;

        // Must be rolling slowly — flying shots that clip the trigger don't count.
        if (rb.linearVelocity.magnitude >= maxEntrySpeed) return;

        _holed = true;

        int strokes = HoleScorecard.Instance != null ? HoleScorecard.Instance.StrokeCount : 0;
        Debug.Log($"[CupDetector] Holed out in {strokes} stroke(s)!");

        OnBallHoledOut?.Invoke();
        GameStateManager.Instance?.SetState(GameStateManager.GameState.HoledOut);
    }
}
