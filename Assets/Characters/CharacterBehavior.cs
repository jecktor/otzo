using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class CustomerBehavior : MonoBehaviour
{
	[Header("Store navigation")]
	[Tooltip("Assign shelf/pickup points here")]
	public Transform[] storeLocations;
	
	[Tooltip("Assign store exit here")]
	public Transform exit;

	[Tooltip("Assign the cash register object here (has QueueManager)")]
	public Transform cashRegister;
	
	[Tooltip("Reference to grocery manager")]
	public GroceryManager groceryManager;

	[Tooltip("Player's current money (debug only)")]
	public float playerMoney = 0f;

	NavMeshAgent agent;
	Animator anim;

	bool isShopping = true;
	bool isBusy = false;

	void Start()
	{
		agent = GetComponent<NavMeshAgent>();
		anim = GetComponent<Animator>();

		// Ensure NavMeshAgent settings are safe
		agent.updateRotation = true;
		agent.updatePosition = true;
		agent.isStopped = false;

		// Start by going to a random store shelf
		GoToRandomStoreLocation();
	}

	void Update()
	{
		// --- Update animation based on velocity ---
		float speed = agent.velocity.magnitude;
		anim.SetFloat("Speed", speed);

		// --- Handle arrival logic ---
		if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance && !isBusy)
		{
			if (isShopping)
			{
				// Finished shopping trip to shelf
				StartCoroutine(PickUpRoutine());
			}
			else
			{
				// Already in queue, check if it's our turn
				QueueManager qm = cashRegister.GetComponent<QueueManager>();
				int myPos = qm.GetPositionInQueue(this);

				if (myPos == 0) // First in line
					StartCoroutine(CheckoutRoutine());
			}
		}
	}

	// --- Choose a random shelf and go there ---
	void GoToRandomStoreLocation()
	{
		if (storeLocations.Length == 0) return;

		Transform destination = storeLocations[Random.Range(0, storeLocations.Length)];
		agent.SetDestination(destination.position);
	}

	// --- Play the pickup animation at the shelf ---
	IEnumerator PickUpRoutine()
	{
		isBusy = true;
		agent.isStopped = true;

		// Trigger animation
		anim.SetTrigger("PickUp");

		// Wait for animation to play (adjust to match clip length)
		yield return new WaitForSeconds(5f);

		agent.isStopped = false;

		// After pickup, move toward the queue
		if (isShopping)
		{
			isShopping = false;
			QueueManager qm = cashRegister.GetComponent<QueueManager>();
			if (qm != null)
			{
				Transform slot = qm.RequestSlot(this);
				agent.SetDestination(slot.position);
			}
		}

		isBusy = false;
	}

	// --- Handle checkout when at front of queue ---
	IEnumerator CheckoutRoutine()
	{
		isBusy = true;
		agent.isStopped = true;

		// Play the checkout animation
		anim.SetTrigger("PickUp");
		yield return new WaitForSeconds(3f);

		// Show groceries on counter
		float payment = 0f;
		if (groceryManager != null)
		{
			payment = groceryManager.ShowRandomGroceries();
			Debug.Log($"Customer shows groceries worth ${payment}");
		}

		// Wait for player confirmation
		Debug.Log($"{name} waiting for player to confirm checkout...");
		yield return new WaitUntil(() => 
			CheckoutZone.playerInZone && Input.GetKeyDown(KeyCode.E));

		// Player gets money
		playerMoney += payment;
		Debug.Log($"Player total money: ${playerMoney}");

		// Hide groceries again
		if (groceryManager != null)
			groceryManager.HideAllGroceries();

		// Notify the queue manager that this customer is done
		QueueManager qm = cashRegister.GetComponent<QueueManager>();
		qm.OnCustomerFinished(this);

		// Head to exit
		agent.isStopped = false;
		agent.SetDestination(exit.position);

		// Wait until customer leaves
		yield return new WaitUntil(() =>
			!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance);

		Destroy(gameObject);
		isBusy = false;
	}

}
