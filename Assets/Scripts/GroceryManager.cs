using UnityEngine;
using System.Collections.Generic;

public class GroceryManager : MonoBehaviour
{
	[Header("Groceries in ascending price order (0 = cheapest)")]
	public List<GameObject> groceries = new List<GameObject>();

	[Header("Price per grocery index (optional override)")]
	public float basePrice = 5f; // price multiplier per index (e.g. 0 = $5, 9 = $45)

	[Header("Current total")]
	public float currentTotal = 0f;

	// Show 1–3 random groceries and calculate total price
	public float ShowRandomGroceries()
	{
		HideAllGroceries();
		currentTotal = 0f;

		int count = Random.Range(1, 4); // 1–3 items
		List<int> used = new List<int>();

		for (int i = 0; i < count; i++)
		{
			int index;
			do
			{
				index = Random.Range(0, groceries.Count);
			} while (used.Contains(index));
			used.Add(index);

			// Activate grocery
			groceries[index].SetActive(true);

			// Add to price
			currentTotal += basePrice * (index + 1);
		}

		return currentTotal;
	}

	// Hide all groceries
	public void HideAllGroceries()
	{
		foreach (GameObject g in groceries)
			g.SetActive(false);
	}
}
