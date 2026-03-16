using UnityEngine;

/// <summary>
/// Attach this to the Main Camera.
/// Follows a golf ball with a smooth movement and keeps it in view.
/// </summary>
public class FollowCamera : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("Drag the golf ball Transform here.")]
    [SerializeField] private Transform ballTarget;

    [Header("Follow Settings")]
    [Tooltip("Camera position relative to the ball (X = side, Y = height, Z = behind/in front).")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 6f, -10f);

    [Tooltip("How quickly the camera catches up to the target.")]
    [Min(0.01f)]
    [SerializeField] private float followSpeed = 5f;

    private void Start()
    {
        // Optional helper: if no target is assigned, try to find one tagged "GolfBall".
        if (ballTarget == null)
        {
            GameObject found = GameObject.FindWithTag("GolfBall");
            if (found != null)
            {
                ballTarget = found.transform;
            }
        }
    }

    private void LateUpdate()
    {
        // LateUpdate is best for cameras so it follows after physics/movement updates.
        if (ballTarget == null) return;

        // 1) Smoothly move toward the desired follow position.
        Vector3 desiredPosition = ballTarget.position + offset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);

        // 2) Keep the ball in view by rotating to look at it.
        Vector3 lookDirection = ballTarget.position - transform.position;
        if (lookDirection.sqrMagnitude > 0.0001f)
        {
            Quaternion desiredRotation = Quaternion.LookRotation(lookDirection, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, followSpeed * Time.deltaTime);
        }
    }
}
