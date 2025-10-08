using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private CharacterController characterController;

    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 0f;
    public float gravity = -16;

    [Header("Camera Settings")]
    public float mouseSensitivity = 0.6f;
    public Transform camPos;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    private Vector3 velocity;
    private bool isGrounded;
    private float xRotation = 0f;
    private bool canJump = true;

    void Awake()
    {
        characterController = GetComponent<CharacterController>();
        if (groundCheck == null) groundCheck = transform;
    }

    public void ProcessMovement(Vector2 input, bool jumpInput)
    {
        Vector3 move = (transform.right * input.x) + (transform.forward * input.y);
        characterController.Move(move * moveSpeed * Time.deltaTime);

        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
            canJump = true;
        }

        if (jumpInput && isGrounded && canJump)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
            canJump = false;
        }

        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }

    public void ProcessLook(Vector2 input)
    {
        float mouseX = input.x * mouseSensitivity;
        float mouseY = input.y * mouseSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90, 90);

        if (camPos != null)
            camPos.localRotation = Quaternion.Euler(xRotation, 0, 0);

        transform.Rotate(Vector3.up * mouseX);
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
        }
    }
}
