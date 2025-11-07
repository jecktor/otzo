using UnityEngine;
using TMPro;

public class CheckoutPromptUI : MonoBehaviour
{
	[Header("Assign the TextMeshPro element here")]
	public TextMeshProUGUI promptText;

	private float currentPayment = 0f;
	private bool customerReady = false;

	void OnEnable()
	{
		CustomerBehavior.OnCustomerReady += HandleCustomerReady;
		CustomerBehavior.OnCustomerLeft += HandleCustomerLeft;
	}

	void OnDisable()
	{
		CustomerBehavior.OnCustomerReady -= HandleCustomerReady;
		CustomerBehavior.OnCustomerLeft -= HandleCustomerLeft;
	}

	void Update()
	{
		if (promptText == null) return;

		// Show prompt only if player is behind counter AND a customer is waiting
		if (CheckoutZone.playerInZone && customerReady)
		{
			promptText.gameObject.SetActive(true);
			promptText.text = $"Presiona [E] para Cobrar (${currentPayment:F2})";
		}
		else
		{
			promptText.gameObject.SetActive(false);
		}
	}

	void HandleCustomerReady(float payment)
	{
		currentPayment = payment;
		customerReady = true;
	}

	void HandleCustomerLeft()
	{
		customerReady = false;
		currentPayment = 0f;
		if (promptText != null)
			promptText.gameObject.SetActive(false);
	}
}
