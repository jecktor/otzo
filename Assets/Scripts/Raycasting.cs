using UnityEngine;

namespace Otzo
{
	public class Raycasting : MonoBehaviour
	{
		public static GameObject TargetObject;

		void Update()
		{
			if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit))
			{
				TargetObject = hit.collider.gameObject;
			}
			else
			{
				TargetObject = null;
			}
		}
	}
}
