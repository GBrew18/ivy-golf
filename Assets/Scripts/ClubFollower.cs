using UnityEngine;
public class ClubFollower : MonoBehaviour
{
    public Transform target;
    public Vector3 worldOffset = new Vector3(0.4f, 0f, 0f);
    void LateUpdate()
    {
        if (target == null) return;
        transform.position = target.position + worldOffset;
        transform.rotation = target.rotation;
    }
}
