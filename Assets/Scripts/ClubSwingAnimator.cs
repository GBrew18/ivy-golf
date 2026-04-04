using System.Collections;
using UnityEngine;

/// <summary>
/// Drives GolfClub transform (position + rotation) for address idle, backswing,
/// and downswing follow-through. Tracks the AimController pivot so the club always
/// sits to the right of the ball relative to the aim direction.
///
/// Swing plane: address rotation is Euler(-55, yaw, 0) so the shaft leans forward
/// at address. Backswing adds positive X rotation (up and back over the shoulder).
/// Follow-through adds negative X rotation (forward past the ball).
/// </summary>
public class ClubSwingAnimator : MonoBehaviour
{
    private GolfClub       _club;
    private BallShooter    _shooter;
    private ClubDefinition _def;
    private Transform      _aimPivot;   // AimController's transform

    // Address lean: shaft tilts -55° (forward) to match real golf address position.
    // Backswing and follow-through are deltas added on top of this constant.
    private const float AddressXOffset = -55f;
    private const float LerpSpeed      = 8f;

    // _currentXRot is the DELTA from address (0 = at address, positive = backswing up)
    private float _currentXRot;
    private float _currentZRot;
    private bool  _downswingPlaying;

    public void Init(GolfClub club, BallShooter shooter, ClubDefinition def)
    {
        _club    = club;
        _shooter = shooter;
        _def     = def;

        // Cache the aim pivot so we can read its right vector for positioning
        AimController ac = FindObjectOfType<AimController>();
        _aimPivot = ac != null ? ac.transform : null;
    }

    public void OnClubChanged(ClubDefinition def)
    {
        _def = def;
        _club.Build(def);
        _currentXRot = 0f;
        _currentZRot = 0f;
    }

    private void OnEnable()
    {
        if (GameStateManager.Instance != null)
            GameStateManager.Instance.OnStateChanged += OnStateChanged;
    }

    private void OnDisable()
    {
        if (GameStateManager.Instance != null)
            GameStateManager.Instance.OnStateChanged -= OnStateChanged;
    }

    private void OnStateChanged(GameStateManager.GameState state)
    {
        if (state == GameStateManager.GameState.InFlight && !_downswingPlaying)
            StartCoroutine(Downswing());
        else if (state == GameStateManager.GameState.Aiming)
        {
            StopAllCoroutines();
            _downswingPlaying = false;
            _currentXRot      = 0f;
            _currentZRot      = 0f;
        }
    }

    private void Update()
    {
        if (_club == null || _shooter == null) return;

        PositionClub();

        if (_downswingPlaying) return;

        var state = GameStateManager.Instance?.CurrentState ?? GameStateManager.GameState.Aiming;

        if (state == GameStateManager.GameState.Charging)
        {
            float t = _shooter.maxForce > 0f ? _shooter.CurrentForce / _shooter.maxForce : 0f;

            // Backswing delta: 0 at address → +backswingDegrees at full charge
            // abs() because maxBackswingDegrees is stored as negative in ClubDefinition
            float backswingDelta = Mathf.Abs(_def.maxBackswingDegrees);
            float targetX        = Mathf.Lerp(0f, backswingDelta, t);

            float wristCock = _def.clubType == ClubType.Putter ? 0f : -12f;
            float targetZ   = Mathf.Lerp(0f, wristCock, t);

            _currentXRot = Mathf.Lerp(_currentXRot, targetX, LerpSpeed * Time.deltaTime);
            _currentZRot = Mathf.Lerp(_currentZRot, targetZ, LerpSpeed * Time.deltaTime);
        }
        else if (state == GameStateManager.GameState.Aiming)
        {
            // Gentle idle sway around address (delta ≈ 0 → total ≈ AddressXOffset)
            float sway   = Mathf.Sin(Time.time * 1.2f) * 1.5f;
            _currentXRot = Mathf.Lerp(_currentXRot, sway,  LerpSpeed * Time.deltaTime);
            _currentZRot = Mathf.Lerp(_currentZRot, 0f,    LerpSpeed * Time.deltaTime);
        }

        ApplyRotation();
    }

    /// <summary>
    /// Place the club pivot to the RIGHT of the ball (relative to aim direction),
    /// 0.45 units right and 0.95 units up. Uses AimController's right vector so the
    /// club follows the player's aim, not world right.
    /// </summary>
    private void PositionClub()
    {
        Transform ball  = _shooter.transform;
        Vector3   right = _aimPivot != null ? _aimPivot.right : ball.right;

        Vector3 pivotPos = ball.position + right * 0.45f;
        pivotPos.y = ball.position.y + 0.95f;
        _club.transform.position = pivotPos;
    }

    /// <summary>
    /// Apply rotation: base yaw follows aim direction; X = AddressXOffset + _currentXRot
    /// so the club starts leaning forward at address and goes up/back during backswing.
    /// </summary>
    private void ApplyRotation()
    {
        float yaw = _aimPivot != null
            ? _aimPivot.eulerAngles.y - 8f
            : _shooter.transform.eulerAngles.y - 8f;

        _club.transform.rotation = Quaternion.Euler(
            AddressXOffset + _currentXRot,
            yaw,
            _currentZRot);
    }

    private IEnumerator Downswing()
    {
        _downswingPlaying = true;

        float startX = _currentXRot;
        float startZ = _currentZRot;

        // Phase 1: from backswing → follow-through (-28° delta = 27° past address)
        float elapsed  = 0f;
        float duration = 0.30f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t  = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            _currentXRot = Mathf.Lerp(startX, -28f, t);
            _currentZRot = Mathf.Lerp(startZ, 0f,   t);
            PositionClub();
            ApplyRotation();
            yield return null;
        }

        _currentXRot = -28f;
        _currentZRot = 0f;

        // Phase 2: hold follow-through
        yield return new WaitForSeconds(0.25f);

        // Phase 3: return to address (delta = 0)
        startX   = _currentXRot;
        elapsed  = 0f;
        duration = 0.70f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t  = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            _currentXRot = Mathf.Lerp(startX, 0f, t);
            PositionClub();
            ApplyRotation();
            yield return null;
        }

        _currentXRot      = 0f;
        _downswingPlaying = false;
    }
}
