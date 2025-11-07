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
	
	[Tooltip("Time (seconds) penalty")]
	public float meltdownPenalty = 10f;

	[Header("Customer setup")]
	public Transform[] storeLocations;   // assign shelf points
	public Transform cashRegister;       // assign register object
	public Transform exit;               // assign exit
	public GroceryManager groceryManager; // ✅ new reference
	public ScanMiniGame scanMiniGame;
	public AudioSource customerEnter;
	public AudioSource groceries;

	private List<GameObject> activeCustomers = new List<GameObject>();
	private float timer;
	private float penaltyTimer;
	private bool meltdownInProgress;

	public bool IsMeltdownInProgress => meltdownInProgress;

	public void ClearAllCustomers()
	{
		CustomerBehavior[] customers = FindObjectsOfType<CustomerBehavior>();

		foreach (var customer in customers)
		{
			if (customer != null)
				customer.StartCoroutine(customer.Leave());
		}
		
		meltdownInProgress = true;
	}

	public bool IsStoreFull => activeCustomers.Count == maxCustomersInStore;

	void Update()
	{
		// Clean up nulls (customers destroyed after exiting)
		activeCustomers.RemoveAll(c => c == null);

		if (meltdownInProgress)
		{
			penaltyTimer += Time.deltaTime;
			if (penaltyTimer >= meltdownPenalty)
			{
				meltdownInProgress = false;
				penaltyTimer = 0f;
			}
		}

		// Timer logic
		timer += Time.deltaTime;
		if (!meltdownInProgress && timer >= spawnRate && activeCustomers.Count < maxCustomersInStore)
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
			behavior.scanMiniGame = scanMiniGame;
			behavior.groceries = groceries;
		}
		
		customerEnter.Play();
	}
}
