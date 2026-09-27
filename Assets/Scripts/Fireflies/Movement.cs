using UnityEngine;

public class Movement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float gravity = -9.81f;
    
    private CharacterController controller;
    private Vector3 velocity;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        // 1. Get input
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        // 2. Calculate movement vector relative to player rotation
        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        
        // 3. Move the character controller
        controller.Move(move * moveSpeed * Time.deltaTime);

        // 4. Handle simple gravity so the player falls down
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Keeps the player snapped to the ground
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}
