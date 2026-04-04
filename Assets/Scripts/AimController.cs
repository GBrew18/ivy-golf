using UnityEngine;

public class AimController : MonoBehaviour
{
    public float rotationSpeed = 100f;

    void Update()
    {
        // Only allow aiming when the game is in the Aiming state.
        if (GameStateManager.Instance != null &&
            GameStateManager.Instance.CurrentState != GameStateManager.GameState.Aiming)
            return;

        float input = 0f;

        if (Input.GetKey(KeyCode.A))
            input = -1f;

        if (Input.GetKey(KeyCode.D))
            input = 1f;

        transform.Rotate(0f, input * rotationSpeed * Time.deltaTime, 0f);
    }
}
