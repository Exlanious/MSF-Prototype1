using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // Public variables for tweaking movement in the Unity Inspector.
    public float moveSpeed = 5.0f;

    private Vector2 moveVector;

    void Update()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");
        moveVector = new Vector3(moveX, moveY).normalized;
    }

    /// <summary>
    /// Called every physics step. It's best practice to handle Rigidbody movement in FixedUpdate.
    /// </summary>
    void FixedUpdate()
    {
        // Rotate the character to face the direction of movement.
        if (moveVector != Vector2.zero)
        {
            transform.position += (Vector3)(moveVector * moveSpeed * Time.fixedDeltaTime);
        }
    }



}
