using System.Collections;
using UnityEngine;

/// <summary>
/// On scene load, shows a 3-second flyover from a high overview position
/// looking down the fairway toward the green, then smoothly descends to the
/// normal aim-camera position behind the tee. The regular FollowCamera is
/// re-enabled at the end.
///
/// Added automatically by HoleBootstrapper.
/// </summary>
public class TeeBoxCamera : MonoBehaviour
{
    private void Start()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        FollowCamera followCam = Object.FindFirstObjectByType<FollowCamera>();
        HoleBuilder  builder   = Object.FindFirstObjectByType<HoleBuilder>();

        StartCoroutine(Flyover(cam, followCam, builder));
    }

    private IEnumerator Flyover(Camera cam, FollowCamera followCam, HoleBuilder builder)
    {
        // Disable the follow camera so we can take over.
        if (followCam != null) followCam.enabled = false;

        // ── Determine key positions ───────────────────────────────────────────
        Vector3 teePos   = builder != null ? builder.TeeBallPosition : new Vector3(0f, 2.15f, 0f);
        Vector3 greenPos = builder != null ? builder.CupPosition     : new Vector3(0f, 6.12f, 248f);

        // Start: high up, offset behind the tee, looking toward the green.
        Vector3 overviewPos = teePos + new Vector3(0f, 45f, -25f);

        // End: just behind and above the tee ball — normal aim-cam feel.
        Vector3 aimPos = teePos + new Vector3(0f, 6f, -10f);

        // ── Set initial camera state ──────────────────────────────────────────
        cam.transform.position = overviewPos;
        cam.transform.LookAt(greenPos);
        Quaternion startRot = cam.transform.rotation;

        // Aim rotation: look slightly forward from behind the tee.
        Quaternion endRot = Quaternion.LookRotation(
            (teePos + Vector3.forward * 20f) - aimPos, Vector3.up);

        // ── Fly ───────────────────────────────────────────────────────────────
        float duration = 3.0f;
        float elapsed  = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            cam.transform.position = Vector3.Lerp(overviewPos, aimPos, t);
            cam.transform.rotation = Quaternion.Slerp(startRot, endRot, t);
            yield return null;
        }

        cam.transform.position = aimPos;
        cam.transform.rotation = endRot;

        // ── Hand back to the regular follow camera ────────────────────────────
        if (followCam != null) followCam.enabled = true;

        Destroy(gameObject);
    }
}
