using System.Collections;
using UnityEngine;

/// <summary>
/// Drives GolfClub transform (position + rotation) for address idle, backswing,
/// and downswing follow-through. Also repositions the club pivot to track the ball.
/// </summary>
public class ClubSwingAnimator : MonoBehaviour
{
    private GolfClub      _club;
    private BallShooter   _shooter;
    private ClubDefinition _def;

    private const float LerpSpeed = 8f;

    private float _currentXRot;
    private float _currentZRot;
    private bool  _downswingPlaying;

    public void Init(GolfClub club, BallShooter shooter, ClubDefinition def)
    {
        _club    = club;
        _shooter = shooter;
        _def     = def;
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
            float t       = _shooter.maxForce > 0f ? _shooter.CurrentForce / _shooter.maxForce : 0f;
            float targetX = Mathf.Lerp(0f, _def.maxBackswingDegrees, t);
            float wristCock = _def.clubType == ClubType.Putter ? 0f : -12f;
            float targetZ = Mathf.Lerp(0f, wristCock, t);

            _currentXRot = Mathf.Lerp(_currentXRot, targetX, LerpSpeed * Time.deltaTime);
            _currentZRot = Mathf.Lerp(_currentZRot, targetZ, LerpSpeed * Time.deltaTime);
        }
        else if (state == GameStateManager.GameState.Aiming)
        {
            float sway   = Mathf.Sin(Time.time * 1.2f) * 1.5f;
            _currentXRot = Mathf.Lerp(_currentXRot, sway,  LerpSpeed * Time.deltaTime);
            _currentZRot = Mathf.Lerp(_currentZRot, 0f,    LerpSpeed * Time.deltaTime);
        }

        ApplyRotation();
    }

    private void PositionClub()
    {
        // Place pivot 0.55 units to the ball's right at hand height.
        Transform ball   = _shooter.transform;
        Vector3 pivotPos = ball.position + ball.right * 0.55f;
        pivotPos.y       = ball.position.y + 0.95f;
        _club.transform.position = pivotPos;
    }

    private void ApplyRotation()
    {
        // Base yaw follows ball aim, angled 8° inward toward the ball.
        float yaw = _shooter.transform.eulerAngles.y - 8f;
        _club.transform.rotation = Quaternion.Euler(_currentXRot, yaw, _currentZRot);
    }

    private IEnumerator Downswing()
    {
        _downswingPlaying = true;

        float startX = _currentXRot;
        float startZ = _currentZRot;

        // Phase 1: backswing → +28° follow-through over 0.30 s
        float elapsed  = 0f;
        float duration = 0.30f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t  = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            _currentXRot = Mathf.Lerp(startX, 28f, t);
            _currentZRot = Mathf.Lerp(startZ, 0f,  t);
            PositionClub();
            ApplyRotation();
            yield return null;
        }

        _currentXRot = 28f;
        _currentZRot = 0f;

        // Phase 2: hold follow-through for 0.25 s
        yield return new WaitForSeconds(0.25f);

        // Phase 3: return to address over 0.70 s
        startX  = _currentXRot;
        elapsed = 0f;
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
