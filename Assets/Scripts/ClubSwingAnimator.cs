using System.Collections;
using UnityEngine;

/// <summary>
/// Plays a quick forward-swing animation on the aim pivot when a shot fires,
/// then eases back to the address position. Attach to the same GameObject as
/// <see cref="BallShooter"/> (the aim pivot).
///
/// Auto-wired by <see cref="ClubSwingBootstrapper"/> — no scene setup required.
/// </summary>
public class ClubSwingAnimator : MonoBehaviour
{
    // ── Tuning ────────────────────────────────────────────────────────────────
    private const float SwingAngle    = 80f;   // degrees the pivot tips forward on release
    private const float SwingDuration = 0.16f; // fast snap through
    private const float ReturnDuration = 0.50f; // slower ease back to address

    // ── Runtime state ─────────────────────────────────────────────────────────
    private float     _addressRotX;   // X-angle recorded at Start (address position)
    private Coroutine _swingRoutine;

    // ── Unity lifecycle ───────────────────────────────────────────────────────
    private void Start()
    {
        // Record the address rotation from whatever the pivot starts at.
        _addressRotX = NormalizeAngle(transform.localEulerAngles.x);

        if (GameStateManager.Instance != null)
            GameStateManager.Instance.OnStateChanged += OnStateChanged;
    }

    private void OnDestroy()
    {
        if (GameStateManager.Instance != null)
            GameStateManager.Instance.OnStateChanged -= OnStateChanged;
    }

    // ── State handler ─────────────────────────────────────────────────────────
    private void OnStateChanged(GameStateManager.GameState state)
    {
        if (state == GameStateManager.GameState.InFlight)
        {
            // Kick off swing on shot release.
            if (_swingRoutine != null) StopCoroutine(_swingRoutine);
            _swingRoutine = StartCoroutine(SwingAndReturn());
        }
        else if (state == GameStateManager.GameState.Aiming)
        {
            // Snap back to address immediately on reset (R key).
            if (_swingRoutine != null) { StopCoroutine(_swingRoutine); _swingRoutine = null; }
            SetLocalRotX(_addressRotX);
        }
    }

    // ── Swing coroutine ───────────────────────────────────────────────────────
    private IEnumerator SwingAndReturn()
    {
        // Phase 1 — swing through: tip the pivot forward quickly.
        float from    = GetLocalRotX();
        float through = _addressRotX - SwingAngle; // negative X = forward tilt
        float elapsed = 0f;

        while (elapsed < SwingDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / SwingDuration);
            SetLocalRotX(Mathf.LerpAngle(from, through, Mathf.SmoothStep(0f, 1f, t)));
            yield return null;
        }
        SetLocalRotX(through);

        // Phase 2 — return to address: ease back smoothly.
        elapsed = 0f;
        while (elapsed < ReturnDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / ReturnDuration);
            SetLocalRotX(Mathf.LerpAngle(through, _addressRotX, Mathf.SmoothStep(0f, 1f, t)));
            yield return null;
        }
        SetLocalRotX(_addressRotX);
        _swingRoutine = null;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    /// <summary>Reads the current local X Euler angle, normalized to [-180, 180].</summary>
    private float GetLocalRotX()
        => NormalizeAngle(transform.localEulerAngles.x);

    private void SetLocalRotX(float x)
    {
        Vector3 e = transform.localEulerAngles;
        e.x = x;
        transform.localEulerAngles = e;
    }

    /// <summary>Maps a Unity Euler angle (0–360) to the signed range [-180, 180].</summary>
    private static float NormalizeAngle(float a)
        => a > 180f ? a - 360f : a;
}

/// <summary>
/// Attaches <see cref="ClubSwingAnimator"/> to the BallShooter GameObject at runtime.
/// </summary>
public static class ClubSwingBootstrapper
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        BallShooter shooter = Object.FindFirstObjectByType<BallShooter>();
        if (shooter == null) return;
        if (shooter.GetComponent<ClubSwingAnimator>() != null) return;

        shooter.gameObject.AddComponent<ClubSwingAnimator>();
    }
}
