using UnityEngine;
using TMPro;

public class PlayerWallet : MonoBehaviour
{
	[Header("Assign the TextMeshPro for displaying money")]
	public TextMeshProUGUI moneyText;

	private float totalMoney = 0f;

	void OnEnable()
	{
		CustomerBehavior.OnCustomerReady += OnCustomerReady; // to listen if needed
		CustomerBehavior.OnCustomerLeft += OnCustomerLeft;
	}

	void OnDisable()
	{
		CustomerBehavior.OnCustomerReady -= OnCustomerReady;
		CustomerBehavior.OnCustomerLeft -= OnCustomerLeft;
	}

	void Start()
	{
		UpdateDisplay();
	}

	// Optional: could handle these events if we wanted to react differently
	void OnCustomerReady(float _)
	{
		// Optional: maybe highlight UI or pulse color
	}

	void OnCustomerLeft()
	{
		// The money increment actually happens in CustomerBehavior,
		// so we'll handle the event there instead
	}

	// 👇 Call this from CustomerBehavior when player earns money
	public void AddMoney(float amount)
	{
		totalMoney += amount;
		UpdateDisplay();
	}

	void UpdateDisplay()
	{
		if (moneyText != null)
			moneyText.text = $"${totalMoney:F2}";
	}
}
