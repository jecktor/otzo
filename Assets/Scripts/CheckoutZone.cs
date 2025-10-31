using UnityEngine;

public class CheckoutZone : MonoBehaviour
{
	public static bool playerInZone = false;

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Player"))
			playerInZone = true;
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.CompareTag("Player"))
			playerInZone = false;
	}
}
