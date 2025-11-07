using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;
using System.Collections;

public class QueueManager : MonoBehaviour
{
	[Header("Queue slots (front to back)")]
	public Transform[] queueSlots;

	[Header("Store pickup spots (unique per customer)")]
	public Transform[] storeSpots; // assign shelf/pickup points here

	// ----- Queue data -----
	private readonly List<CustomerBehavior> queue = new List<CustomerBehavior>();
	public bool IsQueueFull => queue.Count >= queueSlots.Length;
	public int GetQueueCount() => queue.Count;

	// ----- Store spot reservations -----
	private readonly Queue<Transform> availableSpots = new Queue<Transform>();
	private readonly Dictionary<CustomerBehavior, Transform> spotByCustomer = new Dictionary<CustomerBehavior, Transform>();

	void Awake()
	{
		if (storeSpots != null)
			foreach (var t in storeSpots)
				if (t != null) availableSpots.Enqueue(t);
	}

	// -------- STORE SPOTS --------
	public Transform RequestStoreSpot(CustomerBehavior customer)
	{
		if (spotByCustomer.TryGetValue(customer, out var existing)) return existing;
		if (availableSpots.Count == 0) return null;

		var spot = availableSpots.Dequeue();
		spotByCustomer[customer] = spot;
		return spot;
	}

	public void ReleaseStoreSpot(CustomerBehavior customer)
	{
		if (spotByCustomer.TryGetValue(customer, out var spot) && spot != null)
		{
			spotByCustomer.Remove(customer);
			availableSpots.Enqueue(spot);
		}
	}

	// -------- QUEUE --------
	public Transform RequestSlot(CustomerBehavior customer)
	{
		if (!queue.Contains(customer)) queue.Add(customer);
		int index = Mathf.Min(queue.Count - 1, queueSlots.Length - 1);
		return queueSlots[index];
	}

	public void OnCustomerFinished(CustomerBehavior finishedCustomer)
	{
		ReleaseStoreSpot(finishedCustomer); // safety
		if (!queue.Contains(finishedCustomer)) return;

		queue.Remove(finishedCustomer);
		UpdateQueuePositions();
	}

	public int GetPositionInQueue(CustomerBehavior c) => queue.IndexOf(c);

	private void UpdateQueuePositions()
	{
		for (int i = 0; i < queue.Count; i++)
		{
			var customer = queue[i];
			if (!customer) continue;

			int slotIndex = Mathf.Min(i, queueSlots.Length - 1);
			var newSlot = queueSlots[slotIndex];

			var agent = customer.GetComponent<NavMeshAgent>();
			if (agent) agent.SetDestination(newSlot.position);
		}
	}

	// ------- Bulk clear helpers (optional) -------
	public IEnumerator ClearAllCustomers()
	{
		var customers = GameObject.FindObjectsOfType<CustomerBehavior>();
		foreach (var c in customers)
			if (c) yield return c.StartCoroutine(c.Leave());
	}

	public void ClearAllCustomersInstant()
	{
		var customers = GameObject.FindObjectsOfType<CustomerBehavior>();
		foreach (var c in customers)
			if (c) c.StartCoroutine(c.Leave());
	}
}
