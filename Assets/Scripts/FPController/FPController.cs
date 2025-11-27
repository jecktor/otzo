using UnityEngine;
using Unity.Cinemachine;
using EasyPeasyFirstPersonController;

namespace Otzo
{
    [RequireComponent(typeof(CharacterController))]

    public class FPController : MonoBehaviour
    {
        [Header("Movement Parameters")]
        public float MaxSpeed = 3.5f;
        public float Acceleration = 15f;

        public Vector3 CurrentVelocity { get; private set; }
        public float CurrentSpeed { get; private set; }

        [Header("Looking Parameters")]
        public Vector2 LookSensitivity = new Vector2(0.1f, 0.1f);

        public float PitchLimit = 85f;

        [SerializeField] float currentPitch = 0f;

        public float CurrentPitch
        {
            get => currentPitch;
            set
            {
                currentPitch = Mathf.Clamp(value, -PitchLimit, PitchLimit);
            }
        }

        [Header("Input")]
        public Vector2 MoveInput;
        public Vector2 LookInput;

        [Header("Components")]
        [SerializeField] CinemachineCamera fpCamera;
        [SerializeField] CharacterController characterController;

        // Referencia al controlador original si lo necesitas
        private FirstPersonController easyPeasyController;

        void OnValidate()
        {
            if (characterController == null)
            {
                characterController = GetComponent<CharacterController>();
            }
        }

        void Start()
        {
            // Obtener referencia al controlador de EasyPeasy
            easyPeasyController = GetComponent<FirstPersonController>();

            // Suscribirse a los eventos del juego
            if (GameEvents.Instance != null)
            {
                GameEvents.Instance.OnPlayerControlChanged += HandlePlayerControlChanged;
                GameEvents.Instance.OnCustomerMeltdown += HandleCustomerMeltdown;
            }
        }

        void OnDestroy()
        {
            // Desuscribirse de los eventos para evitar memory leaks
            if (GameEvents.Instance != null)
            {
                GameEvents.Instance.OnPlayerControlChanged -= HandlePlayerControlChanged;
                GameEvents.Instance.OnCustomerMeltdown -= HandleCustomerMeltdown;
            }
        }

        private void HandlePlayerControlChanged(bool enabled)
        {
            SetControl(enabled);
        }

        private void HandleCustomerMeltdown()
        {
            // Reacción específica cuando hay meltdown de clientes
            Debug.Log("Player: Handling customer meltdown");
            SetControl(true); // Asegurar que el player tenga control
        }

        public void SetControl(bool enabled)
        {
            if (easyPeasyController != null)
            {
                easyPeasyController.SetControl(enabled);
            }
            else
            {
                Debug.LogWarning("EasyPeasy FirstPersonController not found");
            }
        }

        void Update()
        {
            MoveUpdate();
            LookUpdate();
        }

        void MoveUpdate()
        {
            Vector3 motion = transform.forward * MoveInput.y + transform.right * MoveInput.x;
            motion.y = 0f;
            motion.Normalize();

            if (motion.sqrMagnitude >= 0.01f)
            {
                CurrentVelocity = Vector3.MoveTowards(CurrentVelocity, motion * MaxSpeed, Acceleration * Time.deltaTime);
            }
            else
            {
                CurrentVelocity = Vector3.MoveTowards(CurrentVelocity, Vector3.zero, Acceleration * Time.deltaTime);
            }

            float verticalVelocity = Physics.gravity.y * 20f * Time.deltaTime;

            Vector3 fullVelocity = new Vector3(CurrentVelocity.x, verticalVelocity, CurrentVelocity.z);

            characterController.Move(fullVelocity * Time.deltaTime);

            CurrentSpeed = CurrentVelocity.magnitude;
        }

        void LookUpdate()
        {
            Vector2 input = new Vector2(LookInput.x * LookSensitivity.x, LookInput.y * LookSensitivity.y);

            CurrentPitch -= input.y;

            fpCamera.transform.localRotation = Quaternion.Euler(CurrentPitch, 0f, 0f);

            transform.Rotate(Vector3.up * input.x);
        }
    }
}