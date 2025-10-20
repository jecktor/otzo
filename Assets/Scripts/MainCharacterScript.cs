using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerMovement))]
public class MainCharacterScript : MonoBehaviour
{
    public static MainCharacterScript Instance { get; private set; }

    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction jumpAction;

    private PlayerMovement movement;


    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeComponents();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializeComponents()
    {
        playerInput = GetComponent<PlayerInput>();
        movement = GetComponent<PlayerMovement>();

        moveAction = playerInput.actions.FindAction("Movement");
        lookAction = playerInput.actions.FindAction("Look");
        jumpAction = playerInput.actions.FindAction("Jump");
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {

        if (scene.name == "room")
        {
            SetupPlayerAtFixedPosition();
        }
        else
        {
            SetupPlayerWithSpawnPoint();
        }
    }

    void SetupPlayerAtFixedPosition()
    {
        movement.TeleportToPosition(
            new Vector3(0f, 2f, 0f),
            Quaternion.identity
        );
    }

    void SetupPlayerWithSpawnPoint()
    {
        movement.TeleportToPosition(
            new Vector3(6.63f, 2.852f, -17.76f),
            Quaternion.identity
        );
    }

    void Update()
    {
        if (Instance != this) return;

        Vector2 moveInput = moveAction.ReadValue<Vector2>();
        Vector2 lookInput = lookAction.ReadValue<Vector2>();
        bool jumpInput = jumpAction.WasPressedThisFrame();

        movement.ProcessMovement(moveInput, jumpInput);
        movement.ProcessLook(lookInput);
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
}