using System;
using UnityEngine;

/// <summary>
/// Singleton MonoBehaviour that owns the current shot-flow state and
/// broadcasts transitions to any interested listener.
/// Place one instance in the scene on its own GameObject.
/// </summary>
public class GameStateManager : MonoBehaviour
{
    /// <summary>All high-level phases of a single shot.</summary>
    public enum GameState { Aiming, Charging, InFlight, Landed }

    /// <summary>The single shared instance of <see cref="GameStateManager"/>.</summary>
    public static GameStateManager Instance { get; private set; }

    /// <summary>The phase the game is currently in.</summary>
    public GameState CurrentState { get; private set; } = GameState.Aiming;

    /// <summary>Fired whenever the state changes; passes the new <see cref="GameState"/> as the argument.</summary>
    public event Action<GameState> OnStateChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// Transitions to <paramref name="newState"/> and fires <see cref="OnStateChanged"/>.
    /// No-ops if the state is already <paramref name="newState"/>.
    /// </summary>
    public void SetState(GameState newState)
    {
        if (CurrentState == newState)
            return;

        CurrentState = newState;
        OnStateChanged?.Invoke(newState);
    }
}
