using UnityEngine;
using UnityEngine.InputSystem;

namespace Otzo {
	[RequireComponent(typeof(FPController))]
	
	public class Player : MonoBehaviour
	{
		[Header("Components")]
		[SerializeField] FPController FPController;
		
		void OnMove(InputValue value)
		{
			FPController.MoveInput = value.Get<Vector2>();
		}
		
		void OnLook(InputValue value)
		{
			FPController.LookInput = value.Get<Vector2>();
		}
		
		void OnValidate()
		{
			if (FPController == null) FPController = GetComponent<FPController>();
		}
		
		void Start()
		{
			Cursor.visible = false;
			Cursor.lockState = CursorLockMode.Locked;
		}
	}
}
