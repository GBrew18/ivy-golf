using UnityEngine;

public class AimController : MonoBehaviour
{
    public float rotationSpeed = 100f;

    void Update()
    {
        float input = 0f;

        if (Input.GetKey(KeyCode.A))
            input = -1f;

        if (Input.GetKey(KeyCode.D))
            input = 1f;

        transform.Rotate(0f, input * rotationSpeed * Time.deltaTime, 0f);
    }
}