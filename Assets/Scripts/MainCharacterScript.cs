using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerMovement))]
public class MainCharacterScript : MonoBehaviour
{
    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction jumpAction;

    private PlayerMovement movement;

    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        movement = GetComponent<PlayerMovement>();

        // Asignar acciones
        moveAction = playerInput.actions.FindAction("Movement");
        lookAction = playerInput.actions.FindAction("Look");
        jumpAction = playerInput.actions.FindAction("Jump");
    }

    void Update()
    {
        Vector2 moveInput = moveAction.ReadValue<Vector2>();
        Vector2 lookInput = lookAction.ReadValue<Vector2>();
        bool jumpInput = jumpAction.WasPressedThisFrame();

        movement.ProcessMovement(moveInput, jumpInput);
        movement.ProcessLook(lookInput);
    }
}
