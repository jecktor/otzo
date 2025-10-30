using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class CustomerBehavior : MonoBehaviour
{
	[Header("Store navigation")]
	public Transform[] storeLocations;   // assign shelves or pickup points here
	public Transform cashRegister;       // assign the checkout point

	NavMeshAgent agent;
	Animator anim;

	bool isShopping = true;
	bool isBusy = false;

	void Start()
	{
		agent = GetComponent<NavMeshAgent>();
		anim = GetComponent<Animator>();

		// Initial movement setup
		agent.updateRotation = true;
		agent.updatePosition = true;
		agent.isStopped = false;

		GoToRandomStoreLocation();
	}

	void Update()
	{
		// --- Animator control (replaces CharacterMovement.cs) ---
		float speed = agent.velocity.magnitude;
		anim.SetFloat("Speed", speed / agent.speed);

		// --- Arrival logic ---
		if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance && !isBusy)
		{
			if (isShopping)
				StartCoroutine(PickUpRoutine());
			else
				anim.SetFloat("Speed", 0f);
		}
	}

	void GoToRandomStoreLocation()
	{
		if (storeLocations.Length == 0) return;
		Transform destination = storeLocations[Random.Range(0, storeLocations.Length)];
		agent.SetDestination(destination.position);
	}

	IEnumerator PickUpRoutine()
	{
		isBusy = true;
		agent.isStopped = true;

		// Trigger animation (plays your PickUp clip)
		anim.SetTrigger("PickUp");

		// Wait roughly the duration of the animation
		yield return new WaitForSeconds(7f); // adjust to actual pickup clip length

		agent.isStopped = false;

		// After picking up, go to cash register
		if (isShopping)
		{
			isShopping = false;
			agent.SetDestination(cashRegister.position);
		}

		isBusy = false;
	}
}
