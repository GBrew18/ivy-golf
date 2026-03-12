using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BallShooter : MonoBehaviour
{
    public float maxForce = 20f;
    public float chargeSpeed = 15f;

    private Rigidbody rb;
    private float currentForce = 0f;
    private bool isCharging = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Start charging when space is pressed
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isCharging = true;
            currentForce = 0f;
        }

        // Increase power while space is held
        if (Input.GetKey(KeyCode.Space) && isCharging)
        {
            currentForce += chargeSpeed * Time.deltaTime;
            currentForce = Mathf.Clamp(currentForce, 0f, maxForce);
        }

        // Release shot when space is released
        if (Input.GetKeyUp(KeyCode.Space) && isCharging)
        {
            Shoot();
            isCharging = false;
        }
    }

    void Shoot()
    {
        rb.AddForce(transform.forward * currentForce, ForceMode.Impulse);
    }
}