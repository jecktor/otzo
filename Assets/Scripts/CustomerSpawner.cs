using UnityEngine;
using System.Collections.Generic;

public class CustomerSpawner : MonoBehaviour
{
	[Header("Customer spawning")]
	[Tooltip("The prefab with NavMeshAgent, Animator, and CustomerBehavior")]
	public GameObject customerPrefab;

	[Tooltip("Where customers spawn (usually the store exit/entrance)")]
	public Transform spawnPoint;

	[Tooltip("Maximum number of active customers allowed")]
	public int maxCustomersInStore = 5;

	[Tooltip("Time (seconds) between spawns")]
	public float spawnRate = 3f;

	[Header("Customer setup")]
	public Transform[] storeLocations;   // assign shelf points
	public Transform cashRegister;       // assign register object
	public Transform exit;               // assign exit
	public GroceryManager groceryManager; // ✅ new reference

	private List<GameObject> activeCustomers = new List<GameObject>();
	private float timer;

	void Update()
	{
		// Clean up nulls (customers destroyed after exiting)
		activeCustomers.RemoveAll(c => c == null);

		// Timer logic
		timer += Time.deltaTime;
		if (timer >= spawnRate && activeCustomers.Count < maxCustomersInStore)
		{
			SpawnCustomer();
			timer = 0f;
		}
	}

	void SpawnCustomer()
	{
		if (customerPrefab == null || spawnPoint == null) return;

		GameObject newCustomer = Instantiate(customerPrefab, spawnPoint.position, spawnPoint.rotation);
		activeCustomers.Add(newCustomer);

		// Assign setup references
		CustomerBehavior behavior = newCustomer.GetComponent<CustomerBehavior>();
		if (behavior != null)
		{
			behavior.storeLocations = storeLocations;
			behavior.cashRegister = cashRegister;
			behavior.exit = exit;
			behavior.groceryManager = groceryManager; // ✅ add this
		}
	}
}
